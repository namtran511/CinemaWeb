namespace CinemaWeb.Services
{
    public class ImageStorageService
    {
        private readonly string _imagesFolder;

        public ImageStorageService()
        {
            _imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<string> SaveAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File ảnh không hợp lệ.");

            var fileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_imagesFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void Delete(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            var fullPath = Path.Combine(_imagesFolder, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
