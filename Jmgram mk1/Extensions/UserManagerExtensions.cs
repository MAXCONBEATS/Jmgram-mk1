using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Jmgram_mk1.src.JMgram.Core.Entities;
using System;

public static class UserManagerExtensions
{
    public static async Task<AppIdentityUser?> FindByPhoneNumberAsync(this UserManager<AppIdentityUser> userManager, string phoneNumber)
    {
        return await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }
}