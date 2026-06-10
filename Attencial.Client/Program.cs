using Attencial.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5158")
});

await builder.Build().RunAsync();
// 4. Register Dashboard Services
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<ProfessorService>();
builder.Services.AddScoped<AdminService>();
