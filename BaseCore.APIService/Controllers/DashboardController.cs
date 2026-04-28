using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;

namespace BaseCore.APIService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IRepository<User> _userRepository;

        public DashboardController(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,
            IRepository<User> userRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        // GET /api/dashboard/stats — thống kê tổng hợp cho dashboard
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();

                var totalProducts = products.Count();
                var totalCategories = categories.Count();

                if (!User.IsInRole("Admin"))
                {
                    return Ok(new
                    {
                        totalProducts,
                        totalCategories,
                        totalUsers = (int?)null,
                        totalOrders = (int?)null,
                        pendingOrders = (int?)null,
                        processingOrders = (int?)null,
                        completedOrders = (int?)null,
                        totalRevenue = (decimal?)null
                    });
                }

                var orders = (await _orderRepository.GetAllAsync()).ToList();
                var totalUsers = (await _userRepository.GetAllAsync()).Count();

                return Ok(new
                {
                    totalProducts,
                    totalCategories,
                    totalUsers,
                    totalOrders = orders.Count,
                    pendingOrders = orders.Count(o => o.Status == "Pending"),
                    processingOrders = orders.Count(o => o.Status == "Processing"),
                    completedOrders = orders.Count(o => o.Status == "Completed"),
                    totalRevenue = orders
                        .Where(o => o.Status == "Completed")
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0m
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải thống kê: " + ex.Message });
            }
        }
    }
}
