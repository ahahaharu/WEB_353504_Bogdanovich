using MediatR;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.API.Data;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.Domain.Models;

namespace WEB_353504_Bogdanovich.API.UseCases
{
    public sealed record GetListOfProducts(string? categoryNormalizedName, int pageNo = 1, int pageSize = 3) : IRequest<ResponseData<ListModel<Dish>>>;

    public class GetListOfProductsHandler(AppDbContext db) : IRequestHandler<GetListOfProducts, ResponseData<ListModel<Dish>>>
    {
        private readonly int _maxPageSize = 20;

        public async Task<ResponseData<ListModel<Dish>>> Handle(GetListOfProducts request, CancellationToken cancellationToken)
        {
            var query = db.Dishes.Include(d => d.Category).AsQueryable();
            if (!string.IsNullOrEmpty(request.categoryNormalizedName))
            {
                query = query.Where(d => d.Category != null && d.Category.NormalizedName == request.categoryNormalizedName);
            }
            int totalItems = await query.CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalItems / request.pageSize);

            var data = await query.Skip((request.pageNo - 1) * request.pageSize).Take(request.pageSize).ToListAsync(cancellationToken);

            var listModel = new ListModel<Dish>
            {
                Items = data,
                CurrentPage = request.pageNo,
                TotalPages = totalPages
            };
            return ResponseData<ListModel<Dish>>.Success(listModel);
        }
    }
}
