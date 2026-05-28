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
	[Route("/register")]
	public class Register : ComponentBase
	{
		private string fullName = string.Empty;

		private string email = string.Empty;

		private string password = string.Empty;

		private string role = "Student";

		private string rollNumber = string.Empty;

		private bool showPassword;

		private string errorMessage = string.Empty;

		private bool isLoading;

		private bool isSuccess;

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
				renderTreeBuilder.AddContent(2, "Register | Attencial Academic");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "font-body-md text-on-surface bg-background canvas-bg h-screen flex flex-col overflow-hidden relative selection:bg-brand-coral selection:text-white");
			__builder.AddMarkupContent(6, "<div class=\"absolute inset-0 pointer-events-none overflow-hidden\"><div class=\"absolute w-72 h-72 rounded-full border-2 border-primary/20 top-[5%] -right-24 floating-slow\"></div>\n        <div class=\"absolute w-52 h-52 rounded-full border-2 border-tertiary/20 bottom-[10%] -left-20 floating-medium\"></div>\n        <div class=\"absolute w-28 h-28 rounded-full bg-primary/10 top-[35%] left-[8%] floating-fast\"></div>\n        <div class=\"absolute w-20 h-20 rounded-full border-2 border-primary/15 top-[55%] right-[12%] floating-medium\" style=\"animation-delay: -4s;\"></div>\n        <div class=\"absolute w-14 h-14 rounded-full bg-tertiary/10 bottom-[25%] right-[35%] floating-slow\" style=\"animation-delay: -2s;\"></div></div>\n\n    \n    ");
			__builder.AddMarkupContent(7, "<header class=\"relative top-0 left-0 w-full px-margin-mobile md:px-margin-desktop h-14 z-50 flex items-center\"><a href=\"/\" class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter no-underline\">Attencial</a></header>\n\n    \n    ");
			__builder.OpenElement(8, "main");
			__builder.AddAttribute(9, "class", "flex-1 flex items-center justify-center px-margin-mobile md:px-margin-desktop pb-4");
			__builder.OpenElement(10, "div");
			__builder.AddAttribute(11, "class", "relative w-full max-w-[420px]");
			__builder.AddAttribute(12, "style", "animation: gentle-rise 0.8s 0.15s ease both;");
			if (!string.IsNullOrEmpty(errorMessage))
			{
				__builder.OpenElement(13, "div");
				__builder.AddAttribute(14, "class", "border border-error/30 p-4 mb-8 flex items-start gap-3");
				__builder.AddAttribute(15, "style", "background: rgba(186,26,26,0.04);");
				__builder.AddMarkupContent(16, "<span class=\"material-symbols-outlined text-error text-lg flex-shrink-0 mt-0.5\">error</span>\n                    ");
				__builder.OpenElement(17, "span");
				__builder.AddAttribute(18, "class", "font-body-md text-sm text-on-surface-variant");
				__builder.AddContent(19, errorMessage);
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (isSuccess)
			{
				__builder.AddMarkupContent(20, "<div class=\"bg-background border border-on-surface p-8 md:p-12 neo-shadow relative z-10 text-center\"><span class=\"material-symbols-outlined text-[56px] text-primary block mb-4\">check_circle</span>\n                    <h3 class=\"font-headline-lg text-headline-lg-mobile md:text-headline-lg text-on-surface mb-4\">Registration Complete!</h3>\n                    <p class=\"font-body-md text-on-surface-variant mb-8\">Your account and face enrollment are complete.</p>\n                    <a href=\"/login\" class=\"bg-on-surface text-background px-10 py-4 font-label-caps text-label-caps tracking-widest hover:bg-primary transition-all duration-300 no-underline inline-flex items-center gap-2\">\n                        GO TO LOGIN\n                        <span class=\"material-symbols-outlined text-sm\">arrow_forward</span></a></div>\n                ");
				__builder.AddMarkupContent(21, "<div class=\"mt-6 flex justify-center text-on-surface-variant px-2\"><span class=\"font-label-caps text-[10px] tracking-tighter uppercase\">Academic Standard</span></div>");
			}
			else
			{
				__builder.OpenElement(22, "div");
				__builder.AddAttribute(23, "class", "bg-background border border-on-surface p-5 md:p-7 neo-shadow relative z-10");
				__builder.AddMarkupContent(24, "<div class=\"absolute -top-3 -right-3 z-20 animate-bob\"><div class=\"bg-primary text-surface px-4 py-1.5 font-label-caps text-label-caps tracking-[0.2em] shadow-[4px_4px_0px_0px_rgba(27,28,26,0.1)]\">\n                            CREATE ACCOUNT\n                        </div></div>\n                    ");
				__builder.AddMarkupContent(25, "<div class=\"mb-5 mt-1\"><span class=\"font-label-caps text-label-caps text-primary tracking-[0.3em] block mb-1\">JOIN THE ACADEMY</span></div>\n\n                    ");
				__builder.OpenElement(26, "form");
				__builder.AddAttribute(27, "onsubmit", EventCallback.Factory.Create<EventArgs>((object)this, (Func<Task>)HandleRegister));
				__builder.AddAttribute(28, "class", "space-y-5");
				__builder.OpenElement(29, "div");
				__builder.AddAttribute(30, "class", "space-y-3");
				__builder.AddMarkupContent(31, "<span class=\"font-label-caps text-label-caps block text-secondary\">CHOOSE YOUR ROLE</span>\n                            ");
				__builder.OpenElement(32, "div");
				__builder.AddAttribute(33, "class", "role-toggle flex border border-on-surface");
				__builder.OpenElement(34, "input");
				__builder.AddAttribute(35, "type", "radio");
				__builder.AddAttribute(36, "name", "role");
				__builder.AddAttribute(37, "onchange", EventCallback.Factory.Create((object)this, (Action<ChangeEventArgs>)OnRoleChanged));
				__builder.AddAttribute(38, "value", "Student");
				__builder.AddAttribute(39, "checked", role == "Student");
				__builder.AddAttribute(40, "class", "hidden");
				__builder.AddAttribute(41, "id", "role-student");
				__builder.CloseElement();
				__builder.AddMarkupContent(42, "\n                                ");
				__builder.AddMarkupContent(43, "<label class=\"flex-1 text-center py-3 font-label-caps text-label-caps cursor-pointer transition-all duration-300 hover:bg-surface-container\" for=\"role-student\">STUDENT</label>\n                                <div class=\"w-[1px] bg-on-surface\"></div>\n                                ");
				__builder.OpenElement(44, "input");
				__builder.AddAttribute(45, "type", "radio");
				__builder.AddAttribute(46, "name", "role");
				__builder.AddAttribute(47, "onchange", EventCallback.Factory.Create((object)this, (Action<ChangeEventArgs>)OnRoleChanged));
				__builder.AddAttribute(48, "value", "Professor");
				__builder.AddAttribute(49, "checked", role == "Professor");
				__builder.AddAttribute(50, "class", "hidden");
				__builder.AddAttribute(51, "id", "role-professor");
				__builder.CloseElement();
				__builder.AddMarkupContent(52, "\n                                ");
				__builder.AddMarkupContent(53, "<label class=\"flex-1 text-center py-3 font-label-caps text-label-caps cursor-pointer transition-all duration-300 hover:bg-surface-container\" for=\"role-professor\">PROFESSOR</label>");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(54, "\n\n                        \n                        ");
				__builder.OpenElement(55, "div");
				__builder.AddAttribute(56, "class", "space-y-6");
				__builder.OpenElement(57, "div");
				__builder.AddAttribute(58, "class", "relative group");
				__builder.AddMarkupContent(59, "<label class=\"font-label-caps text-label-caps block text-on-surface-variant mb-1 group-focus-within:text-brand-coral transition-colors\" for=\"fullName\">FULL NAME</label>\n                                ");
				__builder.OpenElement(60, "input");
				__builder.AddAttribute(61, "class", "w-full bg-transparent border-t-0 border-x-0 border-b border-on-surface px-0 py-2 focus:ring-0 focus:border-brand-coral focus:border-b-2 placeholder:text-surface-container-highest transition-all outline-none font-body-md");
				__builder.AddAttribute(62, "id", "fullName");
				__builder.AddAttribute(63, "placeholder", (role == "Student") ? "Alexander Thorne" : "Dr. Julian Thorne");
				__builder.AddAttribute(64, "type", "text");
				__builder.AddAttribute(65, "required");
				__builder.AddAttribute(66, "value", BindConverter.FormatValue(fullName));
				__builder.AddAttribute(67, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					fullName = __value;
				}, fullName));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(68, "\n                            \n                            ");
				__builder.OpenElement(69, "div");
				__builder.AddAttribute(70, "class", "relative group");
				__builder.AddMarkupContent(71, "<label class=\"font-label-caps text-label-caps block text-on-surface-variant mb-1 group-focus-within:text-brand-coral transition-colors\" for=\"email\">EMAIL ADDRESS</label>\n                                ");
				__builder.OpenElement(72, "input");
				__builder.AddAttribute(73, "class", "w-full bg-transparent border-t-0 border-x-0 border-b border-on-surface px-0 py-2 focus:ring-0 focus:border-brand-coral focus:border-b-2 placeholder:text-surface-container-highest transition-all outline-none font-body-md");
				__builder.AddAttribute(74, "id", "email");
				__builder.AddAttribute(75, "placeholder", "curator@attencial.edu");
				__builder.AddAttribute(76, "type", "email");
				__builder.AddAttribute(77, "required");
				__builder.AddAttribute(78, "value", BindConverter.FormatValue(email));
				__builder.AddAttribute(79, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					email = __value;
				}, email));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(80, "\n                            \n                            ");
				__builder.OpenElement(81, "div");
				__builder.AddAttribute(82, "class", "relative group overflow-hidden " + ((role == "Student") ? "max-h-20 opacity-100" : "max-h-0 opacity-0"));
				__builder.AddAttribute(83, "style", "transition: all 0.4s ease;");
				__builder.AddMarkupContent(84, "<label class=\"font-label-caps text-label-caps block text-on-surface-variant mb-1 group-focus-within:text-brand-coral transition-colors\" for=\"rollNumber\">ROLL NUMBER</label>\n                                ");
				__builder.OpenElement(85, "input");
				__builder.AddAttribute(86, "class", "w-full bg-transparent border-t-0 border-x-0 border-b border-on-surface px-0 py-2 focus:ring-0 focus:border-brand-coral focus:border-b-2 placeholder:text-surface-container-highest transition-all outline-none font-body-md");
				__builder.AddAttribute(87, "id", "rollNumber");
				__builder.AddAttribute(88, "placeholder", "e.g. 241871");
				__builder.AddAttribute(89, "type", "text");
				__builder.AddAttribute(90, "value", BindConverter.FormatValue(rollNumber));
				__builder.AddAttribute(91, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					rollNumber = __value;
				}, rollNumber));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(92, "\n                            \n                            ");
				__builder.OpenElement(93, "div");
				__builder.AddAttribute(94, "class", "relative group");
				__builder.AddMarkupContent(95, "<label class=\"font-label-caps text-label-caps block text-on-surface-variant mb-1 group-focus-within:text-brand-coral transition-colors\" for=\"password\">PASSWORD</label>\n                                ");
				__builder.OpenElement(96, "div");
				__builder.AddAttribute(97, "class", "relative");
				__builder.OpenElement(98, "input");
				__builder.AddAttribute(99, "class", "w-full bg-transparent border-t-0 border-x-0 border-b border-on-surface px-0 py-2 pr-8 focus:ring-0 focus:border-brand-coral focus:border-b-2 placeholder:text-surface-container-highest transition-all outline-none font-body-md");
				__builder.AddAttribute(100, "id", "password");
				__builder.AddAttribute(101, "placeholder", "••••••••");
				__builder.AddAttribute(102, "type", showPassword ? "text" : "password");
				__builder.AddAttribute(103, "required");
				__builder.AddAttribute(104, "minlength", "6");
				__builder.AddAttribute(105, "value", BindConverter.FormatValue(password));
				__builder.AddAttribute(106, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					password = __value;
				}, password));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.AddMarkupContent(107, "\n                                    ");
				__builder.OpenElement(108, "button");
				__builder.AddAttribute(109, "type", "button");
				__builder.AddAttribute(110, "class", "absolute right-0 top-1/2 -translate-y-1/2 text-on-surface-variant/40 hover:text-brand-coral transition-colors bg-transparent border-0 cursor-pointer p-0");
				__builder.AddAttribute(111, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)TogglePasswordVisibility));
				__builder.OpenElement(112, "span");
				__builder.AddAttribute(113, "class", "material-symbols-outlined text-lg");
				__builder.AddContent(114, showPassword ? "visibility_off" : "visibility");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(115, "\n                            \n                            ");
				__builder.AddMarkupContent(116, "<div class=\"relative group\"><label class=\"font-label-caps text-label-caps block text-on-surface-variant mb-1 group-focus-within:text-brand-coral transition-colors\" for=\"confirm-password\">CONFIRM PASSWORD</label>\n                                <input class=\"w-full bg-transparent border-t-0 border-x-0 border-b border-on-surface px-0 py-2 focus:ring-0 focus:border-brand-coral focus:border-b-2 placeholder:text-surface-container-highest transition-all outline-none font-body-md\" id=\"confirm-password\" placeholder=\"••••••••\" type=\"password\"></div>");
				__builder.CloseElement();
				__builder.AddMarkupContent(117, "\n\n                        \n                        ");
				__builder.OpenElement(118, "div");
				__builder.AddAttribute(119, "class", "space-y-4 pt-2");
				__builder.OpenElement(120, "button");
				__builder.AddAttribute(121, "type", "submit");
				__builder.AddAttribute(122, "class", "w-full bg-on-surface text-background font-label-caps text-label-caps py-5 tracking-widest hover:bg-brand-coral active:opacity-70 transition-all duration-300 flex justify-center items-center gap-2 group border-0 cursor-pointer");
				__builder.AddAttribute(123, "disabled", isLoading);
				if (isLoading)
				{
					__builder.AddMarkupContent(124, "<span>Creating account...</span>");
				}
				else
				{
					__builder.AddMarkupContent(125, "<span>SIGN UP</span>\n                                    ");
					__builder.AddMarkupContent(126, "<span class=\"material-symbols-outlined text-sm group-hover:translate-x-1 transition-transform\">arrow_forward</span>");
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(127, "\n                            ");
				__builder.AddMarkupContent(128, "<div class=\"flex flex-col items-center gap-4 text-center\"><a class=\"font-label-caps text-label-caps text-on-surface-variant hover:text-primary transition-colors no-underline\" href=\"/login\">\n                                    ALREADY HAVE AN ACCOUNT? LOG IN\n                                </a></div>");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(129, "\n\n                    \n                    ");
				__builder.AddMarkupContent(130, "<div class=\"absolute -bottom-4 -left-4 w-8 h-8 flex items-center justify-center bg-background border border-on-surface\"><span class=\"material-symbols-outlined text-xs\">close</span></div>");
				__builder.CloseElement();
				__builder.AddMarkupContent(131, "<div class=\"mt-6 flex justify-center text-on-surface-variant px-2\"><span class=\"font-label-caps text-[10px] tracking-tighter uppercase\">Academic Standard</span></div>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}

		private void OnRoleChanged(ChangeEventArgs e)
		{
			role = e.Value?.ToString() ?? "Student";
		}

		private void TogglePasswordVisibility()
		{
			showPassword = !showPassword;
		}

		private async Task HandleRegister()
		{
			isLoading = true;
			errorMessage = string.Empty;
			string text = ((role == "Student") ? rollNumber.Trim() : string.Empty);
			RegisterRequest value = new RegisterRequest
			{
				FullName = fullName.Trim(),
				Email = email.Trim(),
				Password = password,
				Role = role,
				RollNumber = text
			};
			try
			{
				HttpResponseMessage httpResponseMessage = await Http.PostAsJsonAsync("api/auth/register", value);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					LoginRequest value2 = new LoginRequest
					{
						Email = email.Trim(),
						Password = password,
						Role = role
					};
					HttpResponseMessage httpResponseMessage2 = await Http.PostAsJsonAsync("api/auth/login", value2);
					if (httpResponseMessage2.IsSuccessStatusCode)
					{
						ApiResponse<LoginResponse> apiResponse = await httpResponseMessage2.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
						if (apiResponse?.Data?.Token != null)
						{
							await JS.InvokeVoidAsync("authStorage.setToken", apiResponse.Data.Token);
							Nav.NavigateTo("/enroll-face", forceLoad: true);
							return;
						}
					}
					isSuccess = true;
					errorMessage = "Account created but face enrollment setup failed. You can enroll from the Profile page after login.";
				}
				else
				{
					errorMessage = (await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<string>>())?.Message ?? "Registration failed.";
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
