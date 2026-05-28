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
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/login")]
	public class Login : ComponentBase
	{
		private string email = string.Empty;

		private string password = string.Empty;

		private bool showPassword;

		private bool rememberMe;

		private string errorMessage = string.Empty;

		private bool isLoading;

		[Inject]
		private NavigationManager Nav { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		[Inject]
		private HttpClient Http { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenComponent<PageTitle>(0);
			__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.AddMarkupContent(2, "Log In — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "font-body-md text-on-background canvas-bg h-screen overflow-hidden relative");
			__builder.AddMarkupContent(6, "<div class=\"absolute inset-0 pointer-events-none overflow-hidden\"><div class=\"absolute w-64 h-64 rounded-full border-2 border-primary/20 top-[10%] -left-20 floating-slow\"></div>\n        <div class=\"absolute w-48 h-48 rounded-full border-2 border-tertiary/20 bottom-[15%] -right-16 floating-medium\"></div>\n        <div class=\"absolute w-32 h-32 rounded-full bg-primary/10 top-[40%] right-[10%] floating-fast\"></div>\n        <div class=\"absolute w-24 h-24 rounded-full border-2 border-primary/15 top-[60%] left-[15%] floating-medium\" style=\"animation-delay: -3s;\"></div>\n        <div class=\"absolute w-16 h-16 rounded-full bg-tertiary/10 bottom-[30%] left-[40%] floating-slow\" style=\"animation-delay: -5s;\"></div></div>\n\n    ");
			__builder.AddMarkupContent(7, "<header class=\"absolute top-0 left-0 w-full flex justify-between items-center px-margin-mobile md:px-margin-desktop h-16 z-50\" style=\"animation: gentle-rise 0.6s ease both;\"><a href=\"/\" class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter no-underline\">Attencial</a></header>\n\n    ");
			__builder.OpenElement(8, "main");
			__builder.AddAttribute(9, "class", "h-full flex items-center justify-center px-margin-mobile");
			__builder.OpenElement(10, "div");
			__builder.AddAttribute(11, "class", "relative z-10 w-full max-w-sm");
			__builder.AddAttribute(12, "style", "animation: gentle-rise 0.8s 0.15s ease both;");
			__builder.OpenElement(13, "div");
			__builder.AddAttribute(14, "class", "bg-surface neo-border p-6 md:p-10 neo-shadow");
			__builder.AddMarkupContent(15, "<div class=\"mb-6 text-center\"><h1 class=\"font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-1\">Welcome back</h1>\n                    <p class=\"font-body-md text-on-surface-variant opacity-80 text-sm\">Sign in to manage your academic journey.</p></div>");
			if (!string.IsNullOrEmpty(errorMessage))
			{
				__builder.OpenElement(16, "div");
				__builder.AddAttribute(17, "class", "border border-error/30 p-3 mb-6 flex items-start gap-2 text-sm");
				__builder.AddAttribute(18, "style", "background: rgba(186,26,26,0.04);");
				__builder.AddMarkupContent(19, "<span class=\"material-symbols-outlined text-error text-base\">error</span>\n                        ");
				__builder.OpenElement(20, "span");
				__builder.AddAttribute(21, "class", "text-on-surface-variant");
				__builder.AddContent(22, errorMessage);
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.OpenElement(23, "form");
			__builder.AddAttribute(24, "onsubmit", EventCallback.Factory.Create<EventArgs>((object)this, (Func<Task>)HandleLogin));
			__builder.AddAttribute(25, "class", "space-y-5");
			__builder.OpenElement(26, "div");
			__builder.AddAttribute(27, "class", "relative group");
			__builder.AddMarkupContent(28, "<label class=\"block font-label-caps text-label-caps mb-1 text-on-surface-variant group-focus-within:text-primary transition-colors\">UNIVERSITY EMAIL</label>\n                        ");
			__builder.OpenElement(29, "input");
			__builder.AddAttribute(30, "class", "w-full bg-transparent border-0 border-b border-on-surface-variant/40 focus:ring-0 focus:border-primary focus:border-b-2 transition-all py-1.5 px-0 font-body-md placeholder:text-on-surface-variant/30");
			__builder.AddAttribute(31, "placeholder", "name@academic.edu");
			__builder.AddAttribute(32, "type", "email");
			__builder.AddAttribute(33, "required");
			__builder.AddAttribute(34, "value", BindConverter.FormatValue(email));
			__builder.AddAttribute(35, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				email = __value;
			}, email));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(36, "\n\n                    ");
			__builder.OpenElement(37, "div");
			__builder.AddAttribute(38, "class", "relative group");
			__builder.OpenElement(39, "div");
			__builder.AddAttribute(40, "class", "flex justify-between items-center mb-1");
			__builder.AddMarkupContent(41, "<label class=\"font-label-caps text-label-caps text-on-surface-variant group-focus-within:text-primary transition-colors\">PASSWORD</label>\n                            ");
			__builder.OpenElement(42, "button");
			__builder.AddAttribute(43, "type", "button");
			__builder.AddAttribute(44, "class", "font-label-caps text-[10px] text-tertiary-container hover:underline bg-transparent border-0 cursor-pointer p-0");
			__builder.AddAttribute(45, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)ShowForgotPasswordMessage));
			__builder.AddContent(46, "FORGOT?");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(47, "\n                    ");
			__builder.OpenElement(48, "div");
			__builder.AddAttribute(49, "class", "relative");
			__builder.OpenElement(50, "input");
			__builder.AddAttribute(51, "class", "w-full bg-transparent border-0 border-b border-on-surface-variant/40 focus:ring-0 focus:border-primary focus:border-b-2 transition-all py-1.5 pr-8 px-0 font-body-md placeholder:text-on-surface-variant/30");
			__builder.AddAttribute(52, "placeholder", "••••••••");
			__builder.AddAttribute(53, "type", showPassword ? "text" : "password");
			__builder.AddAttribute(54, "required");
			__builder.AddAttribute(55, "value", BindConverter.FormatValue(password));
			__builder.AddAttribute(56, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				password = __value;
			}, password));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(57, "\n                        ");
			__builder.OpenElement(58, "button");
			__builder.AddAttribute(59, "type", "button");
			__builder.AddAttribute(60, "class", "absolute right-0 top-1/2 -translate-y-1/2 text-on-surface-variant/40 hover:text-primary transition-colors bg-transparent border-0 cursor-pointer p-0");
			__builder.AddAttribute(61, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)TogglePasswordVisibility));
			__builder.OpenElement(62, "span");
			__builder.AddAttribute(63, "class", "material-symbols-outlined text-lg");
			__builder.AddContent(64, showPassword ? "visibility_off" : "visibility");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(65, "\n\n                    ");
			__builder.OpenElement(66, "div");
			__builder.AddAttribute(67, "class", "flex items-center gap-3");
			__builder.OpenElement(68, "input");
			__builder.AddAttribute(69, "class", "w-4 h-4 border-on-surface focus:ring-primary text-primary");
			__builder.AddAttribute(70, "id", "remember");
			__builder.AddAttribute(71, "type", "checkbox");
			__builder.AddAttribute(72, "checked", BindConverter.FormatValue(rememberMe));
			__builder.AddAttribute(73, "onchange", EventCallback.Factory.CreateBinder(this, delegate(bool __value)
			{
				rememberMe = __value;
			}, rememberMe));
			__builder.SetUpdatesAttributeName("checked");
			__builder.CloseElement();
			__builder.AddMarkupContent(74, "\n                        ");
			__builder.AddMarkupContent(75, "<label class=\"font-label-sm text-on-surface-variant\" for=\"remember\">Remember this workstation</label>");
			__builder.CloseElement();
			__builder.AddMarkupContent(76, "\n\n                    ");
			__builder.OpenElement(77, "button");
			__builder.AddAttribute(78, "type", "submit");
			__builder.AddAttribute(79, "class", "w-full bg-on-surface text-surface py-3 font-label-caps text-label-caps hover:bg-primary transition-colors duration-300 group flex items-center justify-center gap-2 border-0 cursor-pointer");
			__builder.AddAttribute(80, "disabled", isLoading);
			if (isLoading)
			{
				__builder.AddMarkupContent(81, "<span>Signing in...</span>");
			}
			else
			{
				__builder.AddMarkupContent(82, "<span>LOG IN</span>\n                            ");
				__builder.AddMarkupContent(83, "<span class=\"material-symbols-outlined text-[18px] group-hover:translate-x-1 transition-transform\">arrow_forward</span>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(84, "\n\n                ");
			__builder.AddMarkupContent(85, "<div class=\"mt-6 pt-5 border-t border-outline-variant/20 flex flex-col items-center gap-3\"><p class=\"font-body-md text-sm text-on-surface-variant\">New to the faculty system?</p>\n                    <a href=\"register\" class=\"font-label-caps text-label-caps text-on-surface border border-on-surface px-8 py-2.5 hover:bg-surface-variant transition-colors no-underline\">\n                        CREATE ACCOUNT\n                    </a></div>");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}

		private void TogglePasswordVisibility()
		{
			showPassword = !showPassword;
		}

		private void ShowForgotPasswordMessage()
		{
			errorMessage = "Password reset is not yet available. Please contact your administrator.";
		}

		private async Task HandleLogin()
		{
			isLoading = true;
			errorMessage = string.Empty;
			LoginRequest value = new LoginRequest
			{
				Email = email,
				Password = password
			};
			try
			{
				HttpResponseMessage httpResponseMessage = await Http.PostAsJsonAsync("api/auth/login", value);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					ApiResponse<LoginResponse> result = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
					if (result?.Data?.Token != null)
					{
						await JS.InvokeVoidAsync("authStorage.setToken", result.Data.Token);
						await Task.Delay(200);
						string uri = ((result.Data.Role == "Professor") ? "professor-dashboard" : "dashboard");
						Nav.NavigateTo(uri, forceLoad: true);
					}
					else
					{
						errorMessage = "Login succeeded but token was missing.";
					}
				}
				else
				{
					errorMessage = "Invalid email or password.";
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Connection error: " + ex.Message;
			}
			finally
			{
				isLoading = false;
			}
		}
	}
}
