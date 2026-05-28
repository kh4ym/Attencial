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
	[Route("/enrollment-review")]
	public class EnrollmentReview : ComponentBase
	{
		private List<EnrollmentRequestDto> requests = new List<EnrollmentRequestDto>();

		private bool isLoading = true;

		private string? message;

		private int? submittingId;

		private HashSet<int> rejectOpenIds = new HashSet<int>();

		private Dictionary<int, string> rejectionNotes = new Dictionary<int, string>();

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
				renderTreeBuilder.AddMarkupContent(2, "Enrollment Review Queue — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "min-h-screen canvas-bg");
			__builder.OpenElement(6, "div");
			__builder.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile lg:px-margin-desktop py-12");
			__builder.OpenElement(8, "div");
			__builder.AddAttribute(9, "class", "card-neo-raised p-6 md:p-8");
			__builder.AddMarkupContent(10, "<div class=\"text-center mb-4\"><div class=\"inline-flex items-center justify-center w-14 h-14 mb-3 border border-on-surface\"><span class=\"material-symbols-outlined text-2xl text-primary\">person_check</span></div>\n                <h1 class=\"font-headline-md text-headline-md text-on-surface mb-1\">Enrollment Review Queue</h1>\n                <p class=\"text-body-md text-on-surface-variant\">Review and approve student enrollment requests for your courses.</p></div>");
			if (message != null)
			{
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "card-neo mb-4 flex items-start gap-2 animate-fade-in");
				__builder.AddAttribute(13, "style", "border-left: 3px solid " + (message.StartsWith("✅") ? "#006191" : "#ba1a1a") + ";");
				__builder.OpenElement(14, "span");
				__builder.AddAttribute(15, "class", "material-symbols-outlined mt-0.5");
				__builder.AddAttribute(16, "style", "color: " + (message.StartsWith("✅") ? "#006191" : "#ba1a1a") + ";");
				__builder.AddContent(17, message.StartsWith("✅") ? "check_circle" : "warning");
				__builder.CloseElement();
				__builder.AddMarkupContent(18, "\n                    ");
				__builder.OpenElement(19, "span");
				__builder.AddAttribute(20, "class", "text-body-md");
				__builder.AddContent(21, message.Replace("✅ ", "").Replace("❌ ", ""));
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (isLoading)
			{
				__builder.AddMarkupContent(22, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined animate-spin text-primary mb-3 block\">refresh</span>\n                    <p class=\"text-body-md text-on-surface-variant\">Loading pending enrollment requests...</p></div>");
			}
			else if (requests.Count == 0)
			{
				__builder.AddMarkupContent(23, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined text-4xl text-tertiary mb-3 block\">sentiment_satisfied</span>\n                    <h4 class=\"font-headline-md text-headline-md text-on-surface mb-1\">All Clear!</h4>\n                    <p class=\"text-body-md text-on-surface-variant\">No pending enrollment requests for your courses.</p></div>");
			}
			else
			{
				__builder.OpenElement(24, "div");
				__builder.AddAttribute(25, "class", "flex flex-col gap-4");
				foreach (EnrollmentRequestDto req in requests)
				{
					__builder.OpenElement(26, "div");
					__builder.AddAttribute(27, "class", "card-neo-hover p-4 animate-fade-in");
					__builder.OpenElement(28, "div");
					__builder.AddAttribute(29, "class", "flex justify-between items-start flex-wrap gap-2 mb-3");
					__builder.OpenElement(30, "div");
					__builder.OpenElement(31, "h4");
					__builder.AddAttribute(32, "class", "font-headline-md text-headline-md text-on-surface mb-0");
					__builder.AddContent(33, req.StudentName);
					__builder.CloseElement();
					__builder.AddMarkupContent(34, "\n                                    ");
					__builder.OpenElement(35, "span");
					__builder.AddAttribute(36, "class", "badge-neo px-2 py-1 mt-1 text-label-caps");
					__builder.AddMarkupContent(37, "<span class=\"material-symbols-outlined text-xs\">badge</span> ");
					__builder.AddContent(38, req.RollNumber);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(39, "\n                                ");
					__builder.OpenElement(40, "div");
					__builder.AddAttribute(41, "class", "text-right");
					__builder.OpenElement(42, "div");
					__builder.AddAttribute(43, "class", "font-bold text-body-md text-on-surface");
					__builder.AddContent(44, req.CourseName);
					__builder.CloseElement();
					__builder.AddMarkupContent(45, "\n                                    ");
					__builder.OpenElement(46, "span");
					__builder.AddAttribute(47, "class", "badge-neo px-2 py-1 mt-1 text-label-caps");
					__builder.AddContent(48, req.CourseCode);
					__builder.CloseElement();
					__builder.AddMarkupContent(49, "\n                                    ");
					__builder.OpenElement(50, "div");
					__builder.AddAttribute(51, "class", "text-on-surface-variant mt-1 font-label-sm");
					__builder.AddAttribute(52, "style", "font-size: 11px;");
					__builder.AddMarkupContent(53, "\n                                        Requested ");
					__builder.AddContent(54, req.RequestedAt.ToLocalTime().ToString("g"));
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
					if (rejectOpenIds.Contains(req.Id))
					{
						__builder.OpenElement(55, "div");
						__builder.AddAttribute(56, "class", "mb-3");
						__builder.AddMarkupContent(57, "<label class=\"font-label-caps text-label-caps text-on-surface-variant mb-1 block\">\n                                        REJECTION NOTE (OPTIONAL)\n                                    </label>\n                                    ");
						__builder.OpenElement(58, "textarea");
						__builder.AddAttribute(59, "class", "form-neo");
						__builder.AddAttribute(60, "style", "min-height: 60px;");
						__builder.AddAttribute(61, "placeholder", "Provide a reason for rejection (shown to the student)...");
						__builder.AddAttribute(62, "value", BindConverter.FormatValue(rejectionNotes[req.Id]));
						__builder.AddAttribute(63, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
						{
							rejectionNotes[req.Id] = __value;
						}, rejectionNotes[req.Id]));
						__builder.SetUpdatesAttributeName("value");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.OpenElement(64, "div");
					__builder.AddAttribute(65, "class", "flex gap-2 flex-wrap pt-3 border-t border-on-surface/10");
					if (!rejectOpenIds.Contains(req.Id))
					{
						__builder.OpenElement(66, "button");
						__builder.AddAttribute(67, "class", "btn-neo-primary flex-grow");
						__builder.AddAttribute(68, "id", "approve-btn-" + req.Id);
						__builder.AddAttribute(69, "disabled", submittingId == req.Id);
						__builder.AddAttribute(70, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => Approve(req.Id)));
						if (submittingId == req.Id)
						{
							__builder.AddMarkupContent(71, "<span class=\"material-symbols-outlined animate-spin\">refresh</span>");
						}
						else
						{
							__builder.AddMarkupContent(72, "<span class=\"material-symbols-outlined\">check_circle</span>");
						}
						__builder.AddMarkupContent(73, "                                        Approve\n                                    ");
						__builder.CloseElement();
						__builder.AddMarkupContent(74, "\n                                    ");
						__builder.OpenElement(75, "button");
						__builder.AddAttribute(76, "class", "btn-neo-outline flex-grow");
						__builder.AddAttribute(77, "id", "reject-open-btn-" + req.Id);
						__builder.AddAttribute(78, "disabled", submittingId == req.Id);
						__builder.AddAttribute(79, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
						{
							OpenReject(req.Id);
						}));
						__builder.AddMarkupContent(80, "<span class=\"material-symbols-outlined\">cancel</span> Reject\n                                    ");
						__builder.CloseElement();
					}
					else
					{
						__builder.OpenElement(81, "button");
						__builder.AddAttribute(82, "class", "btn-neo-danger flex-grow");
						__builder.AddAttribute(83, "id", "reject-confirm-btn-" + req.Id);
						__builder.AddAttribute(84, "disabled", submittingId == req.Id);
						__builder.AddAttribute(85, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => Reject(req.Id)));
						if (submittingId == req.Id)
						{
							__builder.AddMarkupContent(86, "<span class=\"material-symbols-outlined animate-spin\">refresh</span>");
						}
						else
						{
							__builder.AddMarkupContent(87, "<span class=\"material-symbols-outlined\">cancel</span>");
						}
						__builder.AddMarkupContent(88, "                                        Confirm Rejection\n                                    ");
						__builder.CloseElement();
						__builder.AddMarkupContent(89, "\n                                    ");
						__builder.OpenElement(90, "button");
						__builder.AddAttribute(91, "class", "btn-neo-outline");
						__builder.AddAttribute(92, "style", "flex: 0 0 auto;");
						__builder.AddAttribute(93, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
						{
							CancelReject(req.Id);
						}));
						__builder.AddMarkupContent(94, "<span class=\"material-symbols-outlined\">arrow_back</span> Cancel\n                                    ");
						__builder.CloseElement();
					}
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
				await LoadRequests(text);
			}
		}

		private async Task LoadRequests(string token)
		{
			isLoading = true;
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/courses/enrollment-requests/pending");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				if (res.IsSuccessStatusCode)
				{
					requests = (await res.Content.ReadFromJsonAsync<ApiResponse<List<EnrollmentRequestDto>>>())?.Data ?? new List<EnrollmentRequestDto>();
					foreach (EnrollmentRequestDto request in requests)
					{
						rejectionNotes.TryAdd(request.Id, string.Empty);
					}
				}
				else
				{
					message = "❌ " + ((await res.Content.ReadFromJsonAsync<ApiResponse<string>>())?.Message ?? $"Failed to load requests ({res.StatusCode}).");
				}
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

		private void OpenReject(int id)
		{
			rejectOpenIds.Add(id);
			rejectionNotes.TryAdd(id, string.Empty);
		}

		private void CancelReject(int id)
		{
			rejectOpenIds.Remove(id);
		}

		private async Task Approve(int requestId)
		{
			submittingId = requestId;
			message = null;
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/courses/enrollment-requests/{requestId}/approve");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode && (object)apiResponse != null && apiResponse.Success)
				{
					message = "✅ " + apiResponse.Message;
					await LoadRequests(token);
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Approval failed.");
				}
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				submittingId = null;
			}
		}

		private async Task Reject(int requestId)
		{
			submittingId = requestId;
			message = null;
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				string valueOrDefault = rejectionNotes.GetValueOrDefault(requestId, string.Empty);
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/courses/enrollment-requests/{requestId}/reject");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					Note = valueOrDefault
				});
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode && (object)apiResponse != null && apiResponse.Success)
				{
					message = "✅ " + apiResponse.Message;
					rejectOpenIds.Remove(requestId);
					await LoadRequests(token);
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Rejection failed.");
				}
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				submittingId = null;
			}
		}
	}
}
