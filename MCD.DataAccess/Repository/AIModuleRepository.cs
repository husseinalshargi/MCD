using MCD.DataAccess.Data;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository
{
    public class AIModuleRepository : Repository<AIModule>, IAIModuleRepository
    {
        ApplicationDbContext _db;
        // dependency injection 
        public AIModuleRepository(ApplicationDbContext db) : base(db) // in order to pass the db context obj to the repository class and use it here
        {
            _db = db;
        }
    }
}
