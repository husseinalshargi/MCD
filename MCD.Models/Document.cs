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
        //the document id is the key
        [Key]
        public int Id { get; set; }
        
        //to add a foregin key (uncomment after adding all tables...)
        public int UserId { get; set; }
        //[ForeignKey("UserId")]
        //public User user { get; set; }

        public int CategoryId { get; set; }
        //[ForeignKey("CategoryId")]
        //public Category category { get; set; }

        [Required] //to be requierd in the data validation when entering the data
        [Range(1,50)]
        public required string Title { get; set; }

        [Required] //to be requierd in the data validation when entering the data
        [Range(1, 50)]
        public required string FileName { get; set; } // to use in path (which will be in wwwroot)\ FileName.FileType

        [Required]
        public required string FileType { get; set; } // PDF, JPG, txt, etc...

        [Required]
        [DataType(DataType.Date)] //to ensure that the type will be date
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")] //we need the zero to make it a placeholder to insert the datetime in the template
        public required DateTime UploadDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public required DateTime UpdateDate { get; set; } 

        [Required] 
        public required string AITaskStatus { get; set; } // processing processed etc..
    }
}
