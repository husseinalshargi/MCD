using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MCD.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config; //to use app settings for the apy key
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }


        public System.Threading.Tasks.Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var BrevoSection = _config.GetSection("Brevo"); //to get the section from app settings for the email sender api key
            string APIKey = BrevoSection["APIKey"];
            if (IsValidEmail(email))
            {
                try
                {
                    //api configuration for brevo
                    Configuration.Default.ApiKey["api-key"] = APIKey; //set the api key for the email sender
                    var APIInstance = new TransactionalEmailsApi(); //create an instance of the email sender api

                    //create an email
                    var emailToSend = new SendSmtpEmail(
                        sender: new SendSmtpEmailSender("MyCleverDoc", "mycleverdoc@hotmail.com"),
                        to: new List<SendSmtpEmailTo> { new SendSmtpEmailTo(email, email) },
                        subject: subject,
                        htmlContent: htmlMessage
                    );

                    return APIInstance.SendTransacEmailAsync(emailToSend); //send the email
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    return System.Threading.Tasks.Task.CompletedTask; // return a completed task instead of null (to not crash the app)
                }
            }
            else
            {
                Console.WriteLine($"Invalid email address: {email}");
                return System.Threading.Tasks.Task.CompletedTask; // return a completed task instead of null (to not crash the app)
            }
        }
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                email = email.Trim();
                var addr = new System.Net.Mail.MailAddress(email);

                // Extra check: domain must contain at least one dot (e.g., "gmail.com")
                var domain = addr.Host;
                return domain.Contains('.') && addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
