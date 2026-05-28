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
using System.Globalization;
using System.Net.Http.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/leave-requests")]
	public class LeaveRequests : ComponentBase
	{
		private List<LeaveRequestResponseDto> leaves = new List<LeaveRequestResponseDto>();

		private bool isLoadingHistory = true;

		private bool isSubmitting;

		private string? message;

		private string newLeaveType = "";

		private DateTime newStartDate = DateTime.Today;

		private DateTime newEndDate = DateTime.Today.AddDays(1.0);

		private string newReason = "";

		private string? fileBase64;

		private string? fileName;

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
				renderTreeBuilder.AddMarkupContent(2, "Leave Requests — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "card-neo-raised animate-fade-in w-full mx-auto canvas-bg");
			__builder.AddAttribute(6, "style", "max-width: 800px;");
			__builder.AddMarkupContent(7, "<div class=\"text-center mb-4\"><div class=\"inline-flex items-center justify-center w-14 h-14 mb-3 border border-on-surface\"><span class=\"material-symbols-outlined text-2xl text-primary\">calendar_month</span></div>\n        <h2 class=\"font-headline-md text-headline-md text-on-surface mb-1\">My Leave Requests</h2>\n        <p class=\"text-body-md text-on-surface-variant\">Submit leave applications and track their approval status.</p></div>");
			if (message != null)
			{
				string text = (message.StartsWith("❌") ? "#ba1a1a" : (message.StartsWith("✅") ? "#006191" : "#006191"));
				string textContent = (message.StartsWith("❌") ? "warning" : (message.StartsWith("✅") ? "check_circle" : "info"));
				__builder.OpenElement(8, "div");
				__builder.AddAttribute(9, "class", "card-neo mb-4 flex items-start gap-2 animate-fade-in");
				__builder.AddAttribute(10, "style", "border-left: 3px solid " + text + ";");
				__builder.OpenElement(11, "span");
				__builder.AddAttribute(12, "class", "material-symbols-outlined mt-0.5");
				__builder.AddAttribute(13, "style", "color: " + text + ";");
				__builder.AddContent(14, textContent);
				__builder.CloseElement();
				__builder.AddMarkupContent(15, "\n            ");
				__builder.OpenElement(16, "span");
				__builder.AddAttribute(17, "class", "text-body-md text-start");
				__builder.AddContent(18, message.Replace("❌ ", "").Replace("✅ ", ""));
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.OpenElement(19, "div");
			__builder.AddAttribute(20, "class", "grid grid-cols-1 md:grid-cols-12 gap-gutter");
			__builder.OpenElement(21, "div");
			__builder.AddAttribute(22, "class", "md:col-span-5");
			__builder.OpenElement(23, "div");
			__builder.AddAttribute(24, "class", "card-neo-hover p-3");
			__builder.AddMarkupContent(25, "<h5 class=\"font-headline-md text-headline-md mb-3\"><span class=\"material-symbols-outlined text-primary\" style=\"vertical-align: middle;\">note_add</span> Apply for Leave\n                </h5>\n\n                ");
			__builder.OpenElement(26, "form");
			__builder.AddAttribute(27, "onsubmit", EventCallback.Factory.Create<EventArgs>((object)this, (Func<Task>)SubmitLeave));
			__builder.OpenElement(28, "div");
			__builder.AddAttribute(29, "class", "mb-3");
			__builder.AddMarkupContent(30, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">LEAVE TYPE</label>\n                        ");
			__builder.OpenElement(31, "select");
			__builder.AddAttribute(32, "class", "form-neo-select w-full");
			__builder.AddAttribute(33, "required");
			__builder.AddAttribute(34, "value", BindConverter.FormatValue(newLeaveType));
			__builder.AddAttribute(35, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				newLeaveType = __value;
			}, newLeaveType));
			__builder.SetUpdatesAttributeName("value");
			__builder.OpenElement(36, "option");
			__builder.AddAttribute(37, "value");
			__builder.AddContent(38, "-- Select Type --");
			__builder.CloseElement();
			__builder.AddMarkupContent(39, "\n                            ");
			__builder.OpenElement(40, "option");
			__builder.AddAttribute(41, "value", "Sick");
			__builder.AddContent(42, "Sick Leave");
			__builder.CloseElement();
			__builder.AddMarkupContent(43, "\n                            ");
			__builder.OpenElement(44, "option");
			__builder.AddAttribute(45, "value", "Casual");
			__builder.AddContent(46, "Casual Leave");
			__builder.CloseElement();
			__builder.AddMarkupContent(47, "\n                            ");
			__builder.OpenElement(48, "option");
			__builder.AddAttribute(49, "value", "Medical");
			__builder.AddContent(50, "Medical Leave");
			__builder.CloseElement();
			__builder.AddMarkupContent(51, "\n                            ");
			__builder.OpenElement(52, "option");
			__builder.AddAttribute(53, "value", "Personal");
			__builder.AddContent(54, "Personal Leave");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(55, "\n\n                    ");
			__builder.OpenElement(56, "div");
			__builder.AddAttribute(57, "class", "mb-3");
			__builder.AddMarkupContent(58, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">START DATE</label>\n                        ");
			__builder.OpenElement(59, "input");
			__builder.AddAttribute(60, "type", "date");
			__builder.AddAttribute(61, "class", "form-neo py-1 w-full");
			__builder.AddAttribute(62, "required");
			__builder.AddAttribute(63, "value", BindConverter.FormatValue(newStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
			__builder.AddAttribute(64, "onchange", EventCallback.Factory.CreateBinder(this, delegate(DateTime __value)
			{
				newStartDate = __value;
			}, newStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(65, "\n\n                    ");
			__builder.OpenElement(66, "div");
			__builder.AddAttribute(67, "class", "mb-3");
			__builder.AddMarkupContent(68, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">END DATE</label>\n                        ");
			__builder.OpenElement(69, "input");
			__builder.AddAttribute(70, "type", "date");
			__builder.AddAttribute(71, "class", "form-neo py-1 w-full");
			__builder.AddAttribute(72, "required");
			__builder.AddAttribute(73, "value", BindConverter.FormatValue(newEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
			__builder.AddAttribute(74, "onchange", EventCallback.Factory.CreateBinder(this, delegate(DateTime __value)
			{
				newEndDate = __value;
			}, newEndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(75, "\n\n                    ");
			__builder.OpenElement(76, "div");
			__builder.AddAttribute(77, "class", "mb-3");
			__builder.AddMarkupContent(78, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">REASON</label>\n                        ");
			__builder.OpenElement(79, "textarea");
			__builder.AddAttribute(80, "class", "form-neo w-full");
			__builder.AddAttribute(81, "style", "min-height: 80px;");
			__builder.AddAttribute(82, "placeholder", "Describe the reason for leave...");
			__builder.AddAttribute(83, "required");
			__builder.AddAttribute(84, "value", BindConverter.FormatValue(newReason));
			__builder.AddAttribute(85, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				newReason = __value;
			}, newReason));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(86, "\n\n                    ");
			__builder.OpenElement(87, "div");
			__builder.AddAttribute(88, "class", "mb-3");
			__builder.AddMarkupContent(89, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">PDF ATTACHMENT (OPTIONAL)</label>\n                        ");
			__builder.OpenElement(90, "div");
			__builder.AddAttribute(91, "class", "flex flex-col gap-1");
			__builder.OpenComponent<InputFile>(92);
			__builder.AddComponentParameter(93, "OnChange", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(EventCallback.Factory.Create((object)this, (Func<InputFileChangeEventArgs, Task>)OnInputFileChange)));
			__builder.AddComponentParameter(94, "accept", ".pdf");
			__builder.AddComponentParameter(95, "class", "form-neo file:btn-neo-outline file:border-0 file:mr-2 file:px-3 file:py-1 file:text-label-caps");
			__builder.AddComponentParameter(96, "style", "font-size: 12px;");
			__builder.CloseComponent();
			if (!string.IsNullOrEmpty(fileName))
			{
				__builder.OpenElement(97, "span");
				__builder.AddAttribute(98, "class", "text-tertiary font-label-sm");
				__builder.AddAttribute(99, "style", "font-size: 11px;");
				__builder.AddMarkupContent(100, "<span class=\"material-symbols-outlined text-sm\" style=\"vertical-align: middle;\">description</span> ");
				__builder.AddContent(101, fileName);
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(102, "\n\n                    ");
			__builder.OpenElement(103, "button");
			__builder.AddAttribute(104, "type", "submit");
			__builder.AddAttribute(105, "class", "btn-neo-primary w-full py-2 mt-2");
			__builder.AddAttribute(106, "disabled", isSubmitting);
			if (isSubmitting)
			{
				__builder.AddMarkupContent(107, "<span class=\"material-symbols-outlined animate-spin\">refresh</span>\n                            ");
				__builder.AddMarkupContent(108, "<span>Submitting...</span>");
			}
			else
			{
				__builder.AddMarkupContent(109, "<span>Submit Request</span>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(110, "\n\n        ");
			__builder.OpenElement(111, "div");
			__builder.AddAttribute(112, "class", "md:col-span-7");
			__builder.OpenElement(113, "div");
			__builder.AddAttribute(114, "class", "card-neo-hover p-3 h-full flex flex-col");
			__builder.AddMarkupContent(115, "<h5 class=\"font-headline-md text-headline-md mb-3\"><span class=\"material-symbols-outlined text-primary\" style=\"vertical-align: middle;\">history</span> Application History\n                </h5>");
			if (isLoadingHistory)
			{
				__builder.AddMarkupContent(116, "<div class=\"text-center py-12 my-auto\"><span class=\"material-symbols-outlined animate-spin text-primary mb-3 block\">refresh</span>\n                        <p class=\"text-body-md text-on-surface-variant\">Loading applications...</p></div>");
			}
			else if (leaves.Count == 0)
			{
				__builder.AddMarkupContent(117, "<div class=\"text-center py-12 my-auto text-on-surface-variant\"><span class=\"material-symbols-outlined text-3xl mb-2 block text-on-surface-variant/50\">folder_open</span>\n                        No leave applications found.\n                    </div>");
			}
			else
			{
				__builder.OpenElement(118, "div");
				__builder.AddAttribute(119, "class", "overflow-auto grow");
				__builder.AddAttribute(120, "style", "max-height: 480px; padding-right: 4px;");
				foreach (LeaveRequestResponseDto leaf in leaves)
				{
					__builder.OpenElement(121, "div");
					__builder.AddAttribute(122, "class", "card-neo-hover p-3 mb-3 animate-fade-in");
					__builder.AddAttribute(123, "style", "font-size: 12px;");
					__builder.OpenElement(124, "div");
					__builder.AddAttribute(125, "class", "flex justify-between items-start mb-2");
					__builder.OpenElement(126, "div");
					__builder.OpenElement(127, "span");
					__builder.AddAttribute(128, "class", "font-bold font-body-md text-on-surface");
					__builder.AddContent(129, leaf.LeaveType);
					__builder.AddContent(130, " Leave");
					__builder.CloseElement();
					__builder.AddMarkupContent(131, "\n                                        ");
					__builder.OpenElement(132, "span");
					__builder.AddAttribute(133, "class", "text-on-surface-variant block font-label-sm");
					__builder.AddAttribute(134, "style", "font-size: 11px;");
					__builder.AddContent(135, leaf.StartDate.ToLocalTime().ToString("d"));
					__builder.AddContent(136, " to ");
					__builder.AddContent(137, leaf.EndDate.ToLocalTime().ToString("d"));
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(138, "\n                                    ");
					__builder.OpenElement(139, "span");
					__builder.AddAttribute(140, "class", "px-2 py-1 " + ((leaf.Status == "Approved") ? "badge-neo badge-neo-success" : ((leaf.Status == "Rejected") ? "badge-neo badge-neo-pending" : "badge-neo")));
					__builder.AddAttribute(141, "style", "font-size: 10px;");
					__builder.AddContent(142, leaf.Status);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(143, "\n\n                                ");
					__builder.OpenElement(144, "p");
					__builder.AddAttribute(145, "class", "text-on-surface-variant mb-2 text-body-md");
					__builder.AddAttribute(146, "style", "line-height: 1.4;");
					__builder.AddMarkupContent(147, "<strong>Reason:</strong> ");
					__builder.AddContent(148, leaf.Reason);
					__builder.CloseElement();
					if (!string.IsNullOrEmpty(leaf.AttachmentUrl))
					{
						__builder.OpenElement(149, "div");
						__builder.AddAttribute(150, "class", "mb-2");
						__builder.OpenElement(151, "a");
						__builder.AddAttribute(152, "href", GetAbsoluteAttachmentUrl(leaf.AttachmentUrl));
						__builder.AddAttribute(153, "target", "_blank");
						__builder.AddAttribute(154, "class", "no-underline font-label-caps text-label-caps text-primary");
						__builder.AddMarkupContent(155, "<span class=\"material-symbols-outlined text-sm\" style=\"vertical-align: middle;\">description</span> View Attached PDF\n                                        ");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					if (!string.IsNullOrEmpty(leaf.AdminNote))
					{
						__builder.OpenElement(156, "div");
						__builder.AddAttribute(157, "class", "border-t border-on-surface/10 pt-2 mt-2");
						__builder.AddAttribute(158, "style", "font-size: 11px;");
						__builder.AddMarkupContent(159, "<strong class=\"text-on-surface\">Reviewer Note:</strong>\n                                        ");
						__builder.OpenElement(160, "p");
						__builder.AddAttribute(161, "class", "text-on-surface-variant mb-0 italic mt-1 font-body-md");
						__builder.AddContent(162, "\"");
						__builder.AddContent(163, leaf.AdminNote);
						__builder.AddContent(164, "\"");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (string.IsNullOrEmpty(text) || text == "null")
			{
				Nav.NavigateTo("/login");
			}
			else
			{
				await LoadHistory(text);
			}
		}

		private async Task LoadHistory(string token)
		{
			isLoadingHistory = true;
			leaves.Clear();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/leave");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					ApiResponse<List<LeaveRequestResponseDto>> apiResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<List<LeaveRequestResponseDto>>>();
					if (apiResponse != null && apiResponse.Success && apiResponse.Data != null)
					{
						leaves = apiResponse.Data;
					}
				}
			}
			catch (Exception ex)
			{
				message = "❌ Error loading history: " + ex.Message;
			}
			finally
			{
				isLoadingHistory = false;
			}
		}

		private async Task OnInputFileChange(InputFileChangeEventArgs e)
		{
			IBrowserFile file = e.File;
			if (file == null)
			{
				return;
			}
			if (file.ContentType != "application/pdf")
			{
				message = "❌ Only PDF files are supported.";
				return;
			}
			try
			{
				int num = 5242880;
				using Stream stream = file.OpenReadStream(num);
				using MemoryStream ms = new MemoryStream();
				await stream.CopyToAsync(ms);
				byte[] inArray = ms.ToArray();
				fileBase64 = Convert.ToBase64String(inArray);
				fileName = file.Name;
				message = null;
			}
			catch (Exception ex)
			{
				message = "❌ Failed to read file: " + ex.Message;
			}
		}

		private async Task SubmitLeave()
		{
			if (isSubmitting)
			{
				return;
			}
			if (string.IsNullOrEmpty(newLeaveType))
			{
				message = "❌ Please select a leave type.";
				return;
			}
			if (newEndDate < newStartDate)
			{
				message = "❌ End Date cannot be earlier than Start Date.";
				return;
			}
			isSubmitting = true;
			message = "⏳ Submitting leave application...";
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				LeaveRequestCreateRequest inputValue = new LeaveRequestCreateRequest
				{
					LeaveType = newLeaveType,
					Reason = newReason.Trim(),
					StartDate = newStartDate,
					EndDate = newEndDate,
					AttachmentBase64 = fileBase64,
					AttachmentFileName = fileName
				};
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/leave");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpRequestMessage.Content = JsonContent.Create(inputValue);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode && apiResponse != null && apiResponse.Success)
				{
					message = "✅ Leave request submitted successfully!";
					newLeaveType = "";
					newStartDate = DateTime.Today;
					newEndDate = DateTime.Today.AddDays(1.0);
					newReason = "";
					fileBase64 = null;
					fileName = null;
					await LoadHistory(token);
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Submission failed.");
				}
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				isSubmitting = false;
			}
		}

		private string GetAbsoluteAttachmentUrl(string relativeUrl)
		{
			return (Http.BaseAddress?.ToString() ?? "http://localhost:5158/").TrimEnd('/') + "/" + relativeUrl.TrimStart('/');
		}

		private string GetBadgeClass(string status)
		{
			if (!(status == "Approved"))
			{
				if (status == "Rejected")
				{
					return "bg-danger text-white";
				}
				return "bg-warning text-dark";
			}
			return "bg-success text-white";
		}
	}
}
