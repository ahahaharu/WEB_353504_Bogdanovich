using System.Text.Json;

namespace WEB_353504_Bogdanovich.UI.Services.CategoryService
{
    public class ApiCategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<ApiCategoryService> _logger;

        public ApiCategoryService(HttpClient httpClient, ILogger<ApiCategoryService> logger)
        {
            _httpClient = httpClient;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        public async Task<ResponseData<List<Category>>> GetCategoryListAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_httpClient.BaseAddress);
                var responseContent = await response.Content.ReadAsStringAsync(); // Для отладки
                _logger.LogInformation($"Получен JSON от /api/categories: {responseContent}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ResponseData<List<Category>>>(_serializerOptions);
                    return data ?? ResponseData<List<Category>>.Error("Данные пусты или в неверном формате.");
                }
                _logger.LogError($"Не удалось получить данные с сервера. Статус: {response.StatusCode}");
                return ResponseData<List<Category>>.Error($"Ошибка сервера: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Ошибка сети при запросе категорий: {ex.Message}");
                return ResponseData<List<Category>>.Error($"Ошибка сети: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Ошибка десериализации JSON: {ex.Message}");
                return ResponseData<List<Category>>.Error($"Ошибка обработки данных: {ex.Message}");
            }
        }
    }
}
