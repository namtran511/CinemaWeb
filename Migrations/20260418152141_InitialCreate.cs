using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ghes",
                columns: table => new
                {
                    MaGhe = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenGhe = table.Column<string>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ghes", x => x.MaGhe);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenDangNhap = table.Column<string>(type: "TEXT", nullable: false),
                    MatKhau = table.Column<string>(type: "TEXT", nullable: false),
                    HoTen = table.Column<string>(type: "TEXT", nullable: false),
                    VaiTro = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    SoDienThoai = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Phims",
                columns: table => new
                {
                    MaPhim = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenPhim = table.Column<string>(type: "TEXT", nullable: false),
                    HinhAnh = table.Column<string>(type: "TEXT", nullable: false),
                    MoTa = table.Column<string>(type: "TEXT", nullable: false),
                    ThoiLuong = table.Column<int>(type: "INTEGER", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false),
                    TrailerUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phims", x => x.MaPhim);
                });

            migrationBuilder.CreateTable(
                name: "Ves",
                columns: table => new
                {
                    MaVe = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaPhim = table.Column<int>(type: "INTEGER", nullable: false),
                    GheId = table.Column<int>(type: "INTEGER", nullable: false),
                    SuatChieuId = table.Column<int>(type: "INTEGER", nullable: false),
                    TenPhim = table.Column<string>(type: "TEXT", nullable: false),
                    TenGhe = table.Column<string>(type: "TEXT", nullable: false),
                    DanhSachGhe = table.Column<string>(type: "TEXT", nullable: false),
                    TongTien = table.Column<double>(type: "REAL", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TrangThai = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ves", x => x.MaVe);
                });

            migrationBuilder.CreateTable(
                name: "SuatChieus",
                columns: table => new
                {
                    MaSuat = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhimId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThoiGian = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuatChieus", x => x.MaSuat);
                    table.ForeignKey(
                        name: "FK_SuatChieus_Phims_PhimId",
                        column: x => x.PhimId,
                        principalTable: "Phims",
                        principalColumn: "MaPhim",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Ghes",
                columns: new[] { "MaGhe", "TenGhe", "TrangThai" },
                values: new object[,]
                {
                    { 1, "A1", "Trong" },
                    { 2, "A2", "Trong" },
                    { 3, "A3", "Trong" },
                    { 4, "A4", "Trong" },
                    { 5, "A5", "Trong" },
                    { 6, "A6", "Trong" },
                    { 7, "A7", "Trong" },
                    { 8, "A8", "Trong" },
                    { 9, "A9", "Trong" },
                    { 10, "A10", "Trong" },
                    { 11, "A11", "Trong" },
                    { 12, "A12", "Trong" },
                    { 13, "A13", "Trong" },
                    { 14, "A14", "Trong" },
                    { 15, "A15", "Trong" },
                    { 16, "A16", "Trong" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SuatChieus_PhimId",
                table: "SuatChieus",
                column: "PhimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ghes");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "SuatChieus");

            migrationBuilder.DropTable(
                name: "Ves");

            migrationBuilder.DropTable(
                name: "Phims");
        }
    }
}
