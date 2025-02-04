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
    public class SharedDocumentRepository : Repository<SharedDocument>, ISharedDocumentRepository
    {
        private ApplicationDbContext _db;
        public SharedDocumentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(SharedDocument obj)
        {
            _db.SharedDocuments.Update(obj);
        }
    }
}
