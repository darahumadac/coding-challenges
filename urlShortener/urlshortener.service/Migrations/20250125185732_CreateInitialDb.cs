using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace urlshortener.service.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrlMappings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LongUrl = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShortUrl = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappings_LongUrl",
                table: "UrlMappings",
                column: "LongUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappings_ShortUrl",
                table: "UrlMappings",
                column: "ShortUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlMappings");
        }
    }
}
