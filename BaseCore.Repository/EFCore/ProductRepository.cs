using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;

namespace BaseCore.Repository.EFCore
{
    /// <summary>
    /// Product Repository using Entity Framework Core
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<(List<Product> Products, int TotalCount)> SearchAsync(
            string? keyword, int? categoryId, int? manufacturerId,
            decimal? minPrice, decimal? maxPrice, string? brand,
            bool discountOnly, int page, int pageSize);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
    }

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<Product?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(p => p.Id == (int)id);
        }

        public async Task<(List<Product> Products, int TotalCount)> SearchAsync(
            string? keyword, int? categoryId, int? manufacturerId,
            decimal? minPrice, decimal? maxPrice, string? brand,
            bool discountOnly, int page, int pageSize)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                var kw = keyword.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(kw) ||
                    (p.Description != null && p.Description.ToLower().Contains(kw)));
            }

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId);

            if (manufacturerId.HasValue && manufacturerId > 0)
                query = query.Where(p => p.ManufacturerId == manufacturerId);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price * (1 - p.DiscountPercent / 100m) >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price * (1 - p.DiscountPercent / 100m) <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(brand))
            {
                var b = brand.ToLower();
                query = query.Where(p => p.Manufacturer != null && p.Manufacturer.Name.ToLower().Contains(b));
            }

            if (discountOnly)
                query = query.Where(p => p.DiscountPercent > 0);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .ToListAsync();
        }
    }
}
