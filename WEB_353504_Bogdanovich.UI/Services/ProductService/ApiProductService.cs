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
                var urlBuilder = new StringBuilder(_httpClient.BaseAddress.ToString());
                if (!string.IsNullOrEmpty(categoryNormalizedName))
                {
                    urlBuilder.Append($"{categoryNormalizedName}/");
                }
                if (pageNo > 1)
                {
                    urlBuilder.Append($"?pageNo={pageNo}");
                }
                if (_pageSize != 3) // По умолчанию 3, как в GetListOfProducts
                {
                    urlBuilder.Append(string.IsNullOrEmpty(urlBuilder.ToString().Split('?').LastOrDefault()) ? "?" : "&");
                    urlBuilder.Append($"pageSize={_pageSize}");
                }

                var response = await _httpClient.GetAsync(new Uri(urlBuilder.ToString()));
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

        public Task<ResponseData<Dish>> GetProductByIdAsync(int id) => throw new NotImplementedException();
        public Task UpdateProductAsync(int id, Dish product, IFormFile? formFile) => throw new NotImplementedException();
        public Task DeleteProductAsync(int id) => throw new NotImplementedException();
        public Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile) => throw new NotImplementedException();
    }
}
