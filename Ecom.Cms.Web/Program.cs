using Ecom.Cms.Application.Authentication;
using Ecom.Cms.Application.User;
using Ecom.Cms.Web.Common.Auth;
using Ecom.Cms.Web.Common.AuthCookie;
using Ecom.Cms.Web.Common.Config;
using Ecom.Cms.Web.Common.HeaderHandler;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
// Đăng ký IHttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthenticationExtensions(builder.Configuration); //Authentication
builder.Services.AddScoped<AuthTokenCookie>(); // cấu hình cookie lưu token
builder.Services.AddTransient<AuthenticationHeaderHandler>(); // cấu hình kiểm tra token 401
builder.Services.AddApplicationHeadeHandler(builder.Configuration); // cấu hình header handler


//cấu hình appsetting
builder.Services.AddConfigAppSetting(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

// application DI
builder.Services.AddApplicationAuthenticationDependencyInjection(builder.Configuration);
builder.Services.AddApplicationUserDependencyInjection(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession(); // Phải nằm trước Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
