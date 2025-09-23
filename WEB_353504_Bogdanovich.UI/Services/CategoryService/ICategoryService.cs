using WEB_353504_Bogdanovich.Domain.Models;
using WEB_353504_Bogdanovich.Domain.Entities;

namespace WEB_353504_Bogdanovich.UI.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<ResponseData<List<Category>>> GetCategoryListAsync();
    }
}
