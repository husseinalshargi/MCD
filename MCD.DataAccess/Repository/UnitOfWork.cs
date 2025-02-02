using MCD.DataAccess.Data;
using MCD.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            //here the implementation of the repos in order to use this class for all of them
            Category = new CategoryRepository(_db);
        }


        public void Save()
        {
            _db.SaveChanges(); 
        }
    }
}
