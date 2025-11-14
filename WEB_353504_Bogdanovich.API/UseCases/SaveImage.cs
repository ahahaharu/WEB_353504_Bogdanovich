using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System;

namespace WEB_353504_Bogdanovich.API.UseCases
{
    // Запрос на сохранение файла
    public sealed record SaveImage(IFormFile file) : IRequest<string>;

    // Обработчик запроса
    public class SaveImageHandler : IRequestHandler<SaveImage, string>
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaveImageHandler(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
        {
            if (request.file == null)
            {
                return Task.FromResult("Images/noimage.jpg");
            }

            // Получаем wwwroot/Images
            var imagesPath = Path.Combine(_env.WebRootPath, "Images");

            // Генерируем уникальное имя файла
            var randomName = $"{Guid.NewGuid()}{Path.GetExtension(request.file.FileName)}";
            var filePath = Path.Combine(imagesPath, randomName);

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                request.file.CopyTo(stream);
            }

            // Формируем URL, по которому файл будет доступен
            var contextRequest = _httpContextAccessor.HttpContext!.Request;
            var host = $"{contextRequest.Scheme}://{contextRequest.Host}";
            var url = $"{host}/Images/{randomName}";

            return Task.FromResult(url);
        }
    }
}