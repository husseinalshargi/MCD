using MCD.DataAccess.Data;
using MCD.Models;
using MCD.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager; //to manage users (create the user that will be admin)
        private readonly RoleManager<IdentityRole> _roleManager; //to manage roles (make the user an admin)
        public DbInitializer(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            //add the migrations if they are not applied --> to update/create tables in the database automatically
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0) //there are migrations that were not applied using package manager
                {
                    _db.Database.Migrate(); //apply the migrations
                }
            }
            catch (Exception ex)
            {
                //log the error
                Console.WriteLine(ex);
            }

            //create roles and admin user if they are not created
            //rather than async type we add getawait, getresult
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult()) //to check if the roles exists in the roles table in the db
            {
                //if not then create all roles
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult(); //of type identity role with the name taken from SD file, also create async make the execution faster
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();


                //create admin user if it isn't there
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "MyCleverDoc@hotmail.com",
                    Email = "MyCleverDoc@hotmail.com",
                    FirstName = "MCD",
                    LastName = "Admin",
                    PhoneNumber = "0544061784",
                }, "1122Ab-").GetAwaiter().GetResult(); //create the user with the password

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "MyCleverDoc@hotmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Employee).GetAwaiter().GetResult(); //add the user to the admin role

            }

            return;


        }
    }
}
