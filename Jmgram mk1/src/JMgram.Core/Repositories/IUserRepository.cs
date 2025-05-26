using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    public interface IUserRepository
    {
        Task<bool> IsPhoneTaken(string phone);

        Task Add(AppIdentityUser user);
        Task CreateUserProfile(UserProfile userProfile);
        Task UpdateProfile(UserProfile profile);
        Task Update(AppIdentityUser user);
        Task Delete(AppIdentityUser user);
        Task<AppIdentityUser?> GetById(string id);
        Task<List<AppIdentityUser>> GetByPhones(List<string> phones);
        Task<UserProfile?> GetUserProfileById(string userId);
        Task<AppIdentityUser?> GetUserByPhoneNumber(string phoneNumber);
        Task<List<Contact>> GetContactsForUser(string userId);
        Task<string> GetUserFirstNameById(string userId);
        Task<string?> GetUserIdAsync(ClaimsPrincipal principal);
    }
    public class UserRepository : IUserRepository
    {
        private readonly JMgramDbContext _dbContext;
        private readonly IChatRepository _chatRepository;
        private readonly UserManager<AppIdentityUser> _userManager;

        public UserRepository(JMgramDbContext dbContext, IChatRepository chatRepository, UserManager<AppIdentityUser> userManager)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _chatRepository = chatRepository;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<bool> IsPhoneTaken(string phone)
        {
            return await _dbContext.Users.AnyAsync(u => u.Phone == phone);
        }


        public async Task Add(AppIdentityUser user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        public async Task CreateUserProfile(UserProfile userProfile)
        {
            _dbContext.UserProfiles.Add(userProfile);
            await _dbContext.SaveChangesAsync();
        }
        public async Task Update(AppIdentityUser user)
        {
            if (user != null)
            {
                _dbContext.Users.Attach(user);
                _dbContext.Entry(user).Property(x => x.Phone).IsModified = true;
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task Delete(AppIdentityUser user)
        {
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task UpdateProfile(UserProfile profile)
        {
            _dbContext.UserProfiles.Update(profile);
            await _dbContext.SaveChangesAsync();
            await _chatRepository.UpdateChatNamesForUser(profile.UserId, profile.FirstName);
        }
        public async Task<List<AppIdentityUser>> GetByPhones(List<string> phones)
        {
            return await _dbContext.Users.Where(u => phones.Contains(u.Phone)).ToListAsync();
        }
        public async Task<AppIdentityUser?> GetById(string id)
        {
            return await _dbContext.Users.Include(u => u.UserProfile).FirstOrDefaultAsync(u => u.Id == id.ToString());
        }
        public async Task<string?> GetUserIdAsync(ClaimsPrincipal principal)
        {
            return _userManager.GetUserId(principal);
        }
        public async Task<UserProfile?> GetUserProfileById(string userId)
        {
            return await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        public async Task<AppIdentityUser?> GetUserByPhoneNumber(string phoneNumber)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }
        public async Task<List<Contact>> GetContactsForUser(string userId)
        {
            return await _dbContext.Contacts
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        public async Task<string> GetUserFirstNameById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            var userFirstName = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.FirstName)
                .FirstOrDefaultAsync();
            return userFirstName;
        }
    }
}
