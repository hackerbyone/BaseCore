using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BaseCore.Repository.EFCore;
using BaseCore.Entities;

namespace BaseCore.LogService
{
    public interface ILogActionService
    {
        Task<IEnumerable<LogAction>> GetAllListAsync();
        Task CreateLog(LogAction logAction);
    }

    public class LogActionService : ILogActionService
    {
        private readonly IRepository<LogAction> _logActionRepository;

        public LogActionService(IRepository<LogAction> logActionRepository)
        {
            _logActionRepository = logActionRepository;
        }

        public async Task<IEnumerable<LogAction>> GetAllListAsync()
        {
            return await _logActionRepository.GetAllAsync();
        }

        public async Task CreateLog(LogAction logAction)
        {
            await _logActionRepository.AddAsync(logAction);
        }
    }
}
