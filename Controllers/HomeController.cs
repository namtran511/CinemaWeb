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

namespace CinemaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly VeService _veService;
        private readonly AdminService _adminService;
        private readonly CinemaService _cinemaService;

        public HomeController(CinemaDbContext context, VeService veService, AdminService adminService, CinemaService cinemaService)
        {
            _veService = veService;
            _adminService = adminService;
            _cinemaService = cinemaService;
            _context = context;
        }

        // --- GIAO DIỆN CHÍNH ---
        public IActionResult Index(string searchString)
        {
            var phims = _context.Phims.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower();
                phims = phims.Where(p => p.TenPhim.ToLower().Contains(searchLower));
            }
            ViewBag.PhimSapChieu = phims.Where(p => p.TrangThai == "Sắp Chiếu").ToList();
            return View(phims.Where(p => p.TrangThai == "Đang Chiếu").ToList());
        }

        public IActionResult ChiTiet(int id)
        {
            var phim = _context.Phims.FirstOrDefault(p => p.MaPhim == id);
            if (phim == null) return RedirectToAction("Index");
            ViewBag.SuatChieus = _context.SuatChieus.Where(s => s.PhimId == id).ToList();
            ViewBag.Ghes = _context.Ghes.ToList();
            return View(phim);
        }

        [HttpGet]
        public IActionResult LayDanhSachGheAjax(int suatId)
        {
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
            if (suat != null && DateTime.TryParseExact(suat.ThoiGian.Split('-')[1].Trim(), "d/M/yyyy", null, DateTimeStyles.None, out DateTime dtSuat))
            {
                if (dtSuat.Date < DateTime.Now.Date)
                    return Content("<script>alert('Suất cũ rồi!'); window.location.href='/Home/ChiTiet/" + maPhim + "';</script>", "text/html");
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
                TrangThai = "Chờ thanh toán",
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
                               .Where(v => v.NgayDat.Year == y && (v.TrangThai == "Đã thanh toán" || v.TrangThai == "Thành công"))
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
                .Where(v => v.NgayDat.Year == nam && v.NgayDat.Month == thang && (v.TrangThai == "Đã thanh toán" || v.TrangThai == "Thành công"))
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
        [HttpPost] public async Task<IActionResult> SuaPhim(Phim m, IFormFile f) { var d = _context.Phims.Find(m.MaPhim); if (d != null) { d.TenPhim = m.TenPhim; d.MoTa = m.MoTa; d.ThoiLuong = m.ThoiLuong; d.TrangThai = m.TrangThai; d.TrailerUrl = m.TrailerUrl; if (f != null && f.Length > 0) { string n = Guid.NewGuid().ToString() + "_" + Path.GetFileName(f.FileName); string p = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", n); using (var s = new FileStream(p, FileMode.Create)) { await f.CopyToAsync(s); } d.HinhAnh = n; } _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        [HttpGet] public IActionResult SuaSuatChieu(int id) { var s = _context.SuatChieus.Find(id); if (s == null) return RedirectToAction("AdminPhim"); ViewBag.Phims = _context.Phims.ToList(); return View(s); }
        [HttpPost] public IActionResult SuaSuatChieu(SuatChieu m) { var d = _context.SuatChieus.Find(m.MaSuat); if (d != null) { d.PhimId = m.PhimId; d.ThoiGian = m.ThoiGian; _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
        [HttpGet] public IActionResult SuaThongTin() { var id = HttpContext.Session.GetInt32("UserId"); return id == null ? RedirectToAction("DangNhap") : View(_context.NguoiDungs.Find(id)); }
        [HttpPost] public IActionResult SuaThongTin(NguoiDung u) { var d = _context.NguoiDungs.Find(u.Id); if (d != null) { d.HoTen = u.HoTen; d.Email = u.Email; d.SoDienThoai = u.SoDienThoai; if (!string.IsNullOrEmpty(u.MatKhau)) d.MatKhau = u.MatKhau; _context.SaveChanges(); HttpContext.Session.SetString("UserName", d.HoTen); } return RedirectToAction("Index"); }
        [HttpGet] public IActionResult DangKy() => View();
        [HttpPost] public IActionResult DangKy(NguoiDung u) { u.VaiTro = "Khách"; _context.NguoiDungs.Add(u); _context.SaveChanges(); return RedirectToAction("DangNhap"); }
        [HttpGet] public IActionResult DangNhap() => View();
        [HttpPost] public IActionResult DangNhap(string username, string password) { var u = _context.NguoiDungs.FirstOrDefault(x => x.TenDangNhap == username && x.MatKhau == password); if (u != null) { HttpContext.Session.SetString("UserRole", u.VaiTro); HttpContext.Session.SetString("UserName", u.HoTen); HttpContext.Session.SetInt32("UserId", u.Id); return RedirectToAction("Index"); } return View(); }
        public IActionResult DangXuat() { HttpContext.Session.Clear(); return RedirectToAction("Index"); }
        [HttpGet] public IActionResult ThemPhim() => View();
        [HttpPost] public async Task<IActionResult> ThemPhim(Phim p, IFormFile f) { if (f != null) { string n = Guid.NewGuid().ToString() + "_" + Path.GetFileName(f.FileName); string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", n); using (var s = new FileStream(path, FileMode.Create)) { await f.CopyToAsync(s); } p.HinhAnh = n; } _context.Phims.Add(p); _context.SaveChanges(); return RedirectToAction("AdminPhim"); }
        [HttpGet] public IActionResult ThemSuatChieu() { ViewBag.Phims = _context.Phims.ToList(); return View(); }
        [HttpPost] public IActionResult ThemSuatChieu(SuatChieu sc) { _context.SuatChieus.Add(sc); _context.SaveChanges(); return RedirectToAction("AdminPhim"); }
        public IActionResult HoanTatThanhToan(int id) { var v = _context.Ves.Find(id); if (v != null) { v.TrangThai = "Đã thanh toán"; _context.SaveChanges(); } return RedirectToAction("AdminPhim"); }
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
                string n = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileAnhCombo.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", n);

                using (var s = new FileStream(path, FileMode.Create))
                {
                    await FileAnhCombo.CopyToAsync(s);
                }
                c.HinhAnh = n;
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