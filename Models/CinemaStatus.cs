namespace CinemaWeb.Models
{
    public static class CinemaStatus
    {
        public const string PhimDangChieu = "Đang Chiếu";
        public const string PhimSapChieu = "Sắp Chiếu";
        public const string PhimNgungChieu = "Ngừng Chiếu";

        public const string GheTrong = "Trong";
        public const string GheDaDat = "Đã đặt";

        public const string VeChoThanhToan = "Chờ thanh toán";
        public const string VeDaThanhToan = "Đã thanh toán";
        public const string VeThanhCong = "Thành công";

        public static class Admin
        {
            public const string UserKhach = "Khách";
            public const string UserAdmin = "Admin";
        }
    }
}
