using FluentMigrator;

namespace AutoSpex.Persistence;

[MigrationId(1, 01, 00, "Add the criterion table to support storing the configured filters and verifications")]
[Tags("Project")]
public class MP10100 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Criterion")
            .WithColumn("CriterionId").AsString().PrimaryKey()
            .WithColumn("NodeId").AsString().NotNullable().ForeignKey("Node", "NodeId")
            .WithColumn("Usage").AsString().NotNullable()
            .WithColumn("Element").AsString().NotNullable()
            .WithColumn("Property").AsString().NotNullable()
            .WithColumn("Operation").AsString().NotNullable()
            .WithColumn("Args").AsString().NotNullable();
    }
}