using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using YTStdLogger.Core;

namespace YTStdEntity.Tenant;

/// <summary>
/// 租户分离服务。
/// 将指定租户的数据从源库迁移到独立目标库。
/// <para>
/// 执行流程：
/// <list type="number">
/// <item>检查目标库表结构 → 创建缺失表</item>
/// <item>关闭目标库所有相关表的触发器</item>
/// <item>租户表：迁移 WHERE tenant_id IN (@tenantIds) 的数据</item>
/// <item>非租户表：全量迁移数据</item>
/// <item>不迁移 _Log 结尾的表</item>
/// <item>恢复目标库所有触发器</item>
/// </list>
/// </para>
/// </summary>
public sealed class TenantSeparationService
{
    private readonly TenantSeparationOptions _options;

    /// <summary>
    /// 初始化租户分离服务。
    /// </summary>
    /// <param name="options">租户分离配置</param>
    public TenantSeparationService(TenantSeparationOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// 执行租户数据迁移。
    /// </summary>
    /// <param name="sourceConnectionString">源库连接字符串</param>
    /// <param name="tenantId">操作租户ID（用于日志）</param>
    /// <param name="userId">操作用户ID（用于日志）</param>
    /// <returns>迁移的总记录数</returns>
    public async ValueTask<int> MigrateAsync(string sourceConnectionString, int tenantId, long userId)
    {
        Logger.Info(tenantId, userId, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[TenantSeparationService] 开始租户分离, 租户=");
            vsb.Append(string.Join(",", _options.TenantIds));
            return vsb.ToString();
        });

        int totalMigrated = 0;

        await using var srcConn = new NpgsqlConnection(sourceConnectionString);
        await srcConn.OpenAsync().ConfigureAwait(false);

        await using var targetConn = new NpgsqlConnection(_options.TargetConnectionString);
        await targetConn.OpenAsync().ConfigureAwait(false);

        // Step 1: 获取源库所有公共表
        var tables = new List<string>();
        await using (var cmd = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'",
            srcConn))
        {
            await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                tables.Add(reader.GetString(0));
            }
        }

        Logger.Debug(tenantId, userId, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[TenantSeparationService] 发现 ");
            vsb.Append(tables.Count);
            vsb.Append(" 张表");
            return vsb.ToString();
        });

        // 过滤掉 _Log 结尾的表
        var migraTables = new List<string>();
        for (int i = 0; i < tables.Count; i++)
        {
            if (!tables[i].EndsWith("_Log", StringComparison.Ordinal))
            {
                migraTables.Add(tables[i]);
            }
            else
            {
                Logger.Debug(tenantId, userId, () =>
                {
                    var vsb = new ValueStringBuilder(64);
                    vsb.Append("[TenantSeparationService] 跳过 Log 表: ");
                    vsb.Append(tables[i]);
                    return vsb.ToString();
                });
            }
        }

        // Step 2: 检查目标库表结构并创建缺失表
        for (int i = 0; i < migraTables.Count; i++)
        {
            string tbl = migraTables[i];
            await EnsureTargetTableAsync(srcConn, targetConn, tbl, tenantId, userId).ConfigureAwait(false);
        }

        // Step 3: 关闭目标库触发器
        for (int i = 0; i < migraTables.Count; i++)
        {
            string tbl = migraTables[i];
            try
            {
                var vsbDisable = new ValueStringBuilder(64);
                vsbDisable.Append("ALTER TABLE \"");
                vsbDisable.Append(tbl);
                vsbDisable.Append("\" DISABLE TRIGGER ALL");
                await using var disableCmd = new NpgsqlCommand(vsbDisable.ToString(), targetConn);
                await disableCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Debug(tenantId, userId, () =>
                {
                    var vsb = new ValueStringBuilder(128);
                    vsb.Append("[TenantSeparationService] 关闭触发器失败: ");
                    vsb.Append(tbl);
                    vsb.Append(", ");
                    vsb.Append(ex.Message);
                    return vsb.ToString();
                });
            }
        }

        try
        {
            // Step 4: 迁移数据
            for (int i = 0; i < migraTables.Count; i++)
            {
                string tbl = migraTables[i];
                bool isTenantTable = await HasTenantIdColumnAsync(srcConn, tbl).ConfigureAwait(false);
                int migrated;

                if (isTenantTable)
                {
                    migrated = await MigrateTenantDataAsync(srcConn, targetConn, tbl, _options.TenantIds, tenantId, userId)
                        .ConfigureAwait(false);
                }
                else
                {
                    migrated = await MigrateFullDataAsync(srcConn, targetConn, tbl, tenantId, userId)
                        .ConfigureAwait(false);
                }

                totalMigrated += migrated;
                Logger.Debug(tenantId, userId, () =>
                {
                    var vsb = new ValueStringBuilder(64);
                    vsb.Append("[TenantSeparationService] 迁移 ");
                    vsb.Append(tbl);
                    vsb.Append(": ");
                    vsb.Append(migrated);
                    vsb.Append(" 条");
                    return vsb.ToString();
                });
            }
        }
        finally
        {
            // Step 5: 恢复目标库触发器
            for (int i = 0; i < migraTables.Count; i++)
            {
                string tbl = migraTables[i];
                try
                {
                    var vsbEnable = new ValueStringBuilder(64);
                    vsbEnable.Append("ALTER TABLE \"");
                    vsbEnable.Append(tbl);
                    vsbEnable.Append("\" ENABLE TRIGGER ALL");
                    await using var enableCmd = new NpgsqlCommand(vsbEnable.ToString(), targetConn);
                    await enableCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Debug(tenantId, userId, () =>
                    {
                        var vsb = new ValueStringBuilder(128);
                        vsb.Append("[TenantSeparationService] 恢复触发器失败: ");
                        vsb.Append(tbl);
                        vsb.Append(", ");
                        vsb.Append(ex.Message);
                        return vsb.ToString();
                    });
                }
            }
        }

        Logger.Info(tenantId, userId, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[TenantSeparationService] 迁移完成，共 ");
            vsb.Append(totalMigrated);
            vsb.Append(" 条记录");
            return vsb.ToString();
        });
        return totalMigrated;
    }

    /// <summary>
    /// 确保目标库存在指定表（若不存在则从源库复制 DDL）。
    /// </summary>
    private static async ValueTask EnsureTargetTableAsync(
        NpgsqlConnection srcConn, NpgsqlConnection targetConn, string tableName,
        int tenantId, long userId)
    {
        // 检查目标库是否存在该表
        await using var checkCmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @t",
            targetConn);
        checkCmd.Parameters.AddWithValue("t", tableName);
        long count = (long)(await checkCmd.ExecuteScalarAsync().ConfigureAwait(false) ?? 0);

        if (count > 0)
        {
            Logger.Debug(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[TenantSeparationService] 目标表已存在: ");
                vsb.Append(tableName);
                return vsb.ToString();
            });
            return;
        }

        // 从 pg_dump 风格获取建表 DDL（简化版：使用 information_schema 构建）
        var columns = new List<(string Name, string Type, bool Nullable)>();
        await using (var colCmd = new NpgsqlCommand(
            "SELECT column_name, udt_name, is_nullable, character_maximum_length " +
            "FROM information_schema.columns WHERE table_schema = 'public' AND table_name = @t " +
            "ORDER BY ordinal_position", srcConn))
        {
            colCmd.Parameters.AddWithValue("t", tableName);
            await using var reader = await colCmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                string colName = reader.GetString(0);
                string colType = reader.GetString(1);
                bool isNullable = reader.GetString(2) == "YES";
                int? maxLen = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                string fullType;
                if (maxLen.HasValue)
                {
                    var vsbType = new ValueStringBuilder(32);
                    vsbType.Append(colType);
                    vsbType.Append('(');
                    vsbType.Append(maxLen.Value);
                    vsbType.Append(')');
                    fullType = vsbType.ToString();
                }
                else
                {
                    fullType = colType;
                }
                columns.Add((colName, fullType, isNullable));
            }
        }

        if (columns.Count == 0)
        {
            Logger.Debug(tenantId, userId, () =>
            {
                var vsb = new ValueStringBuilder(64);
                vsb.Append("[TenantSeparationService] 源表无列信息: ");
                vsb.Append(tableName);
                return vsb.ToString();
            });
            return;
        }

        // 构建 CREATE TABLE 语句
        var vsbCreate = new ValueStringBuilder(512);
        vsbCreate.Append("CREATE TABLE IF NOT EXISTS \"");
        vsbCreate.Append(tableName);
        vsbCreate.Append("\" (");
        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0) vsbCreate.Append(", ");
            vsbCreate.Append('"');
            vsbCreate.Append(columns[i].Name);
            vsbCreate.Append("\" ");
            vsbCreate.Append(columns[i].Type);
            if (!columns[i].Nullable) vsbCreate.Append(" NOT NULL");
        }
        vsbCreate.Append(')');
        string createSql = vsbCreate.ToString();

        await using var createCmd = new NpgsqlCommand(createSql, targetConn);
        await createCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        Logger.Debug(tenantId, userId, () =>
        {
            var vsb = new ValueStringBuilder(64);
            vsb.Append("[TenantSeparationService] 创建目标表: ");
            vsb.Append(tableName);
            return vsb.ToString();
        });
    }

    /// <summary>
    /// 检查表是否有 tenant_id 列。
    /// </summary>
    private static async ValueTask<bool> HasTenantIdColumnAsync(NpgsqlConnection conn, string tableName)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM information_schema.columns " +
            "WHERE table_schema = 'public' AND table_name = @t AND column_name = 'tenant_id'",
            conn);
        cmd.Parameters.AddWithValue("t", tableName);
        long count = (long)(await cmd.ExecuteScalarAsync().ConfigureAwait(false) ?? 0);
        return count > 0;
    }

    /// <summary>
    /// 迁移租户数据（WHERE tenant_id IN）。
    /// </summary>
    private static async ValueTask<int> MigrateTenantDataAsync(
        NpgsqlConnection srcConn, NpgsqlConnection targetConn,
        string tableName, int[] tenantIds, int tenantId, long userId)
    {
        // 获取列名
        var columns = await GetColumnNamesAsync(srcConn, tableName).ConfigureAwait(false);
        if (columns.Count == 0) return 0;

        string columnList = BuildColumnList(columns);
        var vsbSelect = new ValueStringBuilder(256);
        vsbSelect.Append("SELECT ");
        vsbSelect.Append(columnList);
        vsbSelect.Append(" FROM \"");
        vsbSelect.Append(tableName);
        vsbSelect.Append("\" WHERE tenant_id = ANY(@tids)");
        string selectSql = vsbSelect.ToString();

        return await CopyDataAsync(srcConn, targetConn, tableName, columns, columnList, selectSql,
            cmd => cmd.Parameters.AddWithValue("tids", tenantIds), tenantId, userId).ConfigureAwait(false);
    }

    /// <summary>
    /// 全量迁移非租户数据。
    /// </summary>
    private static async ValueTask<int> MigrateFullDataAsync(
        NpgsqlConnection srcConn, NpgsqlConnection targetConn,
        string tableName, int tenantId, long userId)
    {
        var columns = await GetColumnNamesAsync(srcConn, tableName).ConfigureAwait(false);
        if (columns.Count == 0) return 0;

        string columnList = BuildColumnList(columns);
        var vsbSelect = new ValueStringBuilder(256);
        vsbSelect.Append("SELECT ");
        vsbSelect.Append(columnList);
        vsbSelect.Append(" FROM \"");
        vsbSelect.Append(tableName);
        vsbSelect.Append('"');
        string selectSql = vsbSelect.ToString();

        return await CopyDataAsync(srcConn, targetConn, tableName, columns, columnList, selectSql,
            null, tenantId, userId).ConfigureAwait(false);
    }

    /// <summary>
    /// 从源表读取数据并使用 UPSERT 写入目标库。
    /// </summary>
    private static async ValueTask<int> CopyDataAsync(
        NpgsqlConnection srcConn, NpgsqlConnection targetConn,
        string tableName, List<string> columns, string columnList, string selectSql,
        Action<NpgsqlCommand>? addParams, int tenantId, long userId)
    {
        int migrated = 0;

        await using var selectCmd = new NpgsqlCommand(selectSql, srcConn);
        addParams?.Invoke(selectCmd);

        await using var reader = await selectCmd.ExecuteReaderAsync().ConfigureAwait(false);

        // 构建参数列表和 UPSERT SQL（仅构建一次）
        string paramList = BuildParamList(columns.Count);
        string updateList = BuildUpdateList(columns);
        var vsbUpsert = new ValueStringBuilder(512);
        vsbUpsert.Append("INSERT INTO \"");
        vsbUpsert.Append(tableName);
        vsbUpsert.Append("\" (");
        vsbUpsert.Append(columnList);
        vsbUpsert.Append(") VALUES (");
        vsbUpsert.Append(paramList);
        vsbUpsert.Append(") ON CONFLICT (id) DO UPDATE SET ");
        vsbUpsert.Append(updateList);
        string upsertSql = vsbUpsert.ToString();

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            await using var upsertCmd = new NpgsqlCommand(upsertSql, targetConn);
            for (int i = 0; i < columns.Count; i++)
            {
                var vsbParam = new ValueStringBuilder(8);
                vsbParam.Append('p');
                vsbParam.Append(i);
                upsertCmd.Parameters.AddWithValue(vsbParam.ToString(), reader.GetValue(i) ?? DBNull.Value);
            }
            await upsertCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            migrated++;
        }

        return migrated;
    }

    /// <summary>
    /// 获取表的列名列表。
    /// </summary>
    private static async ValueTask<List<string>> GetColumnNamesAsync(NpgsqlConnection conn, string tableName)
    {
        var columns = new List<string>();
        await using var cmd = new NpgsqlCommand(
            "SELECT column_name FROM information_schema.columns " +
            "WHERE table_schema = 'public' AND table_name = @t ORDER BY ordinal_position", conn);
        cmd.Parameters.AddWithValue("t", tableName);
        await using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            columns.Add(reader.GetString(0));
        }
        return columns;
    }

    /// <summary>
    /// 构建带引号的列名列表字符串。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildColumnList(List<string> columns)
    {
        var vsb = new ValueStringBuilder(stackalloc char[256]);
        for (int i = 0; i < columns.Count; i++)
        {
            if (i > 0) vsb.Append(',');
            vsb.Append('"');
            vsb.Append(columns[i]);
            vsb.Append('"');
        }
        return vsb.ToString();
    }

    /// <summary>
    /// 构建参数占位符列表（@p0,@p1,...）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildParamList(int count)
    {
        var vsb = new ValueStringBuilder(stackalloc char[256]);
        for (int i = 0; i < count; i++)
        {
            if (i > 0) vsb.Append(',');
            vsb.Append("@p");
            vsb.Append(i);
        }
        return vsb.ToString();
    }

    /// <summary>
    /// 构建 ON CONFLICT UPDATE 子句（跳过第一列 id）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildUpdateList(List<string> columns)
    {
        var vsb = new ValueStringBuilder(stackalloc char[512]);
        for (int i = 1; i < columns.Count; i++)
        {
            if (i > 1) vsb.Append(',');
            vsb.Append('"');
            vsb.Append(columns[i]);
            vsb.Append("\"=EXCLUDED.\"");
            vsb.Append(columns[i]);
            vsb.Append('"');
        }
        return vsb.ToString();
    }
}
