using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BareqAlNaqool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTreeMemberRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "TreeMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpouseId",
                table: "TreeMembers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreeMembers_ParentId",
                table: "TreeMembers",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TreeMembers_SpouseId",
                table: "TreeMembers",
                column: "SpouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreeMembers_TreeMembers_ParentId",
                table: "TreeMembers",
                column: "ParentId",
                principalTable: "TreeMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreeMembers_TreeMembers_SpouseId",
                table: "TreeMembers",
                column: "SpouseId",
                principalTable: "TreeMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Best-effort backfill for existing generation-based seed data:
            // attach each non-root member to the latest member one generation above.
            migrationBuilder.Sql(
                """
                UPDATE "TreeMembers" AS child
                SET "ParentId" = parent."Id"
                FROM (
                    SELECT DISTINCT ON ("Generation")
                        "Id",
                        "Generation"
                    FROM "TreeMembers"
                    ORDER BY "Generation", "SortOrder" DESC, "Id" DESC
                ) AS parent
                WHERE child."ParentId" IS NULL
                  AND child."Generation" > 0
                  AND parent."Generation" = child."Generation" - 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreeMembers_TreeMembers_ParentId",
                table: "TreeMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_TreeMembers_TreeMembers_SpouseId",
                table: "TreeMembers");

            migrationBuilder.DropIndex(
                name: "IX_TreeMembers_ParentId",
                table: "TreeMembers");

            migrationBuilder.DropIndex(
                name: "IX_TreeMembers_SpouseId",
                table: "TreeMembers");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "TreeMembers");

            migrationBuilder.DropColumn(
                name: "SpouseId",
                table: "TreeMembers");
        }
    }
}
