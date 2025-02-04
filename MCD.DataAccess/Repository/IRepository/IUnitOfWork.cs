using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork //to make all repos operations in one place
    {
        //add here the repos interfaces
        ICategoryRepository Category { get; }
        IAIModuleRepository Module { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IAuditLogRepository AuditLog { get; }
        IDocumentRepository Document { get; }
        IEntityRepository Entity { get; }
        IExtractedDocumentRepository ExtractedDocument { get; }
        ISharedDocumentRepository SharedDocument { get; }

        void Save(); //as some all have the same concept
    }
}
