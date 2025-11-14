using System.Text.Json;
using System.Text;

namespace WEB_353504_Bogdanovich.UI.Services.ProductService
{
    public class ApiProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly int _pageSize;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<ApiProductService> _logger;

        public ApiProductService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiProductService> logger)
        {
            _httpClient = httpClient;
            _pageSize = configuration.GetSection("ItemsPerPage").Get<int>();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        public async Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            try
            {
                var urlBuilder = new StringBuilder("Dish");

                if (!string.IsNullOrEmpty(categoryNormalizedName))
                {
                    urlBuilder.Append($"/{categoryNormalizedName}");
                }

                string querySeparator = "?";


                urlBuilder.Append($"{querySeparator}pageNo={pageNo}");
                querySeparator = "&";


                if (_pageSize != 3)
                {
                    urlBuilder.Append($"{querySeparator}pageSize={_pageSize}");
                }


                var response = await _httpClient.GetAsync(urlBuilder.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Dish>>>(_serializerOptions);
                    return data ?? ResponseData<ListModel<Dish>>.Error("Данные пусты или в неверном формате.");
                }
                _logger.LogError($"Не удалось получить данные с сервера. Статус: {response.StatusCode}");
                return ResponseData<ListModel<Dish>>.Error($"Ошибка сервера: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Ошибка сети при запросе блюд: {ex.Message}");
                return ResponseData<ListModel<Dish>>.Error($"Ошибка сети: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Ошибка десериализации JSON: {ex.Message}");
                return ResponseData<ListModel<Dish>>.Error($"Ошибка обработки данных: {ex.Message}");
            }
        }


        public async Task<ResponseData<Dish>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Dish/{id}");

                var responseContent = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {

                    var dish = JsonSerializer.Deserialize<Dish>(responseContent, _serializerOptions);

                    if (dish != null)
                    {

                        return ResponseData<Dish>.Success(dish);
                    }

                    return ResponseData<Dish>.Error("API вернул пустой объект.");
                }

                _logger.LogError($"[DELETE DIAGNOSTIC] Ошибка сервера. Статус: {response.StatusCode}. Ответ: {responseContent}");


                var errorData = JsonSerializer.Deserialize<ResponseData<Dish>>(responseContent, _serializerOptions);

                return errorData ?? ResponseData<Dish>.Error($"Ошибка сервера: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DELETE DIAGNOSTIC] Ошибка сети при запросе блюда {id}: {ex.Message}");
                return ResponseData<Dish>.Error($"Ошибка сети: {ex.Message}");
            }
        }

        public async Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            try
            {
                var content = new MultipartFormDataContent();

                var dishJson = JsonSerializer.Serialize(product);

                content.Add(new StringContent(dishJson, Encoding.UTF8, "application/json"), "dish");

                if (formFile != null && formFile.Length > 0)
                {
                    var fileContent = new StreamContent(formFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);


                    content.Add(fileContent, "file", formFile.FileName);
                }

                // ИСПРАВЛЕНО: Явно указываем относительный путь "Dish"
                var response = await _httpClient.PostAsync("Dish", content);

                if (response.IsSuccessStatusCode)
                {
                    var createdDish = await response.Content.ReadFromJsonAsync<Dish>(_serializerOptions);

                    if (createdDish != null)
                    {
                        return ResponseData<Dish>.Success(createdDish);
                    }
                    return ResponseData<Dish>.Error("Не удалось десериализовать созданный объект.");
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Ошибка создания блюда: {response.StatusCode} - {error}");

                string errorMessage = string.IsNullOrEmpty(error) ? $"Ошибка сервера: {response.StatusCode}" : $"Ошибка сервера: {response.StatusCode}. Детали: {error}";

                return ResponseData<Dish>.Error(errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Исключение при создании блюда: {ex.Message}");
                return ResponseData<Dish>.Error($"Ошибка: {ex.Message}");
            }
        }

        public async Task<ResponseData<object>> UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            try
            {
                var content = new MultipartFormDataContent();

                // ИСПРАВЛЕНО: Сериализуем весь объект в JSON, как требует API-эндпоинт MapPut
                var dishJson = JsonSerializer.Serialize(product);
                content.Add(new StringContent(dishJson, Encoding.UTF8, "application/json"), "dish");

                // Добавляем файл
                if (formFile != null && formFile.Length > 0)
                {
                    var fileContent = new StreamContent(formFile.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);

                    // Ключ "file" (вместо "formFile" или других)
                    content.Add(fileContent, "file", formFile.FileName);
                }

                // Отправляем PUT запрос
                var response = await _httpClient.PutAsync($"Dish/{id}", content);

                return response.IsSuccessStatusCode
                    ? ResponseData<object>.Success(null)
                    : ResponseData<object>.Error($"Ошибка: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return ResponseData<object>.Error(ex.Message);
            }
        }

        public async Task<ResponseData<object>> DeleteProductAsync(int id)
        {
            try
            {
                // Правильный относительный путь: Dish/{id}
                var response = await _httpClient.DeleteAsync($"Dish/{id}");

                return response.IsSuccessStatusCode
                    ? ResponseData<object>.Success(null)
                    : ResponseData<object>.Error($"Ошибка: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return ResponseData<object>.Error(ex.Message);
            }
        }
    }
}