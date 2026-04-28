using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCore.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;
        private readonly IRepository<Product> _productRepository;

        public OrderService(
            IRepository<Order> orderRepository, 
            IRepository<OrderDetail> orderDetailRepository,
            IRepository<Product> productRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Pending";

            await _orderRepository.AddAsync(order);
            return order;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            var orders = await _orderRepository.FindAsync(o => o.UserId == userId);
            var orderList = orders.OrderByDescending(o => o.OrderDate).ToList();

            // Load order details and products
            foreach (var order in orderList)
            {
                var details = await _orderDetailRepository.FindAsync(od => od.OrderId == order.Id);
                order.OrderDetails = details.ToList();

                foreach (var detail in order.OrderDetails)
                {
                    detail.Product = await _productRepository.GetByIdAsync(detail.ProductId);
                }
            }

            return orderList;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order != null)
            {
                var details = await _orderDetailRepository.FindAsync(od => od.OrderId == order.Id);
                order.OrderDetails = details.ToList();

                foreach (var detail in order.OrderDetails)
                {
                    detail.Product = await _productRepository.GetByIdAsync(detail.ProductId);
                }
            }

            return order;
        }
    }
}
