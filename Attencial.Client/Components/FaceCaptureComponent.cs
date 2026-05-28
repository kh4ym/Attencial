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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Components
{
	public class FaceCaptureComponent : ComponentBase
	{
		private readonly string cameraElementId = $"fcam_{Guid.NewGuid():N}".Substring(0, 8);

		private List<string> photos = new List<string>();

		private bool cameraStarted;

		private bool isSubmitting;

		[Parameter]
		public string SubmitEndpoint { get; set; } = string.Empty;

		[Parameter]
		public EventCallback OnComplete { get; set; }

		[Parameter]
		public EventCallback<string> OnError { get; set; }

		[Parameter]
		public EventCallback OnCancel { get; set; }

		[Parameter]
		public EventCallback<int> PhotosChanged { get; set; }

		[Parameter]
		public bool ShowCancel { get; set; }

		[Parameter]
		public bool HideActionArea { get; set; }

		[Parameter]
		public bool Minimal { get; set; }

		public int PhotosCount => photos.Count;

		public bool IsCameraStarted => cameraStarted;

		public bool IsSubmitting => isSubmitting;

		[Inject]
		private HttpClient Http { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenElement(0, "div");
			__builder.AddAttribute(1, "class", Minimal ? "" : "bg-surface border border-on-surface p-8 md:p-12 space-y-10 animate-fade-in");
			if (!Minimal)
			{
				__builder.OpenElement(2, "div");
				__builder.AddAttribute(3, "class", "flex justify-between items-center");
				__builder.OpenElement(4, "h2");
				__builder.AddAttribute(5, "class", "font-headline-md text-headline-md");
				__builder.AddContent(6, (photos.Count >= 3) ? "Ready to Submit" : "Live Stream");
				__builder.CloseElement();
				if (cameraStarted)
				{
					__builder.AddMarkupContent(7, "<span class=\"flex items-center gap-2 text-primary\"><span class=\"w-2 h-2 bg-primary animate-pulse-dot\"></span>\n                    <span class=\"font-label-caps text-label-sm\">Live</span></span>");
				}
				__builder.CloseElement();
			}
			__builder.OpenElement(8, "div");
			__builder.AddAttribute(9, "class", "relative w-full " + (Minimal ? "aspect-[4/3] rounded-2xl border-2 border-on-surface-variant/30" : "aspect-square border border-on-surface") + " bg-on-surface overflow-hidden");
			__builder.OpenElement(10, "video");
			__builder.AddAttribute(11, "id", cameraElementId);
			__builder.AddAttribute(12, "autoplay");
			__builder.AddAttribute(13, "class", "absolute inset-0 w-full h-full");
			__builder.AddAttribute(14, "style", "object-fit: cover; transform: scaleX(-1);");
			__builder.CloseElement();
			if (cameraStarted && photos.Count < 3)
			{
				if (Minimal)
				{
					__builder.OpenElement(15, "div");
					__builder.AddAttribute(16, "class", "absolute inset-0 flex items-center justify-center z-10 pointer-events-none");
					__builder.OpenElement(17, "svg");
					__builder.AddAttribute(18, "width", "80");
					__builder.AddAttribute(19, "height", "320");
					__builder.AddAttribute(20, "viewBox", "0 0 80 320");
					__builder.AddAttribute(21, "class", "w-14 h-80");
					__builder.OpenElement(22, "path");
					__builder.AddAttribute(23, "d", (photos.Count == 0) ? "M 40 10 Q 40 160 40 310" : ((photos.Count == 1) ? "M 40 10 Q -15 160 40 310" : "M 40 10 Q 95 160 40 310"));
					__builder.AddAttribute(24, "fill", "none");
					__builder.AddAttribute(25, "stroke", "#b0252b");
					__builder.AddAttribute(26, "stroke-width", "2.5");
					__builder.AddAttribute(27, "stroke-linecap", "round");
					__builder.AddAttribute(28, "style", "transition: d 1.2s ease");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				else
				{
					__builder.AddMarkupContent(29, "<div class=\"absolute left-0 right-0 h-0.5 bg-primary scan-line z-10\" style=\"box-shadow: 0 0 15px rgba(176,37,43,0.6);\"></div>");
					__builder.AddMarkupContent(30, "<div class=\"absolute inset-0 flex items-center justify-center pointer-events-none z-20\"><div class=\"camera-guide w-[80%] h-[80%] mx-auto my-auto flex items-center justify-center relative\"><div class=\"absolute top-0 left-0 w-6 h-6 border-t-2 border-l-2 border-primary opacity-60\"></div>\n                        <div class=\"absolute top-0 right-0 w-6 h-6 border-t-2 border-r-2 border-primary opacity-60\"></div>\n                        <div class=\"absolute bottom-0 left-0 w-6 h-6 border-b-2 border-l-2 border-primary opacity-60\"></div>\n                        <div class=\"absolute bottom-0 right-0 w-6 h-6 border-b-2 border-r-2 border-primary opacity-60\"></div></div></div>");
					__builder.OpenElement(31, "div");
					__builder.AddAttribute(32, "class", "absolute inset-0 flex flex-col items-center justify-center z-30");
					__builder.AddMarkupContent(33, "<svg viewBox=\"0 0 100 100\" class=\"absolute\" style=\"width: 190px; height: 230px; color: rgba(255, 255, 255, 0.2); pointer-events: none;\"><path d=\"M 50 15 C 32 15, 30 50, 32 70 C 35 85, 45 92, 50 92 C 55 92, 65 85, 68 70 C 70 50, 68 15, 50 15 Z\" fill=\"none\" stroke=\"currentColor\" stroke-dasharray=\"3,3\" stroke-width=\"1\"></path></svg>");
					if (photos.Count == 1)
					{
						__builder.AddMarkupContent(34, "<div class=\"absolute bottom-8 flex items-center gap-2\"><span class=\"material-symbols-outlined text-background text-sm\">arrow_back</span>\n                            <span class=\"font-label-caps text-background text-[10px]\">Tilt Left</span></div>");
					}
					else if (photos.Count == 2)
					{
						__builder.AddMarkupContent(35, "<div class=\"absolute bottom-8 flex items-center gap-2\"><span class=\"material-symbols-outlined text-background text-sm\">arrow_forward</span>\n                            <span class=\"font-label-caps text-background text-[10px]\">Tilt Right</span></div>");
					}
					else if (photos.Count == 0)
					{
						__builder.AddMarkupContent(36, "<div class=\"absolute bottom-8\"><span class=\"font-label-caps text-background text-[10px]\">Look Straight</span></div>");
					}
					__builder.CloseElement();
				}
			}
			if (!cameraStarted && photos.Count > 0)
			{
				__builder.OpenElement(37, "div");
				__builder.AddAttribute(38, "class", "absolute inset-0 flex items-center justify-center bg-on-surface/60");
				__builder.OpenElement(39, "div");
				__builder.AddAttribute(40, "class", "text-center");
				__builder.AddMarkupContent(41, "<span class=\"material-symbols-outlined text-background text-6xl\">check_circle</span>\n                    ");
				__builder.OpenElement(42, "p");
				__builder.AddAttribute(43, "class", "text-background font-label-caps text-[10px] tracking-widest mt-2");
				__builder.AddContent(44, photos.Count);
				__builder.AddContent(45, " of 3 Captured");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.CloseElement();
			if (!Minimal && !HideActionArea)
			{
				__builder.OpenElement(46, "div");
				__builder.AddAttribute(47, "class", "flex justify-center gap-3");
				__builder.OpenElement(48, "span");
				__builder.AddAttribute(49, "class", ((photos.Count >= 1) ? "bg-primary text-on-primary" : "bg-surface-container-highest text-on-surface-variant") + " font-label-caps px-3 py-1.5 flex items-center gap-1.5 transition-colors");
				__builder.OpenElement(50, "span");
				__builder.AddAttribute(51, "class", "material-symbols-outlined text-sm");
				__builder.AddAttribute(52, "style", "font-variation-settings: 'FILL' 1;");
				__builder.AddContent(53, (photos.Count >= 1) ? "check_circle" : "radio_button_unchecked");
				__builder.CloseElement();
				__builder.AddMarkupContent(54, "\n                Front\n            ");
				__builder.CloseElement();
				__builder.AddMarkupContent(55, "\n            ");
				__builder.OpenElement(56, "span");
				__builder.AddAttribute(57, "class", ((photos.Count >= 2) ? "bg-primary text-on-primary" : "bg-surface-container-highest text-on-surface-variant") + " font-label-caps px-3 py-1.5 flex items-center gap-1.5 transition-colors");
				__builder.OpenElement(58, "span");
				__builder.AddAttribute(59, "class", "material-symbols-outlined text-sm");
				__builder.AddAttribute(60, "style", "font-variation-settings: 'FILL' 1;");
				__builder.AddContent(61, (photos.Count >= 2) ? "check_circle" : "radio_button_unchecked");
				__builder.CloseElement();
				__builder.AddMarkupContent(62, "\n                Left\n            ");
				__builder.CloseElement();
				__builder.AddMarkupContent(63, "\n            ");
				__builder.OpenElement(64, "span");
				__builder.AddAttribute(65, "class", ((photos.Count >= 3) ? "bg-primary text-on-primary" : "bg-surface-container-highest text-on-surface-variant") + " font-label-caps px-3 py-1.5 flex items-center gap-1.5 transition-colors");
				__builder.OpenElement(66, "span");
				__builder.AddAttribute(67, "class", "material-symbols-outlined text-sm");
				__builder.AddAttribute(68, "style", "font-variation-settings: 'FILL' 1;");
				__builder.AddContent(69, (photos.Count >= 3) ? "check_circle" : "radio_button_unchecked");
				__builder.CloseElement();
				__builder.AddMarkupContent(70, "\n                Right\n            ");
				__builder.CloseElement();
				__builder.CloseElement();
				if (photos.Count < 3)
				{
					__builder.OpenElement(71, "div");
					__builder.AddAttribute(72, "class", "grid grid-cols-2 gap-4");
					__builder.OpenElement(73, "button");
					__builder.AddAttribute(74, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)StartCamera));
					__builder.AddAttribute(75, "disabled", cameraStarted);
					__builder.AddAttribute(76, "class", "border border-on-surface text-on-surface font-label-caps py-4 px-6 hover:bg-surface-container-highest transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed");
					__builder.AddMarkupContent(77, "<span class=\"material-symbols-outlined\">videocam</span>\n                    Open Camera\n                ");
					__builder.CloseElement();
					__builder.AddMarkupContent(78, "\n                ");
					__builder.OpenElement(79, "button");
					__builder.AddAttribute(80, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CapturePhoto));
					__builder.AddAttribute(81, "disabled", !cameraStarted || photos.Count >= 3);
					__builder.AddAttribute(82, "class", "bg-on-surface text-background font-label-caps py-4 px-6 hover:bg-primary transition-all duration-300 flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed");
					__builder.AddMarkupContent(83, "<span class=\"material-symbols-outlined\">camera</span>\n                    Capture (");
					__builder.AddContent(84, photos.Count);
					__builder.AddMarkupContent(85, "/3)\n                ");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(86, "button");
					__builder.AddAttribute(87, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitEnrollment));
					__builder.AddAttribute(88, "disabled", isSubmitting);
					__builder.AddAttribute(89, "class", "w-full bg-on-surface text-background font-label-caps py-6 px-8 hover:bg-primary transition-all duration-300 flex items-center justify-center gap-3 group disabled:opacity-50 disabled:cursor-not-allowed");
					if (isSubmitting)
					{
						__builder.AddMarkupContent(90, "<span class=\"material-symbols-outlined animate-spin\">sync</span>\n                    ");
						__builder.AddMarkupContent(91, "<span>Enrolling...</span>");
					}
					else
					{
						__builder.AddMarkupContent(92, "<span class=\"material-symbols-outlined\">cloud_upload</span>\n                    ");
						__builder.AddMarkupContent(93, "<span>Submit Enrollment</span>");
					}
					__builder.CloseElement();
				}
				if (ShowCancel)
				{
					__builder.OpenElement(94, "button");
					__builder.AddAttribute(95, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)async delegate
					{
						await OnCancel.InvokeAsync();
					}));
					__builder.AddAttribute(96, "class", "w-full border border-on-surface-variant/30 text-on-surface-variant font-label-caps py-2 px-4 hover:border-primary hover:text-primary transition-all duration-300 flex items-center justify-center gap-2 text-sm mt-2");
					__builder.AddMarkupContent(97, "\n                Cancel\n            ");
					__builder.CloseElement();
				}
			}
			__builder.CloseElement();
		}

		public async Task StartCamera()
		{
			await JS.InvokeVoidAsync("cameraInterop.startCamera", cameraElementId);
			cameraStarted = true;
			StateHasChanged();
		}

		public async Task CapturePhoto()
		{
			string item = await JSRuntimeExtensions.InvokeAsync<string>(JS, "cameraInterop.captureFrame", new object[1] { cameraElementId });
			photos.Add(item);
			StateHasChanged();
			await PhotosChanged.InvokeAsync(photos.Count);
		}

		public void ResetPhotos()
		{
			photos.Clear();
			StateHasChanged();
			PhotosChanged.InvokeAsync(0);
		}

		public async Task SubmitEnrollment()
		{
			isSubmitting = true;
			StateHasChanged();
			try
			{
				if (string.IsNullOrEmpty(SubmitEndpoint))
				{
					await OnError.InvokeAsync("No enrollment endpoint configured.");
					return;
				}
				string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				if (string.IsNullOrEmpty(text) || text == "null" || text == "undefined")
				{
					await OnError.InvokeAsync("You must be logged in to enroll.");
					return;
				}
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, SubmitEndpoint);
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", text);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					images = photos
				});
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				if (res.IsSuccessStatusCode)
				{
					photos.Clear();
					cameraStarted = false;
					await OnComplete.InvokeAsync();
					return;
				}
				string text2 = await res.Content.ReadAsStringAsync();
				string error;
				try
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(text2);
					error = (jsonDocument.RootElement.TryGetProperty("message", out var value) ? (value.GetString() ?? "Unknown error") : $"Server error ({res.StatusCode})");
				}
				catch
				{
					error = $"Server error ({res.StatusCode}): {text2.Substring(0, Math.Min(text2.Length, 200))}";
				}
				photos.Clear();
				cameraStarted = false;
				await JS.InvokeVoidAsync("cameraInterop.stopCamera");
				await OnError.InvokeAsync(error);
			}
			catch (Exception ex)
			{
				photos.Clear();
				cameraStarted = false;
				await JS.InvokeVoidAsync("cameraInterop.stopCamera");
				await OnError.InvokeAsync("Connection error: " + ex.Message);
			}
			finally
			{
				isSubmitting = false;
			}
		}
	}
}
