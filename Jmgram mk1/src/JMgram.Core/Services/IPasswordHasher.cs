using Microsoft.AspNetCore.Identity;

namespace Jmgram_mk1.src.JMgram.Core.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword);
    }

    public class AspNetCorePasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password); // Первый аргумент null, т.к. не нужен User object
        }

        public PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword)
        {
            return _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword); // Первый аргумент null, т.к. не нужен User object
        }
    }
}
