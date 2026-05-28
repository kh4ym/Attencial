using Attencial.Client;
using Attencial.Client.Components;
using Attencial.Client.Layout;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Layout
{
	public class NavMenu : ComponentBase, IDisposable
	{
		private bool isLoggedIn;

		private string userRole = string.Empty;

		private string currentUri = string.Empty;

		[Inject]
		private HttpClient Http { get; set; }

		[Inject]
		private NavigationManager Nav { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenElement(0, "header");
			__builder.AddAttribute(1, "class", "bg-background border-b border-on-surface-variant/20 fixed top-0 left-0 right-0 z-50 h-16 flex items-center");
			__builder.AddAttribute(2, "b-c8fp1rjic8");
			__builder.OpenElement(3, "div");
			__builder.AddAttribute(4, "class", "w-full max-w-max-width mx-auto px-margin-mobile md:px-margin-desktop flex justify-between items-center");
			__builder.AddAttribute(5, "b-c8fp1rjic8");
			__builder.OpenElement(6, "div");
			__builder.AddAttribute(7, "class", "flex items-center gap-8");
			__builder.AddAttribute(8, "b-c8fp1rjic8");
			__builder.AddMarkupContent(9, "<a href=\"/\" class=\"flex items-center gap-2 no-underline\" b-c8fp1rjic8><span class=\"material-symbols-outlined text-primary text-2xl animate-blink\" b-c8fp1rjic8>visibility</span>\n                <span class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter\" b-c8fp1rjic8>Attencial</span></a>\n            ");
			__builder.OpenElement(10, "nav");
			__builder.AddAttribute(11, "class", "hidden md:flex gap-6");
			__builder.AddAttribute(12, "b-c8fp1rjic8");
			if (isLoggedIn)
			{
				__builder.OpenElement(13, "a");
				__builder.AddAttribute(14, "href", "dashboard");
				__builder.AddAttribute(15, "class", "font-label-caps text-label-caps " + (GetActive("dashboard") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
				__builder.AddAttribute(16, "b-c8fp1rjic8");
				__builder.AddContent(17, "Dashboard");
				__builder.CloseElement();
				if (userRole == "Professor")
				{
					__builder.OpenElement(24, "a");
					__builder.AddAttribute(25, "href", "session");
					__builder.AddAttribute(26, "class", "font-label-caps text-label-caps " + (GetActive("session") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(27, "b-c8fp1rjic8");
					__builder.AddContent(28, "Start Session");
					__builder.CloseElement();
					__builder.OpenElement(42, "a");
					__builder.AddAttribute(43, "href", "enrollment-review");
					__builder.AddAttribute(44, "class", "font-label-caps text-label-caps " + (GetActive("enrollment-review") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(45, "b-c8fp1rjic8");
					__builder.AddContent(46, "Review");
					__builder.CloseElement();
					__builder.AddMarkupContent(47, "\n                        ");
					__builder.OpenElement(48, "a");
					__builder.AddAttribute(49, "href", "professor-dashboard");
					__builder.AddAttribute(50, "class", "font-label-caps text-label-caps " + (GetActive("professor-dashboard") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(51, "b-c8fp1rjic8");
					__builder.AddContent(52, "Analytics");
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(53, "a");
					__builder.AddAttribute(54, "href", "courses");
					__builder.AddAttribute(55, "class", "font-label-caps text-label-caps " + (GetActive("courses") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(56, "b-c8fp1rjic8");
					__builder.AddContent(57, "Courses");
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(58, "\n\n        ");
			__builder.OpenElement(59, "div");
			__builder.AddAttribute(60, "class", "flex items-center gap-4");
			__builder.AddAttribute(61, "b-c8fp1rjic8");
			if (isLoggedIn)
			{
				__builder.AddMarkupContent(62, "<a href=\"profile\" class=\"material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors no-underline\" b-c8fp1rjic8 title=\"Profile\">person</a>\n                ");
				__builder.OpenElement(63, "button");
				__builder.AddAttribute(64, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Logout));
				__builder.AddAttribute(65, "class", "material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors bg-transparent border-0 cursor-pointer");
				__builder.AddAttribute(66, "b-c8fp1rjic8");
				__builder.AddAttribute(67, "title", "Logout");
				__builder.AddContent(68, "logout");
				__builder.CloseElement();
			}
			else
			{
				__builder.AddMarkupContent(68, "<a href=\"login\" class=\"font-label-caps text-label-caps bg-primary text-surface px-6 py-2 hover:bg-[#f05454] transition-colors no-underline\" b-c8fp1rjic8>LOGIN</a>\n                ");
				__builder.AddMarkupContent(69, "<a href=\"register\" class=\"font-label-caps text-label-caps bg-surface text-on-surface border border-on-surface px-6 py-2 hover:bg-surface-variant transition-colors no-underline\" b-c8fp1rjic8>SIGN UP</a>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(70, "\n\n<div class=\"h-16\" b-c8fp1rjic8></div>");
		}

		protected override async Task OnInitializedAsync()
		{
			currentUri = Nav.ToBaseRelativePath(Nav.Uri);
			Nav.LocationChanged += OnLocationChanged;
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			isLoggedIn = !string.IsNullOrEmpty(text) && text != "null" && text != "undefined";
			if (isLoggedIn)
			{
				await LoadRole(text);
			}
		}

		private async Task LoadRole(string token)
		{
			_ = 1;
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using (JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync()))
					{
						userRole = jsonDocument.RootElement.GetProperty("data").GetProperty("role").GetString() ?? string.Empty;
						return;
					}
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[NavMenu] Role load failed: {ex.Message}");
			}
		}

		private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
		{
			currentUri = Nav.ToBaseRelativePath(e.Location);
			StateHasChanged();
		}

		private bool GetActive(string path)
		{
			return currentUri.Split('?')[0].Equals(path, StringComparison.OrdinalIgnoreCase);
		}

		private async Task Logout()
		{
			await JS.InvokeVoidAsync("authStorage.removeToken");
			Nav.NavigateTo("/login", forceLoad: true);
		}

		public void Dispose()
		{
			Nav.LocationChanged -= OnLocationChanged;
		}
	}
}
