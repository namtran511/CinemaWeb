using CinemaWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CinemaWeb.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options) { }

        public DbSet<Phim> Phims { get; set; }
        public DbSet<SuatChieu> SuatChieus { get; set; }
        public DbSet<Ghe> Ghes { get; set; }
        public DbSet<Ve> Ves { get; set; }
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<ComboBapNuoc> ComboBapNuocs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Ghe>().HasData(
                new Ghe { MaGhe = 1, TenGhe = "A1", TrangThai = "Trong" },
                new Ghe { MaGhe = 2, TenGhe = "A2", TrangThai = "Trong" },
                new Ghe { MaGhe = 3, TenGhe = "A3", TrangThai = "Trong" },
                new Ghe { MaGhe = 4, TenGhe = "A4", TrangThai = "Trong" },
                new Ghe { MaGhe = 5, TenGhe = "A5", TrangThai = "Trong" },
                new Ghe { MaGhe = 6, TenGhe = "A6", TrangThai = "Trong" },
                new Ghe { MaGhe = 7, TenGhe = "A7", TrangThai = "Trong" },
                new Ghe { MaGhe = 8, TenGhe = "A8", TrangThai = "Trong" },
                new Ghe { MaGhe = 9, TenGhe = "A9", TrangThai = "Trong" },
                new Ghe { MaGhe = 10, TenGhe = "A10", TrangThai = "Trong" },
                new Ghe { MaGhe = 11, TenGhe = "A11", TrangThai = "Trong" },
                new Ghe { MaGhe = 12, TenGhe = "A12", TrangThai = "Trong" },
                new Ghe { MaGhe = 13, TenGhe = "A13", TrangThai = "Trong" },
                new Ghe { MaGhe = 14, TenGhe = "A14", TrangThai = "Trong" },
                new Ghe { MaGhe = 15, TenGhe = "A15", TrangThai = "Trong" },
                new Ghe { MaGhe = 16, TenGhe = "A16", TrangThai = "Trong" }
            );
        }
    }
}