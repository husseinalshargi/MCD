using MCD.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// to Add services to the container. -> add in thus file
builder.Services.AddControllersWithViews();
// to add the app db context
builder.Services.AddDbContext<ApplicationDbContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // x=>y : lambda where x is the input and y is the function

// the option is that the user cannot log in without conforming his account (place it in <ident...>(options => options.SignIn.RequireConfirmedAccount = true)
// the next part is to add all tables of identity in db
builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages(); //so that it handles razor pages where there is only area-page
//such as in authentication




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