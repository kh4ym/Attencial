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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/enroll-face")]
	public class EnrollFace : ComponentBase
	{
		private bool isLoading = true;

		private bool isProfessor;

		private string submitEndpoint = string.Empty;

		private string? errorMessage;

		private string returnUrl = string.Empty;

		private FaceCaptureComponent? faceCapture;

		private int photosTaken;

		private bool cameraActive;

		private bool isLockedOut;

		[Inject]
		private NavigationManager Nav { get; set; }

		[Inject]
		private HttpClient Http { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenComponent<PageTitle>(0);
			__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.AddMarkupContent(2, "Face Enrollment — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "fixed inset-0 z-50 bg-background canvas-bg flex flex-col");
			__builder.OpenElement(6, "header");
			__builder.AddAttribute(7, "class", "h-14 flex items-center justify-between px-margin-mobile md:px-margin-desktop border-b border-on-surface-variant/20 flex-shrink-0");
			__builder.AddMarkupContent(8, "<div class=\"flex items-center\"><span class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter\">Attencial</span>\n            <span class=\"font-label-caps text-label-caps text-on-surface-variant ml-4 tracking-[0.2em]\">Face Enrollment</span></div>");
			if (!string.IsNullOrEmpty(returnUrl))
			{
				__builder.OpenElement(9, "a");
				__builder.AddAttribute(10, "href", returnUrl);
				__builder.AddAttribute(11, "class", "font-label-caps text-sm no-underline flex items-center gap-1 transition-all duration-300 " + (isLockedOut ? "bg-primary text-on-primary px-4 py-2" : "text-on-surface-variant hover:text-primary"));
				__builder.OpenElement(12, "span");
				__builder.AddAttribute(13, "class", "material-symbols-outlined text-sm");
				__builder.AddContent(14, isLockedOut ? "arrow_back" : "close");
				__builder.CloseElement();
				__builder.AddMarkupContent(15, "\n                ");
				__builder.AddContent(16, isLockedOut ? "Go Back" : "Cancel");
				__builder.CloseElement();
			}
			__builder.CloseElement();
			if (isLoading)
			{
				__builder.AddMarkupContent(17, "<div class=\"flex-1 flex items-center justify-center\"><span class=\"material-symbols-outlined animate-spin text-primary text-3xl\">refresh</span></div>");
			}
			else
			{
				__builder.OpenElement(18, "div");
				__builder.AddAttribute(19, "class", "flex-1 flex items-start justify-center pt-4 lg:pt-6 overflow-hidden relative");
				__builder.OpenElement(20, "div");
				__builder.AddAttribute(21, "class", "absolute inset-y-0 left-0 w-full lg:w-[calc(100%-420px)] flex items-center justify-center p-4 lg:p-8 transition-all duration-700 ease-in-out " + (cameraActive ? "translate-x-0 opacity-100" : "-translate-x-full opacity-0"));
				__builder.OpenElement(22, "div");
				__builder.AddAttribute(23, "class", "w-full max-w-[480px]");
				__builder.OpenComponent<FaceCaptureComponent>(24);
				__builder.AddComponentParameter(25, "SubmitEndpoint", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(submitEndpoint));
				__builder.AddComponentParameter(26, "OnComplete", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<Task>)OnEnrollmentComplete)));
				__builder.AddComponentParameter(27, "OnError", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<string, Task>)HandleEnrollmentError)));
				__builder.AddComponentParameter(28, "PhotosChanged", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(this, delegate(int n)
				{
					photosTaken = n;
					StateHasChanged();
				})));
				__builder.AddComponentParameter(29, "Minimal", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
				__builder.AddComponentParameter(30, "HideActionArea", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(value: true));
				__builder.AddComponentReferenceCapture(31, delegate(object __value)
				{
					faceCapture = (FaceCaptureComponent)__value;
				});
				__builder.CloseComponent();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(32, "\n\n            ");
				__builder.OpenElement(33, "div");
				__builder.AddAttribute(34, "class", "w-full max-w-[420px] bg-surface border border-on-surface-variant/20 p-6 lg:p-10 flex flex-col justify-center transition-all duration-700 ease-in-out " + (cameraActive ? "lg:translate-x-[calc(50vw-210px)]" : "translate-x-0"));
				__builder.OpenElement(35, "div");
				__builder.AddAttribute(36, "class", "space-y-6");
				__builder.OpenElement(37, "div");
				__builder.AddMarkupContent(38, "<span class=\"font-label-caps text-primary tracking-widest uppercase text-sm\">Face Enrollment Required</span>\n                        ");
				__builder.AddMarkupContent(39, "<h2 class=\"font-headline-lg text-headline-lg text-on-surface mt-1 mb-2\">Enroll Your Face</h2>\n                        ");
				__builder.OpenElement(40, "p");
				__builder.AddAttribute(41, "class", "font-body-md text-on-surface-variant");
				__builder.AddContent(42, isProfessor ? "Register your face to access the faculty dashboard, create sessions, and manage courses." : "Enroll your face to access your dashboard, view courses, and mark attendance.");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(43, "\n\n                    ");
				__builder.OpenElement(44, "div");
				__builder.AddAttribute(45, "class", "flex flex-col gap-2");
				__builder.OpenElement(46, "div");
				__builder.AddAttribute(47, "class", "flex items-center gap-3 " + ((photosTaken >= 1) ? "" : "opacity-40"));
				__builder.AddMarkupContent(48, "<span class=\"font-display-lg text-primary/40 leading-none\">01</span>\n                            ");
				__builder.AddMarkupContent(49, "<span class=\"font-label-caps text-sm\">Look straight at the camera</span>");
				if (photosTaken >= 1)
				{
					__builder.AddMarkupContent(50, "<span class=\"material-symbols-outlined text-tertiary text-sm\">check_circle</span>");
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(51, "\n                        ");
				__builder.OpenElement(52, "div");
				__builder.AddAttribute(53, "class", "flex items-center gap-3 " + ((photosTaken >= 2) ? "" : "opacity-40"));
				__builder.AddMarkupContent(54, "<span class=\"font-display-lg text-primary/40 leading-none\">02</span>\n                            ");
				__builder.AddMarkupContent(55, "<span class=\"font-label-caps text-sm\">Tilt your head slightly left</span>");
				if (photosTaken >= 2)
				{
					__builder.AddMarkupContent(56, "<span class=\"material-symbols-outlined text-tertiary text-sm\">check_circle</span>");
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(57, "\n                        ");
				__builder.OpenElement(58, "div");
				__builder.AddAttribute(59, "class", "flex items-center gap-3 " + ((photosTaken >= 3) ? "" : "opacity-40"));
				__builder.AddMarkupContent(60, "<span class=\"font-display-lg text-primary/40 leading-none\">03</span>\n                            ");
				__builder.AddMarkupContent(61, "<span class=\"font-label-caps text-sm\">Tilt your head slightly right</span>");
				if (photosTaken >= 3)
				{
					__builder.AddMarkupContent(62, "<span class=\"material-symbols-outlined text-tertiary text-sm\">check_circle</span>");
				}
				__builder.CloseElement();
				__builder.CloseElement();
				if (!cameraActive)
				{
					__builder.OpenElement(63, "button");
					__builder.AddAttribute(64, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)StartEnrollment));
					__builder.AddAttribute(65, "class", "w-full bg-on-surface text-background font-label-caps py-5 px-8 hover:bg-primary transition-all duration-300 flex items-center justify-center gap-2");
					__builder.AddMarkupContent(66, "<span class=\"material-symbols-outlined\">face</span>\n                            Register Face\n                        ");
					__builder.CloseElement();
				}
				else if (photosTaken < 3)
				{
					__builder.OpenElement(67, "div");
					__builder.OpenElement(68, "button");
					__builder.AddAttribute(69, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CaptureCurrentPhoto));
					__builder.AddAttribute(70, "class", "w-full bg-primary text-on-primary font-label-caps py-5 px-8 hover:bg-[#f05454] transition-all duration-300 flex items-center justify-center gap-2");
					__builder.AddMarkupContent(71, "<span class=\"material-symbols-outlined\">camera</span>\n                                Capture\n                            ");
					__builder.CloseElement();
					__builder.AddMarkupContent(72, "\n                            ");
					__builder.OpenElement(73, "div");
					__builder.AddAttribute(74, "class", "transition-all duration-500 ease-out " + ((photosTaken > 0) ? "max-h-14 mt-2 opacity-100" : "max-h-0 mt-0 opacity-0") + " overflow-hidden");
					__builder.OpenElement(75, "button");
					__builder.AddAttribute(76, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)RetakePhotos));
					__builder.AddAttribute(77, "class", "w-full border border-on-surface-variant/30 text-on-surface-variant font-label-caps py-3 px-6 hover:border-primary hover:text-primary transition-all duration-300 flex items-center justify-center gap-2 text-sm");
					__builder.AddMarkupContent(78, "<span class=\"material-symbols-outlined text-sm\">replay</span>\n                                    Retake\n                                ");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(79, "div");
					__builder.OpenElement(80, "button");
					__builder.AddAttribute(81, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitEnrollment));
					__builder.AddAttribute(82, "disabled", faceCapture?.IsSubmitting ?? false);
					__builder.AddAttribute(83, "class", "w-full bg-primary text-on-primary font-label-caps py-5 px-8 hover:bg-[#f05454] transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed");
					FaceCaptureComponent? faceCaptureComponent = faceCapture;
					if (faceCaptureComponent != null && faceCaptureComponent.IsSubmitting)
					{
						__builder.AddMarkupContent(84, "<span class=\"material-symbols-outlined animate-spin\">sync</span>\n                                    ");
						__builder.AddMarkupContent(85, "<span>Enrolling...</span>");
					}
					else
					{
						__builder.AddMarkupContent(86, "<span class=\"material-symbols-outlined\">cloud_upload</span>\n                                    ");
						__builder.AddMarkupContent(87, "<span>Submit Enrollment</span>");
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(88, "\n                            ");
					__builder.OpenElement(89, "div");
					__builder.AddAttribute(90, "class", "transition-all duration-500 ease-out max-h-14 mt-2 opacity-100 overflow-hidden");
					__builder.OpenElement(91, "button");
					__builder.AddAttribute(92, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)RetakePhotos));
					__builder.AddAttribute(93, "class", "w-full border border-on-surface-variant/30 text-on-surface-variant font-label-caps py-3 px-6 hover:border-primary hover:text-primary transition-all duration-300 flex items-center justify-center gap-2 text-sm");
					__builder.AddMarkupContent(94, "<span class=\"material-symbols-outlined text-sm\">replay</span>\n                                    Retake All\n                                ");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				if (!string.IsNullOrEmpty(errorMessage))
				{
					__builder.OpenElement(95, "div");
					__builder.AddAttribute(96, "class", "border-2 " + (isLockedOut ? "border-primary/50 bg-primary/5" : "border-error/30") + " p-4 flex items-start gap-2 " + (isLockedOut ? "animate-fade-in" : ""));
					__builder.AddAttribute(97, "style", isLockedOut ? "" : "background: rgba(186,26,26,0.04);");
					__builder.OpenElement(98, "span");
					__builder.AddAttribute(99, "class", "material-symbols-outlined " + (isLockedOut ? "text-primary" : "text-error") + " text-sm flex-shrink-0 mt-0.5");
					__builder.AddContent(100, isLockedOut ? "lock" : "error");
					__builder.CloseElement();
					__builder.AddMarkupContent(101, "\n                            ");
					__builder.OpenElement(102, "span");
					__builder.AddAttribute(103, "class", "font-body-md text-sm " + (isLockedOut ? "text-on-surface font-bold" : "text-on-surface-variant"));
					__builder.AddContent(104, errorMessage);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
			string[] array = new Uri(Nav.Uri).Query.TrimStart('?').Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				if (array2.Length == 2 && array2[0] == "returnUrl")
				{
					returnUrl = Uri.UnescapeDataString(array2[1]);
				}
			}
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (string.IsNullOrEmpty(text) || text == "null" || text == "undefined")
			{
				Nav.NavigateTo("/login");
				return;
			}
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", text);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using (JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync()))
					{
						string a = jsonDocument.RootElement.GetProperty("data").GetProperty("role").GetString() ?? "";
						isProfessor = string.Equals(a, "Professor", StringComparison.OrdinalIgnoreCase);
						submitEndpoint = (isProfessor ? "api/faculty/enroll" : "api/enrollment/enroll");
						return;
					}
				}
			}
			catch
			{
			}
			finally
			{
				isLoading = false;
			}
		}

		private async Task StartEnrollment()
		{
			if (faceCapture != null)
			{
				await faceCapture.StartCamera();
				cameraActive = true;
			}
		}

		private async Task HandleEnrollmentError(string msg)
		{
			if (msg.Contains("already enrolled") || msg.Contains("wait") || msg.Contains("every 3 days"))
			{
				cameraActive = false;
				isLockedOut = true;
			}
			errorMessage = msg;
		}

		private async Task CaptureCurrentPhoto()
		{
			if (faceCapture != null)
			{
				await faceCapture.CapturePhoto();
			}
		}

		private async Task RetakePhotos()
		{
			faceCapture?.ResetPhotos();
		}

		private async Task SubmitEnrollment()
		{
			if (faceCapture != null)
			{
				await faceCapture.SubmitEnrollment();
			}
		}

		private async Task OnEnrollmentComplete()
		{
			string uri = ((!string.IsNullOrEmpty(returnUrl)) ? returnUrl : (isProfessor ? "/professor-dashboard" : "/dashboard"));
			Nav.NavigateTo(uri, forceLoad: true);
		}
	}
}
