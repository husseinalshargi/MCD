using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace MCD.Utility
{
    public class GoogleDriveService
    {
        private readonly IConfiguration _config; //to use app settings for the apy key
        public GoogleDriveService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<DriveService> GetDriveService() //in order to interact with google drive
        {
            string[] scopes = { DriveService.Scope.DriveFile }; //to request permissions of the scopes (which are configured in the google api website)
            string ApplicationName = "MCD"; //to show it during authentication

            var GoogleAuth = _config.GetSection("GoogleDrive");
            string ClientId = GoogleAuth["client_id"]; // to get the value of it
            string ClientSecret = GoogleAuth["client_secret"]; //to get the key from app settings
            string RefreshToken = GoogleAuth["refresh_token"];

            //create OAuth client credentials
            var clientSecrets = new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };


            string tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "GoogleTokens"); //if there is a file named googletokens it will take the path


            UserCredential credential;

            if (!string.IsNullOrEmpty(RefreshToken)) // Use a saved refresh token if available
            {
                credential = new UserCredential(
                    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = clientSecrets,// for the api key
                        Scopes = scopes //the things that is we can do with user data
                    }),
                    "user", //that the request for the user
                    new TokenResponse { RefreshToken = RefreshToken }
                );// the request
            }
            else //if the token isn't working then create a new one and request new authorization
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    clientSecrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)); //if there aren't token path then it will create the file
            }

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            }); //create a new instance of the drive api service

        }
        //to give permission to access file in google drive (used when creating a new file, or granting permission to another user to an existing file)
        public static void GiveFilePermission(DriveService Service, string role, string FileId, string UserEmail)
        {
            //in order to create the permission
            var Permission = new Permission()
            {
                Type = "user", //in order to access the file
                Role = role, //so that the user can only see or edit the file also
                EmailAddress = UserEmail //the email of the user
            };

            //create the permission
            var request = Service.Permissions.Create(Permission, FileId).Execute();
        }

        public static async Task RemoveFilePermission(DriveService Service, string FileId, string UserEmail)
        {
            try
            {
                //get the list of only permissions
                var request = Service.Permissions.List(FileId);
                request.Fields = "permissions(id, emailAddress)"; // Get only permission ID and email
                var response = await request.ExecuteAsync();
                var permission = response.Permissions.FirstOrDefault(p => p.EmailAddress == UserEmail);  //find only the permission that we want to remove
                //remove the permission
                await Service.Permissions.Delete(FileId, permission.Id).ExecuteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing permission: {ex.Message}");

            }
        }
    }
}
