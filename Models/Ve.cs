using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models
{
    public class Ve
    {
        [Key]
        public int MaVe { get; set; }
        public int MaPhim { get; set; }
        public int GheId { get; set; }
        public int SuatChieuId { get; set; }
        public string TenPhim { get; set; } = null!;
        public string TenGhe { get; set; } = null!;
        public string DanhSachGhe { get; set; } = null!;

        
        public string? BapNuoc { get; set; }
        public int? NguoiDungId { get; set; }
       

        public double TongTien { get; set; }
        public DateTime NgayDat { get; set; } = DateTime.Now;
        public string TrangThai { get; set; } = "Đã thanh toán";

        public Ve() { }

        // Cập nhật luôn hàm tạo (Constructor) thêm tham số bapNuoc
        public Ve(int maPhim, int suatChieuId, string tenGheParam, string trangThai, string? bapNuoc = null)
        {
            MaPhim = maPhim;
            SuatChieuId = suatChieuId;
            TenGhe = tenGheParam;
            DanhSachGhe = tenGheParam;
            TrangThai = trangThai;
            BapNuoc = bapNuoc; // Gán giá trị bắp nước
            TongTien = 75000;
            NgayDat = DateTime.Now;
            TenPhim = "Phim đặt vé";
        }
    }
}