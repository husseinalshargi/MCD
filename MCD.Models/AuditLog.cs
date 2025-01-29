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
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        //public int UserId { get; set; }
        //[ForeignKey("UserId")]
        //[ValidateNever]
        //public User User { get; set; }

        [Required]
        public string? Action { get; set; }

        [Required]
        [DataType(DataType.Date)] //to ensure that the type will be date
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime ActionDate { get; set; }

    }
}
