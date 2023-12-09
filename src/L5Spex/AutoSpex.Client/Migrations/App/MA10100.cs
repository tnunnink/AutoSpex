using FluentMigrator;

namespace AutoSpex.Client.Migrations.App;

[MigrationId(1, 01, 00, "Adds the type table to indicate supported types.")]
[Tags("App")]
public class MA10100 : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Type")
            .WithColumn("QualifiedName").AsString().PrimaryKey()
            .WithColumn("AssemblyName").AsString().NotNullable()
            .WithColumn("FullName").AsString().NotNullable()
            .WithColumn("Namespace").AsString().NotNullable()
            .WithColumn("TypeName").AsString().NotNullable()
            .WithColumn("QualifiedName").AsString().NotNullable();
            
        /*Create.Table("Element")
            .WithColumn("ElementId").AsInt32().PrimaryKey()
            .WithColumn("TypeId").AsInt32().NotNullable().ForeignKey("TypeId", "Type")
            .WithColumn("ElementName").AsString().NotNullable();

        Create.Table("Property")
            .WithColumn("PropertyId").AsInt32().PrimaryKey()
            .WithColumn("TypeId").AsInt32().NotNullable().ForeignKey("TypeId", "Type")
            .WithColumn("ElementId").AsInt32().NotNullable().ForeignKey("ElementId", "Element")
            .WithColumn("PropertyName").AsString().NotNullable();

        Create.Table("TypeGroup")
            .WithColumn("GroupId").AsInt32().PrimaryKey()
            .WithColumn("TypeId").AsInt32().NotNullable().ForeignKey("TypeId", "Type")
            .WithColumn("GroupName").AsString().NotNullable().Unique();

        Create.Table("Operation")
            .WithColumn("OperationId").AsInt32().PrimaryKey()
            .WithColumn("OperationName").AsString().NotNullable().Unique();

        Create.Table("TypeOperations")
            .WithColumn("OperationId").AsInt32().NotNullable().ForeignKey("OperationId", "Operation")
            .WithColumn("GroupId").AsInt32().NotNullable().ForeignKey("GroupId", "TypeGroup");*/
    }
}