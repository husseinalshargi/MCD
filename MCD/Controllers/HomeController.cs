using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using MCD.DataAccess.Data;
using MCD.DataAccess.Repository;
using MCD.DataAccess.Repository.IRepository;
using MCD.Models;
using MCD.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MCD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _UnitOfWork;
        [BindProperty]
        public DocumentVM DocumentVM { get; set; }

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _UnitOfWork = unitOfWork;
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

                string StoragePath = "C:\\Users\\xskyx\\source\\repos\\MCD\\MCD\\Storage\\";
                string UserStorage = StoragePath + $"\\{userId}";
                // create a new folder if the folder of the user does not exist, named with the id of the user
                if (!Directory.Exists(UserStorage)) 
                {
                    Directory.CreateDirectory(UserStorage);
                }

                //so that we don't create a category when we have a document with a default type
                Category DefaultCategory = _UnitOfWork.Category.Get(u => u.ApplicationUserId == userId && u.CategoryName == "---");
                if (DefaultCategory == null) 
                {
                     DefaultCategory = new Category() { ApplicationUserId = userId, CategoryName = "---" };
                }


                //to save the document
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

                var FilePath = Path.Combine(UserStorage, document.FileName); //the path of the content of the file
                FileStream stream = new FileStream(FilePath, FileMode.Create); //creating a new file place
                using (stream)
                {
                    await model.DocumentFile.CopyToAsync(stream);
                    await stream.FlushAsync(); //ensure that all the content is written inside the folder before closing the stream
                }

                _UnitOfWork.Document.Add(document);
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

        #endregion


    }
}
