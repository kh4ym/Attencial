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
	[Route("/attend")]
	public class Attend : ComponentBase, IAsyncDisposable
	{
		private string? token;

		private bool isValidating = true;

		private string? tokenError;

		private bool cameraStarted;

		private bool isCameraStarting;

		private bool isSubmitting;

		private bool isSuccess;

		private bool messageIsError;

		private string? message;

		private AttendanceTokenValidateResponse? sessionInfo;

		private AttendanceMarkResponse? responseDto;

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
				renderTreeBuilder.AddMarkupContent(2, "Mark Attendance — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "fixed inset-0 bg-background text-on-surface font-body-md canvas-bg flex flex-col overflow-y-auto");
			__builder.OpenElement(6, "main");
			__builder.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile md:px-margin-desktop pt-28 pb-32 md:pb-4 relative z-10 lg:flex-1 lg:min-h-0 flex flex-col animate-fade-in");
			if (isValidating)
			{
				__builder.AddMarkupContent(8, "<div class=\"flex-1 flex items-center justify-center\"><div class=\"text-center\"><div class=\"spinner-ring-lg mb-4\"></div>\n                    <p class=\"font-label-caps text-on-surface-variant\">Verifying link...</p></div></div>");
			}
			else if (!string.IsNullOrEmpty(tokenError))
			{
				__builder.OpenElement(9, "div");
				__builder.AddAttribute(10, "class", "flex-1 flex items-center justify-center");
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "border border-on-surface bg-surface p-10 text-center max-w-md w-full");
				__builder.AddMarkupContent(13, "<span class=\"material-symbols-outlined text-5xl text-primary mb-4 block\">close</span>\n                    ");
				__builder.AddMarkupContent(14, "<h2 class=\"font-headline-md text-headline-md mb-2\">Invalid or Expired</h2>\n                    ");
				__builder.OpenElement(15, "p");
				__builder.AddAttribute(16, "class", "font-body-md text-sm text-on-surface-variant");
				__builder.AddContent(17, tokenError);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else if (isSuccess)
			{
				__builder.OpenElement(18, "div");
				__builder.AddAttribute(19, "class", "flex-1 flex items-center justify-center");
				__builder.OpenElement(20, "div");
				__builder.AddAttribute(21, "class", "border border-on-surface bg-surface p-10 text-center max-w-md w-full");
				__builder.AddMarkupContent(22, "<span class=\"material-symbols-outlined text-5xl text-tertiary mb-4 block\">check_circle</span>\n                    ");
				__builder.AddMarkupContent(23, "<h2 class=\"font-headline-md text-headline-md mb-2\">Verified</h2>\n                    ");
				__builder.OpenElement(24, "p");
				__builder.AddAttribute(25, "class", "font-body-md text-sm text-on-surface-variant");
				__builder.AddContent(26, responseDto?.StudentName);
				__builder.AddContent(27, " (");
				__builder.AddContent(28, responseDto?.RollNumber);
				__builder.AddMarkupContent(29, ") — ");
				__builder.AddContent(30, responseDto?.CourseCode);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else
			{
				__builder.OpenElement(31, "div");
				__builder.AddAttribute(32, "class", "lg:flex-1 flex flex-col lg:flex-row gap-4 lg:min-h-0");
				__builder.OpenElement(33, "div");
				__builder.AddAttribute(34, "class", "flex-[2] flex flex-col lg:min-h-0");
				__builder.AddMarkupContent(35, "<div class=\"flex flex-col gap-3 mb-5 flex-shrink-0\"><div class=\"flex items-center gap-3\"><h1 class=\"font-headline-md text-headline-md\">Mark Attendance</h1>\n                        <div class=\"flex gap-2\"><div class=\"w-3 h-3 bg-primary\"></div>\n                            <div class=\"w-3 h-3 bg-tertiary\"></div></div></div><p class=\"font-body-md text-sm text-on-surface-variant max-w-2xl\">Open the camera when you are ready. The capture button appears only after the feed is live.</p></div>");
				if (!string.IsNullOrEmpty(message))
				{
					__builder.OpenElement(36, "div");
					__builder.AddAttribute(37, "class", "border-l-4 " + (messageIsError ? "border-primary" : "border-tertiary") + " bg-surface-container-low p-3 mb-3 flex items-start gap-2 flex-shrink-0");
					__builder.OpenElement(38, "span");
					__builder.AddAttribute(39, "class", "material-symbols-outlined " + (messageIsError ? "text-primary" : "text-tertiary") + " text-sm mt-0.5");
					__builder.AddContent(40, messageIsError ? "warning" : "info");
					__builder.CloseElement();
					__builder.AddMarkupContent(41, "\n                            ");
					__builder.OpenElement(42, "span");
					__builder.AddAttribute(43, "class", "text-xs text-on-surface");
					__builder.AddContent(44, message);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(45, "div");
				__builder.AddAttribute(46, "class", "relative w-full aspect-[4/3] md:aspect-auto md:flex-1 bg-surface overflow-hidden rounded-2xl md:min-h-[420px] border border-on-surface-variant/20 shadow-[8px_8px_0px_rgba(27,28,26,0.08)]");
				if (cameraStarted || isCameraStarting)
				{
					__builder.OpenElement(47, "video");
					__builder.AddAttribute(48, "id", "camera");
					__builder.AddAttribute(49, "autoplay");
					__builder.AddAttribute(50, "playsinline");
					__builder.AddAttribute(51, "class", "absolute inset-0 w-full h-full");
					__builder.AddAttribute(52, "style", "object-fit: cover; transform: scaleX(-1);");
					__builder.CloseElement();
					__builder.AddMarkupContent(53, "\n                        ");
					if (isCameraStarting)
					{
						__builder.AddMarkupContent(54, "<div class=\"absolute inset-0 z-10 flex items-center justify-center bg-surface/80 backdrop-blur-sm\"><div class=\"text-center px-6\"><div class=\"spinner-ring-lg mb-4\"></div><p class=\"font-label-caps text-label-caps text-on-surface-variant\">Opening camera...</p><p class=\"font-body-md text-sm text-on-surface-variant mt-2\">Grant permission, then frame your face.</p></div></div>");
					}
					else
					{
						__builder.OpenElement(54, "div");
						__builder.AddAttribute(55, "class", "absolute inset-0 flex items-center justify-center pointer-events-none z-10 bg-gradient-to-b from-transparent via-transparent to-black/10");
						__builder.OpenElement(56, "svg");
						__builder.AddAttribute(57, "viewBox", "0 0 100 100");
						__builder.AddAttribute(58, "style", "width: 160px; height: 200px; color: rgba(255,255,255,0.12);");
						__builder.AddAttribute(59, "class", isSubmitting ? "hidden" : "");
						__builder.AddMarkupContent(60, "<path d=\"M 50 15 C 32 15, 30 50, 32 70 C 35 85, 45 92, 50 92 C 55 92, 65 85, 68 70 C 70 50, 68 15, 50 15 Z\" fill=\"none\" stroke=\"currentColor\" stroke-dasharray=\"3,3\" stroke-width=\"1\"></path>");
						__builder.CloseElement();
						__builder.CloseElement();
					}
				}
				else
				{
					__builder.AddMarkupContent(61, "<div class=\"absolute inset-0 flex items-center justify-center px-6 text-center bg-surface-container-low\"><div class=\"max-w-md\"><div class=\"mx-auto mb-4 w-16 h-16 rounded-full border border-outline-variant bg-background flex items-center justify-center\"><span class=\"material-symbols-outlined text-3xl text-primary\">videocam</span></div><p class=\"font-label-caps text-xs tracking-[0.2em] text-on-surface-variant mb-2\">Camera disabled</p><h2 class=\"font-headline-md text-headline-md text-on-surface mb-2\">Open camera to begin</h2><p class=\"font-body-md text-sm text-on-surface-variant\">We will only request camera access after you press the button below.</p></div></div>");
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(62, "\n\n                    ");
				__builder.OpenElement(63, "div");
				__builder.AddAttribute(64, "class", "flex-shrink-0 mt-3");
				if (!cameraStarted)
				{
					__builder.OpenElement(65, "button");
					__builder.AddAttribute(66, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)StartCamera));
					__builder.AddAttribute(67, "disabled", isCameraStarting);
					__builder.AddAttribute(68, "class", "w-full bg-on-surface text-background font-label-caps py-4 rounded-full hover:bg-primary transition-colors flex items-center justify-center gap-2 text-sm disabled:opacity-60 disabled:cursor-not-allowed");
					if (isCameraStarting)
					{
						__builder.AddMarkupContent(69, "<span class=\"spinner-ring-sm mr-2\"></span>\n                                Opening Camera...\n                            ");
					}
					else
					{
						__builder.AddMarkupContent(69, "<span class=\"material-symbols-outlined\">videocam</span>\n                                Open Camera\n                            ");
					}
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(69, "button");
					__builder.AddAttribute(70, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)MarkAttendance));
					__builder.AddAttribute(71, "disabled", isSubmitting);
					__builder.AddAttribute(72, "class", "w-full bg-primary text-on-primary font-label-caps py-4 rounded-full hover:bg-[#f05454] transition-colors flex items-center justify-center gap-2 text-sm disabled:opacity-50 shadow-[8px_8px_0px_rgba(176,37,43,0.12)]");
					if (isSubmitting)
					{
						__builder.AddMarkupContent(73, "<span class=\"spinner-ring-sm mr-2\"></span>\n                                    ");
						__builder.AddMarkupContent(74, "<span>Verifying...</span>");
					}
					else
					{
						__builder.AddMarkupContent(75, "<span class=\"material-symbols-outlined\">verified_user</span>\n                                    ");
						__builder.AddMarkupContent(76, "<span>Capture &amp; Verify</span>");
					}
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(77, "\n\n                \n                ");
				__builder.OpenElement(78, "div");
				__builder.AddAttribute(79, "class", "w-full lg:w-80 border border-on-surface bg-surface p-5 flex flex-col gap-5 flex-shrink-0");
				__builder.OpenElement(80, "div");
				__builder.AddMarkupContent(81, "<span class=\"font-label-caps text-xs text-primary tracking-widest\">Session</span>\n                        ");
				__builder.OpenElement(82, "h2");
				__builder.AddAttribute(83, "class", "font-headline-md text-headline-md text-on-surface mt-1");
				__builder.AddContent(84, sessionInfo?.CourseCode);
				__builder.CloseElement();
				__builder.AddMarkupContent(85, "\n                        ");
				__builder.OpenElement(86, "p");
				__builder.AddAttribute(87, "class", "font-body-md text-sm text-on-surface-variant");
				__builder.AddContent(88, sessionInfo?.CourseName);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(89, "\n                    ");
				__builder.OpenElement(90, "div");
				__builder.AddAttribute(91, "class", "flex items-center gap-3");
				__builder.AddMarkupContent(92, "<span class=\"material-symbols-outlined text-tertiary\">person</span>\n                        ");
				__builder.OpenElement(93, "div");
				__builder.AddMarkupContent(94, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Professor</span>\n                            ");
				__builder.OpenElement(95, "span");
				__builder.AddAttribute(96, "class", "font-label-caps text-sm text-on-surface");
				__builder.AddContent(97, sessionInfo?.ProfessorName);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(98, "\n                    ");
				__builder.AddMarkupContent(99, "<div class=\"border-t border-on-surface-variant/20 pt-4\"><p class=\"font-body-md text-xs text-on-surface-variant\">Ensure good lighting. Center your face and hold steady.</p></div>");
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
			string[] array = new Uri(Nav.Uri).Query.TrimStart('?').Split('&');
			foreach (string text in array)
			{
				int num = text.IndexOf('=');
				if (num >= 0 && text.Substring(0, num).Equals("token", StringComparison.OrdinalIgnoreCase))
				{
					Attend attend = this;
					object stringToUnescape;
					if (num + 1 >= text.Length)
					{
						stringToUnescape = "";
					}
					else
					{
						string text2 = text;
						int num2 = num + 1;
						stringToUnescape = text2.Substring(num2, text2.Length - num2);
					}
					attend.token = Uri.UnescapeDataString((string)stringToUnescape);
					break;
				}
			}
			if (string.IsNullOrEmpty(token))
			{
				tokenError = "Missing token.";
				isValidating = false;
			}
			else
			{
				await ValidateToken();
			}
		}

		private async Task ValidateToken()
		{
			_ = 1;
			try
			{
				HttpResponseMessage httpResponseMessage = await Http.GetAsync("api/attendance/sessions/validate?token=" + Uri.EscapeDataString(token));
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					ApiResponse<AttendanceTokenValidateResponse> apiResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<AttendanceTokenValidateResponse>>();
					if ((object)apiResponse != null && apiResponse.Success && apiResponse.Data != null)
					{
						sessionInfo = apiResponse.Data;
					}
					else
					{
						tokenError = apiResponse?.Message ?? "Validation failed.";
					}
				}
				else
				{
					tokenError = "Invalid link.";
				}
			}
			catch (Exception ex)
			{
				tokenError = "Error: " + ex.Message;
			}
			finally
			{
				isValidating = false;
			}
		}

		private async Task StartCamera()
		{
			message = null;
			messageIsError = false;
			isCameraStarting = true;
			StateHasChanged();
			await Task.Yield();
			try
			{
				await JS.InvokeVoidAsync("cameraInterop.startCamera", "camera");
				cameraStarted = true;
			}
			catch (Exception ex)
			{
				cameraStarted = false;
				messageIsError = true;
				message = "Camera error: " + ex.Message;
			}
			finally
			{
				isCameraStarting = false;
				StateHasChanged();
			}
		}

		private async Task MarkAttendance()
		{
			if (isSubmitting)
			{
				return;
			}
			isSubmitting = true;
			message = "Capturing...";
			messageIsError = false;
			StateHasChanged();
			try
			{
				string base64 = await JSRuntimeExtensions.InvokeAsync<string>(JS, "cameraInterop.captureFrame", new object[1] { "camera" });
				if (base64.Contains(','))
				{
					base64 = base64.Split(',')[1];
				}
				string deviceId = await GetOrCreateDeviceId();
				HttpResponseMessage res = await Http.PostAsJsonAsync("api/attendance/mark", new AttendanceMarkRequest
				{
					Token = token,
					DeviceId = deviceId,
					Image = base64
				});
				ApiResponse<AttendanceMarkResponse> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<AttendanceMarkResponse>>();
				if (res.IsSuccessStatusCode && (object)apiResponse != null && apiResponse.Success && apiResponse.Data != null)
				{
					responseDto = apiResponse.Data;
					isSuccess = true;
					await JS.InvokeVoidAsync("cameraInterop.stopCamera");
					cameraStarted = false;
				}
				else
				{
					messageIsError = true;
					message = apiResponse?.Message ?? "Verification failed.";
				}
			}
			catch (Exception ex)
			{
				messageIsError = true;
				message = ex.Message;
			}
			finally
			{
				isSubmitting = false;
			}
		}

		private async Task<string> GetOrCreateDeviceId()
		{
			string id = await JSRuntimeExtensions.InvokeAsync<string>(JS, "eval", new object[1] { "localStorage.getItem('attencial_device_id') || ''" });
			if (string.IsNullOrEmpty(id))
			{
				id = Guid.NewGuid().ToString();
				await JS.InvokeVoidAsync("eval", "localStorage.setItem('attencial_device_id','" + id + "')");
			}
			return id;
		}

		public async ValueTask DisposeAsync()
		{
			if (cameraStarted || isCameraStarting)
			{
				try
				{
					await JS.InvokeVoidAsync("cameraInterop.stopCamera");
				}
				catch
				{
				}
			}
		}
	}
}
