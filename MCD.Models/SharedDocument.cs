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
    public class SharedDocument
    {
        [Key]
        public int Id { get; set; }

        //uncomment after adding users table
        //[Required]
        //public int UserId { get; set; }
        //[Required]
        //[ForeignKey("UserId")]
        //public User User { get; set; }

        [ValidateNever]
        public int DocumentId { get; set; }
        [ValidateNever]
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime SharedAt { get; set; }
    }
}
