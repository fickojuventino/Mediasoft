using MediaSoft.Data.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MediaSoft.Data.Models
{
    public class RadnikStore : IUserPasswordStore<Radnik>, IUserEmailStore<Radnik>
    {
        private readonly RadnikContext _context;

        public RadnikStore(RadnikContext context)
        {
            _context = context;
        }
        public void Dispose()
        {
        }

        public async Task<IdentityResult> CreateAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            _context.Add(radnik);
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            return affectedRows > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = $"Could not create radnik {radnik.Korisnicko_ime}." });
        }

        public async Task<IdentityResult> DeleteAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            var radnikFromDb = await _context.Korisnici.FindAsync(radnik.Korisnicko_ime);
            _context.Remove(radnikFromDb);
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            return affectedRows > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = $"Could not delete radnik {radnik.Korisnicko_ime}." });
        }

        public async Task<Radnik> FindByIdAsync(string korisnickoIme, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.Korisnici.SingleOrDefaultAsync(u => u.Korisnicko_ime.Equals(korisnickoIme), cancellationToken);
        }

        public async Task<Radnik> FindByNameAsync(string normalizedUserName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await _context.Korisnici.SingleOrDefaultAsync(u => u.Korisnicko_ime.Equals(normalizedUserName.ToLower()),
                cancellationToken);
            return result;
        }

        public Task<string> GetNormalizedUserNameAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Korisnicko_ime);
        }

        public Task<string> GetUserIdAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Korisnicko_ime.ToString());
        }

        public Task<string> GetUserNameAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Korisnicko_ime);
        }

        public Task SetNormalizedUserNameAsync(Radnik radnik, string normalizedName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>(null);
        }

        public Task SetUserNameAsync(Radnik radnik, string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            radnik.Korisnicko_ime = userName;
            return Task.FromResult<object>(null);
        }

        public async Task<IdentityResult> UpdateAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            _context.Update(radnik);
            var affectedRows = await _context.SaveChangesAsync(cancellationToken);
            return affectedRows > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError() { Description = $"Could not update radnik {radnik.Korisnicko_ime}." });
        }

        public Task<string> GetPasswordHashAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Lozinka);
        }

        public Task<bool> HasPasswordAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(!string.IsNullOrWhiteSpace(radnik.Lozinka));
        }

        public Task SetPasswordHashAsync(Radnik radnik, string passwordHash, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            radnik.Lozinka = passwordHash;
            return Task.FromResult<object>(null);
        }

        public async Task<Radnik> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.Korisnici.SingleOrDefaultAsync(u => u.Korisnicko_ime.Equals(normalizedEmail),
                cancellationToken);
        }

        public Task<string> GetEmailAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Korisnicko_ime);
        }

        public Task<bool> GetEmailConfirmedAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<string> GetNormalizedEmailAsync(Radnik radnik, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            return Task.FromResult(radnik.Korisnicko_ime);
        }

        public Task SetEmailAsync(Radnik radnik, string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radnik == null) throw new ArgumentNullException(nameof(radnik));
            //radnik.Username = email;
            return Task.FromResult<object>(null);
        }

        public Task SetEmailConfirmedAsync(Radnik radnik, bool confirmed, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>(null);
        }

        public Task SetNormalizedEmailAsync(Radnik radnik, string normalizedEmail,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>(null);
        }

        public Task<IList<Claim>> GetClaimsAsync(Radnik user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimsAsync(Radnik user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(Radnik user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(Radnik user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Radnik>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
