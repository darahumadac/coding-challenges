using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace approvalworkflow.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "UserRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AuthUserId",
                table: "AppUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_AuthUserId",
                table: "AppUsers",
                column: "AuthUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUsers_AuthUserId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "UserRequests");

            migrationBuilder.DropColumn(
                name: "AuthUserId",
                table: "AppUsers");
        }
    }
}
