using MCD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface IAIModuleRepository : IRepository<AIModule> // T -> AIModule
    {
        //we won't need update as it is only for doing the module work and to save logs
    }
}
