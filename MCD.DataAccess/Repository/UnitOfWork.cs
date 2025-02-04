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
        public IAIModuleRepository Module { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IAuditLogRepository AuditLog { get; private set; }
        public IDocumentRepository Document { get; private set; }
        public IEntityRepository Entity { get; private set; }
        public IExtractedDocumentRepository ExtractedDocument { get; private set; }


        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            //here the implementation of the repos in order to use this class for all of them
            Category = new CategoryRepository(_db);
            Module = new AIModuleRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            AuditLog = new AuditLogRepository(_db);
            Document = new DocumentRepository(_db);
            Entity = new EntityRepository(_db);
            ExtractedDocument = new ExtractedDocumentRepository(_db);
        }


        public void Save()
        {
            _db.SaveChanges(); 
        }
    }
}
