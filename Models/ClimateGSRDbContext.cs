using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace ClimateGSR.Models
{
    /// <summary>
    /// Entity Framework database context for the ClimateGSR application, managing ASP.NET Identity
    /// user accounts as well as Country and Continent datasets.
    /// </summary>
    public class ClimateGSRDbContext : IdentityDbContext<ApplicationUser>
    {
        static ClimateGSRDbContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ClimateGSRDbContext, Migrations.Configuration>(useSuppliedContext: true));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClimateGSRDbContext"/> class using the "DefaultConnection" connection string.
        /// Configures the custom <see cref="DatabaseInitialiser"/> for database seeding.
        /// </summary>
        public ClimateGSRDbContext() : this("DefaultConnection")
        {
        }

        public ClimateGSRDbContext(string connectionString) : base(connectionString, throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Factory method used by OWIN context per-request instantiation.
        /// </summary>
        /// <returns>A new <see cref="ClimateGSRDbContext"/> instance.</returns>
        public static ClimateGSRDbContext Create()
        {
            return new ClimateGSRDbContext();
        }

        /// <summary>
        /// Gets or sets the database set of Country entities.
        /// </summary>
        public DbSet<Country> Countries { get; set; }

        /// <summary>
        /// Gets or sets the database set of Continent entities.
        /// </summary>
        public DbSet<Continent> Continents { get; set; }
        public DbSet<Member> Members { get; set; }

    }
}
