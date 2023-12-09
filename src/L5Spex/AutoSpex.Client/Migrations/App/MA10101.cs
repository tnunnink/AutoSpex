using System;
using System.Linq;
using FluentMigrator;
using L5Sharp.Core;

namespace AutoSpex.Client.Migrations.App;

[MigrationId(1, 01, 01, "Seeds initial type, property, element, operation, and group data for the application.")]
[Tags("App")]
public class MA10101 : Migration
{
    public override void Up()
    {
        var system = typeof(bool).Namespace;

        Insert.IntoTable("Type")
            .Row(new {TypeName = nameof(Boolean), Namespace = system, QualifiedName = typeof(bool).FullName})
            .Row(new {TypeName = nameof(Int16), Namespace = system, QualifiedName = typeof(short).FullName})
            .Row(new {TypeName = nameof(Int32), Namespace = system, QualifiedName = typeof(int).FullName})
            .Row(new {TypeName = nameof(Int64), Namespace = system, QualifiedName = typeof(long).FullName})
            .Row(new {TypeName = nameof(UInt16), Namespace = system, QualifiedName = typeof(ushort).FullName})
            .Row(new {TypeName = nameof(UInt32), Namespace = system, QualifiedName = typeof(uint).FullName})
            .Row(new {TypeName = nameof(UInt64), Namespace = system, QualifiedName = typeof(ulong).FullName})
            .Row(new {TypeName = nameof(Single), Namespace = system, QualifiedName = typeof(float).FullName})
            .Row(new {TypeName = nameof(Double), Namespace = system, QualifiedName = typeof(double).FullName})
            .Row(new {TypeName = nameof(DateTime), Namespace = system, QualifiedName = typeof(DateTime).FullName})
            .Row(new {TypeName = nameof(String), Namespace = system, QualifiedName = typeof(string).FullName});

        var l5Sharp = typeof(L5X).Namespace;
        var types = typeof(L5X).Assembly.GetExportedTypes().OrderBy(t => t.Name).ToList();

        foreach (var type in types)
        {
            Insert.IntoTable("Type").Row(
                new
                {
                    TypeName = type.Name,
                    Namespace = l5Sharp,
                    QualifiedName = type.FullName
                });
        }
    }

    public override void Down()
    {
        Delete.FromTable("Type").AllRows();
    }
}