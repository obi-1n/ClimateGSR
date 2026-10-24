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
    //
    public class ClimateGSRDbContext:IdentityDbContext<ApplicationUser>
    {

        public ClimateGSRDbContext() : base("DefaultConnection", throwIfV1Schema: false) 
        {
            Database.SetInitializer(new DatabaseInitialiser());
        
        }

        public static ClimateGSRDbContext Create()
        {
            return new ClimateGSRDbContext();
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Continent> Continents { get; set; }

        public DbSet<Member> Members { get; set; }

    }
}