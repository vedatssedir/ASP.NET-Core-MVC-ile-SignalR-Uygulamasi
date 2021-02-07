using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    public class DataContext:DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Team> Teams { get; set; }
    }
}
