using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser> // T -> ApplicationUser
    {
        void Update(ApplicationUser obj); //as we will need to update user information
    }
}
