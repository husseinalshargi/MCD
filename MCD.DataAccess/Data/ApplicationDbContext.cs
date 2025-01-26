using Microsoft.EntityFrameworkCore;
using MCD.Models;

namespace MCD.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)//just configeration
        {

        }
    }
}
