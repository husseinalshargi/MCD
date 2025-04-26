using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Utility
{
    public class MCDAIFunctions
    {
        private readonly GoogleDriveService _GoogleDriveService; //to use the google drive service
        public MCDAIFunctions(GoogleDriveService googleDriveService)
        {
            _GoogleDriveService = googleDriveService;
        }
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _baseUrl = "http://localhost:8001"; //the base url of the AI service (without the endpoint)
        public async Task<string> SendDataAsync(string endpoint, int fileId, string fileName, string userId) //handles the sending of data to the AI service custom api
        {
            //if the api not working we will use try catch to handle the error
            try { 
            var DriveService = await _GoogleDriveService.GetDriveService(); //get the google drive service instance
                                                                            // Get the google file ID from the method created in the GoogleDriveService
            string googleFileId = await GoogleDriveService.GetGoogleDriveFileId(DriveService, fileId, fileName, userId);

            // to download the file using its google id
            var getRequest = DriveService.Files.Get(googleFileId); //create a request to get the file
            var stream = new MemoryStream(); //create a memory stream to store the file content
            await getRequest.DownloadAsync(stream); //download the file content to the memory stream
            stream.Position = 0; //reset stream position for reading to make sure it starts from the beginning

                using (var content = new MultipartFormDataContent()) //creates a multipart form request
                {
                    var fileContent = new StreamContent(stream); //converts from file stream into HTTP content
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream"); //set the content type of the file

                    //attach the file to the request
                    content.Add(fileContent, "given_file", fileName);

                    //send the HTTP request to the Python API
                    HttpResponseMessage response = await _httpClient.PostAsync(_baseUrl + endpoint, content);

                    //return API response if it is successful
                    if (response.IsSuccessStatusCode)
                    {
                        string rawResponse = await response.Content.ReadAsStringAsync();

                        // Convert escaped \n into real newlines
                        string formattedResponse = rawResponse.Replace("\\n", "\n");

                        return formattedResponse;
                    }

                    return null;
                }
            }
            catch (Exception e) 
            { 
                Console.WriteLine($"Error: {e.Message}"); //log the error message
                return null; //return null if there is an error
            }
        }


    }
}
