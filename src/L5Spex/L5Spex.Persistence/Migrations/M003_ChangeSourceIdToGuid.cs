using FluentMigrator;

namespace L5Spex.Migrations;

public class M003_ChangeSourceIdToGuid : Migration
{
    public override void Up()
    {
        Delete.Table("Source");
        
        Create.Table("Source")
            .WithColumn("SourceId").AsGuid().PrimaryKey()
            .WithColumn("SourcePath").AsString(255).NotNullable().Unique()
            .WithColumn("Selected").AsBoolean().WithDefaultValue(false);
    }

    public override void Down()
    {
        Delete.Table("Source");
        
        Create.Table("Source")
            .WithColumn("SourceId").AsInt32().PrimaryKey().Identity()
            .WithColumn("SourcePath").AsString(255).NotNullable().Unique()
            .WithColumn("Selected").AsBoolean().WithDefaultValue(false);
    }
}