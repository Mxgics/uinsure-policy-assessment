using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uinsure.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignPropertyContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Properties_PositiveBedrooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                table: "Properties");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Properties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine3",
                table: "Properties",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF EXISTS (SELECT 1 FROM [Properties]) THROW 51002, 'Cannot reverse property migration with retained data; restore a pre-upgrade backup.', 1;");
            migrationBuilder.DropColumn(
                name: "AddressLine3",
                table: "Properties");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Properties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                table: "Properties",
                type: "int",
                nullable: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Properties_PositiveBedrooms",
                table: "Properties",
                sql: "[Bedrooms] > 0");
        }
    }
}
