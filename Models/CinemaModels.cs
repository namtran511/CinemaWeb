using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models
{
    public class Phim
    {
        [Key] public int MaPhim { get; set; }
        [Required] public string TenPhim { get; set; }
        public string HinhAnh { get; set; }
        public string MoTa { get; set; }
        public int ThoiLuong { get; set; } // Phút
        public string TrangThai { get; set; } = "Đang Chiếu";
        public string? TrailerUrl { get; set; } // Thêm dấu chấm hỏi để cho phép Null
    }

    public class SuatChieu
    {
        [Key] public int MaSuat { get; set; }
        public int PhimId { get; set; }

        public Phim Phim { get; set; }
        public string ThoiGian { get; set; }
    }
    public class NguoiDung
    {
        [Key]
        public int Id { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string VaiTro { get; set; } = "Khách";
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