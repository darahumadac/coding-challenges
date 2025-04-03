using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace approvalworkflow.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class DropUserRequestStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UserRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
