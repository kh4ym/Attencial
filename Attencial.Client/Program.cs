using System;
using System.Net.Http;
using System.Threading.Tasks;
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

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(apiUrl!)
        });

        await builder.Build().RunAsync();
    }
}
