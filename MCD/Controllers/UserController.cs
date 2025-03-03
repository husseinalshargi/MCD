using MCD.DataAccess.Data;
using MCD.DataAccess.Repository;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MCD.Controllers
{
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _userManager = userManager; //in order to change roles easily
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangeRole(string userId) 
        {
            var userToChangeRole = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            var roleId = _db.UserRoles.FirstOrDefault(u=> u.UserId ==userId).RoleId;//to get the user current role id
            string roleName = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name; //to get the role name of the user role

            if (roleName == SD.Role_Customer) //if he was customer make him employee
            {
                //first thing remove the user from the customer role
                _userManager.RemoveFromRoleAsync(userToChangeRole, SD.Role_Customer).GetAwaiter().GetResult();
                //now add him to the employee role
                _userManager.AddToRoleAsync(userToChangeRole, SD.Role_Employee).GetAwaiter().GetResult();
            }
            else //if he was employee make him customer
            {
                //first thing remove the user from the emplyee role
                _userManager.RemoveFromRoleAsync(userToChangeRole, SD.Role_Employee).GetAwaiter().GetResult();
                //now add him to the customer role
                _userManager.AddToRoleAsync(userToChangeRole, SD.Role_Customer).GetAwaiter().GetResult();
            }
                return RedirectToAction("Index");
        }














        //to use datatables api for dealing with the tables in our page
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            //list of all users
            List<ApplicationUser> applicationUsers = _unitOfWork.ApplicationUser.GetAll().ToList();

            //all table names are in SSMS (database)
            //in order to get the user role id (the table are created by default in the identity db not UOW)
            var userRoles = _db.UserRoles.ToList(); //get all user roles (table with the user id and the role id)
            var roles = _db.Roles.ToList(); //get all the roles (table with the role id and the role name)

            //to get the role name of each user
            foreach (var user in applicationUsers)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; //as we do not want to call it from the db each time iterating in the loop
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name; //in order to place it in the user object to be shown in the table without placing it in the db (defined in the ApplicationUser model)
            }



            return Json(new { data = applicationUsers });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id) //in order to use the column in application user (created by default) for locking a user
        { //from body -> from the table
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now) //using the same button in the page
            {
                //user is currently locked and we need to unlock him
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000); //locked for the next 1000 years
            }
            _unitOfWork.ApplicationUser.Update(objFromDb); //after setting the lock date
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
