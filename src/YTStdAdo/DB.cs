using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
    private const string IdTableName = "ids";
    private const long LongIdRuntimeMultiplier = 10_000_000_000L;
    private const long LongIdMachineMultiplier = 10_000_000_000_000_000L;
    private const long LongIdBusinessMax = 9_999_999_999L;

    private static readonly ConcurrentQueue<NpgsqlConnection> _pool = new();
    private static readonly object _idInitSync = new();
    private static string _connectionString = "";
    private static int _maxPoolSize;
    private static int _retryCount;
    private static int _poolCount;
    private static int _machineCode;
    private static int _startupRuntime;
    private static int _idStoreInitialized;
    private static long _nextLongId;
    private static long _nextLongIdLimit;

    #region 连接池管理

    /// <summary>初始化连接池，应用启动时调用一次</summary>
    public static void Init(DbOptions options)
    {
        _connectionString = options.BuildConnectionString();
        _maxPoolSize = options.MaxPoolSize;
        _retryCount = options.RetryCount;
        _poolCount = 0;
        _machineCode = ValidateMachineCode(options.MachineCode);
        _startupRuntime = 0;
        _idStoreInitialized = 0;
        _nextLongId = 0;
        _nextLongIdLimit = 0;

        for (int i = 0; i < options.MinPoolSize; i++)
        {
            var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            _pool.Enqueue(conn);
            System.Threading.Interlocked.Increment(ref _poolCount);
        }

        Logger.Info(0, 0, () =>
        {
            var vsb = new ValueStringBuilder(128);
            vsb.Append("[DB.Init] 连接池初始化完成，最小连接数=");
            vsb.Append(options.MinPoolSize);
            vsb.Append("，最大连接数=");
            vsb.Append(options.MaxPoolSize);
            return vsb.ToString();
        });

        EnsureIdStoreInitialized();
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
                Logger.Error(0, 0, () =>
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[DB.ShutdownAsync] 关闭连接异常: ");
                    vsb.Append(ex.Message);
                    return vsb.ToString();
                });
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
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[DB.GetConnection] 无法创建数据库连接，已重试 ");
                    vsb.Append(retries);
                    vsb.Append(" 次: ");
                    vsb.Append(ex.Message);
                    throw new InvalidOperationException(vsb.ToString(), ex);
                }

                Logger.Warn(0, 0, () =>
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[DB.GetConnection] 创建连接失败，第 ");
                    vsb.Append(attempt + 1);
                    vsb.Append(" 次重试: ");
                    vsb.Append(ex.Message);
                    return vsb.ToString();
                });
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

    #region ID 生成

    /// <summary>获取下一个 long 主键值。每调用一次都会递增一次。</summary>
    public static ValueTask<long> GetNextLongIdAsync()
    {
        EnsureIdStoreInitialized();

        long nextId = System.Threading.Interlocked.Increment(ref _nextLongId) - 1;
        long limit = System.Threading.Volatile.Read(ref _nextLongIdLimit);
        if (nextId >= limit)
        {
            throw new OverflowException("[DB.GetNextLongIdAsync] long 主键业务段已耗尽，请增加机器码或重置运行实例。");
        }

        return new ValueTask<long>(nextId);
    }

    /// <summary>获取下一个 int 主键值。每调用一次都会递增一次。</summary>
    public static async ValueTask<int> GetNextIntIdAsync()
    {
        EnsureIdStoreInitialized();

        const string sql = "UPDATE \"ids\" SET \"intid\" = \"intid\" + 1 RETURNING \"intid\" - 1";
        NpgsqlConnection? conn = null;

        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            if (result is null || result is DBNull)
            {
                throw new InvalidOperationException("[DB.GetNextIntIdAsync] 未能从 ids 表获取 int 主键值。");
            }

            int nextId = Convert.ToInt32(result);
            if (nextId < 0)
            {
                throw new OverflowException("[DB.GetNextIntIdAsync] int 主键已超出有效范围。");
            }

            return nextId;
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, () => BuildExceptionLog("DB.GetNextIntIdAsync", ex, sql));
            throw;
        }
        finally
        {
            if (conn is not null)
                ReturnConnection(conn);
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
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetBatchAsync] 批处理创建完成，耗时=");
                vsb.Append(sw.ElapsedMilliseconds);
                vsb.Append("ms");
                return vsb.ToString();
            });
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
                var vsb = new ValueStringBuilder(stackalloc char[128]);
                vsb.Append("[DB.BatchCommitAsync] 批处理提交完成，影响行数=");
                vsb.Append(rowsAffected);
                vsb.Append("，耗时=");
                vsb.Append(sw.ElapsedMilliseconds);
                vsb.Append("ms");
                debugMsg = vsb.ToString();
                Logger.Debug(tenantId, userId, () => debugMsg);
            }

            return new DbUdqResult { Success = true, RowsAffected = rowsAffected, DebugMessage = debugMsg };
        }
        catch (Exception ex)
        {
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.BatchCommitAsync] 批处理提交异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });

            try
            {
                var tx = batch.Transaction;
                if (tx is not null)
                    await tx.RollbackAsync().ConfigureAwait(false);
            }
            catch (Exception rbEx)
            {
                Logger.Error(tenantId, userId, () =>
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[DB.BatchCommitAsync] 回滚异常: ");
                    vsb.Append(rbEx.Message);
                    return vsb.ToString();
                });
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
            Logger.Error(tenantId, userId, () => BuildExceptionLog("DB.InsertAsync", ex, sql, parameters));

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, () => BuildExceptionLog("DB.InsertAsync", rbEx));
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.UpdateAsync] 更新异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, () =>
                    {
                        var vsb = new ValueStringBuilder(128);
                        vsb.Append("[DB.UpdateAsync] 回滚异常: ");
                        vsb.Append(rbEx.Message);
                        return vsb.ToString();
                    });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.DeleteAsync] 删除异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });

            if (batch?.Transaction is not null)
            {
                try { await batch.Transaction.RollbackAsync().ConfigureAwait(false); }
                catch (Exception rbEx)
                {
                    Logger.Error(tenantId, userId, () =>
                    {
                        var vsb = new ValueStringBuilder(128);
                        vsb.Append("[DB.DeleteAsync] 回滚异常: ");
                        vsb.Append(rbEx.Message);
                        return vsb.ToString();
                    });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetListAsync] 查询异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetListTxAsync] 查询异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetListAsync(JSON)] 查询异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetScalarAsync] 标量查询异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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
            Logger.Error(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetScalarTxAsync] 标量查询异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
            return new DbScalarResult<T> { Success = false, ErrorMessage = ex.Message };
        }
    }

    #endregion

    #region DDL 操作

    /// <summary>查询表是否存在</summary>
    public static async ValueTask<DbUdqResult> GetTableInfor(string tableName)
    {
        const string sql = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @tableName";

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            // 直接构建 NpgsqlParameter，省去 PgSqlParam 中间数组分配
            cmd.Parameters.AddWithValue("tableName", tableName);
            Logger.Debug(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetTableInfor] 参数[0]: Name=tableName, Value=");
                vsb.Append(tableName);
                return vsb.ToString();
            });

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            int rowCount = 0;
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                rowCount++;
            }

            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetTableInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 完成，结果行数=");
                vsb.Append(rowCount);
                return vsb.ToString();
            });

            return new DbUdqResult { Success = true, RowsAffected = rowCount };
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetTableInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("tableName", tableName);
            Logger.Debug(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetFieldsInfor] 参数[0]: Name=tableName, Value=");
                vsb.Append(tableName);
                return vsb.ToString();
            });

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var list = new List<DbField>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetFieldsInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 字段完成，字段数=");
                vsb.Append(list.Count);
                return vsb.ToString();
            });

            return (new DbUdqResult { Success = true, RowsAffected = list.Count }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetFieldsInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 字段异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("tableName", tableName);
            Logger.Debug(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetIndexesInfor] 参数[0]: Name=tableName, Value=");
                vsb.Append(tableName);
                return vsb.ToString();
            });

            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            var list = new List<DbIndex>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                list.Add(mapper(reader));
            }

            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.GetIndexesInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 索引完成，索引数=");
                vsb.Append(list.Count);
                return vsb.ToString();
            });

            return (new DbUdqResult { Success = true, RowsAffected = list.Count }, list);
        }
        catch (Exception ex)
        {
            Logger.Error(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append("[DB.GetIndexesInfor] 查询表 ");
                vsb.Append(tableName);
                vsb.Append(" 索引异常: ");
                vsb.Append(ex.Message);
                return vsb.ToString();
            });
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
            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.CreateTable] 表 ");
                vsb.Append(tableName);
                vsb.Append(" 已存在");
                return vsb.ToString();
            });
            return DDLStatus.Existed;
        }

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.CreateTable] 表 ");
                vsb.Append(tableName);
                vsb.Append(" 创建成功");
                return vsb.ToString();
            });
            return DDLStatus.Success;
        }
        catch (Exception ex)
        {
            var vsb = new ValueStringBuilder(256);
            vsb.Append("[DB.CreateTable] 创建表 ");
            vsb.Append(tableName);
            vsb.Append(" 失败: ");
            vsb.Append(ex.Message);
            string errMsg = vsb.ToString();
            Logger.Fatal(0, 0, errMsg);
            Environment.FailFast(errMsg);
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
                var vsbAdd = new ValueStringBuilder(stackalloc char[256]);
                vsbAdd.Append("ALTER TABLE \"");
                vsbAdd.Append(tableName);
                vsbAdd.Append("\" ADD COLUMN \"");
                vsbAdd.Append(fieldName);
                vsbAdd.Append("\" ");
                vsbAdd.Append(typeSpec);
                vsbAdd.Append(' ');
                vsbAdd.Append(nullSpec);
                string sql = vsbAdd.ToString();

                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                Logger.Info(0, 0, () =>
                {
                    var vsb = new ValueStringBuilder(64);
                    vsb.Append("[DB.AlterTable] 表 ");
                    vsb.Append(tableName);
                    vsb.Append(" 添加字段 ");
                    vsb.Append(fieldName);
                    vsb.Append(" 成功");
                    return vsb.ToString();
                });
                return DDLStatus.Success;
            }
            else
            {
                // 修改字段 - 仅扩展长度
                bool changed = false;
                var vsbSql = new ValueStringBuilder(stackalloc char[256]);

                if (!string.IsNullOrEmpty(length))
                {
                    if (int.TryParse(length, out int newLength) &&
                        (existingField.MaxLength is null || newLength > existingField.MaxLength.Value))
                    {
                        var typeSpec = BuildTypeSpec(dataType, length, precision);
                        vsbSql.Append("ALTER TABLE \"");
                        vsbSql.Append(tableName);
                        vsbSql.Append("\" ALTER COLUMN \"");
                        vsbSql.Append(fieldName);
                        vsbSql.Append("\" TYPE ");
                        vsbSql.Append(typeSpec);
                        changed = true;
                    }
                }

                if (nullable != existingField.IsNullable)
                {
                    if (changed) vsbSql.Append("; ");
                    vsbSql.Append("ALTER TABLE \"");
                    vsbSql.Append(tableName);
                    vsbSql.Append("\" ALTER COLUMN \"");
                    vsbSql.Append(fieldName);
                    if (nullable)
                        vsbSql.Append("\" DROP NOT NULL");
                    else
                        vsbSql.Append("\" SET NOT NULL");
                    changed = true;
                }

                if (!changed)
                {
                    vsbSql.Dispose();
                    Logger.Info(0, 0, () =>
                    {
                        var vsb = new ValueStringBuilder(64);
                        vsb.Append("[DB.AlterTable] 表 ");
                        vsb.Append(tableName);
                        vsb.Append(" 字段 ");
                        vsb.Append(fieldName);
                        vsb.Append(" 无需修改");
                        return vsb.ToString();
                    });
                    return DDLStatus.Existed;
                }

                string alterSql = vsbSql.ToString();
                await using var cmd = new NpgsqlCommand(alterSql, conn);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                Logger.Info(0, 0, () =>
                {
                    var vsb = new ValueStringBuilder(64);
                    vsb.Append("[DB.AlterTable] 表 ");
                    vsb.Append(tableName);
                    vsb.Append(" 修改字段 ");
                    vsb.Append(fieldName);
                    vsb.Append(" 成功");
                    return vsb.ToString();
                });
                return DDLStatus.Success;
            }
        }
        catch (Exception ex)
        {
            var vsbErr = new ValueStringBuilder(256);
            vsbErr.Append("[DB.AlterTable] 修改表 ");
            vsbErr.Append(tableName);
            vsbErr.Append(" 字段 ");
            vsbErr.Append(fieldName);
            vsbErr.Append(" 失败: ");
            vsbErr.Append(ex.Message);
            string errMsg = vsbErr.ToString();
            Logger.Fatal(0, 0, errMsg);
            Environment.FailFast(errMsg);
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
                    Logger.Info(0, 0, () =>
                    {
                        var vsb = new ValueStringBuilder(64);
                        vsb.Append("[DB.CreateIndex] 索引 ");
                        vsb.Append(indexName);
                        vsb.Append(" 已存在");
                        return vsb.ToString();
                    });
                    return DDLStatus.Existed;
                }
            }
        }

        NpgsqlConnection? conn = null;
        try
        {
            conn = GetConnection();
            var vsbIdx = new ValueStringBuilder(stackalloc char[256]);
            vsbIdx.Append(unique ? "CREATE UNIQUE INDEX \"" : "CREATE INDEX \"");
            vsbIdx.Append(indexName);
            vsbIdx.Append("\" ON \"");
            vsbIdx.Append(tableName);
            vsbIdx.Append("\" (");
            vsbIdx.Append(fieldNames);
            vsbIdx.Append(')');
            string sql = vsbIdx.ToString();

            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            Logger.Info(0, 0, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[DB.CreateIndex] 索引 ");
                vsb.Append(indexName);
                vsb.Append(" 创建成功");
                return vsb.ToString();
            });
            return DDLStatus.Success;
        }
        catch (Exception ex)
        {
            var vsbErr = new ValueStringBuilder(256);
            vsbErr.Append("[DB.CreateIndex] 创建索引 ");
            vsbErr.Append(indexName);
            vsbErr.Append(" 失败: ");
            vsbErr.Append(ex.Message);
            string errMsg = vsbErr.ToString();
            Logger.Fatal(0, 0, errMsg);
            Environment.FailFast(errMsg);
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
            // 捕获局部变量以避免闭包捕获循环变量
            int idx = i;
            Logger.Debug(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append('[');
                vsb.Append(methodName);
                vsb.Append("] 参数[");
                vsb.Append(idx);
                vsb.Append("]: Name=");
                vsb.Append(p.Name);
                vsb.Append(", Value=");
                vsb.Append(p.Value?.ToString());
                vsb.Append(", DbType=");
                vsb.Append(p.DbType?.ToString());
                return vsb.ToString();
            });
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
            int idx = i;
            Logger.Debug(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(128);
                vsb.Append('[');
                vsb.Append(methodName);
                vsb.Append("] 参数[");
                vsb.Append(idx);
                vsb.Append("]: Name=");
                vsb.Append(p.Name);
                vsb.Append(", Value=");
                vsb.Append(p.Value?.ToString());
                vsb.Append(", DbType=");
                vsb.Append(p.DbType?.ToString());
                return vsb.ToString();
            });
            if (p.DbType.HasValue)
                cmd.Parameters.AddWithValue(p.Name, p.DbType.Value, p.Value ?? DBNull.Value);
            else
                cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        }
    }

    /// <summary>格式化参数值用于调试SQL</summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static string FormatParamValue(object? value)
    {
        if (value is null || value is DBNull)
            return "NULL";
        if (value is string s)
        {
            var vsb = new ValueStringBuilder(stackalloc char[64]);
            vsb.Append('\'');
            vsb.Append(s);
            vsb.Append('\'');
            return vsb.ToString();
        }
        if (value is DateTime dt)
        {
            var vsb = new ValueStringBuilder(stackalloc char[32]);
            vsb.Append('\'');
            vsb.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            vsb.Append('\'');
            return vsb.ToString();
        }
        if (value is bool b)
            return b ? "true" : "false";
        if (value is byte[] bytes)
        {
            var hex = BitConverter.ToString(bytes).Replace("-", "");
            var vsb = new ValueStringBuilder(stackalloc char[64]);
            vsb.Append("'\\x");
            vsb.Append(hex);
            vsb.Append('\'');
            return vsb.ToString();
        }
        return value.ToString() ?? "NULL";
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static string BuildExceptionLog(string methodName, Exception ex, string? sql = null, PgSqlParam[]? parameters = null)
    {
        var vsb = new ValueStringBuilder(1024);
        vsb.Append('[');
        vsb.Append(methodName);
        vsb.Append("] 异常: ");
        vsb.Append(ex.ToString());

        if (!string.IsNullOrEmpty(sql))
        {
            vsb.Append("\nSQL: ");
            vsb.Append(sql);
        }

        if (parameters is { Length: > 0 })
        {
            vsb.Append("\n参数: ");
            AppendParameterSnapshot(ref vsb, parameters);
        }

        return vsb.ToString();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void AppendParameterSnapshot(ref ValueStringBuilder vsb, PgSqlParam[] parameters)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
                vsb.Append("; ");

            var p = parameters[i];
            vsb.Append(p.Name);
            vsb.Append('=');
            vsb.Append(FormatParamValue(p.Value));

            if (p.DbType.HasValue)
            {
                vsb.Append(" (");
                vsb.Append(p.DbType.Value.ToString());
                vsb.Append(')');
            }
        }
    }

    /// <summary>构建可执行的调试SQL（参数替换）</summary>
    private static string BuildDebugInfo(string sql, PgSqlParam[] parameters, int tenantId, long userId, long elapsedMs)
    {
        // 先完成参数替换（Replace 会创建新字符串，但无法避免）
        string result = sql;
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            string paramName;
            if (p.Name.StartsWith("@"))
            {
                paramName = p.Name;
            }
            else
            {
                var vsb2 = new ValueStringBuilder(64);
                vsb2.Append('@');
                vsb2.Append(p.Name);
                paramName = vsb2.ToString();
            }
            result = result.Replace(paramName, FormatParamValue(p.Value));
        }

        // 使用 ValueStringBuilder 拼接后缀
        var vsb = new ValueStringBuilder(stackalloc char[256]);
        vsb.Append(result);
        vsb.Append(" -- tenantId=");
        vsb.Append(tenantId);
        vsb.Append(", userId=");
        vsb.Append(userId);
        vsb.Append(", elapsed=");
        vsb.Append(elapsedMs);
        vsb.Append("ms");
        return vsb.ToString();
    }

    /// <summary>构建字段类型规范</summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static string BuildTypeSpec(string dataType, string length, string precision)
    {
        if (!string.IsNullOrEmpty(precision))
        {
            var vsb = new ValueStringBuilder(stackalloc char[32]);
            vsb.Append(dataType);
            vsb.Append('(');
            vsb.Append(length);
            vsb.Append(',');
            vsb.Append(precision);
            vsb.Append(')');
            return vsb.ToString();
        }
        if (!string.IsNullOrEmpty(length))
        {
            var vsb = new ValueStringBuilder(stackalloc char[32]);
            vsb.Append(dataType);
            vsb.Append('(');
            vsb.Append(length);
            vsb.Append(')');
            return vsb.ToString();
        }
        return dataType;
    }

    private static int ValidateMachineCode(int machineCode)
    {
        if ((uint)machineCode > 99U)
        {
            throw new ArgumentOutOfRangeException(nameof(machineCode), machineCode, "MachineCode 必须在 0-99 范围内。");
        }

        return machineCode;
    }

    private static void EnsureIdStoreInitialized()
    {
        if (System.Threading.Volatile.Read(ref _idStoreInitialized) != 0)
            return;

        lock (_idInitSync)
        {
            if (System.Threading.Volatile.Read(ref _idStoreInitialized) != 0)
                return;

            NpgsqlConnection? conn = null;
            NpgsqlTransaction? tx = null;

            try
            {
                conn = GetConnection();
                tx = conn.BeginTransaction();

                const string createSql =
                    "CREATE TABLE IF NOT EXISTS \"ids\" (" +
                    "\"mcode\" integer NOT NULL, " +
                    "\"runtimes\" integer NOT NULL, " +
                    "\"intid\" integer NOT NULL" +
                    ");";
                using (var createCmd = new NpgsqlCommand(createSql, conn, tx))
                {
                    createCmd.ExecuteNonQuery();
                }

                using (var lockCmd = new NpgsqlCommand($"LOCK TABLE \"{IdTableName}\" IN EXCLUSIVE MODE", conn, tx))
                {
                    lockCmd.ExecuteNonQuery();
                }

                int rowCount;
                using (var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{IdTableName}\"", conn, tx))
                {
                    rowCount = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                if (rowCount == 0)
                {
                    using var insertCmd = new NpgsqlCommand(
                        $"INSERT INTO \"{IdTableName}\" (\"mcode\", \"runtimes\", \"intid\") VALUES (@mcode, 0, 1)", conn, tx);
                    insertCmd.Parameters.AddWithValue("mcode", _machineCode);
                    insertCmd.ExecuteNonQuery();
                }
                else if (rowCount > 1)
                {
                    throw new InvalidOperationException("[DB.EnsureIdStoreInitialized] ids 表存在多条记录，无法保证主键唯一性。");
                }

                int runtime;
                using (var updateCmd = new NpgsqlCommand(
                    $"UPDATE \"{IdTableName}\" SET \"mcode\" = @mcode, \"runtimes\" = \"runtimes\" + 1 RETURNING \"runtimes\"", conn, tx))
                {
                    updateCmd.Parameters.AddWithValue("mcode", _machineCode);
                    runtime = Convert.ToInt32(updateCmd.ExecuteScalar());
                }

                tx.Commit();
                tx = null;

                _startupRuntime = runtime;
                long longIdBase = BuildLongIdBase(_machineCode, runtime);
                System.Threading.Interlocked.Exchange(ref _nextLongId, longIdBase);
                System.Threading.Interlocked.Exchange(ref _nextLongIdLimit, longIdBase + LongIdBusinessMax + 1);
                System.Threading.Volatile.Write(ref _idStoreInitialized, 1);

                Logger.Info(0, 0, () =>
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[DB.EnsureIdStoreInitialized] ids 表初始化完成，mcode=");
                    vsb.Append(_machineCode);
                    vsb.Append("，runtimes=");
                    vsb.Append(_startupRuntime);
                    vsb.Append("，nextLongId=");
                    vsb.Append(longIdBase);
                    return vsb.ToString();
                });
            }
            catch (Exception ex)
            {
                if (tx is not null)
                {
                    try { tx.Rollback(); }
                    catch { }
                }

                Logger.Fatal(0, 0, BuildExceptionLog("DB.EnsureIdStoreInitialized", ex));
                throw;
            }
            finally
            {
                tx?.Dispose();
                if (conn is not null)
                    ReturnConnection(conn);
            }
        }
    }

    private static long BuildLongIdBase(int machineCode, int runtime)
    {
        if ((uint)runtime > 999_999U)
        {
            throw new OverflowException("[DB.BuildLongIdBase] runtimes 超出 6 位预留范围。");
        }

        return ((long)machineCode * LongIdMachineMultiplier) + ((long)runtime * LongIdRuntimeMultiplier);
    }

    #endregion
}
