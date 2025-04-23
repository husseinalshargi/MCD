using Google.Apis.Drive.v3;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using MCD.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.IO.Pipes;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MCD.Areas.Customer.Controllers
{
    [Area("Customer")] //to specify the area of the controller
    [Authorize]        //to make sure that the user is authenticated before accessing the controller
    public class DocumentController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly GoogleDriveService _GoogleDriveService;
        private readonly MCDAIFunctions _MCDAIFunctions;
        public DocumentController(IUnitOfWork unitOfWork, GoogleDriveService googleDriveService, MCDAIFunctions MCDAIFunctions)
        {
            _UnitOfWork = unitOfWork;
            _GoogleDriveService = googleDriveService;
            _MCDAIFunctions = MCDAIFunctions;
        }
        public async Task<IActionResult> MoreInfo(int? id)
        {
            if (!User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated 
            {
                return RedirectToAction("Privacy", "Home");
            }

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //in order to return the content of the file we should first retrieve the file it self
            Document document = _UnitOfWork.Document.Get(u => u.Id == id && u.ApplicationUserId == userId, includeProperties: "Category,ApplicationUser"); //get the document with the document id that are passed in the documents page (more info)

            if (document == null) //if the document not found
            {
                TempData["error"] = "Document not found.";
                return RedirectToAction("Document", "Home");
            }


            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            var isExtracted = await GoogleDriveService.GetGoogleDriveFileId(DriveService, document.Id, $"{Path.GetFileNameWithoutExtension(document.FileName)}_entities.txt", userId); //check if the document is already extracted in google drive



            MoreInfoVM moreInfoVM = new MoreInfoVM()
            {
                Document = document,
                CategoryList = _UnitOfWork.Category.GetAll(u => u.ApplicationUserId == userId).ToList(),
                isConverted = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == document.Id) != null, //to check if the document is already converted to text
                isSummarized = _UnitOfWork.SummarizedDocument.Get(u => u.DocumentId == document.Id) != null, //to check if the document is already Summarized 
                isExtracted = isExtracted != null
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
                TempData["error"] = "Do not leave it empty."; //there is no input
                return RedirectToAction("MoreInfo", new { id = DocumentId }); //return to the function with the document id
            }


            var SharedToUser = _UnitOfWork.ApplicationUser.Get(u => u.Email.ToLower() == SharedToEmail.ToLower());
            if (SharedToUser == null)
            {
                TempData["error"] = $"User with this email does not exist. <a href='/Customer/ShareMCD/ThanksForSharing?emailToShare={SharedToEmail}' class='btn btn-sm btn-link'>Click here</a> to invite him!"; //href because Tag Helpers only work in Razor views — not inside strings.
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
                SharedToEmail = SharedToUser.Email.ToLower(),
                SharedFromId = userId,
                DocumentId = DocumentId,
                SharedAt = DateTime.Now
            });
            _UnitOfWork.Save(); //save changes after adding the shared doc

            TempData["success"] = "Document shared successfully! check your email for more info.";

            //use the google drive class from the utilities
            var DriveService = await _GoogleDriveService.GetDriveService();

            string googleDriveFileId = await GoogleDriveService.GetGoogleDriveFileId(DriveService, DocumentId, _UnitOfWork.Document.Get(u => u.Id == DocumentId).FileName, userId);


            //after getting the id of the file, in order to grant the user access to the file (making the file not accessible by someone isn't the owner or shared to user)
            GoogleDriveService.GiveFilePermission(DriveService, "writer", googleDriveFileId, SharedToUser.Email);

            _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"Gave {SharedToUser.Email.ToLower()} Access",
                FileName = _UnitOfWork.Document.Get(u => u.Id == DocumentId).FileName,
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
                //TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }

            //do not allow the update if the parameters is the same as the current document
            if (document.Title == Title && document.CategoryId.ToString() == Category)
            {
                TempData["error"] = "No changes were made.";
                return RedirectToAction("MoreInfo", new { id = document.Id });
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



            document.Title = Title;
            document.CategoryId = CategoryId; //it will be automatically bounded to the category obj
            document.UpdateDate = DateTime.Now;
            _UnitOfWork.Save();
            TempData["success"] = "Document updated successfully!";

            return RedirectToAction("MoreInfo", new { id = document.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int DocumentId, bool deleteConverted = false, bool deleteSummarized = false, bool deleteExtracted = false)
        {
            //first thing make sure that the user are authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Privacy", "Home"); //home controller if user aren't authenticated
            }
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentId);
            if (document.ApplicationUserId != userId)
            {
                TempData["error"] = "You are not authorized to delete this document.";
                return RedirectToAction("Document", "Home");
            }

            //log action before deleting the document/converted 

            _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = deleteConverted ? "Deleted converted document" : deleteSummarized ? "Deleted summarized document" : deleteExtracted ? "Deleted extracted entities" : "Deleted document",
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log

            //get the file id in google drive to delete from there
            var driveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            string fileNameToDelete = deleteConverted ? Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt" : deleteSummarized ? Path.GetFileNameWithoutExtension(document.FileName) + "_summarized.txt" : deleteExtracted ? Path.GetFileNameWithoutExtension(document.FileName) + "_entities.txt" : document.FileName; //to change the file name to delete that specific file
            var googleDriveFilId = await GoogleDriveService.GetGoogleDriveFileId(driveService, DocumentId, fileNameToDelete, userId);
            var isDeleted = await GoogleDriveService.DeleteFileAsync(driveService, googleDriveFilId); //delete the file from the google drive and return true if deleted successfully

            if (deleteConverted) //delete the converted document
            {
                var extractedDocument = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == DocumentId);
                if (extractedDocument == null && !isDeleted)
                {
                    TempData["error"] = "converted document not found.";
                    return RedirectToAction("Document", "Home");
                }
                if (isDeleted) //if the file deleted successfully from the google drive
                {
                    _UnitOfWork.ExtractedDocument.Remove(extractedDocument);
                    _UnitOfWork.Save();
                    TempData["success"] = "Converted document deleted successfully!";
                }
                else
                {
                    TempData["error"] = "Error deleting the converted document from Google Drive.";
                }
                return RedirectToAction("MoreInfo", new { id = DocumentId });
            }
            else if (deleteSummarized) //delete the summarized document
            {
                var SummarizedDocument = _UnitOfWork.SummarizedDocument.Get(u => u.DocumentId == DocumentId);
                if (SummarizedDocument == null && !isDeleted)
                {
                    TempData["error"] = "Summarized document not found.";
                    return RedirectToAction("Document", "Home");
                }
                if (isDeleted) //if the summarized document deleted successfully from the google drive
                {
                    _UnitOfWork.SummarizedDocument.Remove(SummarizedDocument);
                    _UnitOfWork.Save();
                    TempData["success"] = "Summarized document deleted successfully!";
                }
                else
                {
                    TempData["error"] = "Error deleting the summarized document from Google Drive.";
                }
                return RedirectToAction("MoreInfo", new { id = DocumentId });
            }
            else if (deleteExtracted)
            {
                var ExtractedEntitiesList = _UnitOfWork.Entity.Get(u => u.DocumentId == DocumentId);
                if (ExtractedEntitiesList == null && !isDeleted)
                {
                    TempData["error"] = "Extracted entities not found.";
                    return RedirectToAction("Document", "Home");
                }
                if (isDeleted) //if the summarized document deleted successfully from the google drive
                {
                    document.ExtractedEntities = null;
                    _UnitOfWork.Document.Update(document);  // mark the document as updated
                    _UnitOfWork.Save();
                    TempData["success"] = "Extracted entities deleted successfully!";
                }
                else
                {
                    TempData["error"] = "Error deleting the extracted entities from Google Drive.";
                }
                return RedirectToAction("MoreInfo", new { id = DocumentId });
            }
            else
            {
                //before deleting the document we should delete the shared documents that are related to it to remove access first
                var sharedDocuments = _UnitOfWork.SharedDocument.GetAll(u => u.DocumentId == DocumentId);
                foreach (var sharedDocument in sharedDocuments) //remove each shared document found of the same document id
                {
                    _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
                    {
                        ApplicationUserId = _UnitOfWork.ApplicationUser.Get(u => u.Email.ToLower() == sharedDocument.SharedToEmail.ToLower()).Id, //the user that has access
                        userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email, //the user that deleted the document which will remove the access
                        Action = $"Removed access",
                        FileName = sharedDocument.Document.FileName,
                        ActionDate = DateTime.Now
                    });
                    _UnitOfWork.Save();
                    _UnitOfWork.SharedDocument.Remove(sharedDocument);
                    _UnitOfWork.Save();
                }
                if (isDeleted) //if the file deleted successfully from the google drive
                {
                    var extracted = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == DocumentId); //check if there is a converted version of the document
                    var summarized = _UnitOfWork.SummarizedDocument.Get(u => u.DocumentId == DocumentId); //check if there is a summarized version of the document
                    var extractedEntities = _UnitOfWork.Entity.GetAll(u => u.DocumentId == DocumentId); //check if there is a extracted entities version of the document
                    if (extracted != null) //if the document has a converted version
                    {
                        string extractedNameToDelete = Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt"; //to change the file name to delete converted file
                        var extractedGoogleDriveFilId = await GoogleDriveService.GetGoogleDriveFileId(driveService, DocumentId, extractedNameToDelete, userId);
                        var extractedIsDeleted = await GoogleDriveService.DeleteFileAsync(driveService, extractedGoogleDriveFilId); //delete the file from the google drive and return true if deleted successfully

                        _UnitOfWork.ExtractedDocument.Remove(extracted); //remove the converted version of the document from the database
                        _UnitOfWork.Save();
                    }
                    if (summarized != null) //if the document has summarized version
                    {
                        string summarizedNameToDelete = Path.GetFileNameWithoutExtension(document.FileName) + "_summarized.txt"; //to change the file name to delete converted file
                        var summarizedGoogleDriveFilId = await GoogleDriveService.GetGoogleDriveFileId(driveService, DocumentId, summarizedNameToDelete, userId);
                        var summarizedIsDeleted = await GoogleDriveService.DeleteFileAsync(driveService, summarizedGoogleDriveFilId); //delete the file from the google drive and return true if deleted successfully

                        _UnitOfWork.SummarizedDocument.Remove(summarized); //remove the summarized version from the database
                        _UnitOfWork.Save();
                    }
                    if (extractedEntities != null) //if the document has extracted entities version
                    {
                        string extractedEntitiesNameToDelete = Path.GetFileNameWithoutExtension(document.FileName) + "_entities.txt"; //to change the file name to delete converted file
                        var extractedEntitiesGoogleDriveFilId = await GoogleDriveService.GetGoogleDriveFileId(driveService, DocumentId, extractedEntitiesNameToDelete, userId);
                        var extractedEntitiesIsDeleted = await GoogleDriveService.DeleteFileAsync(driveService, extractedEntitiesGoogleDriveFilId); //delete the file from the google drive and return true if deleted successfully

                        document.ExtractedEntities = null; //remove the extracted entities from the document
                        _UnitOfWork.Document.Update(document);  // update the document
                        _UnitOfWork.Save();

                        var entities = _UnitOfWork.Entity.GetAll(u => u.DocumentId == DocumentId); //get all the entities related to the document
                        _UnitOfWork.Entity.RemoveRange(entities); //remove all the entities related to the document
                        _UnitOfWork.Save();
                    }
                    _UnitOfWork.Document.Remove(document);
                    _UnitOfWork.Save();
                    TempData["success"] = "Document deleted successfully!";
                }
                else
                {
                    TempData["error"] = "Error deleting the document from Google Drive.";
                }
                return RedirectToAction("Document", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SummarizeDocument(int DocumentId)
        {
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentId); //get the document by its id to use its details to get the google drive file id

            //if the user is not authenticated
            if (document.ApplicationUserId != userId)
            {
                TempData["error"] = "You are not authorized to access this document.";
                return RedirectToAction("Index", "Home");
            }

            //see if the file is already converted
            var summarizedDocument = _UnitOfWork.SummarizedDocument.Get(u => u.DocumentId == DocumentId);
            if (summarizedDocument != null) //if the document is already summarized
            {
                TempData["error"] = "Document is already summarized.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }

            if (document == null) //if the document not found
            {
                TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }


            //check if the extension can be used to summarize or need to be converted by ocr
            string extension = Path.GetExtension(document.FileName).ToLower();

            //list of allowed text-based file types
            string[] allowedExtensions = { ".txt", ".csv", ".log", ".json", ".xml", ".md" };


            string summarizedText; //to store the summarized text

            //check if the extension is valid
            if (!allowedExtensions.Contains(extension)) //if it isn't in text format
            {
                var extractedDocument = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == DocumentId); //check if there is a converted version of the document
                if (extractedDocument == null)
                {
                    TempData["error"] = "document can not be summarized, please use text file or you should convert it to text first";
                    return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
                }
                summarizedText = await _MCDAIFunctions.SendDataAsync("/summarize", document.Id, Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt", userId); //send the converted version of the file to the ai service to summarize it
            }
            else //if it can be used in the summarization directly
            {
                //send the file to the ai service to summarize it
                summarizedText = await _MCDAIFunctions.SendDataAsync("/summarize", document.Id, document.FileName, userId);
            }
            if (summarizedText.IsNullOrEmpty())
            {
                TempData["error"] = "Error summarizing the document.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }


            //create a new text file to place summarized content
            string newFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_summarized.txt";
            using var newFileStream = new MemoryStream(Encoding.UTF8.GetBytes(summarizedText)); // Convert string to stream

            //get mcd folder in order to save it
            var folderRequest = DriveService.Files.List();
            folderRequest.Q = "name = 'MCD' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            folderRequest.Fields = "files(id, name)";
            var folderResponse = await folderRequest.ExecuteAsync();
            var mcdFolder = folderResponse.Files.FirstOrDefault();

            if (mcdFolder == null)
            {
                return NotFound("MCD folder not found.");
            }
            string mcdFolderId = mcdFolder.Id;



            // get  User's Folder ID inside MCD
            var userFolderRequest = DriveService.Files.List();
            userFolderRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{mcdFolderId}' in parents and trashed = false";
            userFolderRequest.Fields = "files(id, name)";
            var userFolderResponse = await userFolderRequest.ExecuteAsync();
            string userFolderId = userFolderResponse.Files[0].Id;


            var FileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = document.Id.ToString() + "-" + newFileName,
                Parents = new List<string> { userFolderId }, //the name of the folder in the drive
                Description = $"MCD Documents folder uploaded by: {_UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email}"

            }; //here i will place the file details that will be uploaded in google drive

            //uploading the file logic
            using (var stream = newFileStream)
            {
                var request = DriveService.Files.Create(FileMetaData, stream, "text/plain");
                request.Fields = "id, webViewLink"; //  file id and link
                var uploadedFile = await request.UploadAsync();

                if (uploadedFile.Status != Google.Apis.Upload.UploadStatus.Completed) //if there is an error with the file uploading
                {
                    return StatusCode(500, "Error uploading file to Google Drive.");
                }
                // Get uploaded file details
                var fileData = request.ResponseBody;
                //in order to grant the user access to the file after creating it (making the file not accessible by someone isn't the owner)
                GoogleDriveService.GiveFilePermission(DriveService, "writer", fileData.Id, _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email);
            }
            //after uploading the file to the drive we should add it to the extracted documents table and audit log
            _UnitOfWork.SummarizedDocument.Add(new SummarizedDocument()
            {
                DocumentId = DocumentId,
                SummarizedFileName = newFileName
            });
            _UnitOfWork.Save(); //save the changes after adding the extracted document
            _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"summarize file",
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log
            TempData["success"] = "Document summarized successfully!";

            return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
        }
        [HttpPost]
        public async Task<IActionResult> ConvertToText(int DocumentId)
        {

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentId); //get the document by its id to use its details to get the google drive file id

            //if the user is not authenticated
            if (document.ApplicationUserId != userId)
            {
                TempData["error"] = "You are not authorized to access this document.";
                return RedirectToAction("Index", "Home");
            }

            //see if the file is already converted
            var extractedDocument = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == DocumentId);
            if (extractedDocument != null) //if the document is already converted
            {
                TempData["error"] = "Document is already converted to text.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }


            if (document == null) //if the document not found
            {
                TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }

            //after getting the id of the file, make a request to the ai service to convert the file to text
            //first ensure the document format is supported for OCR
            string fileExtension = Path.GetExtension(document.FileName).ToLower();
            string[] allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".tiff" }; //that could be converted to text
            if (!allowedExtensions.Contains(fileExtension)) //if the file format is not supported
            {
                TempData["error"] = "Unsupported file format. Please use a PDF or image file.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }
            string extractedText = await _MCDAIFunctions.SendDataAsync("/ocr", document.Id, document.FileName, userId); //send the file to the ai service to convert it to text
            if (extractedText == null)
            {
                TempData["error"] = "Error converting the document to text.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }

            //create a new text file with extracted content
            string newFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt";
            using var newFileStream = new MemoryStream(Encoding.UTF8.GetBytes(extractedText)); // Convert string to stream

            //get mcd folder in order to save it
            var folderRequest = DriveService.Files.List();
            folderRequest.Q = "name = 'MCD' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            folderRequest.Fields = "files(id, name)";
            var folderResponse = await folderRequest.ExecuteAsync();
            var mcdFolder = folderResponse.Files.FirstOrDefault();

            if (mcdFolder == null)
            {
                return NotFound("MCD folder not found.");
            }
            string mcdFolderId = mcdFolder.Id;



            // get  User's Folder ID inside MCD
            var userFolderRequest = DriveService.Files.List();
            userFolderRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{mcdFolderId}' in parents and trashed = false";
            userFolderRequest.Fields = "files(id, name)";
            var userFolderResponse = await userFolderRequest.ExecuteAsync();
            string userFolderId = userFolderResponse.Files[0].Id;


            var FileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = document.Id.ToString() + "-" + newFileName,
                Parents = new List<string> { userFolderId }, //the name of the folder in the drive
                Description = $"MCD Documents folder uploaded by: {_UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email}"

            }; //here i will place the file details that will be uploaded in google drive

            //uploading the file logic
            using (var stream = newFileStream)
            {
                var request = DriveService.Files.Create(FileMetaData, stream, "text/plain");
                request.Fields = "id, webViewLink"; //  file id and link
                var uploadedFile = await request.UploadAsync();

                if (uploadedFile.Status != Google.Apis.Upload.UploadStatus.Completed) //if there is an error with the file uploading
                {
                    return StatusCode(500, "Error uploading file to Google Drive.");
                }
                // Get uploaded file details
                var fileData = request.ResponseBody;
                //in order to grant the user access to the file after creating it (making the file not accessible by someone isn't the owner)
                GoogleDriveService.GiveFilePermission(DriveService, "writer", fileData.Id, _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email);
            }
            //after uploading the file to the drive we should add it to the extracted documents table and audit log
            _UnitOfWork.ExtractedDocument.Add(new ExtractedDocument()
            {
                DocumentId = DocumentId,
                ExtractedFileName = newFileName
            });
            _UnitOfWork.Save(); //save the changes after adding the extracted document
            _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"Converted file to text",
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log
            TempData["success"] = "Document converted to text successfully!";

            return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
        }
        public async Task<IActionResult> DownloadFile(int DocumentId, bool downloadConverted = false, bool downloadSummarized = false, bool downloadExtracted = false)
        {
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentId); //get the document by its id to use its details to get the google drive file id
            //if the user is not authenticated
            if (document.ApplicationUserId != userId)
            {
                TempData["error"] = "You are not authorized to access this document.";
                return RedirectToAction("Index", "Home");
            }


            if (document == null) //if the document not found
            {
                TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }

            //get the file id in google drive to download it
            string fileNameToDownload = downloadConverted ? Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt" : downloadSummarized ? Path.GetFileNameWithoutExtension(document.FileName) + "_summarized.txt" : downloadExtracted ? Path.GetFileNameWithoutExtension(document.FileName) + "_entities.txt" : document.FileName; //to change the file name to download that specific file
            var googleDriveFileId = await GoogleDriveService.GetGoogleDriveFileId(DriveService, DocumentId, fileNameToDownload, userId);

            //download the file from google drive
            var stream = await GoogleDriveService.DownloadFileAsync(DriveService, googleDriveFileId);

            if (stream == null) //if there is an error whe downloading the file
            {
                return BadRequest("Failed to download file.");
            }


            _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = (downloadConverted ? "downloaded converted version of " : downloadSummarized ? "downloaded Summarized version of " : downloadExtracted ? "downloaded extracted entities of " : "downloaded ") + document.FileName,
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log

            return File(stream, "application/octet-stream", fileNameToDownload);
        }
        [HttpPost]
        public async Task<IActionResult> GetEntities(int DocumentId)
        {
            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance to interact with the drive
            var document = _UnitOfWork.Document.Get(u => u.Id == DocumentId); //get the document by its id to use its details to get the google drive file id

            //if the user is not authenticated
            if (document.ApplicationUserId != userId)
            {
                TempData["error"] = "You are not authorized to access this document.";
                return RedirectToAction("Index", "Home");
            }

            var extractedEntities = _UnitOfWork.Entity.Get(u => u.DocumentId == DocumentId); //check if there is a converted version of the document

            //see if the file is already converted
            if (extractedEntities != null) //if the document is already has entities file
            {
                TempData["error"] = "Document already extracted entities.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }

            if (document == null) //if the document not found
            {
                TempData["error"] = "Document not found.";
                return RedirectToAction("Index", "Home");
            }


            //check if the extension can be used to extract entities or need to be converted by ocr
            string extension = Path.GetExtension(document.FileName).ToLower();

            //list of allowed text-based file types
            string[] allowedExtensions = { ".txt", ".csv", ".log", ".json", ".xml", ".md" };


            string extractedEntitiesText; //to store the extracted entities

            //check if the extension is valid
            if (!allowedExtensions.Contains(extension)) //if it isn't in text format
            {
                var extractedDocument = _UnitOfWork.ExtractedDocument.Get(u => u.DocumentId == DocumentId); //check if there is a converted version of the document
                if (extractedDocument == null)
                {
                    TempData["error"] = "entities can not be extracted, please use text file or you should convert it to text first";
                    return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
                }
                extractedEntitiesText = await _MCDAIFunctions.SendDataAsync("/entities", document.Id, Path.GetFileNameWithoutExtension(document.FileName) + "_converted.txt", userId); //send the converted version of the file to the ai service to extract entities
            }
            else //if it can be used in the entities extraction directly
            {
                //send the file to the ai service to extract entities
                extractedEntitiesText = await _MCDAIFunctions.SendDataAsync("/entities", document.Id, document.FileName, userId);
            }
            if (extractedEntitiesText.IsNullOrEmpty())
            {
                TempData["error"] = "Error summarizing the document.";
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }

            //create a new text file to place extracted entities content
            string newFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_entities.txt";
            using var newFileStream = new MemoryStream(Encoding.UTF8.GetBytes(extractedEntitiesText)); // Convert string to stream

            //get mcd folder in order to save it
            var folderRequest = DriveService.Files.List();
            folderRequest.Q = "name = 'MCD' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            folderRequest.Fields = "files(id, name)";
            var folderResponse = await folderRequest.ExecuteAsync();
            var mcdFolder = folderResponse.Files.FirstOrDefault();

            if (mcdFolder == null)
            {
                return NotFound("MCD folder not found.");
            }
            string mcdFolderId = mcdFolder.Id;



            // get  User's Folder ID inside MCD
            var userFolderRequest = DriveService.Files.List();
            userFolderRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{mcdFolderId}' in parents and trashed = false";
            userFolderRequest.Fields = "files(id, name)";
            var userFolderResponse = await userFolderRequest.ExecuteAsync();
            string userFolderId = userFolderResponse.Files[0].Id;


            var FileMetaData = new Google.Apis.Drive.v3.Data.File()
            {
                Name = document.Id.ToString() + "-" + newFileName,
                Parents = new List<string> { userFolderId }, //the name of the folder in the drive
                Description = $"MCD Documents folder uploaded by: {_UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email}"

            }; //here i will place the file details that will be uploaded in google drive

            //uploading the file logic
            using (var stream = newFileStream)
            {
                var request = DriveService.Files.Create(FileMetaData, stream, "text/plain");
                request.Fields = "id, webViewLink"; //  file id and link
                var uploadedFile = await request.UploadAsync();

                if (uploadedFile.Status != Google.Apis.Upload.UploadStatus.Completed) //if there is an error with the file uploading
                {
                    return StatusCode(500, "Error uploading file to Google Drive.");
                }
                // Get uploaded file details
                var fileData = request.ResponseBody;
                //in order to grant the user access to the file after creating it (making the file not accessible by someone isn't the owner)
                GoogleDriveService.GiveFilePermission(DriveService, "writer", fileData.Id, _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email);
            }


            var extractedEntitiesList = new List<Entity>();

            // Clean the string
            string cleanText = extractedEntitiesText
                .Replace("```json", "")
                .Replace("```", "")
                .Replace("json", "")
                .Trim();

            // Normalize line endings and split into lines
            var lines = cleanText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var match = Regex.Match(line.Trim(), @"(\w+)\s*,\s*(\w+)");
                if (match.Success)
                {
                    string entityType = match.Groups[1].Value.ToLower();  // e.g., "person"
                    string entityValue = match.Groups[2].Value;            // e.g., "Perlia"

                    extractedEntitiesList.Add(new Entity
                    {
                        DocumentId = document.Id,
                        EntityType = entityType,
                        EntityValue = entityValue
                    });
                }
            }

            document.ExtractedEntities = extractedEntitiesList;
            _UnitOfWork.Document.Update(document);
            _UnitOfWork.Save();


            _UnitOfWork.AuditLog.Add(new AuditLog() // log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"extracted entities of the file",
                FileName = document.FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log
            var entity = _UnitOfWork.Entity.GetAll(u => u.DocumentId == document.Id).ToList();
            if (entity != null)
            {
                TempData["success"] = "Extracted entities successfully!";
            }
            else
            {
                TempData["error"] = "Entities not found!";
            }

            return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
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
