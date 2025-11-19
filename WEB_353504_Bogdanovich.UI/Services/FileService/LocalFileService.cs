namespace WEB_353504_Bogdanovich.UI.Services.FileService
{
    public class LocalFileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _uploadPath; // Путь относительно wwwroot

        public LocalFileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            // Изображения будем сохранять в wwwroot/Images
            _uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

            // Если папки нет, создаем ее
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            // Генерируем уникальное имя файла
            var fileExt = Path.GetExtension(file.FileName);
            var newFileName = $"{Guid.NewGuid()}{fileExt}";
            var fullPath = Path.Combine(_uploadPath, newFileName);

            // Сохраняем файл на диск
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Возвращаем относительный URL-путь для доступа через браузер
            return $"/Images/{newFileName}";
        }

        // Метод, который полезен для AccountController при отсутствии аватара
        public static string GetDefaultAvatarUrl()
        {
            return "/images/default-profile-picture.png";
        }
    }
}
