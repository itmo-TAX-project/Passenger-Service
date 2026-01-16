using FluentMigrator;

namespace Infrastructure.Db.Migrations;

[Migration(001)]
public class Initial : Migration
{
    public override void Up()
    {
        Create.Table("passengers")
            .WithColumn("passengers_id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("passenger_name")
            .AsString()
            .NotNullable()
            .WithColumn("passenger_phone")
            .AsString()
            .NotNullable();

        Create.Table("passenger_preferences")
            .WithColumn("passengers_id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("basic_allowed")
            .AsBoolean()
            .NotNullable()
            .WithColumn("mid_allowed")
            .AsBoolean()
            .NotNullable()
            .Identity()
            .WithColumn("premium_allowed")
            .AsBoolean()
            .NotNullable();
    }

    public override void Down()
    {
        Delete.Table("passengers");
        Delete.Table("passenger_preferences");
    }
}