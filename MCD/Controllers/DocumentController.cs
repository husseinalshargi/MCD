using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using MCD.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
                return RedirectToAction("Privacy", "Home");
            }

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //in order to return the content of the file we should first retrieve the file it self
            Document document = _UnitOfWork.Document.Get(u => u.Id == id, includeProperties:"Category"); //get the document with the document id that are passed in the documents page (more info)

            return View(document);
        }

        //share the document action
        [HttpPost]
        public IActionResult UploadSharedDocument(int DocumentId, string SharedToEmail) //shared id -> user email
        {
            //first thing make sure that the user are authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Privacy", "Home"); //home controller
            }
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (string.IsNullOrEmpty(SharedToEmail))
            {
                TempData["ErrorMessage"] = "User with this email does not exist."; //there aren't a user with this email
                return RedirectToAction(nameof(MoreInfo)); //return the same view
            }


            var SharedToUser = _UnitOfWork.ApplicationUser.Get(u => u.Email.ToLower() == SharedToEmail.ToLower());
            if(SharedToUser == null)
            {
                TempData["ErrorMessage"] = "User with this email does not exist.";
                return RedirectToAction(nameof(MoreInfo));
            }

            // if the document is already shared with the same user
            var existingSharedDocument = _UnitOfWork.SharedDocument.Get(u => u.DocumentId == DocumentId && u.SharedFromId == SharedToUser.Id);
            if (existingSharedDocument != null)
            {
                TempData["ErrorMessage"] = "This document is already shared with this user.";
                return RedirectToAction("SharedDocuments", "Home");
            }



            _UnitOfWork.SharedDocument.Add(new SharedDocument()
            {
                SharedToEmail= SharedToUser.Email.ToLower(),
                SharedFromId =userId,
                DocumentId=DocumentId,
                SharedAt=DateTime.Now
            });
            _UnitOfWork.Save(); //save changes after adding the shared doc

            TempData["SuccessMessage"] = "Document shared successfully!";

            return RedirectToAction("SharedDocuments", "Home");
        }






        [HttpPost]
        public IActionResult AdjustDocument(int DocumentID, string CategoryName, string Title, string Category, string action)
        {
            //first thing make sure that the user are authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Privacy", "Home"); //home controller if user aren't authenticated
            }
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            //to get the document to do an action on it (from the submitted form)
            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentID);
            if (document == null) //if the document not found
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }

            // search for the category using its name so that don't create a new category if there is one, else create a new one
            var category = _UnitOfWork.Category.Get(u=> u.CategoryName.ToLower() == Category.ToLower() && u.ApplicationUserId == userId); //get the category of the user by its name by making them all lower cases
            //check if the category is null then create a new one with the user input
            int CategoryId; //to use it out of the if scope
            if (category == null)
            {
                Category newCategory = new Category()
                {
                    CategoryName = Category,
                    ApplicationUserId = userId
                };
                _UnitOfWork.Category.Add(newCategory);
                _UnitOfWork.Save();
                CategoryId = newCategory.Id;
            }
            else
            {
                CategoryId = category.Id;
            }// in all cases get the id to work with it




            //here we will check the action that the user want to do on the document and do using a switch
            switch (action)
            {
                case "Summarize": //summarizing logic
                    return RedirectToAction("MoreInfo"); 

                case "Delete": //delete the document
                    _UnitOfWork.Document.Remove(document);
                    _UnitOfWork.Save();
                    TempData["SuccessMessage"] = "Document deleted successfully!";
                    return RedirectToAction("Document", "Home");

                case "Update": //update the document
                    document.Title = Title;
                    document.CategoryId = CategoryId; //it will be automatically bounded to the category obj
                    document.UpdateDate = DateTime.Now;
                    _UnitOfWork.Save();
                    TempData["SuccessMessage"] = "Document updated successfully!";
                    return RedirectToAction("Document", "Home");

                default: //in case of something goes wrong
                    TempData["ErrorMessage"] = "Invalid action.";
                    break;
            }




            return RedirectToAction("MoreInfo"); 
        }




    }
}
