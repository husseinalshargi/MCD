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

        public string SharedToEmail { get; set; }

        public string SharedFromId { get; set; }

        
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        [ValidateNever]
        public Document Document { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime SharedAt { get; set; }
    }
}
