using MCD.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MCD.Models;
using MCD.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using MCD.DataAccess.Repository.IRepository;
using MCD.DataAccess.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// to Add services to the container. -> add in thus file
builder.Services.AddControllersWithViews();
// to add the app db context
builder.Services.AddDbContext<ApplicationDbContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // x=>y : lambda where x is the input and y is the function

// the option is that the user cannot log in without conforming his account (place it in <ident...>(options => options.SignIn.RequireConfirmedAccount = true)
// the next part is to add all tables of identity in db
//after creating a class to implement from identityuser -> to extend the table of user. you should replace here from identityuser to the class that extends from it
//also don't forget to change all identityuser to applicationuser -> in all the identity pages
//to add the roles also you should make the function addidentity rhather than addefaultidentity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders(); //to add more columns to the user table you should extend IdentityUser and replace the one in here

//in order to add google authentication:
builder.Services.AddAuthentication().AddCookie().AddGoogle(option =>
{
    var GoogleAuth = builder.Configuration.GetSection("Authentication:Google"); //in order to get it from app settings
    option.ClientId = GoogleAuth["client_id"];
    option.ClientSecret = GoogleAuth["client_secret"];
    option.CallbackPath = "/signin-google";
});

//to add google driva service
builder.Services.AddSingleton<GoogleDriveService>();


builder.Services.AddRazorPages(); //so that it handles razor pages where there is only area-page
//such as in authentication

//in order to send emails in the future edit the send email file
builder.Services.AddScoped<IEmailSender, EmailSender>(); // to tell asp core that the implementation of sending emails will be in email sender

//every time you add a repo you should register it here in order to use dependency injection
//edit: rather than adding one for each repo we could use only one (the one that implement all)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


var app = builder.Build();
//when you add new app settings edit here
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); //if user username and pass is valid then go to use authorization
app.UseAuthorization();
//here it will handle razor pages also
app.MapRazorPages();
// here are the default route if there is any error
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();