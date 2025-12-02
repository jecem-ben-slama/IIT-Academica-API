using Blazored.LocalStorage;
using IIT_Academica_Front.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace IIT_Academica_Front
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            // 1. Register Blazored.LocalStorage
            builder.Services.AddBlazoredLocalStorage();

            // 2. Register the Authentication Services
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

            // 3. Register our Custom Services
           builder.Services.AddScoped<AuthService>();
           builder.Services.AddScoped<UserService>();
           // builder.Services.AddScoped<SubjectService>();
          //builder.Services.AddScoped<NotificationService>(); 
          builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5030") });

            await builder.Build().RunAsync();
        }
    }
}
