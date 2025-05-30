﻿using MCD.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCD.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]        //to make sure that the user is authenticated before accessing the controller
    public class ShareMCDController : Controller
    {
        //for sending emails 
        private readonly IEmailSender _emailSender;
        public ShareMCDController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }
        public IActionResult ThanksForSharing(string emailToShare)
        {
            // Send an email using the EmailSender service to the email address provided in the form to the user for sharing the MCD webapp
            string subject = "Your friend invited you to join MyCleverDoc family";
            string htmlMessage = "<h1>MyCleverDoc</h1><p>Your friend tried to share a document with you, to access the document. Click the link below to join us.</p><a href='https://localhost:7031/Identity/Account/Register'>Join Now</a>";
            try
            {
                _emailSender.SendEmailAsync(emailToShare, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["error"] = "Error sending email. Please try again later or try another email..";
            }
            return View();
        }
        public IActionResult Support()
        {
            return View();
        }
    }
}
