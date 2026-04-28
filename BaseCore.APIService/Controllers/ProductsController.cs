using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;

namespace BaseCore.APIService.Controllers
{
    /// <summary>
    /// Product API Controller
    /// Teaching: RESTful API, CRUD Operations, EF Core (Bài 10, 11)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IManufacturerRepository _manufacturerRepository;

        public ProductsController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderDetailRepository orderDetailRepository,
            IManufacturerRepository manufacturerRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderDetailRepository = orderDetailRepository;
            _manufacturerRepository = manufacturerRepository;
        }

        /// <summary>
        /// Get all products with pagination and search
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] int? manufacturerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalCount) = await _productRepository.SearchAsync(keyword, categoryId, manufacturerId, page, pageSize);

                return Ok(new
                {
                    items = products,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách sản phẩm: " + ex.Message });
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = "Product not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải sản phẩm: " + ex.Message });
            }
        }

        /// <summary>
        /// Create new product (requires authentication)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return BadRequest(new { message = "Không tìm thấy danh mục" });

            if (dto.ManufacturerId.HasValue)
            {
                var mfr = await _manufacturerRepository.GetByIdAsync(dto.ManufacturerId.Value);
                if (mfr == null)
                    return BadRequest(new { message = "Không tìm thấy nhà sản xuất" });
            }

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl ?? "",
                DiscountPercent = dto.DiscountPercent ?? 0,
                ManufacturerId = dto.ManufacturerId
            };

            await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        /// <summary>
        /// Update product (requires authentication)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (dto.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(dto.CategoryId.Value);
                if (category == null)
                    return BadRequest(new { message = "Không tìm thấy danh mục" });
                product.CategoryId = dto.CategoryId.Value;
            }

            if (dto.ManufacturerId.HasValue)
            {
                if (dto.ManufacturerId.Value == 0)
                {
                    product.ManufacturerId = null;
                }
                else
                {
                    var mfr = await _manufacturerRepository.GetByIdAsync(dto.ManufacturerId.Value);
                    if (mfr == null)
                        return BadRequest(new { message = "Không tìm thấy nhà sản xuất" });
                    product.ManufacturerId = dto.ManufacturerId.Value;
                }
            }

            product.Name = dto.Name ?? product.Name;
            product.Price = dto.Price ?? product.Price;
            product.Stock = dto.Stock ?? product.Stock;
            product.Description = dto.Description ?? product.Description;
            product.ImageUrl = dto.ImageUrl ?? product.ImageUrl;
            if (dto.DiscountPercent.HasValue)
                product.DiscountPercent = dto.DiscountPercent.Value;

            await _productRepository.UpdateAsync(product);
            return Ok(product);
        }

        /// <summary>
        /// Delete product (requires authentication)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            var relatedDetails = await _orderDetailRepository.FindAsync(od => od.ProductId == id);
            if (relatedDetails.Any())
                return BadRequest(new { message = "Không thể xóa sản phẩm đã có trong đơn hàng" });

            try
            {
                await _productRepository.DeleteAsync(product);
                return Ok(new { message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }

        /// <summary>
        /// Upload product image (requires authentication)
        /// POST /api/products/{id}/image  — multipart/form-data, field name: "image"
        /// </summary>
        [HttpPost("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, IFormFile? image)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            if (image == null || image.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh" });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Chỉ chấp nhận file .jpg .png .gif .webp" });

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"product_{id}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = System.IO.File.Create(filePath))
                await image.CopyToAsync(stream);

            product.ImageUrl = $"/uploads/{fileName}";
            await _productRepository.UpdateAsync(product);

            return Ok(new { imageUrl = product.ImageUrl, message = "Upload ảnh thành công" });
        }

        /// <summary>
        /// Update product discount
        /// </summary>
        [HttpPut("{id}/discount")]
        [Authorize]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] UpdateDiscountDto dto)
        {
            if (dto.DiscountPercent < 0 || dto.DiscountPercent > 99)
                return BadRequest(new { message = "Phần trăm giảm giá phải từ 0 đến 99" });

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            product.DiscountPercent = dto.DiscountPercent;
            await _productRepository.UpdateAsync(product);

            return Ok(new { message = "Cập nhật giảm giá thành công", discountPercent = product.DiscountPercent, product });
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productRepository.GetByCategoryAsync(categoryId);
            return Ok(products);
        }
    }

    // DTOs
    public class ProductCreateDto
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int? ManufacturerId { get; set; }
    }

    public class ProductUpdateDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? DiscountPercent { get; set; }
        public int? ManufacturerId { get; set; }
    }

    public class UpdateDiscountDto
    {
        public decimal DiscountPercent { get; set; }
    }
}
