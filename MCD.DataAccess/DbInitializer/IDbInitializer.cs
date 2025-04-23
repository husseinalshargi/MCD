using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.DbInitializer
{
    public interface IDbInitializer
    {
        void Initialize(); //this method will be used to initialize the database with default data when the application starts with a new database
    }
}
