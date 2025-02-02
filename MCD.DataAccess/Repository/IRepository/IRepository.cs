using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class //here are the methods in the basic crud operations (without implementation)
        //for more details we could create an interface also a class of the class (table) that needs further adjustments 
    {
        //CRUD:

        //create: 
        void Add(T entity);


        //retrieve:  

        //when it is implemented it will be in T class (could be any class)
        // T -> any class (any table in the db) to preform CRUD operations
        //return all objects
        IEnumerable<T> GetAll(); // return type is a collection of objects of the type T -> to get all objects (instances of a T table)
        // IEnumerable is the best way of iterating over a collection without modifying it Note: no indexing, use id
        // get only one item -> in case you want to edit or delete it so no i enumerable
        T Get(Expression<Func<T, bool>> filter); // Func<T, bool> is a function that takes an object of type T and returns true or false
        // in order to use lambda in it 
        // expression is a wrapper that translate the function into SQL queries (in EF framework) best way to deal with db


        //update: if we wanted to do so but it is not included because each class have its way of updating also not all fields will be updated
        // so it will be defined when it is required in the ClassNameRepository with the save method
        //void Update(T entity);

        //delete:
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity); //to delete a range of values also IEnumerable is used here as it is the best collection for db items


    }
}
