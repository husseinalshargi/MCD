using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models.ViewModels
{
    public class MoreInfoVM
    {
        public Document Document { get; set; }
        public List<Category> CategoryList { get; set; }
        public bool isConverted { get; set; }
        public bool isSummarized { get; set; }
        public bool isExtracted { get; set; }
    }
}
