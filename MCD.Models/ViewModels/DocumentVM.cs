using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models.ViewModels
{
    public class DocumentVM
    {
        public List<Document> documents { get; set; }
        public IFormFile DocumentFile { get; set; } // to input new file (contains file info name, content type, length, data
        public bool Summarize { get; set; } 
    }
}
