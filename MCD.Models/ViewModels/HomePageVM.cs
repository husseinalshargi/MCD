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
    }
}
