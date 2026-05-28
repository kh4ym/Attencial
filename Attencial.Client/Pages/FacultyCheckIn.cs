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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/faculty/check-in")]
	public class FacultyCheckIn : ComponentBase, IAsyncDisposable
	{
		private class PendingShiftDto
		{
			public int RecordId { get; set; }

			public string ProfessorName { get; set; } = string.Empty;

			public string Department { get; set; } = string.Empty;

			public DateTime CheckInTime { get; set; }

			public DateTime? CheckOutTime { get; set; }

			public double? HoursWorked { get; set; }

			public string Status { get; set; } = string.Empty;
		}

		private string activeTab = "scan";

		private bool cameraStarted;

		private bool isSubmitting;

		private bool isLoadingAdmin;

		private string? message;

		private string actionType = "";

		private List<PendingShiftDto> pendingRecords = new List<PendingShiftDto>();

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
				renderTreeBuilder.AddMarkupContent(2, "Faculty HR Check-In — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "card-neo-raised animate-fade-in w-full mx-auto canvas-bg");
			__builder.AddAttribute(6, "style", "max-width: 650px;");
			__builder.AddMarkupContent(7, "<div class=\"text-center mb-4\"><div class=\"inline-flex items-center justify-center w-14 h-14 mb-3 border border-on-surface\"><span class=\"material-symbols-outlined text-2xl text-primary\">verified_user</span></div>\n        <h2 class=\"font-headline-md text-headline-md text-on-surface mb-1\">Faculty HR Portal</h2>\n        <p class=\"text-body-md text-on-surface-variant\">Webcam face-scanning portal for check-in, check-out, and profile activation.</p></div>\n\n    ");
			__builder.OpenElement(8, "div");
			__builder.AddAttribute(9, "class", "flex gap-2 justify-center mb-4");
			__builder.OpenElement(10, "button");
			__builder.AddAttribute(11, "class", (activeTab == "scan") ? "btn-neo-primary" : "btn-neo-outline");
			__builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ChangeTab("scan")));
			__builder.AddMarkupContent(13, "<span class=\"material-symbols-outlined\">videocam</span> Check In / Out\n        ");
			__builder.CloseElement();
			__builder.AddMarkupContent(14, "\n        ");
			__builder.OpenElement(15, "button");
			__builder.AddAttribute(16, "class", (activeTab == "admin") ? "btn-neo-primary" : "btn-neo-outline");
			__builder.AddAttribute(17, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ChangeTab("admin")));
			__builder.AddMarkupContent(18, "<span class=\"material-symbols-outlined\">checklist</span> Review Queue\n        ");
			__builder.CloseElement();
			__builder.CloseElement();
			if (activeTab == "scan")
			{
				__builder.OpenElement(19, "div");
				__builder.AddAttribute(20, "class", "animate-fade-in");
				if (message != null)
				{
					string text = (message.StartsWith("❌") ? "#ba1a1a" : (message.StartsWith("✅") ? "#006191" : "#006191"));
					string textContent = (message.StartsWith("❌") ? "warning" : (message.StartsWith("✅") ? "check_circle" : "info"));
					__builder.OpenElement(21, "div");
					__builder.AddAttribute(22, "class", "card-neo mb-4 flex items-start gap-2 animate-fade-in");
					__builder.AddAttribute(23, "style", "border-left: 3px solid " + text + ";");
					__builder.OpenElement(24, "span");
					__builder.AddAttribute(25, "class", "material-symbols-outlined mt-0.5");
					__builder.AddAttribute(26, "style", "color: " + text + ";");
					__builder.AddContent(27, textContent);
					__builder.CloseElement();
					__builder.AddMarkupContent(28, "\n                    ");
					__builder.OpenElement(29, "span");
					__builder.AddAttribute(30, "class", "text-body-md text-start");
					__builder.AddContent(31, message.Replace("❌ ", "").Replace("✅ ", ""));
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(32, "div");
				__builder.AddAttribute(33, "class", "text-center mb-4 relative mx-auto");
				__builder.AddAttribute(34, "style", "max-width: 400px;");
				__builder.OpenElement(35, "div");
				__builder.AddAttribute(36, "class", "card-neo-hover relative overflow-hidden");
				__builder.AddAttribute(37, "style", "height: 300px;");
				__builder.AddMarkupContent(38, "<video id=\"scanCamera\" autoplay width=\"100%\" height=\"100%\" style=\"object-fit: cover; transform: scaleX(-1);\"></video>");
				if (cameraStarted && !isSubmitting)
				{
					__builder.AddMarkupContent(39, "<div class=\"camera-scan-line\"></div>");
				}
				if (cameraStarted)
				{
					__builder.AddMarkupContent(40, "<div class=\"camera-overlay-container\"><svg viewBox=\"0 0 100 100\" class=\"absolute\" style=\"width: 190px; height: 230px; color: rgba(27, 28, 26, 0.2); pointer-events: none;\"><path d=\"M 50 15 C 32 15, 30 50, 32 70 C 35 85, 45 92, 50 92 C 55 92, 65 85, 68 70 C 70 50, 68 15, 50 15 Z\" fill=\"none\" stroke=\"currentColor\" stroke-dasharray=\"3,3\" stroke-width=\"1\"></path></svg></div>");
				}
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(41, "\n\n            ");
				__builder.OpenElement(42, "div");
				__builder.AddAttribute(43, "class", "flex gap-2");
				if (!cameraStarted)
				{
					__builder.OpenElement(44, "button");
					__builder.AddAttribute(45, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)StartCamera));
					__builder.AddAttribute(46, "class", "btn-neo-primary py-2 w-full");
					__builder.AddMarkupContent(47, "<span class=\"material-symbols-outlined\">videocam</span> Start Kiosk Camera\n                    ");
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(48, "button");
					__builder.AddAttribute(49, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)ProcessCheckIn));
					__builder.AddAttribute(50, "disabled", isSubmitting);
					__builder.AddAttribute(51, "class", "btn-neo-primary py-2 flex-grow");
					if (isSubmitting && actionType == "checkin")
					{
						__builder.AddMarkupContent(52, "<span class=\"material-symbols-outlined animate-spin\">refresh</span>\n                            ");
						__builder.AddMarkupContent(53, "<span>Checking In...</span>");
					}
					else
					{
						__builder.AddMarkupContent(54, "<span><span class=\"material-symbols-outlined\">login</span> Check In</span>");
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(55, "\n                    ");
					__builder.OpenElement(56, "button");
					__builder.AddAttribute(57, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)ProcessCheckOut));
					__builder.AddAttribute(58, "disabled", isSubmitting);
					__builder.AddAttribute(59, "class", "btn-neo-outline py-2 flex-grow");
					if (isSubmitting && actionType == "checkout")
					{
						__builder.AddMarkupContent(60, "<span class=\"material-symbols-outlined animate-spin\">refresh</span>\n                            ");
						__builder.AddMarkupContent(61, "<span>Checking Out...</span>");
					}
					else
					{
						__builder.AddMarkupContent(62, "<span><span class=\"material-symbols-outlined\">logout</span> Check Out</span>");
					}
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (activeTab == "admin")
			{
				__builder.OpenElement(63, "div");
				__builder.AddAttribute(64, "class", "animate-fade-in");
				__builder.OpenElement(65, "div");
				__builder.AddAttribute(66, "class", "flex justify-between items-center mb-3");
				__builder.OpenElement(67, "h4");
				__builder.AddAttribute(68, "class", "font-headline-md text-headline-md text-on-surface");
				__builder.AddContent(69, "Pending Review Shifts (");
				__builder.OpenElement(70, "span");
				__builder.AddAttribute(71, "class", "text-primary");
				__builder.AddContent(72, pendingRecords.Count);
				__builder.CloseElement();
				__builder.AddContent(73, ")");
				__builder.CloseElement();
				__builder.AddMarkupContent(74, "\n                ");
				__builder.OpenElement(75, "button");
				__builder.AddAttribute(76, "class", "btn-neo-outline");
				__builder.AddAttribute(77, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)LoadPendingShifts));
				__builder.AddMarkupContent(78, "<span class=\"material-symbols-outlined\">refresh</span> Refresh\n                ");
				__builder.CloseElement();
				__builder.CloseElement();
				if (isLoadingAdmin)
				{
					__builder.AddMarkupContent(79, "<div class=\"text-center py-12\"><span class=\"material-symbols-outlined animate-spin text-primary mb-3 block\">refresh</span>\n                    <p class=\"text-body-md text-on-surface-variant\">Loading queue...</p></div>");
				}
				else if (pendingRecords.Count == 0)
				{
					__builder.AddMarkupContent(80, "<div class=\"card-neo-hover text-center py-8\"><span class=\"material-symbols-outlined text-3xl text-tertiary mb-2 block\">check_circle</span>\n                    <p class=\"font-body-md font-bold text-on-surface mb-0\">All shifts clear!</p>\n                    <p class=\"text-body-md text-on-surface-variant\">No short-shift records require review.</p></div>");
				}
				else
				{
					__builder.OpenElement(81, "div");
					__builder.AddAttribute(82, "class", "card-neo p-0 overflow-hidden");
					__builder.OpenElement(83, "table");
					__builder.AddAttribute(84, "class", "table-neo");
					__builder.AddMarkupContent(85, "<thead><tr><th>Faculty</th>\n                                <th>Dept</th>\n                                <th class=\"text-center\">Check-In</th>\n                                <th class=\"text-center\">Check-Out</th>\n                                <th class=\"text-center\">Hours</th>\n                                <th class=\"text-center\">Status</th></tr></thead>\n                        ");
					__builder.OpenElement(86, "tbody");
					foreach (PendingShiftDto pendingRecord in pendingRecords)
					{
						__builder.OpenElement(87, "tr");
						__builder.AddAttribute(88, "class", "hover:bg-surface-container-low transition-colors");
						__builder.OpenElement(89, "td");
						__builder.AddAttribute(90, "class", "font-bold text-on-surface");
						__builder.AddContent(91, pendingRecord.ProfessorName);
						__builder.CloseElement();
						__builder.AddMarkupContent(92, "\n                                    ");
						__builder.OpenElement(93, "td");
						__builder.AddAttribute(94, "class", "text-on-surface-variant");
						__builder.AddContent(95, pendingRecord.Department);
						__builder.CloseElement();
						__builder.AddMarkupContent(96, "\n                                    ");
						__builder.OpenElement(97, "td");
						__builder.AddAttribute(98, "class", "text-center text-on-surface-variant");
						__builder.AddContent(99, pendingRecord.CheckInTime.ToLocalTime().ToString("t"));
						__builder.CloseElement();
						__builder.AddMarkupContent(100, "\n                                    ");
						__builder.OpenElement(101, "td");
						__builder.AddAttribute(102, "class", "text-center text-on-surface-variant");
						__builder.AddContent(103, pendingRecord.CheckOutTime?.ToLocalTime().ToString("t"));
						__builder.CloseElement();
						__builder.AddMarkupContent(104, "\n                                    ");
						__builder.OpenElement(105, "td");
						__builder.AddAttribute(106, "class", "text-center font-bold text-primary");
						__builder.AddContent(107, pendingRecord.HoursWorked.HasValue ? pendingRecord.HoursWorked.Value.ToString("F2") : "-");
						__builder.CloseElement();
						__builder.AddMarkupContent(108, "\n                                    ");
						__builder.AddMarkupContent(109, "<td class=\"text-center\"><span class=\"badge-neo badge-neo-pending\">\n                                            ShortShift\n                                        </span></td>");
						__builder.CloseElement();
					}
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
		}

		private async Task ChangeTab(string tab)
		{
			if (cameraStarted)
			{
				await StopCamera();
			}
			activeTab = tab;
			message = null;
			if (tab == "admin")
			{
				await LoadPendingShifts();
			}
		}

		private async Task StartCamera()
		{
			message = null;
			await JS.InvokeVoidAsync("cameraInterop.startCamera", "scanCamera");
			cameraStarted = true;
		}

		private async Task StopCamera()
		{
			try
			{
				await JS.InvokeVoidAsync("cameraInterop.stopCamera");
			}
			catch
			{
			}
			cameraStarted = false;
		}

		private async Task ProcessCheckIn()
		{
			await ProcessAttendanceAction("checkin", "api/faculty/attendance/checkin");
		}

		private async Task ProcessProcessCheckOut()
		{
			await ProcessAttendanceAction("checkout", "api/faculty/attendance/checkout");
		}

		private async Task ProcessCheckOut()
		{
			await ProcessAttendanceAction("checkout", "api/faculty/attendance/checkout");
		}

		private async Task ProcessAttendanceAction(string type, string endpoint)
		{
			if (isSubmitting)
			{
				return;
			}
			isSubmitting = true;
			actionType = type;
			message = "⏳ Verify face and logging record...";
			StateHasChanged();
			try
			{
				string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "cameraInterop.captureFrame", new object[1] { "scanCamera" });
				if (text.Contains(","))
				{
					text = text.Split(',')[1];
				}
				FacultyFaceScanRequest value = new FacultyFaceScanRequest
				{
					Image = text
				};
				HttpResponseMessage res = await Http.PostAsJsonAsync(endpoint, value);
				ApiResponse<object> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<object>>();
				if (res.IsSuccessStatusCode && apiResponse != null && apiResponse.Success)
				{
					message = "✅ " + apiResponse.Message;
					await StopCamera();
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Action failed. Please try again.");
				}
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				isSubmitting = false;
				actionType = "";
			}
		}

		private async Task LoadPendingShifts()
		{
			isLoadingAdmin = true;
			pendingRecords.Clear();
			StateHasChanged();
			try
			{
				string parameter = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/admin/faculty/pending");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					ApiResponse<List<PendingShiftDto>> apiResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<List<PendingShiftDto>>>();
					if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
					{
						pendingRecords = apiResponse.Data;
					}
				}
			}
			catch
			{
			}
			finally
			{
				isLoadingAdmin = false;
			}
		}

		public async ValueTask DisposeAsync()
		{
			if (cameraStarted)
			{
				await StopCamera();
			}
		}
	}
}
