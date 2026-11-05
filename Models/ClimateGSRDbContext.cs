using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace ClimateGSR.Models
{
    /// <summary>
    /// Entity Framework database context for the ClimateGSR application, managing ASP.NET Identity
    /// user accounts as well as Country and Continent datasets.
    /// </summary>
    public class ClimateGSRDbContext : IdentityDbContext<ApplicationUser>
    {
        public ClimateGSRDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        /// <summary>
        /// Factory method used by OWIN context per-request instantiation.
        /// </summary>
        public static ClimateGSRDbContext Create()
        {
            return new ClimateGSRDbContext();
        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Continent> Continents { get; set; }

        public DbSet<Member> Members { get; set; }
    }

    /// <summary>
    /// Custom database initializer that recreates the database and runs CSV seeding.
    /// </summary>
    public class ClimateDbInitializer : CreateDatabaseIfNotExists<ClimateGSRDbContext>
    {
        protected override void Seed(ClimateGSRDbContext context)
        {
            DatabaseInitialiser.Seed(context);
            base.Seed(context);
        }
    }
}