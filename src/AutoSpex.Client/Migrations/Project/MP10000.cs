using FluentMigrator;

namespace AutoSpex.Client.Migrations.Project;

[MigrationId(1, 00, 00, "Initial Project Build")]
[Tags("Project")]
public class MP10000 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Node")
            .WithColumn("NodeId").AsString().PrimaryKey()
            .WithColumn("ParentId").AsString().Nullable()
            .WithColumn("Feature").AsString().NotNullable()
            .WithColumn("NodeType").AsString().NotNullable()
            .WithColumn("Name").AsString().NotNullable()
            .WithColumn("Depth").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Ordinal").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("Description").AsString().Nullable();

        Create.Table("Source")
            .WithColumn("SourceId").AsString().PrimaryKey()
            .WithColumn("Controller").AsString().Nullable()
            .WithColumn("Processor").AsString().Nullable()
            .WithColumn("Revision").AsString().Nullable()
            .WithColumn("IsContext").AsString().Nullable()
            .WithColumn("TargetType").AsString().Nullable()
            .WithColumn("TargetName").AsString().Nullable()
            .WithColumn("ExportedBy").AsString().Nullable()
            .WithColumn("ExportedOn").AsDate().Nullable()
            .WithColumn("Content").AsString().NotNullable();
        
        Create.Table("Spec")
            .WithColumn("SpecId").AsString().PrimaryKey()
            .WithColumn("Element").AsString().NotNullable();
        
        Create.Table("ChangeLog")
            .WithColumn("NodeId").AsString().PrimaryKey()
            .WithColumn("ChangeType").AsInt16().NotNullable()
            .WithColumn("ChangedOn").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("Message").AsString().Nullable();
        
        Create.ForeignKey()
            .FromTable("Node").ForeignColumn("ParentId")
            .ToTable("Node").PrimaryColumn("NodeId")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        
        Create.ForeignKey()
            .FromTable("Source").ForeignColumn("SourceId")
            .ToTable("Node").PrimaryColumn("NodeId")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        
        Create.ForeignKey()
            .FromTable("Spec").ForeignColumn("SpecId")
            .ToTable("Node").PrimaryColumn("NodeId")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        
        Create.ForeignKey()
            .FromTable("ChangeLog").ForeignColumn("NodeId")
            .ToTable("Node").PrimaryColumn("NodeId")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
    }
}