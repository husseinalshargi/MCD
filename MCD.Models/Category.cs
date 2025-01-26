using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models
{
    public class Category
    {
        [Key] //primary key
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public required string CategoryName { get; set; }

    }
}
