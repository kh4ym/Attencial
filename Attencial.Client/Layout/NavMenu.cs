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
			__builder.AddMarkupContent(9, "<a href=\"/\" class=\"flex items-center gap-1 md:gap-2 no-underline\" b-c8fp1rjic8><span class=\"material-symbols-outlined text-primary text-xl md:text-2xl animate-blink\" b-c8fp1rjic8>visibility</span>\n                <span class=\"font-display-lg text-xl md:text-headline-md font-bold text-on-surface tracking-tighter\" b-c8fp1rjic8>Attencial</span></a>");
			__builder.OpenElement(10, "nav");
			__builder.AddAttribute(11, "class", "hidden md:flex gap-6");
			__builder.AddAttribute(12, "b-c8fp1rjic8");
			if (isLoggedIn)
			{
				__builder.OpenElement(13, "a");
				__builder.AddAttribute(14, "href", "dashboard");
				__builder.AddAttribute(15, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("dashboard")));
				__builder.AddEventPreventDefaultAttribute(16, "onclick", value: true);
				__builder.AddAttribute(17, "class", "font-label-caps text-label-caps " + (GetActive("dashboard") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
				__builder.AddAttribute(18, "b-c8fp1rjic8");
				__builder.AddContent(19, "Dashboard");
				__builder.CloseElement();
				if (userRole != "Professor")
				{
					__builder.AddMarkupContent(210, "\n                    ");
					__builder.OpenElement(211, "a");
					__builder.AddAttribute(212, "href", "attendance");
					__builder.AddAttribute(213, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("attendance")));
					__builder.AddEventPreventDefaultAttribute(214, "onclick", value: true);
					__builder.AddAttribute(215, "class", "font-label-caps text-label-caps " + (GetActive("attendance") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(216, "b-c8fp1rjic8");
					__builder.AddContent(217, "Attendance");
					__builder.CloseElement();
				}
				if (userRole == "Professor")
				{
					__builder.OpenElement(24, "a");
					__builder.AddAttribute(25, "href", "session");
					__builder.AddAttribute(251, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("session")));
					__builder.AddEventPreventDefaultAttribute(252, "onclick", value: true);
					__builder.AddAttribute(26, "class", "font-label-caps text-label-caps " + (GetActive("session") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(27, "b-c8fp1rjic8");
					__builder.AddContent(28, "Start Session");
					__builder.CloseElement();
					__builder.OpenElement(42, "a");
					__builder.AddAttribute(43, "href", "enrollment-review");
					__builder.AddAttribute(431, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("enrollment-review")));
					__builder.AddEventPreventDefaultAttribute(432, "onclick", value: true);
					__builder.AddAttribute(44, "class", "font-label-caps text-label-caps " + (GetActive("enrollment-review") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(45, "b-c8fp1rjic8");
					__builder.AddContent(46, "Review");
					__builder.CloseElement();
					__builder.AddMarkupContent(47, "\n                        ");
					__builder.OpenElement(48, "a");
					__builder.AddAttribute(49, "href", "professor-dashboard");
					__builder.AddAttribute(491, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("professor-dashboard")));
					__builder.AddEventPreventDefaultAttribute(492, "onclick", value: true);
					__builder.AddAttribute(50, "class", "font-label-caps text-label-caps " + (GetActive("professor-dashboard") ? "text-primary border-b-2 border-primary pb-1" : "text-on-surface-variant hover:text-primary") + " transition-colors no-underline");
					__builder.AddAttribute(51, "b-c8fp1rjic8");
					__builder.AddContent(52, "Analytics");
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(53, "a");
					__builder.AddAttribute(54, "href", "courses");
					__builder.AddAttribute(541, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("courses")));
					__builder.AddEventPreventDefaultAttribute(542, "onclick", value: true);
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
				__builder.OpenElement(620, "a");
				__builder.AddAttribute(621, "href", "profile");
				__builder.AddAttribute(622, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("profile")));
				__builder.AddEventPreventDefaultAttribute(623, "onclick", value: true);
				__builder.AddAttribute(624, "class", "material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors no-underline");
				__builder.AddAttribute(625, "b-c8fp1rjic8");
				__builder.AddAttribute(626, "title", "Profile");
				__builder.AddAttribute(627, "aria-label", "Profile");
				__builder.AddContent(628, "person");
				__builder.CloseElement();
				__builder.OpenElement(63, "button");
				__builder.AddAttribute(64, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Logout));
				__builder.AddAttribute(65, "class", "material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors bg-transparent border-0 cursor-pointer");
				__builder.AddAttribute(66, "b-c8fp1rjic8");
				__builder.AddAttribute(67, "title", "Logout");
				__builder.AddAttribute(69, "aria-label", "Logout");
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

			// Mobile bottom navigation bar
			if (isLoggedIn)
			{
				__builder.OpenElement(71, "nav");
				__builder.AddAttribute(72, "class", "mobile-nav");
				__builder.AddAttribute(73, "b-c8fp1rjic8");
				__builder.OpenElement(74, "div");
				__builder.AddAttribute(75, "class", "mobile-nav-inner");
				__builder.AddAttribute(76, "b-c8fp1rjic8");


				// Dashboard / Analytics
				var dashHref = userRole == "Professor" ? "professor-dashboard" : "dashboard";
				var dashLabel = userRole == "Professor" ? "Analytics" : "Dashboard";
				__builder.OpenElement(82, "a");
				__builder.AddAttribute(83, "href", dashHref);
				__builder.AddAttribute(831, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab(dashHref)));
				__builder.AddEventPreventDefaultAttribute(832, "onclick", value: true);
				__builder.AddAttribute(84, "class", "mobile-nav-item " + (GetActive(dashHref) ? "active" : ""));
				__builder.AddAttribute(85, "b-c8fp1rjic8");
				__builder.AddMarkupContent(86, "<span class=\"material-symbols-outlined nav-icon\">" + (userRole == "Professor" ? "insights" : "space_dashboard") + "</span>\n                    <span class=\"nav-label\">" + dashLabel + "</span>");
				__builder.CloseElement();

				// Dashboard (professors only)
				if (userRole == "Professor")
				{
					__builder.OpenElement(87, "a");
					__builder.AddAttribute(88, "href", "dashboard");
					__builder.AddAttribute(881, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("dashboard")));
					__builder.AddEventPreventDefaultAttribute(882, "onclick", value: true);
					__builder.AddAttribute(89, "class", "mobile-nav-item " + (GetActive("dashboard") ? "active" : ""));
					__builder.AddAttribute(90, "b-c8fp1rjic8");
					__builder.AddMarkupContent(91, "<span class=\"material-symbols-outlined nav-icon\">space_dashboard</span>\n                    <span class=\"nav-label\">Dashboard</span>");
					__builder.CloseElement();
				}



					// Attendance (students only)
					if (userRole != "Professor")
					{
						__builder.OpenElement(220, "a");
						__builder.AddAttribute(221, "href", "attendance");
						__builder.AddAttribute(2211, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("attendance")));
						__builder.AddEventPreventDefaultAttribute(2212, "onclick", value: true);
						__builder.AddAttribute(222, "class", "mobile-nav-item " + (GetActive("attendance") ? "active" : ""));
						__builder.AddAttribute(223, "b-c8fp1rjic8");
						__builder.AddMarkupContent(224, "<span class=\"material-symbols-outlined nav-icon\">assignment_turned_in</span>\n                    <span class=\"nav-label\">Attendance</span>");
						__builder.CloseElement();
					}
				if (userRole == "Professor")
				{
					// Session
					__builder.OpenElement(87, "a");
					__builder.AddAttribute(88, "href", "session");
					__builder.AddAttribute(883, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("session")));
					__builder.AddEventPreventDefaultAttribute(884, "onclick", value: true);
					__builder.AddAttribute(89, "class", "mobile-nav-item " + (GetActive("session") ? "active" : ""));
					__builder.AddAttribute(90, "b-c8fp1rjic8");
					__builder.AddMarkupContent(91, "<span class=\"material-symbols-outlined nav-icon\">play_circle</span>\n                    <span class=\"nav-label\">Session</span>");
					__builder.CloseElement();
				}
				else
				{
					// Courses
					__builder.OpenElement(92, "a");
					__builder.AddAttribute(93, "href", "courses");
					__builder.AddAttribute(931, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("courses")));
					__builder.AddEventPreventDefaultAttribute(932, "onclick", value: true);
					__builder.AddAttribute(94, "class", "mobile-nav-item " + (GetActive("courses") ? "active" : ""));
					__builder.AddAttribute(95, "b-c8fp1rjic8");
					__builder.AddMarkupContent(96, "<span class=\"material-symbols-outlined nav-icon\">school</span>\n                    <span class=\"nav-label\">Courses</span>");
					__builder.CloseElement();
				}

				if (userRole == "Professor")
				{
					// Review
					__builder.OpenElement(97, "a");
					__builder.AddAttribute(98, "href", "enrollment-review");
					__builder.AddAttribute(981, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateToTab("enrollment-review")));
					__builder.AddEventPreventDefaultAttribute(982, "onclick", value: true);
					__builder.AddAttribute(99, "class", "mobile-nav-item " + (GetActive("enrollment-review") ? "active" : ""));
					__builder.AddAttribute(100, "b-c8fp1rjic8");
					__builder.AddMarkupContent(101, "<span class=\"material-symbols-outlined nav-icon\">rate_review</span>\n                    <span class=\"nav-label\">Review</span>");
					__builder.CloseElement();
				}

				__builder.CloseElement();
				__builder.CloseElement();
			}
		}

		protected override async Task OnInitializedAsync()
		{
			currentUri = Nav.ToBaseRelativePath(Nav.Uri);
			Nav.LocationChanged += OnLocationChanged;
			await RefreshAuthState();
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

		private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
		{
			currentUri = Nav.ToBaseRelativePath(e.Location);
			await RefreshAuthState();
			StateHasChanged();
		}

		private async Task RefreshAuthState()
		{
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			isLoggedIn = !string.IsNullOrEmpty(text) && text != "null" && text != "undefined";
			if (isLoggedIn)
			{
				await LoadRole(text);
				return;
			}
			userRole = string.Empty;
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

		private void NavigateToTab(string target)
		{
			var currentRelative = currentUri.Split('?')[0].Trim('/');
			var targetRelative = target.Trim('/');
			if (currentRelative.Equals(targetRelative, StringComparison.OrdinalIgnoreCase))
			{
				Nav.NavigateTo(target, forceLoad: true);
			}
			else
			{
				Nav.NavigateTo(target);
			}
		}

		public void Dispose()
		{
			Nav.LocationChanged -= OnLocationChanged;
		}
	}
}
