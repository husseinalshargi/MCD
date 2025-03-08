using Google.Apis.Drive.v3;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using MCD.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace MCD.Areas.Customer.Controllers
{
    [Area("Customer")] //to specify the area of the controller
    [Authorize]        //to make sure that the user is authenticated before accessing the controller
    public class DocumentController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly GoogleDriveService _GoogleDriveService;
        public DocumentController (IUnitOfWork unitOfWork, GoogleDriveService googleDriveService)
        {
            _UnitOfWork = unitOfWork;
            _GoogleDriveService = googleDriveService;
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
            Document document = _UnitOfWork.Document.Get(u => u.Id == id, includeProperties:"Category,ApplicationUser"); //get the document with the document id that are passed in the documents page (more info)

            MoreInfoVM moreInfoVM = new MoreInfoVM()
            {
                Document = document,
                CategoryList = _UnitOfWork.Category.GetAll(u => u.ApplicationUserId == userId).ToList()
            }; //to show the user all the categories that he has when he wants to update the document

            return View(moreInfoVM);
        }

        public IActionResult SharedDocuments() //to display documents that are shared from other users 
        {
            //check if the user is authenticated to avoid errors
            if (!User.Identity.IsAuthenticated) 
            {
                return RedirectToAction("Privacy", "Home");
            }
            return View();
        }




        //share the document action
        [HttpPost]
        public async Task<IActionResult> UploadSharedDocument(int DocumentId, string SharedToEmail) //shared id -> user email
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
                TempData["error"] = "Do not leave it empty."; //there aren't a user with this email
                return RedirectToAction("MoreInfo", new { id = DocumentId }); //return to the function with the document id
            }


            var SharedToUser = _UnitOfWork.ApplicationUser.Get(u => u.Email.ToLower() == SharedToEmail.ToLower());
            if(SharedToUser == null)
            {
                TempData["error"] = "User with this email does not exist.";
                return RedirectToAction("MoreInfo", new { id = DocumentId }); //return to the function with the document id
            }

            // if the document is already shared with the same user
            var existingSharedDocument = _UnitOfWork.SharedDocument.Get(u => u.DocumentId == DocumentId && u.SharedFromId == SharedToUser.Id);
            if (existingSharedDocument != null)
            {
                TempData["error"] = "This document is already shared with this user.";
                return RedirectToAction("AccessManagements", "Home");
            }



            _UnitOfWork.SharedDocument.Add(new SharedDocument()
            {
                SharedToEmail= SharedToUser.Email.ToLower(),
                SharedFromId =userId,
                DocumentId=DocumentId,
                SharedAt=DateTime.Now
            });
            _UnitOfWork.Save(); //save changes after adding the shared doc

            TempData["success"] = "Document shared successfully! check your email for more info.";

            //use the google drive class from the utilities
            var DriveService = await _GoogleDriveService.GetDriveService();


            //to get the file id to grant access to the user
            //first mcd folder which has all the user's folders
            var folderRequest = DriveService.Files.List();
            folderRequest.Q = "name = 'MCD' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            folderRequest.Fields = "files(id, name)";
            var folderResponse = await folderRequest.ExecuteAsync();
            var mcdFolder = folderResponse.Files.FirstOrDefault();
            string mcdFolderId = mcdFolder.Id; // get the MCD folder ID

            // get user's folder ID inside MCD
            var userFolderRequest = DriveService.Files.List();
            userFolderRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{mcdFolderId}' in parents and trashed = false";
            userFolderRequest.Fields = "files(id, name)";
            var userFolderResponse = await userFolderRequest.ExecuteAsync();
            var userFolder = userFolderResponse.Files.FirstOrDefault();
            string userFolderId = userFolder.Id; // get the user folder ID

            // finally get the file ID
            var documentName = _UnitOfWork.Document.Get(u => u.Id == DocumentId).FileName;
            var userFileToShareRequest = DriveService.Files.List();
            userFileToShareRequest.Q = $"name = '{documentName}' and '{userFolderId}' in parents and trashed = false";
            userFileToShareRequest.Fields = "files(id, name)";
            var userFileToShareResponse = await userFileToShareRequest.ExecuteAsync();
            var userFileToShare = userFileToShareResponse.Files.FirstOrDefault();



            //after getting the id of the file, in order to grant the user access to the file (making the file not accessible by someone isn't the owner or shared to user)
            GoogleDriveService.GiveFilePermission(DriveService, "writer", userFileToShare.Id, SharedToUser.Email);

            _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"Gave ${SharedToUser.Email.ToLower()} Access",
                FileName = _UnitOfWork.Document.Get(u=>u.Id == DocumentId).FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log



            return RedirectToAction("AccessManagements", "Home");
        }






        [HttpPost]
        public IActionResult AdjustDocument(int DocumentID, string Title, string Category, string action, string NewCategory)
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
                TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }

            _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"Edited MetaData",
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log


            int CategoryId;
            // Check if a new category is provided
            if (Category == "new" && !string.IsNullOrEmpty(NewCategory))
            {
                // search for the category using its name so that don't create a new category
                var oldCategory = _UnitOfWork.Category.Get(u => u.CategoryName.ToLower() == NewCategory.ToLower() && u.ApplicationUserId == userId);
                if (oldCategory != null) //if the category already exists
                {
                    CategoryId = oldCategory.Id; // Use the old category ID

                }
                else
                {
                    // Create new category logic
                    var newCategory = new Category
                    {
                        CategoryName = NewCategory,
                        ApplicationUserId = userId
                    };
                    _UnitOfWork.Category.Add(newCategory);
                    _UnitOfWork.Save();
                    CategoryId = newCategory.Id; // Use the new category ID
                }
            }
            else //if we don't want to create a new category
            {
                // Get the old category ID
                var oldCategory = _UnitOfWork.Category.Get(u => u.Id == Convert.ToInt32(Category) && u.ApplicationUserId == userId);
                if (oldCategory != null) 
                {
                    CategoryId = oldCategory.Id;
                }
                else //to avoid errors if the category not found
                {
                    TempData["error"] = "Category not found.";
                    return RedirectToAction("Index", "Home");
                }
            }



            //here we will check the action that the user want to do on the document and do using a switch
            switch (action)
            {
                case "Summarize": //summarizing logic
                    return RedirectToAction("MoreInfo"); 

                case "Delete": //delete the document
                    _UnitOfWork.Document.Remove(document);
                    _UnitOfWork.Save();
                    TempData["success"] = "Document deleted successfully!";
                    return RedirectToAction("Document", "Home");

                case "Update": //update the document
                    document.Title = Title;
                    document.CategoryId = CategoryId; //it will be automatically bounded to the category obj
                    document.UpdateDate = DateTime.Now;
                    _UnitOfWork.Save();
                    TempData["success"] = "Document updated successfully!";
                    return RedirectToAction("Document", "Home");

                default: //in case of something goes wrong
                    TempData["error"] = "Invalid action.";
                    break;
            }




            return RedirectToAction("MoreInfo"); 
        }


        #region api calls
        [HttpGet]
        public IActionResult GetallSharedDocuments() //to get all shared documents in datatables API
        {
            //to get the user id to get all shared documents to him by using his email
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var SharedToUser = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId); //get the user by his id to get all shared documents to him using his email (shared to property)


            List<SharedDocument> SharedDocumentList = _UnitOfWork.SharedDocument.GetAll(u => u.SharedToEmail.ToLower() == SharedToUser.Email.ToLower(),
                includeProperties: "Document,Document.ApplicationUser").ToList();
            return Json(new { data = SharedDocumentList });

        }

        #endregion

    }
}
