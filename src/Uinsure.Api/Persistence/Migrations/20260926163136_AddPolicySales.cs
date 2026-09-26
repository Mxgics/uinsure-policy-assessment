using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uinsure.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicySales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MutationRevision = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PolicyTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HasClaims = table.Column<bool>(type: "bit", nullable: false),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyTerms", x => x.Id);
                    table.CheckConstraint("CK_PolicyTerms_EndAfterStart", "[EndDate] >= [StartDate]");
                    table.CheckConstraint("CK_PolicyTerms_PositivePremium", "[Premium] > 0");
                    table.ForeignKey(
                        name: "FK_PolicyTerms_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_PositiveAmount", "[Amount] > 0");
                    table.ForeignKey(
                        name: "FK_Payments_PolicyTerms_PolicyTermId",
                        column: x => x.PolicyTermId,
                        principalTable: "PolicyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policyholders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policyholders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policyholders_PolicyTerms_PolicyTermId",
                        column: x => x.PolicyTermId,
                        principalTable: "PolicyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyTermId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Bedrooms = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.Id);
                    table.CheckConstraint("CK_Properties_PositiveBedrooms", "[Bedrooms] > 0");
                    table.ForeignKey(
                        name: "FK_Properties_PolicyTerms_PolicyTermId",
                        column: x => x.PolicyTermId,
                        principalTable: "PolicyTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PolicyTermId",
                table: "Payments",
                column: "PolicyTermId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Reference",
                table: "Payments",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Reference",
                table: "Policies",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policyholders_PolicyTermId",
                table: "Policyholders",
                column: "PolicyTermId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyTerms_PolicyId",
                table: "PolicyTerms",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PolicyTermId",
                table: "Properties",
                column: "PolicyTermId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Policyholders");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "PolicyTerms");

            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
