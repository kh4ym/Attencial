using System;
using System.Net.Http;
using System.Threading.Tasks;
using Attencial.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Attencial.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var apiUrl = !string.IsNullOrEmpty(builder.Configuration["ApiBaseUrl"])
            ? builder.Configuration["ApiBaseUrl"]
            : builder.HostEnvironment.BaseAddress;

        builder.Services.AddTransient<UnauthorizedHttpHandler>();
        builder.Services.AddScoped(sp =>
        {
            var handler = sp.GetRequiredService<UnauthorizedHttpHandler>();
            handler.InnerHandler = new HttpClientHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(apiUrl!)
            };
        });

        await builder.Build().RunAsync();
    }
}

