using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProLoginprofile.Models.Entities;

namespace ProLoginprofile.Models
{
    public class AppDBcontext : DbContext
    {
        public AppDBcontext(DbContextOptions<AppDBcontext>options):base(options)
        {

        }
        public DbSet<Users> users { get; set; }
        public DbSet<Programs> programs { get; set; }
        public DbSet<Programs_Users> programs_users { get; set; }
    }
}