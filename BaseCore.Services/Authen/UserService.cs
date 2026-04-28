using BaseCore.Common;
using BaseCore.Entities;
using BaseCore.Repository.EFCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCore.Services.Authen
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<List<User>> GetAll();
        Task<User> GetById(string id);
        Task<User> GetByUsername(string username);
        Task<User> Create(User user, string password);
        Task Update(User user, string password = null);
        Task Delete(string id);
        Task<(List<User> Users, int TotalCount)> Search(string keyword, int page, int pageSize);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await _userRepository.GetByUsernameAsync(username);

            // check if username exists
            if (user == null)
            {
                Console.WriteLine($"Authenticate: user not found for username '{username}'");
                return null;
            }

            // Debug info: log user's active status and salt/hash presence (no password in logs)
            Console.WriteLine($"Authenticate: found user='{user.UserName}', IsActive={user.IsActive}, SaltPresent={(user.Salt != null && user.Salt.Length>0)}, PasswordLength={(user.Password==null?0:user.Password.Length)}");

            // verify password using hash or plain text
            bool isValidPassword = false;

            if (user.Salt != null && user.Salt.Length > 0)
            {
                // Hashed password
                isValidPassword = TokenHelper.IsValidPassword(password, user.Salt, user.Password);
            }
            else
            {
                // Plain text password (for seeded/legacy users)
                isValidPassword = (user.Password == password);
            }

            if (!isValidPassword)
            {
                Console.WriteLine($"Password verification failed for user: {username}");
                return null;
            }

            Console.WriteLine($"User authenticated successfully: {username}");

            // authentication successful
            return user;
        }

        public async Task<List<User>> GetAll()
        {
            var items = await _userRepository.GetAllAsync();
            return items.ToList();
        }

        public async Task<User> GetById(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetByUsername(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User> Create(User user, string password)
        {
            // Hash password with salt
            byte[] salt;
            user.Password = TokenHelper.HashPassword(password, out salt);
            user.Salt = salt;

            user.Created = DateTime.Now;
            user.IsActive = true;
            // Ensure non-nullable DB columns have defaults to avoid SQL errors
            user.Contact ??= string.Empty;
            user.Email ??= string.Empty;
            user.Phone ??= string.Empty;
            user.Position ??= string.Empty;
            user.Image ??= string.Empty;

            await _userRepository.CreateAsync(user);
            return user;
        }

        public async Task Update(User user, string password = null)
        {
            if (!string.IsNullOrEmpty(password))
            {
                byte[] salt;
                user.Password = TokenHelper.HashPassword(password, out salt);
                user.Salt = salt;
            }
            await _userRepository.UpdateAsync(user);
        }

        public async Task Delete(string id)
        {
            await _userRepository.DeleteByIdAsync(id);
        }

        public async Task<(List<User> Users, int TotalCount)> Search(string keyword, int page, int pageSize)
        {
            return await _userRepository.SearchAsync(keyword, page, pageSize);
        }
    }
}
