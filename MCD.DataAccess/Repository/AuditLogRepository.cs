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
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        ApplicationDbContext _db;
        public AuditLogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
