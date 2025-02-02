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
    public class CategoryRepository : Repository<Category>, ICategoryRepository // implement from repository so that that we don't duplicate the methods as there is the common ones
    {
        ApplicationDbContext _db;
        // dependency injection 
        public CategoryRepository(ApplicationDbContext db) : base(db) // in order to pass the db context obj to the repository class and use it here
        {
            _db = db;
        }


        public void Update(Category obj)
        {
            //to update the category
            _db.Categories.Update(obj);
        }
    }
}
