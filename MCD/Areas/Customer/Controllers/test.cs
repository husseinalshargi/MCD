using Microsoft.AspNetCore.Mvc;
using MCD.Utility;
using System;

namespace MCD.Areas.Customer.Controllers
{
    public class Test : Controller
    {
        public async Task<IActionResult> Index()
        {
            string filePath = @"C:\\Users\\xskyx\\OneDrive\\سطح المكتب\\Gr118 book\\GR118 Ch5.pdf"; // Change to your actual file path

            using (FileStream stream = File.OpenRead(filePath))
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
            string response = await TestMCDClient.PostAsync("api/users", newUser);
            ViewBag.ApiResponse = response;

            return View();

        }
    }
}
