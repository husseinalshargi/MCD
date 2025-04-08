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
        public int TotalDocuments { get; set; } // to show the number of documents in the view
        public int pdfCount { get; set; } // to show the number of pdf documents in the view
        public int imageCount { get; set; } // to show the number of image documents in the view
        public int textCount { get; set; } // to show the number of text documents in the view
    }
}
