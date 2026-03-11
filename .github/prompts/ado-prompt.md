# 面向 Claude Opus 4.6 的 ADO 数据访问层生产级实现提示词
## PostgreSQL / Npgsql 专用，高性能、低 GC、AOT 友好的 ADO 数据访问层

> **状态**：模板 / 待完善。

1. 开发语言：.NET10.0，并且以nativeAOT 方式编译。必须完全适配nativeAOT，禁止使用如:linq、反射、dynamic等。 
2. 高性能，支持在较低配置的云服务器上能够运行。 
3. 低内存，支持在较低配置的云服务器上能够运行，程序中减少缓存，仅获取必要数据。
1. 防止AI漂移
1. 你是一个资深 .NET 高性能基础设施工程师。请在 **.NET 10.0 + NativeAOT** 环境下，设计并实现一套**极致轻量、高性能、低内存、零反射、零动态、零LINQ依赖**的特性组件工程（可作为类库直接引用）。 
1. 你的输出必须是：**完整可编译的代码**（非伪代码）、完整的项目文件（**仔细推断后生成放到提示词中**）、中文规范注释、以及最小可运行示例。  
严禁需求漂移，严禁引入未要求的复杂依赖。

---

## 实现要求
  1. 引用nuget包Npgsql
  1. 实现对postgresql数据库进行增删改查操作
  1. 实现连接池
  1. 可设置连接池大小
      1. 可设置最大连接池
      1. 可设置最小连接池
      1. 初始化时自动创建最小数量的连接池
  1. 可设置连接超时时间
  1. 可设置连接重试次数
  1. 可设置连接池注销时间
  1. 有DB静态类提供公共静态方法
      1. 获取事务，用于事务处理
          - 方法名：GetBatchAsync(int tenantId, long userId)
            - 从连接池获取连接`var conn = GetTransaction();`
            - 创建事务 `await conn.BeginTransactionAsync();`
            - 创建NpgsqlBatch并关联事务
            - 创建NpgsqlBatchCommand执行`select set_config('app.user_id', @userId, true);`
               - 使用set_config的目的用于在触发器中获取userId
            - batch.BatchCommands.Add(cmd1);
          - 返回值：NpgsqlBatch batch
      1. 获取事务，用于事务处理
          - 方法名：GetTransaction()
            - 从连接池获取连接连接
          - 返回值：NpgsqlConnection
      1. 批量提交事务处理
          - 方法名：BatchCommitAsync(NpgsqlBatch batch)
          - 返回 DbUdqResult 
            ```csharp
            public sealed class DbUdqResult 
            {
                public bool Success { get; init; }

                public int RowsAffected { get; init; }

                public string? ErrorMessage { get; init; }

                public string? DebugMessage { get; init; }
            }
            ```
      1. 更新数据
          - 方法名
            - UpdateAsync(string sql, PgSqlParam[] parameters, int tenantId, long userId)
            - UpdateTxAsync(NpgsqlBatch batch,  string sql, PgSqlParam[] parameters, int tenantId, long userId)
          - 传参是原始sql与传参集合，操作人信息
          - 执行过程：
            - 如果不是Tx方法，先调用GetBatchAsync
            ```csharp
             using var cmd = new NpgsqlBatchCommand(sql);
             foreach (var parameter in parameters)
             cmd.Parameters.AddWithValue(parameter.DbType, parameter.Value);
             batch.BatchCommands.Add(cmd);
             await batch.ExecuteNonQueryAsync();
             await batch.Transaction.CommitAsync();
            ```
            -  如果是Tx方法，使用batch构建NpgsqlBatchCommand，但不提交
          - 返回结构:DbUdqResult

                ```csharp
                public sealed class DbUdqResult 
                {
                    public bool Success { get; init; }

                    public int RowsAffected { get; init; }

                    public string? ErrorMessage { get; init; }

                    public string? DebugMessage { get; init; }
                }
                ```

            - ErrorMessage 前端用户可以看懂的错误提示
            - DebugMessage 完整的错误堆栈，并包含原始sql、传参集合，以及sql拼接传参的查询执行sql用于调试，操作人，执行时间
      1. 插入数据
          - 方法名：同上
          - 传参是原始sql与传参集合，操作人信息
          - 执行过程：同上
          - 返回结构 DbInsResult 
            ```csharp
            public sealed class DbInsResult 
            {
                public bool Success { get; init; }

                public long Id { get; init; }

                public string? ErrorMessage { get; init; }

                public string? DebugMessage { get; init; }
            }
            ```
          - Id 新增的id
      1. 删除数据表
          - 方法名：同上
          - 传参是原始sql与传参集合，操作人信息
          - 执行过程：同上
          - 返回结构 DbUdqResult
      1. 查询多行数据
          - 方法名
            - GetListAsync<T>(string sql, PgSqlParam[] parameters,Func<NpgsqlDataReader, T> mapper, int tenantId, long userId)
            - GetListTxAsync<T>(NpgsqlBatch batch,string sql, PgSqlParam[] parameters,Func<NpgsqlDataReader, T> mapper, int tenantId, long userId)
            - GetListAsync( string sql, PgSqlParam[] parameters,Utf8JsonWriter writer, Action<Utf8JsonWriter,NpgsqlDataReader> writermap, int tenantId, long userId)
               - 该方法是用于直接构建json返回 
          - 传参是原始sql、传参集合，操作人信息，Func<NpgsqlDataReader, T> mapper
            - marper: 映射方法
            - 其他过程：同上
          - 返回标准结构 DbUdqResult 
            - RowsAffected 获取的总行数
      1. 查询单个数据
          - 方法名
            - GetScalarAsync<T>(string sql, PgSqlParam[] parameters, int tenantId, long userId)
            - GetScalarTxAsync<T>((NpgsqlBatch batch,string sql, PgSqlParam[] parameters, int tenantId, long userId)
          - 传参是原始sql、传参集合，操作人信息
          - 其他过程：同上
          - 返回标准结构
            - 是否成功
            - 错误提示
            - 完整的错误堆栈，并包含原始sql、传参集合，以及sql拼接传参的查询执行sql用于调试，操作人，执行时间
      1. 获取数据表是否存在
          - 方法名 GetTableInfor(string tableName)
          - 内部还是实质调用GetListAsync
          - 返回结构 DbUdqResult
          - RowsAffected 等于1，表示数据表存在
      1. 获取数据表字段信息
          - 方法名 GetFieldsInfor(string tableName，Func<NpgsqlDataReader, DbField> mapper)
          - DbField是数据库表字段信息
          - 内部还是实质调用GetListAsync
          - 返回结构 DbUdqResult
      1. 获取数据表索引信息
          - 方法名 GetFieldsInfor(string tableName，Func<NpgsqlDataReader, DbIndex> mapper)
          - DbIndex是数据库表字段信息
          - 内部还是实质调用GetListAsync
          - 返回结构 DbUdqResult
      1. 创建数据库表
          - 方法名 CreateTable(string tableName, string sql)
          - 返回枚举：DDLStatus
              ```csharp
              public enum DDLStatus
              { Success, Failed, Existed }
              ```
          - 先判断数据表是否存在，如果存在则返回Existed
          - 物理是否成功均必须记录完整的日志：执行时间、sql、执行状态
      1. 修改数据库表字段
          - 方法名 AlterTable(string tableName,string fieldName, string dataType, string length, string precision, bool nullable,bool isPrimaryKey)
          - 返回枚举：DDLStatus
          - 先判字段是否存在，不存在则创建，字段存在则判断类型、长度、精度是否一致，只有修改长度大于原始长度时才能够修改，其他情况不能修改，创建或修改成功返回Success，不能修改返回Failed
          - 物理是否成功均必须记录完整的日志：执行时间、sql、执行状态
      1. 新增数据库表索引
          - 方法名 CreateIndex(string indexName, string tableName, string fieldNames, bool unique)
          - 返回枚举：DDLStatus
          - 先判断索引是否存在，不存在则创建，存在则返回Existed，不在创建
          - 必须记录完整的日志：执行时间、sql、执行状态
      1. 记录日志需要使用int tenantid,long userid。如果方法未提供的，使用tenantid = 0,userid = 0为默认值传参到日志库记录