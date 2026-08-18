using CinemaWeb.Data;
using CinemaWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaWeb.Services
{
    public class VeService
    {
        private readonly CinemaDbContext _context;
        public VeService(CinemaDbContext context) => _context = context;

        public async Task<Ve> DatVeAsync(int suatId, int gheId)
        {
            var suat = await _context.SuatChieus.FindAsync(suatId);
            if (suat == null) throw new Exception("Vui lòng chọn suất chiếu!");
            var isBooked = await _context.Ves.AnyAsync(v => v.SuatChieuId == suatId && v.GheId == gheId);
            if (isBooked) throw new Exception("Ghế này đã có người đặt rồi!");
            var phim = await _context.Phims.FindAsync(suat.PhimId);
            var ghe = await _context.Ghes.FindAsync(gheId);
            var veMoi = new Ve(suatId, gheId, phim.TenPhim, ghe.TenGhe);
            _context.Ves.Add(veMoi);
            await _context.SaveChangesAsync();
            return veMoi;
        }
    }

    // --- ADMIN SERVICE: CHUYÊN QUẢN LÝ (THÊM, SỬA, XÓA) ---
    public class AdminService
    {
        private readonly CinemaDbContext _context;
        public AdminService(CinemaDbContext context) => _context = context;

        // Quản lý Phim
        public List<Phim> LayTatCaPhim() => _context.Phims.ToList();
        public Phim LayPhimTheoId(int id) => _context.Phims.Find(id);
        public void ThemPhim(Phim p) { _context.Phims.Add(p); _context.SaveChanges(); }
        public void CapNhatPhim(Phim p) { _context.Phims.Update(p); _context.SaveChanges(); }
        public void XoaPhim(int id)
        {
            var p = _context.Phims.Find(id);
            if (p != null) { _context.Phims.Remove(p); _context.SaveChanges(); }
        }

        // Quản lý Suất Chiếu (M đưa hết về đây cho đúng chuẩn Admin)
        public List<SuatChieu> LayTatCaSuatChieu() => _context.SuatChieus.Include(s => s.Phim).ToList();
        public SuatChieu LaySuatChieuTheoId(int id) => _context.SuatChieus.Find(id);
        public void ThemSuatChieu(SuatChieu s) { _context.SuatChieus.Add(s); _context.SaveChanges(); }
        public void CapNhatSuatChieu(SuatChieu s) { _context.SuatChieus.Update(s); _context.SaveChanges(); }
        public void XoaSuatChieu(int id)
        {
            var s = _context.SuatChieus.Find(id);
            if (s != null) { _context.SuatChieus.Remove(s); _context.SaveChanges(); }
        }
    }

    // --- CINEMA SERVICE: CHỈ DÙNG CHO TRANG CHỦ (HIỂN THỊ) ---
    public class CinemaService
    {
        private readonly CinemaDbContext _context;
        public CinemaService(CinemaDbContext context) => _context = context;

        public List<Phim> LayPhimDangChieu() => _context.Phims.Where(p => p.TrangThai == "Đang Chiếu").ToList();
        public List<Phim> LayPhimSapChieu() => _context.Phims.Where(p => p.TrangThai == "Sắp Chiếu").ToList();
    }
}