using System.Data;
using FluentMigrator;

namespace L5Spex.Migrations;

[Migration(1)]
public class M001_InitialBuild : Migration
{
    public override void Up()
    {
        Create.Table("Source")
            .WithColumn("SourceId").AsInt32().PrimaryKey().Identity()
            .WithColumn("SourcePath").AsString().NotNullable().Unique();

        Create.Table("Set")
            .WithColumn("SetId").AsString().PrimaryKey()
            .WithColumn("Name").AsString().NotNullable().Unique()
            .WithColumn("Description").AsString().Nullable();
        
        Create.Table("SpecType")
            .WithColumn("SpecTypeId").AsInt16().PrimaryKey().Identity()
            .WithColumn("Name").AsString().NotNullable().Unique();

        Create.Table("Spec")
            .WithColumn("SpecId").AsString().PrimaryKey()
            .WithColumn("SetId").AsString().NotNullable().ForeignKey("Set", "SetId")
            .WithColumn("SpecTypeId").AsString().NotNullable().ForeignKey("SpecType", "SpecTypeId")
            .WithColumn("DependsOnSpecId").AsString().Nullable().ForeignKey("Spec", "SpecId").OnDelete(Rule.Cascade)
            .WithColumn("Name").AsString().NotNullable().Unique()
            .WithColumn("Description").AsString().Nullable()
            .WithColumn("Modified").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Created").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);
    }

    public override void Down()
    {
        Delete.Table("Source");
        Delete.Table("Set");
        Delete.Table("SpecType");
        Delete.Table("Spec");
    }
}