using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using YTStdLogger.Core;
using YTStdSqlBuilder;

namespace YTStdAdo;

/// <summary>
/// 全局静态数据库入口类。
/// 所有数据库操作通过此类进行，内部管理连接池。
/// </summary>
public static partial class DB
{
    private static readonly ConcurrentQueue<NpgsqlConnection> _pool = new();
    private static string _connectionString = "";
    private static int _maxPoolSize;
    private static int _retryCount;
    private static int _poolCount;

    #region 连接池管理

    /// <summary>初始化连接池，应用启动时调用一次</summary>
    public static void Init(DbOptions options)
    {
        _connectionString = options.BuildConnectionString();
        _maxPoolSize = options.MaxPoolSize;
        _retryCount = options.RetryCount;
        _poolCount = 0;

        for (int i = 0; i < options.MinPoolSize; i++)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            _pool.Enqueue(conn);
            System.Threading.Interlocked.Increment(ref _poolCount);
        }

        Logger.Info(0, 0, $"[DB.Init] 连接池初始化完成，最小连接数={options.MinPoolSize}，最大连接数={options.MaxPoolSize}");
    }

    /// <summary>优雅关闭连接池</summary>
    public static async ValueTask ShutdownAsync()
    {
        Logger.Info(0, 0, "[DB.ShutdownAsync] 开始关闭连接池...");

        while (_pool.TryDequeue(out var conn))
        {
            try
            {
                await conn.CloseAsync().ConfigureAwait(false);
                await conn.DisposeAsync().ConfigureAwait(false);
                System.Threading.Interlocked.Decrement(ref _poolCount);
            }
            catch (Exception ex)
            {
                Logger.Error(0, 0, $"[DB.ShutdownAsync] 关闭连接异常: {ex.Message}");
            }
        }

        Logger.Info(0, 0, "[DB.ShutdownAsync] 连接池已关闭");
    }

    /// <summary>从连接池获取连接，池空时创建新连接（含重试逻辑）</summary>
    public static NpgsqlConnection GetConnection()
    {
        if (_pool.TryDequeue(out var conn))
        {
            if (conn.State == ConnectionState.Open)
                return conn;

            try { conn.Dispose(); } catch { }
            System.Threading.Interlocked.Decrement(ref _poolCount);
        }

        int retries = _retryCount;
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var newConn = new NpgsqlConnection(_connectionString);
                newConn.Open();
                System.Threading.Interlocked.Increment(ref _poolCount);
                return newConn;
            }
            catch (Exception ex)
            {
                if (attempt == retries)
                    throw new InvalidOperationException(
                        $"[DB.GetConnection] 无法创建数据库连接，已重试 {retries} 次: {ex.Message}", ex);

                Logger.Warn(0, 0, $"[DB.GetConnection] 创建连接失败，第 {attempt + 1} 次重试: {ex.Message}");
                System.Threading.Thread.Sleep(100 * (attempt + 1));
            }
        }

        throw new InvalidOperationException("[DB.GetConnection] 无法获取数据库连接");
    }

    /// <summary>归还连接到连接池（检查连接状态）</summary>
    public static void ReturnConnection(NpgsqlConnection conn)
    {
        if (conn.State == ConnectionState.Open)
        {
            _pool.Enqueue(conn);
        }
        else
        {
            try { conn.Dispose(); } catch { }
            System.Threading.Interlocked.Decrement(ref _poolCount);
        }
    }

    #endregion

    #region 事务管理

    /// <summary>获取批处理对象（含事务与 set_config）</summary>
    public static async ValueTask<NpgsqlBatch> GetBatchAsync(int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        var conn = GetConnection();
        var tx = await conn.BeginTransactionAsync().ConfigureAwait(false);
        var batch = new NpgsqlBatch(conn, tx);

        var setConfigCmd = new NpgsqlBatchCommand(
            "SELECT set_config('app.tenant_id', @_cfg_tid, true), set_config('app.user_id', @_cfg_uid, true)");
        setConfigCmd.Parameters.AddWithValue("_cfg_tid", tenantId.ToString());
        setConfigCmd.Parameters.AddWithValue("_cfg_uid", userId.ToString());
        batch.BatchCommands.Add(setConfigCmd);

        if (sw is not null)
        {
            sw.Stop();
            Logger.Debug(tenantId, userId, () =>
                $"[DB.GetBatchAsync] 批处理创建完成，耗时={sw.ElapsedMilliseconds}ms");
        }

        return batch;
    }

    /// <summary>执行批处理并提交事务</summary>
    public static async ValueTask<DbUdqResult> BatchCommitAsync(NpgsqlBatch batch, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlConnection? conn = batch.Connection;

        try
        {
            int rowsAffected = await batch.ExecuteNonQueryAsync().ConfigureAwait(false);

            if (batch.Connection?.State == ConnectionState.Open)
            {
                var tx = batch.Transaction;
                if (tx is not null)
                    await tx.CommitAsync().ConfigureAwait(false);
            }

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = $"[DB.BatchCommitAsync] 批处理提交完成，影响行数={rowsAffected}，耗时={sw.ElapsedMilliseconds}ms";
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbUdqResult { Success = true, RowsAffected = rowsAffected, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.BatchCommitAsync] 批处理提交异常: {ex.Message}");

            try
            {
                var tx = batch.Transaction;
                if (tx is not null)
                    await tx.RollbackAsync().ConfigureAwait(false);
            }
            catch (Exception rbEx)
            {
                Logger.Error(tenantId, userId, $"[DB.BatchCommitAsync] 回滚异常: {rbEx.Message}");
            }

            return new DbUdqResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    #endregion

    #region CRUD - Insert

    /// <summary>执行插入操作（独立事务），返回插入记录ID</summary>
    public static async ValueTask<DbInsResult> InsertAsync(
        string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlBatch? batch = null;
        NpgsqlConnection? conn = null;

        try
        {
            batch = await GetBatchAsync(tenantId, userId).ConfigureAwait(false);
            conn = batch.Connection;

            var cmd = new NpgsqlBatchCommand(sql);
            AddParameters(cmd, parameters, tenantId, userId, "InsertAsync");
            batch.BatchCommands.Add(cmd);

            await using var reader = await batch.ExecuteReaderAsync().ConfigureAwait(false);
            long id = 0;
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                // set_config 结果行，跳过
            }
            if (await reader.NextResultAsync().ConfigureAwait(false) &&
                await reader.ReadAsync().ConfigureAwait(false))
            {
                id = reader.GetInt64(0);
            }

            await reader.CloseAsync().ConfigureAwait(false);

            var tx = batch.Transaction;
            if (tx is not null)
                await tx.CommitAsync().ConfigureAwait(false);

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbInsResult { Success = true, Id = id, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.InsertAsync] 插入异常: {ex.Message}");

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, $"[DB.InsertAsync] 回滚异常: {rbEx.Message}");
                }
            }

            return new DbInsResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>在已有批处理中添加插入命令（不提交）</summary>
    public static ValueTask<DbInsResult> InsertTxAsync(
        NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        var cmd = new NpgsqlBatchCommand(sql);
        AddParameters(cmd, parameters, tenantId, userId, "InsertTxAsync");
        batch.BatchCommands.Add(cmd);

        string? debugMsg = null;
        if (sw is not null)
        {
            sw.Stop();
            debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
            Logger.Debug(tenantId, userId, () => debugMsg);
        }

        return new ValueTask<DbInsResult>(
            new DbInsResult { Success = true, Id = 0, DebugMessage = debugMsg });
    }

    #endregion

    #region CRUD - Update

    /// <summary>执行更新操作（独立事务）</summary>
    public static async ValueTask<DbUdqResult> UpdateAsync(
        string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlBatch? batch = null;
        NpgsqlConnection? conn = null;

        try
        {
            batch = await GetBatchAsync(tenantId, userId).ConfigureAwait(false);
            conn = batch.Connection;

            var cmd = new NpgsqlBatchCommand(sql);
            AddParameters(cmd, parameters, tenantId, userId, "UpdateAsync");
            batch.BatchCommands.Add(cmd);

            int rowsAffected = await batch.ExecuteNonQueryAsync().ConfigureAwait(false);

            var tx = batch.Transaction;
            if (tx is not null)
                await tx.CommitAsync().ConfigureAwait(false);

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbUdqResult { Success = true, RowsAffected = rowsAffected, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.UpdateAsync] 更新异常: {ex.Message}");

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, $"[DB.UpdateAsync] 回滚异常: {rbEx.Message}");
                }
            }

            return new DbUdqResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>在已有批处理中添加更新命令（不提交）</summary>
    public static ValueTask UpdateTxAsync(
        NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        var cmd = new NpgsqlBatchCommand(sql);
        AddParameters(cmd, parameters, tenantId, userId, "UpdateTxAsync");
        batch.BatchCommands.Add(cmd);

        if (sw is not null)
        {
            sw.Stop();
            Logger.Debug(tenantId, userId, () =>
                BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds));
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region CRUD - Delete

    /// <summary>执行删除操作（独立事务）</summary>
    public static async ValueTask<DbUdqResult> DeleteAsync(
        string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlBatch? batch = null;
        NpgsqlConnection? conn = null;

        try
        {
            batch = await GetBatchAsync(tenantId, userId).ConfigureAwait(false);
            conn = batch.Connection;

            var cmd = new NpgsqlBatchCommand(sql);
            AddParameters(cmd, parameters, tenantId, userId, "DeleteAsync");
            batch.BatchCommands.Add(cmd);

            int rowsAffected = await batch.ExecuteNonQueryAsync().ConfigureAwait(false);

            var tx = batch.Transaction;
            if (tx is not null)
                await tx.CommitAsync().ConfigureAwait(false);

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbUdqResult { Success = true, RowsAffected = rowsAffected, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.DeleteAsync] 删除异常: {ex.Message}");

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, $"[DB.DeleteAsync] 回滚异常: {rbEx.Message}");
                }
            }

            return new DbUdqResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>在已有批处理中添加删除命令（不提交）</summary>
    public static ValueTask DeleteTxAsync(
        NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        var cmd = new NpgsqlBatchCommand(sql);
        AddParameters(cmd, parameters, tenantId, userId, "DeleteTxAsync");
        batch.BatchCommands.Add(cmd);

        if (sw is not null)
        {
            sw.Stop();
            Logger.Debug(tenantId, userId, () =>
                BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds));
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region CRUD - Query

    /// <summary>执行查询操作，返回映射列表</summary>
    public static async ValueTask<(DbUdqResult Result, List<T>? Data)> GetListAsync<T>(
        string sql, PgSqlParam[] parameters, Func<NpgsqlDataReader, T> mapper,
        int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlConnection? conn = null;

        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, tenantId, userId, "GetListAsync");

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var list = new List<T>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return (new DbUdqResult { Success = true, RowsAffected = list.Count, DebugMessage = debugMsg }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.GetListAsync] 查询异常: {ex.Message}");
            return (new DbUdqResult { Success = false, ErrorMessage = ex.Message }, null);
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>在已有批处理中执行查询操作，返回映射列表</summary>
    public static async ValueTask<(DbUdqResult Result, List<T>? Data)> GetListTxAsync<T>(
        NpgsqlBatch batch, string sql, PgSqlParam[] parameters,
        Func<NpgsqlDataReader, T> mapper, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        try
        {
            var cmd = new NpgsqlBatchCommand(sql);
            AddParameters(cmd, parameters, tenantId, userId, "GetListTxAsync");
            batch.BatchCommands.Add(cmd);

            await using var reader = await batch.ExecuteReaderAsync().ConfigureAwait(false);

            // 跳过 set_config 结果集
            int cmdIndex = batch.BatchCommands.Count;
            for (int i = 0; i < cmdIndex - 1; i++)
            {
                await reader.NextResultAsync().ConfigureAwait(false);
            }

            var list = new List<T>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return (new DbUdqResult { Success = true, RowsAffected = list.Count, DebugMessage = debugMsg }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.GetListTxAsync] 查询异常: {ex.Message}");
            return (new DbUdqResult { Success = false, ErrorMessage = ex.Message }, null);
        }
    }

    /// <summary>执行查询操作，零DTO直接写入JSON</summary>
    public static async ValueTask<DbUdqResult> GetListAsync(
        string sql, PgSqlParam[] parameters,
        System.Text.Json.Utf8JsonWriter writer, Action<System.Text.Json.Utf8JsonWriter, NpgsqlDataReader> writerMap,
        int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlConnection? conn = null;

        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, tenantId, userId, "GetListAsync(JSON)");

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            int rowCount = 0;
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                writerMap(writer, reader);
                rowCount++;
            }

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbUdqResult { Success = true, RowsAffected = rowCount, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.GetListAsync(JSON)] 查询异常: {ex.Message}");
            return new DbUdqResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    #endregion

    #region CRUD - Scalar

    /// <summary>执行标量查询（独立连接）</summary>
    public static async ValueTask<DbScalarResult<T>> GetScalarAsync<T>(
        string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;
        NpgsqlConnection? conn = null;

        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, tenantId, userId, "GetScalarAsync");

            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

            T? value = default;
            if (result is not null && result is not DBNull)
                value = (T)result;

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbScalarResult<T> { Success = true, Value = value, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.GetScalarAsync] 标量查询异常: {ex.Message}");
            return new DbScalarResult<T> { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>在已有批处理中执行标量查询</summary>
    public static async ValueTask<DbScalarResult<T>> GetScalarTxAsync<T>(
        NpgsqlBatch batch, string sql, PgSqlParam[] parameters, int tenantId, long userId)
    {
        Stopwatch? sw = Logger.IsTenantDebugEnabled(tenantId) ? Stopwatch.StartNew() : null;

        try
        {
            var cmd = new NpgsqlBatchCommand(sql);
            AddParameters(cmd, parameters, tenantId, userId, "GetScalarTxAsync");
            batch.BatchCommands.Add(cmd);

            await using var reader = await batch.ExecuteReaderAsync().ConfigureAwait(false);

            // 跳过前面的结果集
            int cmdIndex = batch.BatchCommands.Count;
            for (int i = 0; i < cmdIndex - 1; i++)
            {
                await reader.NextResultAsync().ConfigureAwait(false);
            }

            T? value = default;
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                var result = reader.GetValue(0);
                if (result is not DBNull)
                    value = (T)result;
            }

            string? debugMsg = null;
            if (sw is not null)
            {
                sw.Stop();
                debugMsg = BuildDebugInfo(sql, parameters, tenantId, userId, sw.ElapsedMilliseconds);
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbScalarResult<T> { Success = true, Value = value, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, $"[DB.GetScalarTxAsync] 标量查询异常: {ex.Message}");
            return new DbScalarResult<T> { Success = false, ErrorMessage = ex.Message };
        }
    }

    #endregion

    #region DDL 操作

    /// <summary>查询表是否存在</summary>
    public static async ValueTask<DbUdqResult> GetTableInfor(string tableName)
    {
        const string sql = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @tableName";
        var parameters = new PgSqlParam[]
        {
            new PgSqlParam("tableName", tableName)
        };

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, 0, 0, "GetTableInfor");

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            int rowCount = 0;
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                rowCount++;
            }

            Logger.Info(0, 0, $"[DB.GetTableInfor] 查询表 {tableName} 完成，结果行数={rowCount}");

            return new DbUdqResult { Success = true, RowsAffected = rowCount };
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, $"[DB.GetTableInfor] 查询表 {tableName} 异常: {ex.Message}");
            return new DbUdqResult { Success = false, ErrorMessage = ex.Message };
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>查询表字段信息</summary>
    public static async ValueTask<(DbUdqResult Result, List<DbField>? Data)> GetFieldsInfor(
        string tableName, Func<NpgsqlDataReader, DbField> mapper)
    {
        const string sql =
            "SELECT column_name, data_type, character_maximum_length, numeric_precision, numeric_scale, is_nullable, " +
            "CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END AS is_primary_key " +
            "FROM information_schema.columns c " +
            "LEFT JOIN (" +
            "SELECT ku.column_name FROM information_schema.table_constraints tc " +
            "JOIN information_schema.key_column_usage ku ON tc.constraint_name = ku.constraint_name " +
            "WHERE tc.table_schema = 'public' AND tc.table_name = @tableName AND tc.constraint_type = 'PRIMARY KEY'" +
            ") pk ON c.column_name = pk.column_name " +
            "WHERE c.table_schema = 'public' AND c.table_name = @tableName " +
            "ORDER BY c.ordinal_position";

        var parameters = new PgSqlParam[]
        {
            new PgSqlParam("tableName", tableName)
        };

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, 0, 0, "GetFieldsInfor");

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var list = new List<DbField>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            Logger.Info(0, 0, $"[DB.GetFieldsInfor] 查询表 {tableName} 字段完成，字段数={list.Count}");

            return (new DbUdqResult { Success = true, RowsAffected = list.Count }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, $"[DB.GetFieldsInfor] 查询表 {tableName} 字段异常: {ex.Message}");
            return (new DbUdqResult { Success = false, ErrorMessage = ex.Message }, null);
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>查询表索引信息</summary>
    public static async ValueTask<(DbUdqResult Result, List<DbIndex>? Data)> GetIndexesInfor(
        string tableName, Func<NpgsqlDataReader, DbIndex> mapper)
    {
        const string sql =
            "SELECT indexname, tablename, indexdef FROM pg_indexes " +
            "WHERE schemaname = 'public' AND tablename = @tableName";

        var parameters = new PgSqlParam[]
        {
            new PgSqlParam("tableName", tableName)
        };

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            AddParameters(cmd, parameters, 0, 0, "GetIndexesInfor");

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var list = new List<DbIndex>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            Logger.Info(0, 0, $"[DB.GetIndexesInfor] 查询表 {tableName} 索引完成，索引数={list.Count}");

            return (new DbUdqResult { Success = true, RowsAffected = list.Count }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, $"[DB.GetIndexesInfor] 查询表 {tableName} 索引异常: {ex.Message}");
            return (new DbUdqResult { Success = false, ErrorMessage = ex.Message }, null);
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>创建表（若不存在）</summary>
    public static async ValueTask<DDLStatus> CreateTable(string tableName, string sql)
    {
        var tableResult = await GetTableInfor(tableName).ConfigureAwait(false);
        if (tableResult.Success && tableResult.RowsAffected > 0)
        {
            Logger.Info(0, 0, $"[DB.CreateTable] 表 {tableName} 已存在");
            return DDLStatus.Existed;
        }

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            Logger.Info(0, 0, $"[DB.CreateTable] 表 {tableName} 创建成功");
            return DDLStatus.Success;
        }
        catch (Exception ex)
        {
            Logger.Fatal(0, 0, $"[DB.CreateTable] 创建表 {tableName} 失败: {ex.Message}");
            Environment.FailFast($"[DB.CreateTable] 创建表 {tableName} 失败: {ex.Message}");
            return DDLStatus.Failed;
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>修改表结构（添加或修改字段）</summary>
    public static async ValueTask<DDLStatus> AlterTable(
        string tableName, string fieldName, string dataType, string length, string precision,
        bool nullable, bool isPrimaryKey)
    {
        var fieldsResult = await GetFieldsInfor(tableName, reader => new DbField
        {
            FieldName = reader.GetString(0),
            DataType = reader.GetString(1),
            MaxLength = reader.IsDBNull(2) ? null : reader.GetInt32(2),
            NumericPrecision = reader.IsDBNull(3) ? null : reader.GetInt32(3),
            NumericScale = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            IsNullable = reader.GetString(5) == "YES",
            IsPrimaryKey = reader.GetBoolean(6)
        }).ConfigureAwait(false);

        DbField? existingField = null;
        if (fieldsResult.Data is not null)
        {
            for (int i = 0; i < fieldsResult.Data.Count; i++)
            {
                if (string.Equals(fieldsResult.Data[i].FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    existingField = fieldsResult.Data[i];
                    break;
                }
            }
        }

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();

            if (existingField is null)
            {
                // 添加字段
                var typeSpec = BuildTypeSpec(dataType, length, precision);
                var nullSpec = nullable ? "NULL" : "NOT NULL";
                var sql = $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{fieldName}\" {typeSpec} {nullSpec}";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                Logger.Info(0, 0, $"[DB.AlterTable] 表 {tableName} 添加字段 {fieldName} 成功");
                return DDLStatus.Success;
            }
            else
            {
                // 修改字段 - 仅扩展长度
                bool changed = false;
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(length))
                {
                    if (int.TryParse(length, out int newLength) &&
                        (existingField.MaxLength is null || newLength > existingField.MaxLength.Value))
                    {
                        var typeSpec = BuildTypeSpec(dataType, length, precision);
                        sb.Append($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{fieldName}\" TYPE {typeSpec}");
                        changed = true;
                    }
                }

                if (nullable != existingField.IsNullable)
                {
                    if (changed) sb.Append("; ");
                    if (nullable)
                        sb.Append($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{fieldName}\" DROP NOT NULL");
                    else
                        sb.Append($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{fieldName}\" SET NOT NULL");
                    changed = true;
                }

                if (!changed)
                {
                    Logger.Info(0, 0, $"[DB.AlterTable] 表 {tableName} 字段 {fieldName} 无需修改");
                    return DDLStatus.Existed;
                }

                await using var cmd = new NpgsqlCommand(sb.ToString(), conn);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                Logger.Info(0, 0, $"[DB.AlterTable] 表 {tableName} 修改字段 {fieldName} 成功");
                return DDLStatus.Success;
            }
        }
        catch (Exception ex)
        {
            Logger.Fatal(0, 0, $"[DB.AlterTable] 修改表 {tableName} 字段 {fieldName} 失败: {ex.Message}");
            Environment.FailFast($"[DB.AlterTable] 修改表 {tableName} 字段 {fieldName} 失败: {ex.Message}");
            return DDLStatus.Failed;
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    /// <summary>创建索引（若不存在）</summary>
    public static async ValueTask<DDLStatus> CreateIndex(
        string indexName, string tableName, string fieldNames, bool unique)
    {
        var indexesResult = await GetIndexesInfor(tableName, reader => new DbIndex
        {
            IndexName = reader.GetString(0),
            TableName = reader.GetString(1)
        }).ConfigureAwait(false);

        if (indexesResult.Data is not null)
        {
            for (int i = 0; i < indexesResult.Data.Count; i++)
            {
                if (string.Equals(indexesResult.Data[i].IndexName, indexName, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.Info(0, 0, $"[DB.CreateIndex] 索引 {indexName} 已存在");
                    return DDLStatus.Existed;
                }
            }
        }

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            var uniqueStr = unique ? "UNIQUE " : "";
            var sql = $"CREATE {uniqueStr}INDEX \"{indexName}\" ON \"{tableName}\" ({fieldNames})";

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            Logger.Info(0, 0, $"[DB.CreateIndex] 索引 {indexName} 创建成功");
            return DDLStatus.Success;
        }
        catch (Exception ex)
        {
            Logger.Fatal(0, 0, $"[DB.CreateIndex] 创建索引 {indexName} 失败: {ex.Message}");
            Environment.FailFast($"[DB.CreateIndex] 创建索引 {indexName} 失败: {ex.Message}");
            return DDLStatus.Failed;
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
        }
    }

    #endregion

    #region 私有辅助方法

    /// <summary>为 NpgsqlBatchCommand 添加参数</summary>
    private static void AddParameters(NpgsqlBatchCommand cmd, PgSqlParam[] parameters, int tenantId, long userId, string methodName)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            Logger.Debug(tenantId, userId, () =>
                $"[{methodName}] 参数[{i}]: Name={p.Name}, Value={p.Value}, DbType={p.DbType}");
            if (p.DbType.HasValue)
                cmd.Parameters.AddWithValue(p.Name, p.DbType.Value, p.Value ?? DBNull.Value);
            else
                cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        }
    }

    /// <summary>为 NpgsqlCommand 添加参数</summary>
    private static void AddParameters(NpgsqlCommand cmd, PgSqlParam[] parameters, int tenantId, long userId, string methodName)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            Logger.Debug(tenantId, userId, () =>
                $"[{methodName}] 参数[{i}]: Name={p.Name}, Value={p.Value}, DbType={p.DbType}");
            if (p.DbType.HasValue)
                cmd.Parameters.AddWithValue(p.Name, p.DbType.Value, p.Value ?? DBNull.Value);
            else
                cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        }
    }

    /// <summary>格式化参数值用于调试SQL</summary>
    private static string FormatParamValue(object? value)
    {
        if (value is null || value is DBNull)
            return "NULL";
        if (value is string s)
            return $"'{s}'";
        if (value is DateTime dt)
            return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
        if (value is bool b)
            return b ? "true" : "false";
        if (value is byte[] bytes)
            return $"'\\x{BitConverter.ToString(bytes).Replace("-", "")}'";
        return value.ToString() ?? "NULL";
    }

    /// <summary>构建可执行的调试SQL（参数替换）</summary>
    private static string BuildDebugInfo(string sql, PgSqlParam[] parameters, int tenantId, long userId, long elapsedMs)
    {
        var sb = new StringBuilder(sql);
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var paramName = p.Name.StartsWith("@") ? p.Name : "@" + p.Name;
            sb.Replace(paramName, FormatParamValue(p.Value));
        }

        sb.Append($" -- tenantId={tenantId}, userId={userId}, elapsed={elapsedMs}ms");
        return sb.ToString();
    }

    /// <summary>构建字段类型规范</summary>
    private static string BuildTypeSpec(string dataType, string length, string precision)
    {
        if (!string.IsNullOrEmpty(precision))
            return $"{dataType}({length},{precision})";
        if (!string.IsNullOrEmpty(length))
            return $"{dataType}({length})";
        return dataType;
    }

    #endregion
}
