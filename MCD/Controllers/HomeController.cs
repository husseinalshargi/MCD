using System.Diagnostics;
using System.Security.Claims;
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


            // in order to return a list of all the documents, or a single one for uploading:
            List<Document> DocumentList = _UnitOfWork.Document.GetAll().Where(u => u.ApplicationUserId == userId).ToList(); //to convert from IEnumerable object to List object to pass to the index page

            var viewModel = new DocumentVM //as we will handle showing the documents and update new ones in the same place
            {
                documents = DocumentList
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UploadDocument(DocumentVM model) //put here the input
        {
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
    }
}
