using Jmgram_mk1.src.JMgram.Core.Entities;
using Jmgram_mk1.src.JMgram.Core.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Repositories
{
    // Интерфейс репозитория
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
        Task<List<AppIdentityUser?>> GetByIds(List<string> userIds);
        Task<UserProfile?> GetUserProfileById(string userId);

    }
    public class UserRepository : IUserRepository
    {
        private readonly JMgramDbContext _dbContext;

        public UserRepository(JMgramDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
                _dbContext.Users.Attach(user); //Прикрепляем user к контексту
                _dbContext.Entry(user).Property(x => x.Phone).IsModified = true; // говорим что меняем только Phone
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task Delete(AppIdentityUser user) // Исправлено: Реализован метод Delete(User)
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
        }
        public async Task<List<AppIdentityUser>> GetByPhones(List<string> phones)
        {
            return await _dbContext.Users.Where(u => phones.Contains(u.Phone)).ToListAsync();
        }
        public async Task<AppIdentityUser?> GetById(string id)
        {
            return await _dbContext.Users.Include(u => u.UserProfile).FirstOrDefaultAsync(u => u.Id == id.ToString());
        }
        public async Task<List<AppIdentityUser?>> GetByIds(List<string> userIds)
        {
            return await _dbContext.Users.Where(u => userIds.Contains(u.Id.ToString())).ToListAsync();
        }
        public async Task<UserProfile?> GetUserProfileById(string userId)
        {
            return await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
        }
        

    }
}
