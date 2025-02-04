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
    public class SummarizedDocumentRepository : Repository<SummarizedDocument>, ISummarizedDocumentRepository
    {
        private ApplicationDbContext _db;
        public SummarizedDocumentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(SummarizedDocument obj)
        {
            _db.SummarizedDocuments.Update(obj);
        }
    }
}
