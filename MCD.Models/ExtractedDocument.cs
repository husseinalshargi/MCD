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
    public class ExtractedDocument
    {
        // ExtractedDocumentId is the key -> it will replace the name by default
        [Key]
        public int Id { get; set; }

        // add document foregin key 
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        [ValidateNever] //because it isn't entered by the user
        public Document Document { get; set; } //for adding a forgein key we need to connect it with another table 

        [Required]
        public required string ExtractedFileName { get; set; }

    }
}
