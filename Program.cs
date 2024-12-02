var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => {
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
});

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
