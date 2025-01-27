using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace urlshortener.service.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndexesAndKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UrlMappings",
                table: "UrlMappings");

            migrationBuilder.DropIndex(
                name: "IX_UrlMappings_LongUrl",
                table: "UrlMappings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UrlMappings",
                table: "UrlMappings",
                column: "LongUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UrlMappings",
                table: "UrlMappings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UrlMappings",
                table: "UrlMappings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappings_LongUrl",
                table: "UrlMappings",
                column: "LongUrl",
                unique: true);
        }
    }
}
