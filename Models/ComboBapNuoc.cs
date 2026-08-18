using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models
{
    public class ComboBapNuoc
    {
        [Key]
        public int MaCombo { get; set; }
        public string TenCombo { get; set; } = null!;
        public string MoTa { get; set; } = null!;
        public double Gia { get; set; }

        // Lưu tên file ảnh tải lên
        public string? HinhAnh { get; set; }
    }
}