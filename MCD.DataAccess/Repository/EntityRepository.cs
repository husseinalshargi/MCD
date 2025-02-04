using MCD.DataAccess.Data;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository
{
    public class EntityRepository : Repository<Entity>, IEntityRepository
    {
        private ApplicationDbContext _db;
        // dependency injection 
        public EntityRepository(ApplicationDbContext db) : base(db) // in order to pass the db context obj to the repository class and use it here
        {
            _db = db;
        }


        public void Update(Entity obj)
        {
            //to update the Entity
            _db.Entities.Update(obj);
        }
    }
}
