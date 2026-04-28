using Microsoft.EntityFrameworkCore;
using BaseCore.Entities;

namespace BaseCore.Repository.EFCore
{
    public interface IManufacturerRepository : IRepository<Manufacturer>
    {
        Task<Manufacturer?> GetByNameAsync(string name);
    }

    public class ManufacturerRepository : Repository<Manufacturer>, IManufacturerRepository
    {
        public ManufacturerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Manufacturer?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower());
        }
    }
}
