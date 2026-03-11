# 面向 Claude Opus 4.6 的 ADO 数据访问层生产级实现提示词
#
## 系统要求
1. 开发语言：.NET10.0，并且以nativeAOT 方式编译。必须完全适配nativeAOT，禁止使用如:linq、反射、dynamic等。 
2. 高性能，支持在较低配置的云服务器上能够运行。 
3. 低内存，支持在较低配置的云服务器上能够运行，程序中减少缓存，仅获取必要数据。
1. 防止AI漂移
1. 你是一个资深 .NET 高性能基础设施工程师。请在 **.NET 10.0 + NativeAOT** 环境下，设计并实现一套**极致轻量、高性能、低内存、零反射、零动态、零LINQ依赖**的特性组件工程（可作为类库直接引用）。 
1. 你的输出必须是：**完整可编译的代码**（非伪代码）、完整的项目文件（**仔细推断后生成放到提示词中**）、中文规范注释、以及最小可运行示例。  
严禁需求漂移，严禁引入未要求的复杂依赖。
## 创建如下类基础类
1. 添加DbNullable<T>类，用于Update：DbNullable<T>? value,当为null时，不更新;value.Value = null时，将字段更新为null;其他值时更新为其他值。
```csharp
/// <summary>
/// 核心定义：用于区分“未设置”、“设置为具体值”和“设置为 NULL”的结构体
/// </summary>
public readonly struct DbNullable<T>
{
    /// <summary>
    /// 标记是否显式设置了值（包括设置为 null）
    /// </summary>
    public bool IsSet { get; }

    /// <summary>
    /// 实际的值。如果 IsSet 为 false，此值无意义。
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// 私有构造函数，用于内部逻辑
    /// </summary>
    private DbNullable(T? value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    /// <summary>
    /// 公开构造函数：传入值即表示“设置”
    /// </summary>
    public DbNullable(T? value)
    {
        Value = value;
        IsSet = true;
    }

    /// <summary>
    /// 静态属性：明确表示“设置为 NULL”
    /// 对应 DBNULL.StringValue 等场景
    /// </summary>
    public static DbNullable<T> NullValue => new DbNullable<T>(default(T), true);

    /// <summary>
    /// 静态属性：明确表示“未设置” (默认状态)
    /// </summary>
    public static DbNullable<T> Unset => new DbNullable<T>(default(T), false);

    /// <summary>
    /// 【关键】隐式转换运算符
    /// 允许直接将 T 或 T? 赋值给 DbNullable<T>
    /// 例如：DbNullable<int> x = 10; 或 DbNullable<string> y = null;
    /// </summary>
    public static implicit operator DbNullable<T>(T? value)
    {
        // 只要调用了这个转换，就意味着用户“有意”传值（哪怕是 null）
        return new DbNullable<T>(value, true);
    }

    public override string ToString()
    {
        if (!IsSet) return "[Unset]";
        if (Value == null) return "[Set: null]";
        return $"[Set: {Value}]";
    }
}

/// <summary>
/// 辅助常量类，提供语义化的 NULL 值 (可选)
/// </summary>
public static class DBNULL
{
    public static DbNullable<string> StringValue => DbNullable<string>.NullValue;
    public static DbNullable<int> IntValue => DbNullable<int>.NullValue;
    public static DbNullable<long> LongValue => DbNullable<long>.NullValue;
    public static DbNullable<decimal> DecimalValue => DbNullable<decimal>.NullValue;
    public static DbNullable<DateTime> DateTimeValue => DbNullable<DateTime>.NullValue;
    public static DbNullable<TimeSpan> TimeSpanValue => DbNullable<TimeSpan>.NullValue;

    // 数组类型
    public static DbNullable<string[]> StringArrayValue => DbNullable<string[]>.NullValue;
    public static DbNullable<int[]> IntArrayValue => DbNullable<int[]>.NullValue;
    public static DbNullable<long[]> LongArrayValue => DbNullable<long[]>.NullValue;
    public static DbNullable<decimal[]> DecimalArrayValue => DbNullable<decimal[]>.NullValue;
    public static DbNullable<DateTime[]> DateTimeArrayValue => DbNullable<DateTime[]>.NullValue;
    public static DbNullable<TimeSpan[]> TimeSpanArrayValue => DbNullable<TimeSpan[]>.NullValue;
}
```
1. 添加一个类名YTFieldMeta.cs
  - Name: 字段名称
  - Type: 字段类型
  - Length: 字段长度
  - Precision: 字段精度
  - IsNullable: 是否为空
  - IsPrimaryKey: 是否为主键
  - IsTenant: 是否为租户字段
## 创建与数据库表相关的实体特性相关类

### 实体特性要求
  1. 区别实体是否为租户表。只要包含了TenantId字段，就表示为租户表，如果是租户表
  2. 可自定义物理表名
  3. 可定义索引名、索引字段、索引类型
  4. 需要审计表

### 实体属性特性要求
  1. 属性可定义显示标题，如果没有则使用属性名
  1. 属性可定义数据库字段名，如果没有则使用属性名
  1. 可定义数据库字段类型，如果没有则使用当前属性类型来推断：string:text，decimal:decimal,int:int,long:bigint,DateTime:TIMESTAMPTZ,TimeSpan:TIME,int[]:INTEGER[],string[]:text[],DateTime[]:TIMESTAMPTZ[],TimeSpan[]:TIME[],bool:boolean 
  1. 可定义数据库字段长度，如果没有时：字符串类型默认为text不设置该值，如果有该值则使用varchar(length)
  1. 可定义数据库字段精度，如果没有时：decimal默认为2
  1. 可定义不能为空的特性，默认为否
  1. 可定义是否为主键，默认为否

## 创建实体数据库表与视图维护源生成器
1. 构建源生成器的数据结构来源实体与实体的属性特性，规则如下
     1. 区别实体是否为租户表。只要包含了TenantId属性，就表示为租户表，如果是租户表则需要使用分区表创建语句
        - TenantId：租户Id
     1. EntityAttribute
        - TableName：物理表或视图名称
        - ViewSql：视图SQL
           - 如果设置有值，则使用该SQL创建视图
        - NeedAuditTable：是否需要创建审计表
     1. ColumnAttribute
         - ColumnName：物理字段名称，如果没有则使用实体属性名称
         - IsPrimaryKey：是否为主键
         - Length：字段长度
                - 字段类型为string时有效，如果设置有值，则使用 varchar(长度)，没有则使用text
                - 字段类型为decimal时有效，如果设置有值则使用该值，如果没有则默认12
         - Precision：decimal的精度,默认为2
         - IsRequired:默认为false，true标识不能为空
         - IsPrimaryKey：默认为false，true标识为主键，系统禁止联合主键
         - DbType：数据库字段类型，只适配npgsql的数据类型，如果没有则使用实体属性类型来进行映射转换：
            - int -> int
            - long -> bigint
            - string -> text，如果有长度限制，则使用 varchar(长度)
            - datetime -> TIMESTAMPTZ
            - decimal -> decimal(ColumnAttribute的length, ColumnAttribute的Precision)
            - TimeSpan -> Time
            - boolean -> boolean
            - int[] -> int[]
            - long[] -> bigint[]
            - string[] -> text[]
    1. IndexAttribute
       - IndexName：索引名称
       - Columns：索引字段
       - IndexKind:枚举索引类型Normal、Unique，默认为Normal
1. 源生成器最终生成的数据库表视图维护类要求描述
      1. 生成的类名：{{Entity}}DAL.g.cs
      1. 生成类中包含的方法：
         1. 创建表：CreateTableIfNotExists(bool CreateLogTable)
            1. 调用DB.GetTableInfor方法判断数据库中是否存在该表或视图
                - 如果是表则直接返回false
            1. 如果是表直接在方法内生成const string sql
            1. 如果是表还需要生成备份表语句
                - 表名为：`{实体名}_Log`
                - 字段为：
                    logid: bigint,自增，
                    id: bigint,存储原表的id字段
                    opt:char(1),操作类型，I:插入，U:更新，D:删除
                - 需要创建触发器，触发器名称为：`{实体名}_Log_Trigger`
                  - 将新增、修改、删除的记录id存储到log表
                - 是否创建表与触发器需要根据 CreateLogTable 传参来判断
                - 当需要创建，且与同时需要审计表时，触发器的逻辑放到`{实体名}_Audit_Trigger`中，不要创建两个触发器
            1. 如果需要审计表，根据NeedAuditTable属性来判断是否需要创建审计表
                - 如果原表有租户字段，则它也必须要有
                - 记录原表主键id、的I\U\D操作的时间、操作人、租户id、（如果有，且也需要使用它来分区）、执行前完整字段json。
                - 表名为：`{实体名}_Audit`
                - 根据这些规则来生成创建sql一并放到创建方法中
                - 同时需要在创建表中，添加触发器，在每次的增删改时，记录原始的记录
                     - 触发器名称：`{实体名}_Audit_Trigger`
                     - 记录内容为：主键id、的I\U\D操作的时间、操作人id、租户id、（如果有，且也需要使用它来分区）、执行前完整记录json，是jsonb字段类型存储。
                - 在新增、修改、删除，触发器触发时，将原始记录插入到审计表中
                - 当需要备份表CreateLogTable = true时，将触发器`{实体名}_Log_Trigger`的逻辑放到`{实体名}_Audit_Trigger`中
                - 同时需要给`{实体名}_Audit`添加`{实体名}_Audit_Log`表
                - 并且添加触发器：`{实体名}_Audit_Log_Trigger`
                    - 结构与逻辑与`{实体名}_Log`一致
            1. 如果是视图，则使用ViewSql来生成const string sql
            1. 调用DB.CreateTable方法创建表或视图 
            1. 返回为 bool
            1. 方法过程，并使用Logger.Info记录整个过程信息：
                - 创建表或视图：表/视图名称，表/视图
                - 是表
                    - 判断表是否存在，记录表已存在，如果存在则返回false
                    - 如果不存在则根据实体属性与其实体属性特根据上文中的规则生成const string sql
                    - 调用DB.CreateTable方法创建表
                    - 返回为 bool
                - 是视图
                    - 判断视图是否存在，如果存在返回false
                    - 不存在则使用ViewSql来生成const string sql
                    - 调用DB.CreateTable方法创建视图
                    - 返回为 bool
            1. 创建失败时记录：Logger.Error
                - 创建表失败必须记录：TableName，sql，错误完整堆栈信息
              
          2. 修改字段：EnsureColumnLength()
                1. 只有表结构才有该方法才的完整过程，如果是视图则直接返回 false
                1. 调用DB.GetFieldsInfor方法获取表字段信息
                1. 循环判断该表字段是否均存在
                    - 如果存在且是varchar,且长度小于ColumnAttribute.Length，则生成修改字段长度
                    - 如果存在且是decimal,且decimal现在的长度与精度小于传入的长度与精度（即不能缩小长度与精度，只可以扩展长度与精度），则生成修改sql
                1. 直接在方法内生成const string sql
                1. 调用DB.CreateTable方法创建表或视图
                1. 返回为 bool
                1. 创建过程使用Logger.Info记录：
                    - 正在创建表：TableName
                    - 判断数据库中是否存在表：TableName
                    - 已存在表：TableName
                    - 创建语句：sql
                    - 创建表成功：TableName
                1. 创建失败时记录：Logger.Error
                    - 创建表失败必须记录：TableName，sql，错误完整堆栈信息
          3. 创建索引：CreateIndexIfNotExists()
                1. 只有表结构才有该方法才的完整过程，如果是视图则直接返回 false
                1. 调用DB.GetIndexInfor方法获取表索引信息
                    - 索引名称为：实体名称_索引名称
                    - 如果存在则返回false
                    - 如果根据IndexAttribute来生成const string sql
                    - 创建索引
                    - 返回为 bool
                1. 创建失败时记录：Logger.Error
                    - 创建表失败必须记录：TableName，sql，错误完整堆栈信息   
1. 源生成器最终生成实体通用的CURD操作类要求描述
     1. 生成的类名：{{Entity}}CURD.g.cs
     1. 生成类中包含的方法：
        1. 插入单个数据：InsertAsync与InsertTxAsync，区别是插入数据时是否使用事务
            1. 在方法中定义const string insertSql
                1. 根据实体与实体属性特性来生成
                1. 如："insert into tableName (name) values (@name)"
             1. 有两套重载方法
                1. 多个类型参数：传入参数为根据所有实体属性打散后的参数
                     - 必须传入int tenantId,long userId
                     - 参数名称与实体属性名称一致
                     - 参数可以为null时，参数类型与实体属性类型一致的`{{类型}}?`，且默认值null，形如 `{{类型}}? {{参数名称}} = null`
                          - 如果实体属性为int?
                     - 参数顺序与实体属性顺序一致
                     - 如果参数不能null，则参数类型为实体属性类型
                          - 如果实体属性为int，则参数类型为int
                          - 如果实体属性为int[]，则参数类型为int[]
                     - 如：`InsertAsync(int tenantId,long userId,string name,int? age)`
                1. 单个实体参数（重载方法）：传入实体
                    - 必须传入int tenantId,long userId           
                    - 在该方法中使用调用其多个类型参数的同名方法
                1. 生成NpgsqlParameter[] 参数
                      - 根据实体属性特性来生成
                      - 如传参是string? name
                        ```csharp
                            if(name != null)
                            {
                                var pm = new NpgsqlParameter("name", NpgsqlDbType.Text，name)
                                list.Add(pm);
                            }
                            else
                            {
                                var pm = new NpgsqlParameter("name", NpgsqlDbType.Text，null)
                                list.Add(pm);
                            }
                        ```
                      - 如传参是int id
                        ```csharp
                            var pm = new NpgsqlParameter("name", NpgsqlDbType.Integer，name)
                            list.Add(pm);
                        ```
                1. 然后与insertSql一起调用DB.InsertAsync或DB.InsertTxAsync方法
                1. 所有操作均使用Logger.Debug记录：
                    - 从进入方法开始记录：调用什么方法
                    - 构建NpgsqlParameter[]过程
                    - 插入后返回前记录成功插入的行数
                1. 异常时记录：Logger.Error
        2. 更新单个数据：UpdateAsync与UpdateTxAsync，区别是更新数据时是否使用事务 
              1. 多个类型参数：传入参数为根据所有实体属性打散后的多个参数DbNullable<T>或T
                 - 必须传入int tenantId,long userId,int/long pkid
                     - pkid 的类型根据实体属性特性为IsPrimaryKey的属性的类型来确定
                 - 参数名称与实体属性名称一致
                 - 参数可以为DbNullable<T>
                     - 参数顺序与实体属性顺序一致
                     - DbNullable<T>? 有3种情况：如果为null，表示该字段不更新，如果有值，当它的value为null表示该字段更新为null,如果value为非null表示更新为value
                     - 如果参数不能null，则参数类型为实体属性类型
                          - 如果实体属性为int，则参数类型为DbNullable<int>
                     - 如：`UpdateAsync(int tenantId,long userId,DbNullable<string>? name=null,DbNullable<int>? age=null)`
                1. 单个实体参数（重载方法）：传入实体
                    - 必须传入int tenantId,long userId,int/long pkid  
                    - 在该方法中使用调用其多个类型参数的同名方法
                        - 使用实体均是全部将值逐一匹配传递给多个类型参数方法
                    - 在该方法注释中需要特别说明：使用实体更新，将触发所有值更新，如果实体实例的属性为null，则该字段将更新为null（观察慎用）
                1. 动态生成更新sql与NpgsqlParameter[] 参数
                      - 根据实体属性特性来生成
                      - 如传参是DbNullable<string> name
                        ```csharp
                            ValueStringBuilder sb = new ValueStringBuilder();
                            if(name != null)
                            {
                                sb.Append("name=@name,");
                                var pm = new NpgsqlParameter("name", NpgsqlDbType.Text，name.Value)
                                list.Add(pm);
                            }
                        ```

                1. 然后与insertSql一起调用DB.UpdateAsync或DB.UpdateTxAsync方法
                1. 所有操作均使用Logger.Debug记录：
                    - 从进入方法开始记录：调用什么方法
                    - 构建NpgsqlParameter[]过程
                    - 插入后返回前记录成功插入的行数
                1. 异常时记录：Logger.Error  
        3. 删除单个数据：DeleteAsync与DeleteTxAsync，区别是删除数据时是否使用事务 
              1. - 必须传入int tenantId,long userId,int/long pkid   
        4. 获取单个数据：GetAsync与GetTxAsync，区别是获取数据时是否使用事务
              1. - 必须传入int tenantId,long userId,int/long pkid
              1. 获取单个数据时，返回实体实例
              1. 需要记录整个debug过程，并记录返回的完整数据
        5. 获取单个数据列表：GetListAsync与GetListTxAsync，区别是获取数据时是否使用事务
              1. - 必须传入int tenantId,long userId
              
1. 源生成器最终生成实体通用的实体解释类要求描述        
      1. 生成的类名为：`{实体名}Desc.g.cs`
      1. 类中包含
           - Name，常量数据库中的表名
           - IsTenant，常量是否为多租户表
           - Fields类
              - 各个属性在数据库中字段名称的常量
              - 如：
                 ```csharp
                 public static class Fields
                 {
                    public const string Id = "id";
                    public const string Name = "name";
                    public const string Age = "age";
                 }
                 ```
           - 定义public staitc readonly Dictionary<string, YTStdCommon.YTFieldMeta>  DictFieldMetas = new Dictionary<string, YTStdCommon.YTFieldMeta>();
              - 需要填充DictFieldMetas的值
              - YTFieldMeta的属性需要填充为
                    - Name: 字段名称
                    - Type: 字段类型
                    - Length: 字段长度
                    - Precision: 字段精度
                    - IsNullable: 是否为空
                    - IsPrimaryKey: 是否为主键
                    - IsTenant: 是否为租户字段
            - 定义字段的索引器
              - public static YTFieldMeta this[string filedName]
## 实现增量备份
   1. 一个独立的后台线程，定时增量备份数据
   1. 利用`{实体名}_Log`这个表的顺序进行增量备份
      1. 根据id进行分组，获取到最大的logid，并且获取到该logid的操作类型。返回id,logid,操作类型
      1. 缓存记录最大的logid，在执行完成后，需要根据该值表中小于等于logid的记录删除，表示已经同步过了的
      1. 执行逻辑：配置一个或者多个目标库连接地址
      1. 首先检查一遍表结构的一致性，还是使用{{Entity}}DAL.g.cs中的方法，但数据库链接是备份库的地址连接
      1. 关闭目标库表的触发器
      1. 如果是新增与修改根据id与opt操作获取当前源表的数据，然后写入到目标库表，使用批量插入方式，减少插入次数并且保证数据一致
      1. 不备份_log结尾的表
## 实现分离租户
   1. 实现将一个租户或者多个租户的表迁移到其他数据库
   1. 设置目标库链接地址，设置要迁移的租户
   1. 关闭目标库的触发器
   1. 迁移所选租户的数据与所有非租户表数据
      1. 不迁移_log结尾的表
   1. 恢复目标库的触发器


## 实现通用的审计查询方法

## 创建实体数据元生成器
      1. 创建实体数据元生成器

## 创建实体通用CURD源生成器
   1. 创建实体通用CURD源生成器

## 创建实体日志源生成器

## 创建实体修改前后对比源生成器


