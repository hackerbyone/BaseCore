using BaseCore.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCore.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetOrdersByUserIdAsync(System.Guid userId);
        Task<Order> GetOrderByIdAsync(int id);
    }
}
