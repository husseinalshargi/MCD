using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using MCD.DataAccess.Data;
using MCD.DataAccess.Repository;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using MCD.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MCD.Areas.Customer.Controllers
{
    [Area("Customer")] //to specify the area of the controller
    [Authorize]        //to make sure that the user is authenticated before accessing the controller
    public class HomeController : Controller
    {
        private readonly GoogleDriveService _GoogleDriveService;
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _UnitOfWork;
        [BindProperty]
        public DocumentVM DocumentVM { get; set; }

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, GoogleDriveService googleDriveService)
        {
            _logger = logger;
            _UnitOfWork = unitOfWork;
            _GoogleDriveService = googleDriveService;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated
            {
                //in order to claim the user id (the one that enters the page)
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                var currentUser = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId);

                var homePageVM = new HomePageVM()
                {
                    RecentDocuments = _UnitOfWork.Document.GetAll(u => u.ApplicationUserId == currentUser.Id).OrderByDescending(d => d.UpdateDate).ToList(), //to show in the page recent updated documents
                    CurrentUser = currentUser, //to see if he has password or he confirmed his account
                    TotalDocuments = _UnitOfWork.Document.GetAll(u => u.ApplicationUserId == currentUser.Id).Count(),
                    TotalSharedDocuments = _UnitOfWork.SharedDocument.GetAll(u => u.SharedFromId == userId).Count(),
                    TotalSharedWithDocuments = _UnitOfWork.SharedDocument.GetAll(u => u.SharedToEmail.Trim().ToLower() == currentUser.Email.Trim().ToLower()).Count(),
                    RecentSharedDocuments = _UnitOfWork.SharedDocument.GetAll(u => u.SharedToEmail.Trim().ToLower() == currentUser.Email.Trim().ToLower(), includeProperties : "Document").OrderByDescending(d => d.SharedAt).ToList(), //to show in the page recent shared documents
                    auditLogs = _UnitOfWork.AuditLog.GetAll(u => u.ApplicationUserId == userId).OrderByDescending(d => d.ActionDate).ToList() //to show in the page recent logs in descending order
                };



                return View(homePageVM);
            }
            return RedirectToAction("Privacy");
        }



        public IActionResult AccessManagements()
        {
            if (!User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated 
            {
                return RedirectToAction("Privacy");
            }

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<SharedDocument> AccessManagementsList = _UnitOfWork.SharedDocument.GetAll(u => u.SharedFromId == userId).ToList();

            return View(AccessManagementsList);
        }

        public async Task<IActionResult> RemoveAccess(int SharedAccessId, string FileName)
        {
            if (!User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated 
            {
                return RedirectToAction("Privacy");
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //search for the shared document to delete it ( remove access )
            SharedDocument sharedDocument = _UnitOfWork.SharedDocument.Get(u => u.Id == SharedAccessId);
            if (sharedDocument == null) //to avoid errors
            {
                TempData["error"] = "Error while removing access. Access might be removed already.";
                return RedirectToAction(nameof(AccessManagements));
            }

            //remove the access from the google drive
            //first thing get the document id from google drive
            //use the google drive class from the utilities
            var DriveService = await _GoogleDriveService.GetDriveService();
            //from the utilities in order to skip the logic of searching for the file id in google drive to avoid redundancy
            string googleDriveFileId = await GoogleDriveService.GetGoogleDriveFileId(DriveService, sharedDocument.DocumentId, _UnitOfWork.Document.Get(u => u.Id == sharedDocument.DocumentId).FileName, userId);

            await GoogleDriveService.RemoveFilePermission(DriveService, googleDriveFileId, sharedDocument.SharedToEmail); //remove the access from the user


            //create log for the action
            _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                Action = $"Removed {sharedDocument.SharedToEmail} Access",
                FileName = FileName,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log

            //here is the logic of removing the access
            _UnitOfWork.SharedDocument.Remove(sharedDocument);
            _UnitOfWork.Save(); //save changes

            TempData["success"] = "Access removed successfully.";

            return RedirectToAction(nameof(AccessManagements));
        }



        public IActionResult AuditLogs()
        {
            return View();
        }
        public IActionResult Document()
        {
            if (!User.Identity.IsAuthenticated) // in order to avoid null errors in the next step we will check first if the user is authenticated 
            {
                return RedirectToAction("Privacy");
            }

            //in order to claim the user id (the one that enters the page)
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            // in order to return a list of all the documents
            List<Document> DocumentList = _UnitOfWork.Document.GetAll(includeProperties: "Category").Where(u => u.ApplicationUserId == userId).ToList(); //to convert from IEnumerable object to List object to pass to the index page, for all the documents of the user that is in the page

            var viewModel = new DocumentVM //as we will handle showing the documents and update new ones in the same place we will need a view model as we can't pass more than one model in the same page
            {
                documents = DocumentList,
                TotalDocuments = DocumentList.Count(), //to show the number of documents in the view
                pdfCount = DocumentList.Count(u => u.FileType == "application/pdf"), //to show the number of pdf documents in the view
                textCount = DocumentList.Count(u => u.FileType == "text/plain"), //to show the number of text documents in the view
                imageCount = DocumentList.Count(u => u.FileType == "image/jpeg" || u.FileType == "image/png" || u.FileType == "image/jpg"), //to show the number of image documents in the view
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(DocumentVM model) //put here the input, it will be async in order to improve performance/ handling a lot of large documents and we don't want thread blocking   
        {
            if (model.DocumentFile != null) // if there is a valid file
            {
                //to assign the user id 
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


                //to get the file in IFormFile obj
                if (model.DocumentFile == null)
                {
                    return BadRequest("No file selected."); //as the user did not upload a file
                }
                // in order to write it in the description
                string userEmail = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email; // to get the user email


                //use the google drive class from the utilities
                var DriveService = await _GoogleDriveService.GetDriveService(); // await because it is defined like this 

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
                string userFolderId = null;

                //here is the logic of searching for a folder, if it wasn't here create one
                string parentFolderId = null; // to put in the parent name (folder)
                if (userFolderResponse.Files.Count == 0) // if not found
                {
                    var folderMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = userId, // Folder name
                        Parents = new List<string> { mcdFolderId },
                        MimeType = "application/vnd.google-apps.folder",
                    };

                    var createRequest = DriveService.Files.Create(folderMetaData);
                    createRequest.Fields = "id";
                    var folder = createRequest.Execute();
                    userFolderId = folder.Id; // Use the created folder's ID as the parent
                }
                else
                {
                    userFolderId = userFolderResponse.Files[0].Id; // Use the existing folder's id as the parent
                }

                //create the document reference in the database before uploading the file in order to get the file id of the database to save in google drive

                //so that we don't create a category when we have a document with a default type
                Category DefaultCategory = _UnitOfWork.Category.Get(u => u.ApplicationUserId == userId && u.CategoryName == "---");
                if (DefaultCategory == null)
                {
                    DefaultCategory = new Category() { ApplicationUserId = userId, CategoryName = "---" };
                }


                //to save the document details in the db
                Document document = new()
                {
                    ApplicationUserId = userId,
                    Category = DefaultCategory,
                    Title = model.DocumentFile.FileName,
                    FileName = model.DocumentFile.FileName,
                    FileType = model.DocumentFile.ContentType,
                    UploadDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    AITaskStatus = "---"
                };

                _UnitOfWork.Document.Add(document); //add it to the db
                _UnitOfWork.Save();

                var newDocumentId = document.Id; //to get the id of the document that was just created to add it to the document name in google drive

                var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = newDocumentId + "-" + model.DocumentFile.FileName,
                    Parents = new List<string> { userFolderId }, //the name of the folder in the drive
                    Description = $"MCD Documents folder uploaded by: {userEmail}"

                }; //here i will place the file details that will be uploaded in google drive

                //uploading the file logic
                using (var stream = model.DocumentFile.OpenReadStream())
                {
                    var request = DriveService.Files.Create(FileMetaData, stream, model.DocumentFile.ContentType);
                    request.Fields = "id, webViewLink"; //  file id and link
                    var uploadedFile = await request.UploadAsync();

                    if (uploadedFile.Status != Google.Apis.Upload.UploadStatus.Completed) //if there is an error with the file uploading
                    {
                        return StatusCode(500, "Error uploading file to Google Drive.");
                    }
                    // Get uploaded file details
                    var fileData = request.ResponseBody;
                    //in order to grant the user access to the file after creating it (making the file not accessible by someone isn't the owner)
                    GoogleDriveService.GiveFilePermission(DriveService, "writer", fileData.Id, userEmail);

                }


                _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
                {
                    ApplicationUserId = userId,
                    userEmailAddress = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId).Email,
                    Action = "Created",
                    FileName = model.DocumentFile.FileName,
                    ActionDate = DateTime.Now
                });
                _UnitOfWork.Save(); //save the changes after adding the 
                TempData["success"] = "Document uploaded successfully."; //to show the success message to the user
                return RedirectToAction("MoreInfo", "Document", new { id = document.Id });
            }
            else
            {
                TempData["error"] = "Please select a file to upload."; //to show the error message to the user
            }



                return RedirectToAction(nameof(Document));
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //to use datatables api for dealing with the tables in our page
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var DocumentList = _UnitOfWork.Document.GetAll(includeProperties: "Category,ExtractedEntities").Where(u => u.ApplicationUserId == userId)
        .Select(doc => new //to select the data that we want to show in the datatables api to avoid sending all the data to the client side
        {
            doc.Id,
            doc.Title,
            doc.FileName,
            doc.UploadDate,
            doc.UpdateDate,
            doc.Category.CategoryName,
            EntityText = string.Join(", ", doc.ExtractedEntities.Select(e => e.EntityValue)),
            doc.ApplicationUserId
        }).ToList();

            return Json(new { data = DocumentList });
        }
        [HttpGet]
        public async Task<IActionResult> GetDocument(string userId, int documentId, string fileName)
        {
            //to determine whether the user can access this document or not
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var currentUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value; //to get the id of the current requesting user
            //here it will compare if this is the correct user or not or a user that has the right to access the file
            var currentUser = _UnitOfWork.ApplicationUser.Get(u => u.Id == currentUserId);
            var SharedDocument = _UnitOfWork.SharedDocument.Get(u => u.SharedToEmail.Trim().ToLower() == currentUser.Email.Trim().ToLower()
         && u.Document.FileName.Trim().ToLower() == fileName.Trim().ToLower(),
                includeProperties: "Document");
            if (currentUserId != userId) //if the user is not the same as the one who uploaded the file
            {
                if (SharedDocument == null)
                { // if the document is not shared with the user
                    return Unauthorized("Access denied.");
                }
            }



            var driveService = await _GoogleDriveService.GetDriveService(); // to connect to google drive
            //to enter the app folder in google drive
            var request = driveService.Files.List(); //list of all the files
            request.Q = $"name = 'MCD' and mimeType = 'application/vnd.google-apps.folder' and trashed = false";// search for a folder with the name of MCD
            request.Fields = "files(id)"; // get MCd folder Id
            var response = await request.ExecuteAsync();
            var MCDFolder = response.Files.FirstOrDefault(); // get the first matching folder which is the target
            string mcdFolderId = MCDFolder.Id; //to get the value of the id

            // after that we can search for the user folder inside MCD folder having the user id as its name

            var userFolderRequest = driveService.Files.List();
            userFolderRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{mcdFolderId}' in parents and trashed = false";
            userFolderRequest.Fields = "files(id)";
            var userFolderResponse = await userFolderRequest.ExecuteAsync();
            var userFolder = userFolderResponse.Files.FirstOrDefault();

            if (userFolder == null)
            {
                TempData["error"] = "User folder not found in MCD."; //for the user
                return NotFound("User folder not found in MCD."); //for the developer
            }
            string UserFolderId = userFolder.Id;

            //here is the actual searching of the user requested file 
            var fileRequest = driveService.Files.List();
            fileRequest.Q = $"name = '{documentId + "-" + fileName}' and '{UserFolderId}' in parents and trashed = false";
            fileRequest.Fields = "files(id, name, webViewLink, mimeType)"; //webViewLink in order to see it without needing for any downloading, and mimeType if user wants to update the file
            var fileResponse = await fileRequest.ExecuteAsync();
            var file = fileResponse.Files.FirstOrDefault();

            if (file == null)
            {
                TempData["error"] = "File not found in the user's folder."; //for the user
                return NotFound("File not found in the user's folder."); //for the developer
            }


            // to download the file using its ID
            //var getRequest = driveService.Files.Get(file.Id);
            //var stream = new MemoryStream();
            //await getRequest.DownloadAsync(stream);
            //stream.Position = 0;
            //return File(stream, file.MimeType); // so that we return it as to download



            _UnitOfWork.AuditLog.Add(new AuditLog() //in all cases log the action
            {
                ApplicationUserId = userId,
                userEmailAddress = currentUser.Email,
                Action = "Accessed",
                FileName = file.Name,
                ActionDate = DateTime.Now
            });
            _UnitOfWork.Save(); //save the changes after adding the log
            string fileUrl;
            fileUrl = file.WebViewLink;


            return Json(new { fileUrl }); // in order to see it without needing to download

        }

        [HttpGet]
        public IActionResult GetAllAccessManagements() //to get all shared documents in datatables API
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            List<SharedDocument> SharedDocumentList = _UnitOfWork.SharedDocument.GetAll(u => u.SharedFromId == userId,
                includeProperties: "Document").ToList();
            return Json(new { data = SharedDocumentList });

        }

        #endregion


    }
}
