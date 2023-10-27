using Dapper.Contrib.Extensions;
using FluentMigrator;

namespace L5Spex.Migrations;

[Migration(2)]
public class M002_AddSelectedToSource : Migration
{
    public override void Up()
    {
        Create.Column("Selected").OnTable("Source").AsBoolean().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Column("Selected").FromTable("Source");
    }
}