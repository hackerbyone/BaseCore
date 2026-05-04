using BaseCore.DTO.Product;
using BaseCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Repository
{
    public interface IProductRepository
    {
        Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryDto query);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }

    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Product> Items, int TotalCount)> SearchAsync(ProductQueryDto query)
        {
            var q = _context.Products
                            .Include(p => p.Category)
                            .Include(p => p.Manufacturer)
                            .AsQueryable();

            // ✅ Tiêu chí 1: Từ khoá
            if (!string.IsNullOrWhiteSpace(query.Keyword))
                q = q.Where(p => p.Name.Contains(query.Keyword));

            // ✅ Tiêu chí 2: Danh mục
            if (query.CategoryId.HasValue)
                q = q.Where(p => p.CategoryId == query.CategoryId.Value);

            // ✅ Tiêu chí 3: Giá tối thiểu
            if (query.MinPrice.HasValue)
                q = q.Where(p => p.Price >= query.MinPrice.Value);

            // ✅ Tiêu chí 4: Giá tối đa
            if (query.MaxPrice.HasValue)
                q = q.Where(p => p.Price <= query.MaxPrice.Value);

            // ✅ Tiêu chí 5: Thương hiệu (lọc theo tên nhà sản xuất)
            if (!string.IsNullOrWhiteSpace(query.Brand))
                q = q.Where(p => p.Manufacturer != null && p.Manufacturer.Name.Contains(query.Brand));

            // Đếm tổng trước khi phân trang
            var totalCount = await q.CountAsync();

            // Phân trang
            var items = await q
                .OrderByDescending(p => p.Id)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}