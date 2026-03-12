using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YTStdEntity.Generator.Emitters;
using YTStdEntity.Generator.Models;

namespace YTStdEntity.Generator;

/// <summary>
/// 实体 Source Generator 入口。
/// 扫描标注了 [Entity] 的实体类，自动生成 DAL、CRUD、审计查询、描述类代码。
/// </summary>
[Generator]
public sealed class EntityGenerator : IIncrementalGenerator
{
    private const string EntityAttributeFullName = "YTStdEntity.Attributes.EntityAttribute";
    private const string ColumnAttributeFullName = "YTStdEntity.Attributes.ColumnAttribute";
    private const string IndexAttributeFullName = "YTStdEntity.Attributes.IndexAttribute";
    private const string DetailOfAttributeFullName = "YTStdEntity.Attributes.DetailOfAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                EntityAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, ct) => ParseEntity(ctx, ct))
            .Where(static m => m != null)
            .Select(static (m, _) => m!);

        // Collect all entities to link master-detail relationships
        var allEntities = entityProvider.Collect();

        context.RegisterSourceOutput(allEntities, static (spc, models) =>
        {
            // Build lookup by class name to link DetailOf → master's DetailTables
            var dict = new Dictionary<string, EntityModel>();
            foreach (var m in models)
            {
                dict[m.ClassName] = m;
            }

            // Link detail tables to their master entities
            foreach (var m in models)
            {
                if (m.DetailRelation != null)
                {
                    if (dict.TryGetValue(m.DetailRelation.MasterClassName, out var master))
                    {
                        master.DetailTables.Add(m.DetailRelation);
                    }
                }
            }

            // Emit code for each entity
            foreach (var model in models)
            {
                spc.AddSource($"{model.ClassName}DAL.g.cs", DalEmitter.Emit(model));
                spc.AddSource($"{model.ClassName}CRUD.g.cs", CrudEmitter.Emit(model));
                spc.AddSource($"{model.ClassName}Desc.g.cs", DescEmitter.Emit(model));

                if (model.NeedAuditTable)
                {
                    spc.AddSource($"{model.ClassName}AuditCRUD.g.cs", AuditCrudEmitter.Emit(model));
                }
            }
        });
    }

    private static EntityModel? ParseEntity(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        if (!(ctx.TargetSymbol is INamedTypeSymbol classSymbol))
            return null;

        var entityAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == EntityAttributeFullName);
        if (entityAttr == null)
            return null;

        var model = new EntityModel
        {
            ClassName = classSymbol.Name,
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
        };

        // Parse Entity attribute named arguments
        foreach (var arg in entityAttr.NamedArguments)
        {
            switch (arg.Key)
            {
                case "TableName":
                    model.TableName = arg.Value.Value as string ?? "";
                    break;
                case "ViewSql":
                    model.ViewSql = arg.Value.Value as string;
                    break;
                case "NeedAuditTable":
                    model.NeedAuditTable = arg.Value.Value is true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(model.TableName))
            model.TableName = ToSnakeCase(model.ClassName);

        // Parse properties for columns
        var members = classSymbol.GetMembers();
        foreach (var member in members)
        {
            ct.ThrowIfCancellationRequested();
            if (!(member is IPropertySymbol prop))
                continue;
            if (prop.DeclaredAccessibility != Accessibility.Public)
                continue;
            if (prop.IsStatic || prop.IsIndexer)
                continue;
            if (prop.GetMethod == null || prop.SetMethod == null)
                continue;

            var col = ParseColumn(prop);
            if (col == null)
                continue;

            model.Columns.Add(col);
            if (col.IsPrimaryKey)
                model.PrimaryKey = col;
            if (col.IsTenantField)
                model.IsTenantTable = true;
        }

        // Parse Index attributes
        foreach (var attr in classSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != IndexAttributeFullName)
                continue;

            var args = attr.ConstructorArguments;
            if (args.Length < 2)
                continue;

            var indexName = args[0].Value as string ?? "";
            var columns = new List<string>();

            // Second arg could be a single string or params array
            if (args[1].Kind == TypedConstantKind.Array)
            {
                foreach (var item in args[1].Values)
                {
                    if (item.Value is string s)
                        columns.Add(s);
                }
            }
            else if (args[1].Value is string singleCol)
            {
                columns.Add(singleCol);
            }

            // Additional params args beyond the second
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].Value is string extraCol)
                    columns.Add(extraCol);
            }

            var indexModel = new IndexModel
            {
                IndexName = indexName,
                Columns = columns.Select(c => ToSnakeCase(c)).ToArray(),
            };

            // Check Kind named argument
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Kind" && namedArg.Value.Value is int kindVal)
                {
                    indexModel.IsUnique = kindVal == 1; // IndexKind.Unique = 1
                }
            }

            model.Indexes.Add(indexModel);
        }

        // Parse DetailOf attribute
        foreach (var attr in classSymbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() != DetailOfAttributeFullName)
                continue;

            var args = attr.ConstructorArguments;
            if (args.Length < 1)
                continue;

            var masterType = args[0].Value as INamedTypeSymbol;
            if (masterType == null)
                continue;

            var foreignKey = "";
            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "ForeignKey")
                    foreignKey = namedArg.Value.Value as string ?? "";
            }

            // Get master table name from its Entity attribute
            var masterTableName = ToSnakeCase(masterType.Name);
            var masterEntityAttr = masterType.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == EntityAttributeFullName);
            if (masterEntityAttr != null)
            {
                foreach (var arg in masterEntityAttr.NamedArguments)
                {
                    if (arg.Key == "TableName" && arg.Value.Value is string tn)
                        masterTableName = tn;
                }
            }

            model.DetailRelation = new DetailRelation
            {
                MasterClassName = masterType.Name,
                MasterTableName = masterTableName,
                DetailClassName = model.ClassName,
                DetailTableName = model.TableName,
                ForeignKeyPropertyName = foreignKey,
                ForeignKeyColumnName = ToSnakeCase(foreignKey),
            };
        }

        return model;
    }

    private static ColumnModel? ParseColumn(IPropertySymbol prop)
    {
        var col = new ColumnModel
        {
            PropertyName = prop.Name,
            ColumnName = ToSnakeCase(prop.Name),
        };

        // Determine nullability
        var typeSymbol = prop.Type;
        bool isNullableRef = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        bool isNullableValue = false;
        string clrTypeName;

        // Unwrap Nullable<T>
        if (typeSymbol is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            namedType.TypeArguments.Length == 1)
        {
            typeSymbol = namedType.TypeArguments[0];
            isNullableValue = true;
        }

        clrTypeName = typeSymbol.ToDisplayString();

        // Handle array types
        bool isArray = false;
        string elementTypeName = clrTypeName;
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            isArray = true;
            elementTypeName = arrayType.ElementType.ToDisplayString();
            clrTypeName = elementTypeName + "[]";
        }

        col.ClrTypeName = clrTypeName;
        col.IsNullable = isNullableRef || isNullableValue;

        // Check for tenant field
        if (prop.Name == "TenantId")
            col.IsTenantField = true;

        // Parse Column attribute
        var columnAttr = prop.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ColumnAttributeFullName);

        if (columnAttr != null)
        {
            foreach (var arg in columnAttr.NamedArguments)
            {
                switch (arg.Key)
                {
                    case "ColumnName":
                        if (arg.Value.Value is string cn && !string.IsNullOrEmpty(cn))
                            col.ColumnName = cn;
                        break;
                    case "Title":
                        col.Title = arg.Value.Value as string;
                        break;
                    case "IsPrimaryKey":
                        col.IsPrimaryKey = arg.Value.Value is true;
                        break;
                    case "Length":
                        col.Length = arg.Value.Value is int l ? l : 0;
                        break;
                    case "Precision":
                        col.Precision = arg.Value.Value is int p ? p : 2;
                        break;
                    case "IsRequired":
                        col.IsRequired = arg.Value.Value is true;
                        break;
                    case "DbType":
                        if (arg.Value.Value is string dt && !string.IsNullOrEmpty(dt))
                            col.PgType = dt;
                        break;
                }
            }
        }

        // Auto-map CLR type to PG type if not explicitly set
        if (string.IsNullOrEmpty(col.PgType))
        {
            MapClrTypeToPg(col, clrTypeName, isArray, elementTypeName);
        }

        // Auto-map NpgsqlDbType
        if (string.IsNullOrEmpty(col.NpgsqlDbTypeName))
        {
            col.NpgsqlDbTypeName = GetNpgsqlDbTypeName(clrTypeName, isArray, elementTypeName);
        }

        // Primary keys are always NOT NULL
        if (col.IsPrimaryKey)
            col.IsNullable = false;

        return col;
    }

    private static void MapClrTypeToPg(ColumnModel col, string clrTypeName, bool isArray, string elementTypeName)
    {
        if (isArray)
        {
            switch (elementTypeName)
            {
                case "int":
                    col.PgType = "int[]";
                    break;
                case "long":
                    col.PgType = "bigint[]";
                    break;
                case "string":
                    col.PgType = "text[]";
                    break;
                default:
                    col.PgType = "text[]";
                    break;
            }
            return;
        }

        switch (clrTypeName)
        {
            case "int":
                col.PgType = "int";
                break;
            case "long":
                col.PgType = "bigint";
                break;
            case "string":
                col.PgType = col.Length > 0 ? $"varchar({col.Length})" : "text";
                break;
            case "decimal":
                var len = col.Length > 0 ? col.Length : 12;
                col.PgType = $"decimal({len},{col.Precision})";
                break;
            case "System.DateTime":
                col.PgType = "TIMESTAMPTZ";
                break;
            case "System.TimeSpan":
                col.PgType = "TIME";
                break;
            case "bool":
                col.PgType = "boolean";
                break;
            default:
                col.PgType = "text";
                break;
        }
    }

    private static string GetNpgsqlDbTypeName(string clrTypeName, bool isArray, string elementTypeName)
    {
        if (isArray)
        {
            switch (elementTypeName)
            {
                case "int": return "Array | NpgsqlDbType.Integer";
                case "long": return "Array | NpgsqlDbType.Bigint";
                case "string": return "Array | NpgsqlDbType.Text";
                default: return "Array | NpgsqlDbType.Text";
            }
        }

        switch (clrTypeName)
        {
            case "int": return "Integer";
            case "long": return "Bigint";
            case "string": return "Text";
            case "decimal": return "Numeric";
            case "System.DateTime": return "TimestampTz";
            case "System.TimeSpan": return "Time";
            case "bool": return "Boolean";
            default: return "Text";
        }
    }

    internal static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    internal static string GetReaderMethod(ColumnModel col)
    {
        var clr = col.ClrTypeName;
        if (clr.EndsWith("[]"))
            return "GetFieldValue";

        switch (clr)
        {
            case "int": return "GetInt32";
            case "long": return "GetInt64";
            case "string": return "GetString";
            case "decimal": return "GetDecimal";
            case "System.DateTime": return "GetDateTime";
            case "System.TimeSpan": return "GetFieldValue";
            case "bool": return "GetBoolean";
            default: return "GetString";
        }
    }

    internal static bool NeedsGenericReader(ColumnModel col)
    {
        var clr = col.ClrTypeName;
        return clr.EndsWith("[]") || clr == "System.TimeSpan";
    }

    internal static string GetClrTypeForCode(ColumnModel col)
    {
        return col.ClrTypeName;
    }
}
