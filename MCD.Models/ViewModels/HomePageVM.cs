using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models.ViewModels
{
    public class HomePageVM
    {
        public List<Document> RecentDocuments { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalSharedDocuments { get; set; }
        public int TotalSharedWithDocuments { get; set; }
        public List<SharedDocument> RecentSharedDocuments { get; set; }
        public List<AuditLog> auditLogs { get; set; }
    }
}
