var builder = WebApplication.CreateBuilder(args);

// to Add services to the container. -> add in thus file
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();
// here are the dafault route if there is any error
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//ff
//ff
// third comment
app.Run();
