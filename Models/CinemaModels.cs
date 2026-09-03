using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models
{
    public class Phim
    {
        [Key] public int MaPhim { get; set; }
        [Required] public string TenPhim { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int ThoiLuong { get; set; } // Phút
        public string TrangThai { get; set; } = CinemaStatus.PhimDangChieu;
        public string? TrailerUrl { get; set; } // Thêm dấu chấm hỏi để cho phép Null
    }

    public class SuatChieu
    {
        [Key] public int MaSuat { get; set; }
        public int PhimId { get; set; }
        public int PhongChieu { get; set; } = 1;

        public Phim Phim { get; set; } = null!;
        public string ThoiGian { get; set; } = string.Empty;
    }
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhau { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string VaiTro { get; set; } = CinemaStatus.Admin.UserKhach;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
    }
    public class Ghe
    {
        [Key]
        public int MaGhe { get; set; }

      
        public string TenGhe { get; set; } = null!;

        public string TrangThai { get; set; } = "Trong";
    }
    

}