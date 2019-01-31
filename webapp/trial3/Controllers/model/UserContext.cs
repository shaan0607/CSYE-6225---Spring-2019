using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace trial3.Controller.Model{
    public class UserContext: DbContext{
        public DbSet<User> User {get; set;
        
        }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseMySQL("Filename = ./user.db");
    }
    }
}
