using MediaSoft.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MediaSoft
{
    public class CustomPasswordHasher : IPasswordHasher<Radnik>
    {
        public string HashPassword(Radnik radnik, string password)
        {
            return password;
        }

        public PasswordVerificationResult VerifyHashedPassword(Radnik radnik, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == radnik.Lozinka)
            {
                return PasswordVerificationResult.Success;
            }
            else {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}