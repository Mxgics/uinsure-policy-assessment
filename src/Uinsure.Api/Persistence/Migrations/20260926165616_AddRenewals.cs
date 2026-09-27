using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uinsure.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRenewals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PredecessorPolicyId",
                table: "PolicyTerms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PredecessorTermId",
                table: "PolicyTerms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PolicyTerms_Id_PolicyId",
                table: "PolicyTerms",
                columns: new[] { "Id", "PolicyId" });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyTerms_PredecessorTermId",
                table: "PolicyTerms",
                column: "PredecessorTermId",
                unique: true,
                filter: "[PredecessorTermId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyTerms_PredecessorTermId_PredecessorPolicyId",
                table: "PolicyTerms",
                columns: new[] { "PredecessorTermId", "PredecessorPolicyId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyTerms_PolicyTerms_PredecessorTermId_PredecessorPolicyId",
                table: "PolicyTerms",
                columns: new[] { "PredecessorTermId", "PredecessorPolicyId" },
                principalTable: "PolicyTerms",
                principalColumns: new[] { "Id", "PolicyId" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyTerms_PolicyTerms_PredecessorTermId_PredecessorPolicyId",
                table: "PolicyTerms");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PolicyTerms_Id_PolicyId",
                table: "PolicyTerms");

            migrationBuilder.DropIndex(
                name: "IX_PolicyTerms_PredecessorTermId",
                table: "PolicyTerms");

            migrationBuilder.DropIndex(
                name: "IX_PolicyTerms_PredecessorTermId_PredecessorPolicyId",
                table: "PolicyTerms");

            migrationBuilder.DropColumn(
                name: "PredecessorPolicyId",
                table: "PolicyTerms");

            migrationBuilder.DropColumn(
                name: "PredecessorTermId",
                table: "PolicyTerms");
        }
    }
}
