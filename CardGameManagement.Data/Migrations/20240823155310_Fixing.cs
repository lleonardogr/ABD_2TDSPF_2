using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CardGameManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fixing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Sets_EditionId",
                table: "Cards");

            migrationBuilder.RenameColumn(
                name: "EditionId",
                table: "Cards",
                newName: "SetId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_EditionId",
                table: "Cards",
                newName: "IX_Cards_SetId");

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "Description", "ImageUrl", "Name", "SetId" },
                values: new object[,]
                {
                    { 1, null, null, "Black Lotus", 1 },
                    { 2, null, null, "Ancestral Recall", 1 },
                    { 3, null, null, "Time Walk", 2 }
                });

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Alpha");

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Beta");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Sets_SetId",
                table: "Cards",
                column: "SetId",
                principalTable: "Sets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Sets_SetId",
                table: "Cards");

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.RenameColumn(
                name: "SetId",
                table: "Cards",
                newName: "EditionId");

            migrationBuilder.RenameIndex(
                name: "IX_Cards_SetId",
                table: "Cards",
                newName: "IX_Cards_EditionId");

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Set 1");

            migrationBuilder.UpdateData(
                table: "Sets",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Set 2");

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Sets_EditionId",
                table: "Cards",
                column: "EditionId",
                principalTable: "Sets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
