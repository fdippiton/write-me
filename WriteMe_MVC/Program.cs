using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
{
    option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    option.AccessDeniedPath = "/Home/Privacy";

    //option.Events = new CookieAuthenticationEvents
    //{
    //    OnValidatePrincipal = context =>
    //    {
    //        // Verificar si el tiempo del token ha expirado
    //        var expirationTime = context.Properties.ExpiresUtc;
    //        if (expirationTime.HasValue && expirationTime.Value < DateTimeOffset.UtcNow)
    //        {
    //            // Limpiar la cookie de autenticación
    //            context.RejectPrincipal();
    //            context.Response.Redirect("/Home/Index"); // Puedes ajustar la ruta de redirección según tus necesidades
    //        }

    //        return Task.CompletedTask;
    //    }
    //};
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
