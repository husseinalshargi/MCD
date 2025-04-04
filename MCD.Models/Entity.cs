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
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public Document Document { get; set; }

        [ValidateNever]
        public string? EntityType { get; set; }

        [ValidateNever] //because it isn't entered by the user
        public string? EntityValue { get; set; }
    }
}
