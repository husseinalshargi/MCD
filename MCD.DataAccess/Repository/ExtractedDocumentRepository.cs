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
    public class ExtractedDocumentRepository : Repository<ExtractedDocument>, IExtractedDocumentRepository
    {
        private ApplicationDbContext _db;
        public ExtractedDocumentRepository (ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(ExtractedDocument obj)
        {
            _db.ExtractedDocuments.Update(obj);
        }
    }
}
