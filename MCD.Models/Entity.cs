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
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        // add document foregin key 
        public int DocumentId { get; set; }
        [ForeignKey("DocumentId")]
        [ValidateNever] //because it isn't entered by the user
        public required Document Document { get; set; } //for adding a forgein key we need to connect it with another table  then use it in f key annotation

        [ValidateNever] //because it isn't entered by the user
        [StringLength(30, MinimumLength = 1)] // the length of the string should be in between 1-30
        public string? EntityType { get; set; }

        [ValidateNever] //because it isn't entered by the user
        public required string EntityValue { get; set; }

        [ValidateNever]
        public int StartPosition { get; set; }

        [ValidateNever]
        public int EndPosition { get; set; }
    }
}
