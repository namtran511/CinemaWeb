using CinemaWeb.Data;
using CinemaWeb.Models;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace CinemaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly VeService _veService;
        private readonly AdminService _adminService;
        private readonly CinemaService _cinemaService;
        private readonly ImageStorageService _imageStorageService;
        private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

        public HomeController(CinemaDbContext context, VeService veService, AdminService adminService, CinemaService cinemaService, ImageStorageService imageStorageService)
        {
            _veService = veService;
            _adminService = adminService;
            _cinemaService = cinemaService;
            _imageStorageService = imageStorageService;
            _context = context;
        }

        private bool VerifyPassword(NguoiDung user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhau, password);
                return result != PasswordVerificationResult.Failed;
            }
            catch
            {
                return string.Equals(user.MatKhau, password, StringComparison.Ordinal);
            }
        }

        private bool IsShowTimeExpired(SuatChieu? suat)
        {
            if (suat == null)
                return true;

            if (!ShowTimeHelper.TryParse(suat.ThoiGian, out var dateTime))
                return false;

            return dateTime <= DateTime.Now;
        }

        private bool IsRoomOccupied(int phimId, DateTime showTime, int phongChieu, int? excludeSuatId = null)
        {
            var phim = _context.Phims.Find(phimId);
            if (phim == null)
                return true;

            var targetStart = showTime;
            var targetEnd = targetStart.AddMinutes(phim.ThoiLuong);

            var duLieu = _context.SuatChieus
                .Include(s => s.Phim)
                .Where(s => s.PhongChieu == phongChieu && (!excludeSuatId.HasValue || s.MaSuat != excludeSuatId.Value))
                .ToList();

            foreach (var s in duLieu)
            {
                if (!ShowTimeHelper.TryParse(s.ThoiGian, out var otherStart))
                    continue;

                var otherPhim = s.Phim ?? _context.Phims.Find(s.PhimId);
                var otherDuration = otherPhim?.ThoiLuong ?? phim.ThoiLuong;
                var otherEnd = otherStart.AddMinutes(otherDuration);

                if (targetStart < otherEnd && otherStart < targetEnd)
                    return true;
            }

            return false;
        }

        private List<SuatChieu> GetAvailableShowTimes(int phimId)
        {
            return _context.SuatChieus
                .Where(s => s.PhimId == phimId)
                .ToList()
                .Where(s => !IsShowTimeExpired(s))
                .OrderBy(s =>
                {
                    if (ShowTimeHelper.TryParse(s.ThoiGian, out var dt))
                        return dt;
                    return DateTime.MaxValue;
                })
                .ToList();
        }

        // --- GIAO DIỆN CHÍNH ---
        public IActionResult Index(string searchString)
        {
            var phims = _context.Phims.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.Trim();
                phims = phims.Where(p => p.TenPhim.ToLower().Contains(searchLower.ToLower()));
            }

            var model = new IndexViewModel
            {
                SearchString = searchString,
                PhimDangChieu = phims.Where(p => p.TrangThai == CinemaStatus.PhimDangChieu).ToList(),
                PhimSapChieu = phims.Where(p => p.TrangThai == CinemaStatus.PhimSapChieu).ToList()
            };

            return View(model);
        }

        public IActionResult ChiTiet(int id)
        {
            var phim = _context.Phims.FirstOrDefault(p => p.MaPhim == id);
            if (phim == null) return RedirectToAction("Index");
            ViewBag.SuatChieus = GetAvailableShowTimes(id);
            ViewBag.Ghes = _context.Ghes.ToList();
            return View(phim);
        }

        [HttpGet]
        public IActionResult LayDanhSachGheAjax(int suatId)
        {
            var suat = _context.SuatChieus.Find(suatId);
            if (IsShowTimeExpired(suat))
            {
                return Json(new object[0]);
            }

            var danhSachGheStrings = _context.Ves.Where(v => v.SuatChieuId == suatId).Select(v => v.DanhSachGhe).ToList();
            var gheDaDat = new List<string>();
            foreach (var s in danhSachGheStrings)
            {
                if (!string.IsNullOrEmpty(s)) gheDaDat.AddRange(s.Split(',').Select(x => x.Trim()));
            }
            var result = _context.Ghes.Select(g => new { tenGhe = g.TenGhe, trangThai = gheDaDat.Contains(g.TenGhe) ? "DaDat" : "Trong" }).ToList();
            return Json(result);
        }

        [HttpPost]
        public IActionResult ChonBapNuoc(int maPhim, int suatId, string danhSachGhe)
        {
            if (suatId <= 0) return RedirectToAction("Index");

            var suat = _context.SuatChieus.Find(suatId);
            if (IsShowTimeExpired(suat))
            {
                return RedirectToAction("ChiTiet", new { id = maPhim });
            }

            ViewBag.MaPhim = maPhim;
            ViewBag.SuatId = suatId;
            ViewBag.DanhSachGhe = danhSachGhe;
            ViewBag.TenPhim = _context.Phims.Find(maPhim)?.TenPhim;

            // Lấy danh sách Combo từ DB gửi ra View
            ViewBag.DsCombo = _context.ComboBapNuocs.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult XacNhanThanhToan(int maPhim, int suatId, string danhSachGhe, double? tongTien, string bapNuoc)
        {
            if (string.IsNullOrEmpty(danhSachGhe) || suatId <= 0)
                return RedirectToAction("ChiTiet", new { id = maPhim });

            var suat = _context.SuatChieus.Find(suatId);
            if (IsShowTimeExpired(suat))
            {
                return Content("<script>alert('Suất chiếu đã quá hạn, không thể đặt vé!'); window.location.href='/Home/ChiTiet/" + maPhim + "';</script>", "text/html");
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            var veMoi = new Ve
            {
                MaPhim = maPhim,
                SuatChieuId = suatId,
                TenPhim = _context.Phims.Find(maPhim)?.TenPhim ?? "Phim",
                TenGhe = danhSachGhe,
                DanhSachGhe = danhSachGhe,
                TongTien = tongTien ?? (danhSachGhe.Split(',').Length * 75000),
                NgayDat = DateTime.Now,
                TrangThai = CinemaStatus.VeChoThanhToan,
                BapNuoc = bapNuoc,
                NguoiDungId = userId
            };

            _context.Ves.Add(veMoi);
            _context.SaveChanges();

            return View("ThanhToan", veMoi);
        }

        // --- QUẢN TRỊ ---
        public IActionResult AdminPhim()
        {
            ViewBag.DsSuatChieu = _context.SuatChieus.Include(s => s.Phim).ToList();
            ViewBag.DsVe = _context.Ves.OrderByDescending(v => v.NgayDat).ToList();
            ViewBag.DsCombo = _context.ComboBapNuocs.ToList();

            return View(_context.Phims.ToList());
        }

        public IActionResult GoiYPhim()
        {
            var danhSachPhim = _context.Phims
                .Where(p => p.TrangThai == CinemaStatus.PhimDangChieu)
                .ToList()
                .OrderBy(_ => Guid.NewGuid())
                .Take(10)
                .ToList();

            if (danhSachPhim.Count == 0)
                return RedirectToAction("Index");

            return View(danhSachPhim);
        }

        public IActionResult LichSu()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("DangNhap");

            ViewBag.DsSuatChieu = _context.SuatChieus.ToList();

            var dsVe = _context.Ves
                               .Where(v => v.NguoiDungId == userId)
                               .OrderByDescending(v => v.NgayDat)
                               .ToList();

            return View(dsVe);
        }

        // --- BÁO CÁO DOANH THU ---
        public IActionResult BaoCao(int? nam)
        {
            // Nếu không chọn năm thì tự động lấy năm hiện tại
            int y = nam ?? DateTime.Now.Year;

            // BẮT BUỘC: Gửi cái năm vừa chọn ra ngoài View để ô Dropdown nó nhớ
            ViewBag.SelectedYear = y;

            // Lọc vé theo năm và lấy những vé Đã thanh toán hoặc Thành công
            var data = _context.Ves
                               .Where(v => v.NgayDat.Year == y && (v.TrangThai == CinemaStatus.VeDaThanhToan || v.TrangThai == CinemaStatus.VeThanhCong))
                               .GroupBy(v => v.NgayDat.Month)
                               .Select(g => new ThongKeViewModel
                               {
                                   Thang = g.Key,
                                   DoanhThu = g.Sum(v => v.TongTien),
                                   SoVe = g.Count()
                               }).ToList();

            return View(data);
        }

        // --- ĐÃ THÊM: API LẤY DOANH THU THEO NGÀY DÙNG CHO BIỂU ĐỒ BÓC TÁCH ---
        [HttpGet]
        public IActionResult GetDoanhThuNgayAjax(int nam, int thang)
        {
            // 1. Tìm xem tháng này có bao nhiêu ngày (28, 30 hay 31)
            int daysInMonth = DateTime.DaysInMonth(nam, thang);

            // 2. Gom doanh thu theo từng ngày trong cái tháng đó
            var data = _context.Ves
                .Where(v => v.NgayDat.Year == nam && v.NgayDat.Month == thang && (v.TrangThai == CinemaStatus.VeDaThanhToan || v.TrangThai == CinemaStatus.VeThanhCong))
                .GroupBy(v => v.NgayDat.Day)
                .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(v => v.TongTien) })
                .ToList();

            // 3. Tạo 1 mảng dải đều từ mùng 1 đến cuối tháng (ngày nào ế thì điền 0)
            var result = new List<double>();
            for (int i = 1; i <= daysInMonth; i++)
            {
                var d = data.FirstOrDefault(x => x.Ngay == i);
                result.Add(d != null ? d.DoanhThu : 0);
            }

            // 4. Ném dữ liệu ra ngoài cho Javascript (Chart.js) nó vẽ
            return Json(new { days = daysInMonth, data = result });
        }


        // --- CÁC HÀM KHÁC ---
        [HttpGet] public IActionResult SuaPhim(int id) { var p = _context.Phims.Find(id); return p == null ? RedirectToAction("AdminPhim") : View(p); }
        [HttpPost] public async Task<IActionResult> SuaPhim(Phim m, IFormFile f) { var d = _context.Phims.Find(m.MaPhim); if (d != null) { d.TenPhim = m.TenPhim; d.MoTa = m.MoTa; d.ThoiLuong = m.ThoiLuong; d.TrangThai = m.TrangThai; d.TrailerUrl = m.TrailerUrl; if (f != null && f.Length > 0) { var savedFileName = await _imageStorageService.SaveAsync(f); d.HinhAnh = savedFileName; } _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        [HttpGet] public IActionResult SuaSuatChieu(int id) { var s = _context.SuatChieus.Find(id); if (s == null) return RedirectToAction("AdminPhim"); ViewBag.Phims = _context.Phims.ToList(); return View(s); }
        [HttpPost] public IActionResult SuaSuatChieu(SuatChieu m)
        {
            var d = _context.SuatChieus.Find(m.MaSuat);
            if (d == null)
                return RedirectToAction("AdminPhim");

            if (!ShowTimeHelper.TryParse(m.ThoiGian, out var showTime))
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Thời gian suất chiếu không hợp lệ.";
                return View(d);
            }

            if (showTime <= DateTime.Now)
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Không thể tạo hoặc cập nhật suất chiếu trong quá khứ hoặc bằng thời gian hiện tại.";
                return View(d);
            }

            if (m.PhongChieu < 1 || m.PhongChieu > 9)
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Phòng chiếu phải nằm trong khoảng 1 đến 9.";
                return View(d);
            }

            if (IsRoomOccupied(m.PhimId, showTime, m.PhongChieu, m.MaSuat))
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Phòng này đã có suất chiếu khác chồng thời gian. Hãy chọn phòng hoặc giờ khác.";
                return View(d);
            }

            d.PhimId = m.PhimId;
            d.ThoiGian = m.ThoiGian;
            d.PhongChieu = m.PhongChieu;
            _context.SaveChanges();
            return RedirectToAction("AdminPhim");
        }
        [HttpGet] public IActionResult SuaThongTin() { var id = HttpContext.Session.GetInt32("UserId"); return id == null ? RedirectToAction("DangNhap") : View(_context.NguoiDungs.Find(id)); }
        [HttpPost] public IActionResult SuaThongTin(NguoiDung u) { var d = _context.NguoiDungs.Find(u.Id); if (d != null) { d.HoTen = u.HoTen; d.Email = u.Email; d.SoDienThoai = u.SoDienThoai; if (!string.IsNullOrEmpty(u.MatKhau)) d.MatKhau = _passwordHasher.HashPassword(d, u.MatKhau); _context.SaveChanges(); HttpContext.Session.SetString("UserName", d.HoTen); } return RedirectToAction("Index"); }
        [HttpGet] public IActionResult DangKy() => View();
        [HttpPost] public IActionResult DangKy(NguoiDung u) { u.VaiTro = CinemaStatus.Admin.UserKhach; u.MatKhau = _passwordHasher.HashPassword(u, u.MatKhau); _context.NguoiDungs.Add(u); _context.SaveChanges(); return RedirectToAction("DangNhap"); }
        [HttpGet] public IActionResult DangNhap() => View();
        [HttpPost] public IActionResult DangNhap(string username, string password)
        {
            var u = _context.NguoiDungs.FirstOrDefault(x => x.TenDangNhap == username);
            if (u != null)
            {
                var isValidPassword = VerifyPassword(u, password);

                if (isValidPassword)
                {
                    if (string.Equals(u.MatKhau, password, StringComparison.Ordinal))
                    {
                        u.MatKhau = _passwordHasher.HashPassword(u, password);
                        _context.SaveChanges();
                    }

                    HttpContext.Session.SetString("UserRole", u.VaiTro);
                    HttpContext.Session.SetString("UserName", u.HoTen);
                    HttpContext.Session.SetInt32("UserId", u.Id);
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
            return View();
        }
        public IActionResult DangXuat() { HttpContext.Session.Clear(); return RedirectToAction("Index"); }
        [HttpGet] public IActionResult ThemPhim() => View();
        [HttpPost] public async Task<IActionResult> ThemPhim(Phim p, IFormFile f) { if (f != null && f.Length > 0) { p.HinhAnh = await _imageStorageService.SaveAsync(f); } _context.Phims.Add(p); _context.SaveChanges(); return RedirectToAction("AdminPhim"); }
        [HttpGet] public IActionResult ThemSuatChieu() { ViewBag.Phims = _context.Phims.ToList(); return View(); }
        [HttpPost] public IActionResult ThemSuatChieu(SuatChieu sc)
        {
            if (!ShowTimeHelper.TryParse(sc.ThoiGian, out var showTime))
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Thời gian suất chiếu không hợp lệ.";
                return View(sc);
            }

            if (showTime <= DateTime.Now)
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Không thể tạo suất chiếu trong quá khứ hoặc bằng thời gian hiện tại.";
                return View(sc);
            }

            if (sc.PhongChieu < 1 || sc.PhongChieu > 9)
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Phòng chiếu phải nằm trong khoảng 1 đến 9.";
                return View(sc);
            }

            if (IsRoomOccupied(sc.PhimId, showTime, sc.PhongChieu))
            {
                ViewBag.Phims = _context.Phims.ToList();
                ViewBag.Error = "Phòng này đã có suất chiếu khác chồng thời gian. Chỉ được 1 phòng chiếu trong cùng một thời gian.";
                return View(sc);
            }

            _context.SuatChieus.Add(sc);
            _context.SaveChanges();
            return RedirectToAction("AdminPhim");
        }
        public IActionResult HoanTatThanhToan(int id) { var v = _context.Ves.Find(id); if (v != null) { v.TrangThai = CinemaStatus.VeDaThanhToan; _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        public IActionResult XoaPhim(int id) { var p = _context.Phims.Find(id); if (p != null) { _context.Phims.Remove(p); _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        public IActionResult XoaSuatChieu(int id) { var s = _context.SuatChieus.Find(id); if (s != null) { _context.SuatChieus.Remove(s); _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        public IActionResult AboutUs() => View();

        // --- QUẢN LÝ COMBO BẮP NƯỚC ---
        [HttpGet]
        public IActionResult ThemCombo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ThemCombo(ComboBapNuoc c, IFormFile FileAnhCombo)
        {
            if (FileAnhCombo != null && FileAnhCombo.Length > 0)
            {
                c.HinhAnh = await _imageStorageService.SaveAsync(FileAnhCombo);
            }
            else
            {
                c.HinhAnh = "no-image.jpg";
            }

            _context.ComboBapNuocs.Add(c);
            _context.SaveChanges();

            return RedirectToAction("AdminPhim");
        }

        public IActionResult XoaCombo(int id)
        {
            var c = _context.ComboBapNuocs.Find(id);
            if (c != null)
            {
                _context.ComboBapNuocs.Remove(c);
                _context.SaveChanges();
            }
            return RedirectToAction("AdminPhim");
        }
    }
}