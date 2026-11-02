using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ClimateGSR.Models
{
    /// <summary>
    /// Application user entity representing an authenticated user, extending ASP.NET Identity's <see cref="IdentityUser"/>.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Keep the original score column so upgrading older databases cannot drop it.
        // New game scores use Member.Highscore; the leaderboard honours both versions.
        public int HighScore { get; set; }

        /// <summary>
        /// User's first name.
        /// </summary>
        [StringLength(100)] public string FirstName { get; set; }

        /// <summary>
        /// User's last name or surname.
        /// </summary>
        [StringLength(100)] public string LastName { get; set; }

        /// <summary>
        /// Generates the <see cref="ClaimsIdentity"/> representing the user for cookie-based authentication,
        /// injecting custom identity claims for first name and last name.
        /// </summary>
        /// <param name="manager">The user manager instance.</param>
        /// <returns>A task representing the asynchronous operation with the created ClaimsIdentity.</returns>
        public string RegisteredAt { get; set; }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            if (!string.IsNullOrEmpty(FirstName))
            {
                userIdentity.AddClaim(new Claim("FirstName", FirstName));
                userIdentity.AddClaim(new Claim(ClaimTypes.GivenName, FirstName));
            }

            if (!string.IsNullOrEmpty(LastName))
            {
                userIdentity.AddClaim(new Claim("LastName", LastName));
                userIdentity.AddClaim(new Claim(ClaimTypes.Surname, LastName));
            }

            return userIdentity;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IIdentity"/> to retrieve custom user claims like first name and last name.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Retrieves the user's first name claim from the current identity, falling back to the username if unavailable.
        /// </summary>
        /// <param name="identity">The current user identity.</param>
        /// <returns>The first name string or username fallback.</returns>
        public static string GetFirstName(this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var claim = claimsIdentity.FindFirst("FirstName") ?? claimsIdentity.FindFirst(ClaimTypes.GivenName);
                if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }

            return identity != null ? identity.GetUserName() : string.Empty;
        }

        /// <summary>
        /// Retrieves the user's last name claim from the current identity.
        /// </summary>
        /// <param name="identity">The current user identity.</param>
        /// <returns>The last name string, or empty if not present.</returns>
        public static string GetLastName(this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var claim = claimsIdentity.FindFirst("LastName") ?? claimsIdentity.FindFirst(ClaimTypes.Surname);
                if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }

            return string.Empty;
        }
    }
}
