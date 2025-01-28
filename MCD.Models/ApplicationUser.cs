using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Models
{
    public class ApplicationUser : IdentityUser //in order to extend the properties of the default user table
    {
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.identityuser?view=aspnetcore-9.0
        // for the existing properties
        //Id, UserName, Email, PhoneNumber, password is already in the identityuser class
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        

    }
}
