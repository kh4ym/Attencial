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
	[Route("/admin/leave-review")]
	public class AdminLeaveReview : ComponentBase
	{
		private List<LeaveRequestResponseDto> pendingRequests = new List<LeaveRequestResponseDto>();

		private bool isLoading = true;

		private bool isSubmitting;

		private string? message;

		private Dictionary<int, string> notes = new Dictionary<int, string>();

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
				renderTreeBuilder.AddMarkupContent(2, "Leave Requests Review Queue — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "min-h-screen canvas-bg");
			__builder.OpenElement(6, "div");
			__builder.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile lg:px-margin-desktop py-12");
			__builder.OpenElement(8, "div");
			__builder.AddAttribute(9, "class", "card-neo-raised p-6 md:p-8");
			__builder.AddMarkupContent(10, "<div class=\"text-center mb-4\"><div class=\"inline-flex items-center justify-center w-14 h-14 mb-3 border border-on-surface\"><span class=\"material-symbols-outlined text-2xl text-primary\">verified_user</span></div>\n                <h2 class=\"font-headline-md text-headline-md text-on-surface mb-1\">Leave Review Queue</h2>\n                <p class=\"text-body-md text-on-surface-variant\">Review pending leave applications from faculty members.</p></div>");
			if (message != null)
			{
				string text = (message.StartsWith("❌") ? "#ba1a1a" : (message.StartsWith("✅") ? "#006191" : "#b0252b"));
				string textContent = (message.StartsWith("❌") ? "warning" : (message.StartsWith("✅") ? "check_circle" : "info"));
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "card-neo mb-4 flex items-start gap-2 animate-fade-in");
				__builder.AddAttribute(13, "style", "border-left: 3px solid " + text + ";");
				__builder.OpenElement(14, "span");
				__builder.AddAttribute(15, "class", "material-symbols-outlined mt-0.5");
				__builder.AddAttribute(16, "style", "color: " + text + ";");
				__builder.AddContent(17, textContent);
				__builder.CloseElement();
				__builder.AddMarkupContent(18, "\n                    ");
				__builder.OpenElement(19, "span");
				__builder.AddAttribute(20, "class", "text-body-md text-start");
				__builder.AddContent(21, message.Replace("❌ ", "").Replace("✅ ", ""));
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (isLoading)
			{
				__builder.AddMarkupContent(22, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined animate-spin text-primary mb-3 block\">refresh</span>\n                    <p class=\"text-body-md text-on-surface-variant\">Loading pending reviews...</p></div>");
			}
			else if (pendingRequests.Count == 0)
			{
				__builder.AddMarkupContent(23, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined text-4xl text-tertiary mb-3 block\">sentiment_satisfied</span>\n                    <h4 class=\"font-headline-md text-headline-md text-on-surface mb-1\">All Clear!</h4>\n                    <p class=\"text-body-md text-on-surface-variant\">No pending leave requests found in the review queue.</p></div>");
			}
			else
			{
				__builder.OpenElement(24, "div");
				__builder.AddAttribute(25, "class", "flex flex-col gap-4");
				foreach (LeaveRequestResponseDto req in pendingRequests)
				{
					__builder.OpenElement(26, "div");
					__builder.AddAttribute(27, "class", "card-neo-hover p-4 animate-fade-in");
					__builder.OpenElement(28, "div");
					__builder.AddAttribute(29, "class", "flex justify-between items-start flex-wrap gap-2 mb-3");
					__builder.OpenElement(30, "div");
					__builder.OpenElement(31, "h4");
					__builder.AddAttribute(32, "class", "font-headline-md text-headline-md text-on-surface mb-1");
					__builder.AddContent(33, req.ProfessorName);
					__builder.CloseElement();
					__builder.AddMarkupContent(34, "\n                                    ");
					__builder.OpenElement(35, "span");
					__builder.AddAttribute(36, "class", "badge-neo px-2 py-1 text-label-caps");
					__builder.AddContent(37, req.Department);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(38, "\n                                ");
					__builder.OpenElement(39, "div");
					__builder.AddAttribute(40, "class", "text-right");
					__builder.OpenElement(41, "span");
					__builder.AddAttribute(42, "class", "badge-neo badge-neo-pending px-2 py-1 text-label-caps");
					__builder.AddContent(43, req.LeaveType);
					__builder.AddMarkupContent(44, " Leave\n                                    ");
					__builder.CloseElement();
					__builder.AddMarkupContent(45, "\n                                    ");
					__builder.OpenElement(46, "span");
					__builder.AddAttribute(47, "class", "text-on-surface-variant block mt-1 font-label-sm");
					__builder.AddAttribute(48, "style", "font-size: 11px;");
					__builder.AddMarkupContent(49, "\n                                        Submitted: ");
					__builder.AddContent(50, req.CreatedAt.ToLocalTime().ToString("g"));
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(51, "\n\n                            ");
					__builder.OpenElement(52, "div");
					__builder.AddAttribute(53, "class", "card-neo p-3 mb-3 text-body-md");
					__builder.OpenElement(54, "div");
					__builder.AddAttribute(55, "class", "mb-2");
					__builder.AddMarkupContent(56, "<strong>Date Range:</strong> ");
					__builder.OpenElement(57, "span");
					__builder.AddAttribute(58, "class", "font-bold");
					__builder.AddContent(59, req.StartDate.ToLocalTime().ToString("d"));
					__builder.CloseElement();
					__builder.AddContent(60, " to ");
					__builder.OpenElement(61, "span");
					__builder.AddAttribute(62, "class", "font-bold");
					__builder.AddContent(63, req.EndDate.ToLocalTime().ToString("d"));
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(64, "\n                                ");
					__builder.OpenElement(65, "div");
					__builder.AddAttribute(66, "class", "mb-2");
					__builder.AddMarkupContent(67, "<strong>Reason:</strong>\n                                    ");
					__builder.OpenElement(68, "p");
					__builder.AddAttribute(69, "class", "text-on-surface-variant mb-0 mt-1 font-body-md");
					__builder.AddAttribute(70, "style", "line-height: 1.4; font-style: italic;");
					__builder.AddContent(71, "\"");
					__builder.AddContent(72, req.Reason);
					__builder.AddContent(73, "\"");
					__builder.CloseElement();
					__builder.CloseElement();
					if (!string.IsNullOrEmpty(req.AttachmentUrl))
					{
						__builder.OpenElement(74, "div");
						__builder.AddAttribute(75, "class", "mt-2");
						__builder.OpenElement(76, "a");
						__builder.AddAttribute(77, "href", GetAbsoluteAttachmentUrl(req.AttachmentUrl));
						__builder.AddAttribute(78, "target", "_blank");
						__builder.AddAttribute(79, "class", "font-bold no-underline text-primary font-label-caps text-label-caps");
						__builder.AddMarkupContent(80, "<span class=\"material-symbols-outlined text-sm\">description</span> View/Download PDF Attachment\n                                        ");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(81, "\n\n                            ");
					__builder.OpenElement(82, "div");
					__builder.AddAttribute(83, "class", "pt-3 border-t border-on-surface/10");
					__builder.OpenElement(84, "div");
					__builder.AddAttribute(85, "class", "mb-3");
					__builder.AddMarkupContent(86, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">ADMINISTRATOR NOTE (MIN. 10 CHARACTERS)</label>\n                                    ");
					__builder.OpenElement(87, "textarea");
					__builder.AddAttribute(88, "class", "form-neo");
					__builder.AddAttribute(89, "style", "min-height: 60px;");
					__builder.AddAttribute(90, "placeholder", "Provide notes/reasoning for approval or rejection... (minimum 10 characters)");
					__builder.AddAttribute(91, "value", BindConverter.FormatValue(notes[req.Id]));
					__builder.AddAttribute(92, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
					{
						notes[req.Id] = __value;
					}, notes[req.Id]));
					__builder.SetUpdatesAttributeName("value");
					__builder.CloseElement();
					if (notes.ContainsKey(req.Id) && !string.IsNullOrEmpty(notes[req.Id]) && notes[req.Id].Length < 10)
					{
						__builder.OpenElement(93, "span");
						__builder.AddAttribute(94, "class", "font-label-sm mt-1 block");
						__builder.AddAttribute(95, "style", "color: #ba1a1a;");
						__builder.AddMarkupContent(96, "<span class=\"material-symbols-outlined text-xs\" style=\"vertical-align: middle;\">error</span>\n                                            Note must be at least 10 characters. (Current: ");
						__builder.AddContent(97, notes[req.Id].Length);
						__builder.AddMarkupContent(98, ")\n                                        ");
						__builder.CloseElement();
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(99, "\n\n                                ");
					__builder.OpenElement(100, "div");
					__builder.AddAttribute(101, "class", "flex gap-2");
					__builder.OpenElement(102, "button");
					__builder.AddAttribute(103, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ReviewRequest(req.Id, approve: true)));
					__builder.AddAttribute(104, "disabled", isSubmitting || !IsValidNote(req.Id));
					__builder.AddAttribute(105, "class", "btn-neo-primary flex-grow");
					__builder.AddMarkupContent(106, "<span class=\"material-symbols-outlined\">check_circle</span> Approve Leave\n                                    ");
					__builder.CloseElement();
					__builder.AddMarkupContent(107, "\n                                    ");
					__builder.OpenElement(108, "button");
					__builder.AddAttribute(109, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ReviewRequest(req.Id, approve: false)));
					__builder.AddAttribute(110, "disabled", isSubmitting || !IsValidNote(req.Id));
					__builder.AddAttribute(111, "class", "btn-neo-outline flex-grow");
					__builder.AddMarkupContent(112, "<span class=\"material-symbols-outlined\">cancel</span> Reject Leave\n                                    ");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
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
				await LoadPending(text);
			}
		}

		private async Task LoadPending(string token)
		{
			isLoading = true;
			pendingRequests.Clear();
			notes.Clear();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/admin/leave/pending");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					ApiResponse<List<LeaveRequestResponseDto>> apiResponse = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse<List<LeaveRequestResponseDto>>>();
					if (!(apiResponse != null) || !apiResponse.Success || apiResponse.Data == null)
					{
						return;
					}
					pendingRequests = apiResponse.Data;
					{
						foreach (LeaveRequestResponseDto pendingRequest in pendingRequests)
						{
							notes[pendingRequest.Id] = string.Empty;
						}
						return;
					}
				}
				message = $"❌ Failed to load pending requests ({httpResponseMessage.StatusCode}).";
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				isLoading = false;
			}
		}

		private bool IsValidNote(int id)
		{
			if (notes.ContainsKey(id) && !string.IsNullOrWhiteSpace(notes[id]))
			{
				return notes[id].Trim().Length >= 10;
			}
			return false;
		}

		private async Task ReviewRequest(int id, bool approve)
		{
			if (!IsValidNote(id) || isSubmitting)
			{
				return;
			}
			isSubmitting = true;
			message = (approve ? "⏳ Approving leave request..." : "⏳ Rejecting leave request...");
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				LeaveRequestReviewRequest inputValue = new LeaveRequestReviewRequest
				{
					AdminNote = notes[id].Trim()
				};
				string requestUri = (approve ? $"api/admin/leave/{id}/approve" : $"api/admin/leave/{id}/reject");
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpRequestMessage.Content = JsonContent.Create(inputValue);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode && apiResponse != null && apiResponse.Success)
				{
					message = (approve ? "✅ Leave request approved successfully!" : "✅ Leave request rejected.");
					await LoadPending(token);
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Action failed.");
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
	}
}
