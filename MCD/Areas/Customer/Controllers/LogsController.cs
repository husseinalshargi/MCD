using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using MCD.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MCD.Areas.Customer.Controllers
{
    [Area("Customer")] //to specify the area of the controller
    [Authorize]        //to make sure that the user is authenticated before accessing the controller
    public class LogsController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public LogsController( IUnitOfWork unitOfWork)
        { 
            _UnitOfWork = unitOfWork;
        }

        public IActionResult AuditLogs()
        {
            //check if the user is logged in
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return RedirectToAction("privacy", "Home");
            }
            return View();
        }


        //to use datatables api for dealing with the tables in our page
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<AuditLog> auditLogList = _UnitOfWork.AuditLog.GetAll(u=>u.ApplicationUserId == userId).ToList(); //to get logs of the user that are logged in

            return Json(new { data = auditLogList });
        }
        #endregion


    }
}
