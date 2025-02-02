using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository: IRepository<Category> // T -> Category, to implement all common CRUD operations with added ones
    {
        void Update(Category obj);
    }
}
