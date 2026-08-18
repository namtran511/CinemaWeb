using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaWeb.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BapNuoc",
                table: "Ves",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NguoiDungId",
                table: "Ves",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComboBapNuocs",
                columns: table => new
                {
                    MaCombo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenCombo = table.Column<string>(type: "TEXT", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", nullable: false),
                    Gia = table.Column<double>(type: "REAL", nullable: false),
                    HinhAnh = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboBapNuocs", x => x.MaCombo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComboBapNuocs");

            migrationBuilder.DropColumn(
                name: "BapNuoc",
                table: "Ves");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "Ves");
        }
    }
}
