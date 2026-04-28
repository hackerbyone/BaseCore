using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;

namespace BaseCore.APIService.Controllers
{
    /// <summary>
    /// Category API Controller
    /// Teaching: RESTful API, CRUD Operations (Bài 10)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoriesController(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh sách danh mục: " + ex.Message });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return NotFound(new { message = "Category not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải danh mục: " + ex.Message });
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CategoryDto dto)
        {
            var existing = await _categoryRepository.GetByNameAsync(dto.Name);
            if (existing != null)
                return BadRequest(new { message = "Category name already exists" });

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description ?? ""
            };

            await _categoryRepository.AddAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        /// <summary>
        /// Update category
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            category.Name = dto.Name ?? category.Name;
            category.Description = dto.Description ?? category.Description;

            await _categoryRepository.UpdateAsync(category);
            return Ok(category);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            var relatedProducts = await _productRepository.FindAsync(p => p.CategoryId == id);
            if (relatedProducts.Any())
                return BadRequest(new { message = "Không thể xóa danh mục đang có sản phẩm" });

            try
            {
                await _categoryRepository.DeleteAsync(category);
                return Ok(new { message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa danh mục: " + ex.Message });
            }
        }
    }

    public class CategoryDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }
}
