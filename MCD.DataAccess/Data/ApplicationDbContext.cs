using Microsoft.EntityFrameworkCore;

namespace MCD.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)//just configeration
        {

        }
    }
}
