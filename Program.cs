using CinemaWeb.Data;
using CinemaWeb.Models;
using CinemaWeb.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database (Giữ nguyên chuỗi kết nối của m nhé)
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký các Service (3 "nhân viên" của m)
builder.Services.AddScoped<VeService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<CinemaService>();

// 3. Cấu hình Session & Hỗ trợ giao diện
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<CinemaDbContext>();

    context.Database.EnsureCreated();
    if (!context.Phims.Any())
    {
        context.Phims.AddRange(
new Phim { TenPhim = "Avengers: Endgame", HinhAnh = "endgame.webp", ThoiLuong = 181, TrangThai = "Đang Chiếu", MoTa = "Siêu phẩm kết thúc kỷ nguyên Marvel." },
    new Phim { TenPhim = "Doraemon: Bản tin tương lai", HinhAnh = "doraemon.webp", ThoiLuong = 105, TrangThai = "Đang Chiếu", MoTa = "Hành trình mới của mèo máy và Nobita." },
    new Phim { TenPhim = "Oppenheimer", HinhAnh = "oppenheimer.webp", ThoiLuong = 180, TrangThai = "Đang Chiếu", MoTa = "Câu chuyện về cha đẻ bom nguyên tử." },
    new Phim { TenPhim = "Kung Fu Panda 4", HinhAnh = "kungfupanda.webp", ThoiLuong = 94, TrangThai = "Đang Chiếu", MoTa = "Gấu Po và hành trình trở thành thủ lĩnh tâm linh." },
    new Phim { TenPhim = "Godzilla x Kong", HinhAnh = "godzillaxkong.webp", ThoiLuong = 115, TrangThai = "Đang Chiếu", MoTa = "Đế chế mới của hai quái thú huyền thoại." },
    new Phim { TenPhim = "Inside Out 2", HinhAnh = "insideout2.webp", ThoiLuong = 96, TrangThai = "Đang Chiếu", MoTa = "Những cảm xúc mới xuất hiện khi Riley dậy thì." },
    new Phim { TenPhim = "Dune: Part Two", HinhAnh = "dune2.webp", ThoiLuong = 166, TrangThai = "Đang Chiếu", MoTa = "Cuộc chiến giành lại hành tinh cát Arrakis." },
    new Phim { TenPhim = "Furiosa: Mad Max", HinhAnh = "furiosa.webp", ThoiLuong = 148, TrangThai = "Đang Chiếu", MoTa = "Quá khứ huy hoàng của nữ chiến binh Furiosa." },
    new Phim { TenPhim = "Despicable Me 4", HinhAnh = "minion.webp", ThoiLuong = 95, TrangThai = "Đang Chiếu", MoTa = "Gia đình Gru và các Minions quậy phá." },
    new Phim { TenPhim = "John Wick 4", HinhAnh = "johnwick.webp", ThoiLuong = 169, TrangThai = "Đang Chiếu", MoTa = "Sát thủ huyền thoại đối đầu với Hội đồng tối cao." },

    // --- 10 PHIM SẮP CHIẾU ---
    new Phim { TenPhim = "Spider-Man: Beyond Verse", HinhAnh = "spman.webp", ThoiLuong = 140, TrangThai = "Sắp Chiếu", MoTa = "Miles Morales và cuộc chiến đa vũ trụ cuối cùng." },
    new Phim { TenPhim = "Joker: Folie à Deux", HinhAnh = "joker.webp", ThoiLuong = 120, TrangThai = "Sắp Chiếu", MoTa = "Mối tình điên rồ của Joker và Harley Quinn." },
    new Phim { TenPhim = "The Batman II", HinhAnh = "batman.webp", ThoiLuong = 165, TrangThai = "Sắp Chiếu", MoTa = "Bóng đêm trỗi dậy tại thành phố Gotham." },
    new Phim { TenPhim = "Deadpool & Wolverine", HinhAnh = "dpxwr.webp", ThoiLuong = 127, TrangThai = "Sắp Chiếu", MoTa = "Cặp đôi hoàn cảnh của vũ trụ Marvel." },
    new Phim { TenPhim = "Conan Movie 27", HinhAnh = "conan.webp", ThoiLuong = 110, TrangThai = "Sắp Chiếu", MoTa = "Vụ án hóc búa tại ngôi sao 5 cánh Hakodate." },
    new Phim { TenPhim = "Moana 2", HinhAnh = "moana2webp.webp", ThoiLuong = 100, TrangThai = "Sắp Chiếu", MoTa = "Tiếng gọi từ đại dương xa xôi." },
    new Phim { TenPhim = "Mufasa: The Lion King", HinhAnh = "mufasa.webp", ThoiLuong = 118, TrangThai = "Sắp Chiếu", MoTa = "Khám phá quá khứ của vị vua vĩ đại Mufasa." },
    new Phim { TenPhim = "Gladiator II", HinhAnh = "gladiator2.webp", ThoiLuong = 150, TrangThai = "Sắp Chiếu", MoTa = "Đấu sĩ thành Rome trở lại sau nhiều năm." },
    new Phim { TenPhim = "Sonic 3", HinhAnh = "sonicwebp.webp", ThoiLuong = 105, TrangThai = "Sắp Chiếu", MoTa = "Nhím Sonic đối đầu với kẻ thù mạnh nhất Shadow." },
    new Phim { TenPhim = "Superman (2025)", HinhAnh = "spm.webp", ThoiLuong = 145, TrangThai = "Sắp Chiếu", MoTa = "Kỷ nguyên mới của siêu nhân trong vũ trụ DC." }
        );
        context.SaveChanges();
    }
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();