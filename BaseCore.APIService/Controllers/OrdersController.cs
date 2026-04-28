using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;
using System.Security.Claims;
using System.Linq;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IManufacturerRepository _manufacturerRepository;

        public OrdersController(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductRepository productRepository,
            IManufacturerRepository manufacturerRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _manufacturerRepository = manufacturerRepository;
        }

        // GET /api/orders — đơn hàng của khách hàng hiện tại
        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                    return Unauthorized();

                var orders = await _orderRepository.GetByUserAsync(userGuid);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải đơn hàng: " + ex.Message });
            }
        }

        // GET /api/orders/stats — thống kê đơn hàng (Admin)
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStats()
        {
            var orders = (await _orderRepository.GetAllAsync()).ToList();
            var products = (await _productRepository.GetAllAsync()).ToList();
            var manufacturers = (await _manufacturerRepository.GetAllAsync()).ToList();

            return Ok(new
            {
                totalOrders = orders.Count,
                pendingOrders = orders.Count(o => o.Status == "Pending"),
                processingOrders = orders.Count(o => o.Status == "Processing"),
                completedOrders = orders.Count(o => o.Status == "Completed"),
                cancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                rejectedOrders = orders.Count(o => o.Status == "Rejected"),
                totalRevenue = orders.Where(o => o.Status == "Completed").Sum(o => (decimal?)o.TotalAmount) ?? 0m,
                totalProducts = products.Count,
                totalManufacturers = manufacturers.Count
            });
        }

        // GET /api/orders/all — tất cả đơn hàng (Admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders(
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var allOrders = await _orderRepository.GetAllAsync();

            IEnumerable<Order> filtered = allOrders.OrderByDescending(o => o.OrderDate);
            if (!string.IsNullOrEmpty(status))
                filtered = filtered.Where(o => o.Status == status);

            var totalCount = filtered.Count();
            var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // GET /api/orders/{id} — chi tiết đơn hàng kèm sản phẩm
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

                var details = await _orderDetailRepository.GetByOrderAsync(id);
                var detailsWithProduct = new List<object>();
                foreach (var d in details)
                {
                    var product = await _productRepository.GetByIdAsync(d.ProductId);
                    detailsWithProduct.Add(new
                    {
                        d.Id,
                        d.OrderId,
                        d.ProductId,
                        productName = product?.Name ?? "Sản phẩm đã xóa",
                        productImage = product?.ImageUrl ?? "",
                        d.Quantity,
                        d.UnitPrice
                    });
                }

                return Ok(new { order, details = detailsWithProduct });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message });
            }
        }

        // POST /api/orders — tạo đơn hàng mới (KHÔNG trừ kho, chờ admin duyệt)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in dto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Sản phẩm {item.ProductId} không tồn tại" });

                // Kiểm tra tồn kho (chưa trừ, chỉ kiểm tra để báo lỗi sớm)
                if (product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ tồn kho (còn {product.Stock})" });

                var effectivePrice = product.DiscountPercent > 0
                    ? product.Price * (1 - product.DiscountPercent / 100)
                    : product.Price;

                totalAmount += effectivePrice * item.Quantity;
                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = effectivePrice
                });
                // Không trừ kho ở đây — kho sẽ bị trừ khi admin Duyệt đơn
            }

            var order = new Order
            {
                UserId = userGuid,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Pending",
                ShippingAddress = dto.ShippingAddress ?? ""
            };

            await _orderRepository.AddAsync(order);

            foreach (var detail in orderDetails)
            {
                detail.OrderId = order.Id;
                await _orderDetailRepository.AddAsync(detail);
            }

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, new
            {
                order = new
                {
                    id = order.Id,
                    userId = order.UserId,
                    orderDate = order.OrderDate,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    shippingAddress = order.ShippingAddress
                },
                details = orderDetails.Select(d => new
                {
                    id = d.Id,
                    orderId = d.OrderId,
                    productId = d.ProductId,
                    quantity = d.Quantity,
                    unitPrice = d.UnitPrice
                })
            });
        }

        // PUT /api/orders/{id}/approve — Admin duyệt đơn (trừ kho)
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveOrder(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (order.Status != "Pending")
                return BadRequest(new { message = "Chỉ có thể duyệt đơn hàng đang chờ xử lý" });

            var details = await _orderDetailRepository.GetByOrderAsync(id);

            // Kiểm tra tồn kho trước khi duyệt
            foreach (var detail in details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product == null)
                    return BadRequest(new { message = "Sản phẩm trong đơn hàng không còn tồn tại" });

                if (product.Stock < detail.Quantity)
                    return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ tồn kho (còn {product.Stock}, cần {detail.Quantity})" });
            }

            // Trừ kho sau khi kiểm tra
            foreach (var detail in details)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                product!.Stock -= detail.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            order.Status = "Processing";
            await _orderRepository.UpdateAsync(order);

            return Ok(new { message = "Đã duyệt đơn hàng, kho đã được trừ", order });
        }

        // PUT /api/orders/{id}/reject — Admin từ chối đơn (không trừ kho)
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectOrder(int id, [FromBody] RejectOrderDto? dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (order.Status != "Pending")
                return BadRequest(new { message = "Chỉ có thể từ chối đơn hàng đang chờ xử lý" });

            order.Status = "Rejected";
            await _orderRepository.UpdateAsync(order);

            return Ok(new { message = "Đã từ chối đơn hàng", order });
        }

        // PUT /api/orders/{id}/status — Admin cập nhật trạng thái (Processing → Completed)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Chỉ cho phép chuyển Processing → Completed
            var validTransitions = new Dictionary<string, string>
            {
                { "Processing", "Completed" }
            };

            if (!validTransitions.TryGetValue(order.Status, out var allowed) || allowed != dto.Status)
                return BadRequest(new { message = $"Không thể chuyển trạng thái từ '{order.Status}' sang '{dto.Status}'" });

            order.Status = dto.Status;
            await _orderRepository.UpdateAsync(order);

            return Ok(order);
        }

        // PUT /api/orders/{id}/cancel — hủy đơn (chỉ hoàn kho nếu đã duyệt)
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Khách hàng chỉ hủy được đơn của mình
            if (!isAdmin && (!Guid.TryParse(userId, out var userGuid) || order.UserId != userGuid))
                return Forbid();

            if (order.Status == "Completed" || order.Status == "Cancelled" || order.Status == "Rejected")
                return BadRequest(new { message = $"Không thể hủy đơn hàng ở trạng thái '{order.Status}'" });

            // Chỉ hoàn lại kho nếu đơn đã được duyệt (Processing/Completed — kho đã bị trừ)
            if (order.Status == "Processing")
            {
                var details = await _orderDetailRepository.GetByOrderAsync(id);
                foreach (var detail in details)
                {
                    var product = await _productRepository.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }
            // Nếu Pending: chưa trừ kho nên không cần hoàn

            order.Status = "Cancelled";
            await _orderRepository.UpdateAsync(order);

            return Ok(new { message = "Đơn hàng đã được hủy", order });
        }
    }

    public class CreateOrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
        public string? ShippingAddress { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = "";
    }

    public class RejectOrderDto
    {
        public string? Reason { get; set; }
    }
}
