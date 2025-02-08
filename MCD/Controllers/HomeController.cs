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
using Microsoft.AspNetCore.Mvc;

namespace MCD.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _StoragePath = "C:\\Users\\xskyx\\source\\repos\\MCD\\MCD\\Storage\\";
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
                documents = DocumentList
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

                //to store it locally
                //string StoragePath = "C:\\Users\\xskyx\\source\\repos\\MCD\\MCD\\Storage\\";
                //string UserStorage = StoragePath + $"\\{userId}";
                // create a new folder if the folder of the user does not exist, named with the id of the user
                //if (!Directory.Exists(UserStorage)) 
                //{
                //    Directory.CreateDirectory(UserStorage);
                //}

                //to get the file in IFormFile obj
                if (model.DocumentFile == null)
                {
                    return BadRequest("No file selected."); //as the user did not upload a file
                }
                // in order to write it in the description
                string userEmail = _UnitOfWork.ApplicationUser.Get(u=>u.Id == userId).Email; // to get the user email

                
                //use the google drive class from the utilities
                var DriveService = await _GoogleDriveService.GetDriveService(); // await because it is defined like this 

                //check if there is a file in google drive called {userid} or not (create one if not)
                var ListRequest = DriveService.Files.List(); //to get all the folders
                ListRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder'"; // Searching for the parent folder by name type: that it is a folder
                ListRequest.Fields = "files(id, name)"; //to get the file id and name
                var result = ListRequest.Execute(); //the search happens

                //here is the logic of searching for a file, if it wasn't here create one
                string parentFolderId = null; // to put in the parent name (folder)
                if (result.Files.Count == 0) // if not found
                {
                    var folderMetaData = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = userId, // Folder name
                        MimeType = "application/vnd.google-apps.folder",
                    };

                    var createRequest = DriveService.Files.Create(folderMetaData);
                    createRequest.Fields = "id";
                    var folder = createRequest.Execute();
                    parentFolderId = folder.Id; // Use the created folder's ID as the parent
                }
                else
                {
                    parentFolderId = result.Files[0].Id; // Use the existing folder's id as the parent
                }


                var FileMetaData = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = model.DocumentFile.FileName,
                    Parents = new List<string> { parentFolderId }, //the name of the folder in the drive
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
                }

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

                if (model.Summarize == true)
                {
                    //summarization logic 
                }

            }




            return RedirectToAction(nameof(Index));
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

            List<Document> DocumentList = _UnitOfWork.Document.GetAll(includeProperties: "Category").Where(u => u.ApplicationUserId == userId).ToList();
                        
            return Json(new {data =  DocumentList});
        }
        [HttpGet]
        public async Task<IActionResult> GetDocument(string userId, string fileName)
        {
            //to determine whether the user can access this document or not
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var currentUserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value; //to get the id of the current requesting user
            //here it will compare if this is the correct user or not
            if (currentUserId != userId)
            {
                return RedirectToAction(nameof(Error));
            }



            var driveService = await _GoogleDriveService.GetDriveService(); // to connect to google drive
            //to search for the document
            var request = driveService.Files.List(); //list of all the files
            request.Q = $"name = '{fileName}' and '{userId}' in owners";
            request.Fields = "files(id, name, webViewLink)"; // get these values

            var response = await request.ExecuteAsync();
            var file = response.Files.FirstOrDefault(); // get the first matching file

            if (file == null)
            {
                return NotFound("File not found in Google Drive.");
            }

            //var filePath = Path.Combine(_StoragePath, userId, fileName); // the path of the document
            
            // to download the file using its ID
            //var getRequest = driveService.Files.Get(file.Id);
            //var stream = new MemoryStream();
            //await getRequest.DownloadAsync(stream);
            //stream.Position = 0;
            //return File(stream, file.MimeType); // so that we return it as to download

            return Json(new {fileUrl = file.WebViewLink}); // in order to see it without needing to download

        }


        #endregion


    }
    }
