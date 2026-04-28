using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BaseCore.Repository.EFCore;
using BaseCore.Entities;

namespace BaseCore.LogService
{
    public interface ILogErrorService
    {
        Task<IEnumerable<LogError>> GetAllListAsync();
        Task CreateLog(HttpContext httpContext, string message);
    }

    public class LogErrorService : ILogErrorService
    {
        private readonly IRepository<LogError> _logErrorRepository;

        public LogErrorService(IRepository<LogError> logErrorRepository)
        {
            _logErrorRepository = logErrorRepository;
        }

        public async Task CreateLog(HttpContext httpContext, string message)
        {
            var requestBody = string.Empty;
            httpContext.Request.EnableBuffering();
            using (var reader = new StreamReader(httpContext.Request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            var pathUrl = string.Format("{0}://{1}{2}", httpContext.Request.Scheme, httpContext.Request.Host, httpContext.Request.Path);
            var logError = new LogError
            {
                Header = $"REQUEST HttpMethod: {httpContext.Request.Method}, Path: {pathUrl}, Content-Type: {httpContext.Request.ContentType}",
                Body = requestBody,
                CreatedUser = httpContext.User?.Identity?.Name ?? "System", 
                Message = message
            };

           await _logErrorRepository.AddAsync(logError);
        }

        public async Task<IEnumerable<LogError>> GetAllListAsync()
        {
            return await _logErrorRepository.GetAllAsync();
        }
    }
}
