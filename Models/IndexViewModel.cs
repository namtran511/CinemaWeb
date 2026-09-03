namespace CinemaWeb.Models
{
    public class IndexViewModel
    {
        public List<Phim> PhimDangChieu { get; set; } = new();
        public List<Phim> PhimSapChieu { get; set; } = new();
        public string? SearchString { get; set; }
    }
}
