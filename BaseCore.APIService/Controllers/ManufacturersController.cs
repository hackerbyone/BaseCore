using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturersController : ControllerBase
    {
        private readonly IManufacturerRepository _manufacturerRepository;
        private readonly IProductRepository _productRepository;

        public ManufacturersController(IManufacturerRepository manufacturerRepository, IProductRepository productRepository)
        {
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword = null,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10)
        {
            if (page > 0)
            {
                var (items, totalCount) = await _manufacturerRepository.GetPagedAsync(
                    page,
                    pageSize,
                    string.IsNullOrWhiteSpace(keyword)
                        ? null
                        : m => m.Name.Contains(keyword),
                    m => m.Name);

                return Ok(new
                {
                    items,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }

            var manufacturers = await _manufacturerRepository.GetAllAsync();
            return Ok(manufacturers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
            if (manufacturer == null)
                return NotFound(new { message = "Không tìm thấy nhà sản xuất" });
            return Ok(manufacturer);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ManufacturerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Tên nhà sản xuất không được để trống" });

            var existing = await _manufacturerRepository.GetByNameAsync(dto.Name.Trim());
            if (existing != null)
                return BadRequest(new { message = "Tên nhà sản xuất đã tồn tại" });

            var manufacturer = new Manufacturer
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Website = dto.Website?.Trim(),
                Phone = dto.Phone?.Trim()
            };

            await _manufacturerRepository.AddAsync(manufacturer);
            return CreatedAtAction(nameof(GetById), new { id = manufacturer.Id }, manufacturer);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerDto dto)
        {
            var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
            if (manufacturer == null)
                return NotFound(new { message = "Không tìm thấy nhà sản xuất" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                manufacturer.Name = dto.Name.Trim();

            manufacturer.Description = dto.Description?.Trim() ?? manufacturer.Description;
            manufacturer.Website = dto.Website?.Trim() ?? manufacturer.Website;
            manufacturer.Phone = dto.Phone?.Trim() ?? manufacturer.Phone;

            await _manufacturerRepository.UpdateAsync(manufacturer);
            return Ok(manufacturer);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
            if (manufacturer == null)
                return NotFound(new { message = "Không tìm thấy nhà sản xuất" });

            var (products, count) = await _productRepository.SearchAsync(null, null, id, null, null, null, false, 1, 1);
            if (count > 0)
                return BadRequest(new { message = $"Không thể xóa: còn {count} sản phẩm thuộc nhà sản xuất này" });

            await _manufacturerRepository.DeleteAsync(manufacturer);
            return Ok(new { message = "Xóa nhà sản xuất thành công" });
        }
    }

    public class ManufacturerDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Phone { get; set; }
    }
}
