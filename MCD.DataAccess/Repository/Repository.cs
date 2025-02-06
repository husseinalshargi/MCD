using MCD.DataAccess.Data;
using MCD.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T: class //Repository<T> → A generic class that works with any data type T, then where... to ensure that T always is a reference type 
    {
        //always do not forget to put _db.SaveChanges() after doing something with the db values
        // in order to do something with the db we must do dependency injection:allows objects to receive their dependencies from an external source instead of creating them internally
        // rather than always creating db object for dealing with db (more than one time which will put more coupling) do it one time and pass the reference

        private readonly ApplicationDbContext _db; //create an object of the class responsible of dealing with db
        internal DbSet<T> dbSet; // as we can't use T when dealing with objects in the table (it doesn't know that we want that) so we will use an alternative to that which is creating a set of T
        public Repository(ApplicationDbContext db) //assign the db object
        {
            _db = db;
            this.dbSet = _db.Set<T>(); // after creating the dbSet here we will put the type that we need to preform the operation on
            // example:
            // _db.Table == dbSet as we refiring to the table with T
        }


        public void Add(T entity)
        {
            //after creating the dbSet object 
            dbSet.Add(entity); // to add an instance
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet; //return all the queries and make it a quarriable object as we are getting one of it
            query = query.Where(filter); // all the objects that are filtered will be here
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeprop in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }
            return query.FirstOrDefault(); // first instance in the filter

        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null) //if there is a lambda function 
            {
                query = query.Where(filter); //to search using the filter (lambda function)
            }

            if (!string.IsNullOrEmpty(includeProperties)) // a property that are in another table but linked using foreign key
            {
                foreach (var includeprop in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeprop);
                }
            }

            return query.ToList(); 
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity); // RemoveRange expects an IEnumerable object
        }
    }
}
