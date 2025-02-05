using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models
{
    public class Document //after creating the table in the model we create an object in the data access -> application db context
    {
        //[--] -> validation
        //Document table content:
        // DocumentId is the key
        [Key]
        public int Id { get; set; }

        //identityuser id type is string not int
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

         //to be requierd in the data validation when entering the data
        [StringLength(50, MinimumLength = 1)] // the length of the string should be in between 1-50
        public string Title { get; set; }

         //to be requierd in the data validation when entering the data
        [StringLength(50, MinimumLength = 1)]
        public string FileName { get; set; } // to use in path (which will be in wwwroot)\ FileName.FileType

        
        public string FileType { get; set; } // PDF, JPG, txt, etc...

        
        [DataType(DataType.Date)] //to ensure that the type will be date
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")] //we need the zero to make it a placeholder to insert the datetime in the template
        public DateTime UploadDate { get; set; }

        
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime UpdateDate { get; set; } 

        public string AITaskStatus { get; set; } // processing processed etc..

        // to make the relation one-to-many -> document can have more than one shared instance
        //to remove cascade problem 
        public ICollection<SharedDocument> SharedDocuments { get; set; }
    }
}
