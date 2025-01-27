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
    public class AIModule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        [ValidateNever]
        public required Document Document { get; set; }

        [Required] //required -> with the user input
        [ValidateNever] //as iw will be set in the program
        public required string ModelName { get; set; } //required -> with the app 
    }
}
