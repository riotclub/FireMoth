using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiotClub.FireMoth.Services.Migrations
{
    /// <inheritdoc />
    public partial class CreateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileFingerprints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "varchar(256)", nullable: false),
                    DirectoryName = table.Column<string>(type: "varchar(1000)", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    Base64Hash = table.Column<string>(type: "char(44)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileFingerprints", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileFingerprints");
        }
    }
}
