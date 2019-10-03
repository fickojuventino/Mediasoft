using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaSoft.Data.Models
{
    public class PasswordHasherWorker : IPasswordHasher<Worker>
    {
        public string HashPassword(Worker user, string password)
        {
            return password;
        }

        public PasswordVerificationResult VerifyHashedPassword(Worker user, string hashedPassword, string providedPassword)
        {
            if (hashedPassword == user.PasswordHash)
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
        }
    }
}
