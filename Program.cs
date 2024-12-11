using backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => {
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
});
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));


var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error/Index");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/NotFound", "?statusCode={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}");

app.Run();
