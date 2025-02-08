using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
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
            string[] scopes = { DriveService.Scope.Drive }; //to request permissions of the scopes (which are configured in the google api website)
            string ApplicationName = "MCD"; //to show it during authentication

            var GoogleAuth = _config.GetSection("Authentication:Google");
            string ClientId = GoogleAuth["client_id"]; // to get the value of it
            string ClientSecret = GoogleAuth["client_secret"]; //to get the key from app settings

            //create user credentials
            var clientSecrets = new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets, // for the api key
                scopes, //the things that is we can do with user data
                "user",//that the request for the user
                CancellationToken.None
                ); //here are the request

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            }); //create a new instance of the drive api service

        }


    }
}
