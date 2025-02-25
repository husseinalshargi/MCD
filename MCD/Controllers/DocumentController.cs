using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MCD.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        public DocumentController (IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult MoreInfo(int? id)
        {
            if (!User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated 
            {
                return RedirectToAction("Privacy");
            }

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //in order to return the content of the file we should first retrieve the file it self
            Document document = _UnitOfWork.Document.Get(u => u.Id == id); //get the document with the document id that are passed in the documents page (more info)

            return View(document);
        }
    }
}
