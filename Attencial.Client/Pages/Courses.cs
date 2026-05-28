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
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/courses")]
	public class Courses : ComponentBase
	{
		private List<CourseDto> courses = new List<CourseDto>();

		private bool isLoading = true;

		private string? message;

		private int? submittingCourseId;

		private string activeFilter = "All";

		private IEnumerable<CourseDto> FilteredCourses
		{
			get
			{
				if (!(activeFilter == "All"))
				{
					return courses.Where((CourseDto c) => c.EnrollmentRequestStatus == activeFilter);
				}
				return courses;
			}
		}

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
				renderTreeBuilder.AddMarkupContent(2, "Browse Courses — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "canvas-bg min-h-screen");
			__builder.OpenElement(6, "main");
			__builder.AddAttribute(7, "class", "pt-8 pb-20 px-margin-desktop max-w-max-width mx-auto relative overflow-hidden min-h-screen");
			__builder.AddMarkupContent(8, "<div class=\"absolute top-20 right-0 w-64 h-64 bg-primary-container/10 -z-10 rounded-full blur-3xl\"></div>\n        <div class=\"absolute bottom-40 left-0 w-48 h-48 bg-tertiary-container/5 -z-10 transform rotate-45\"></div>\n\n        ");
			__builder.AddMarkupContent(9, "<section class=\"mb-12\"><div class=\"flex flex-col md:flex-row md:items-end justify-between gap-gutter\"><div class=\"max-w-2xl\"><p class=\"font-label-caps text-label-caps text-primary tracking-widest mb-4\">ACADEMIC CATALOGUE</p>\n                    <h1 class=\"font-headline-lg text-headline-lg text-on-surface mb-2\">Student Course Hub</h1>\n                    <p class=\"font-body-md text-body-md text-on-surface-variant\">Find and request enrollment in available courses. Your professor will review your request.</p></div>\n                <div class=\"hidden xl:block absolute left-6 top-1/4\"><span class=\"vertical-text font-label-caps text-on-surface-variant/30 tracking-widest uppercase\">Intellectual Growth</span></div></div></section>");
			if (message != null)
			{
				__builder.OpenElement(10, "div");
				__builder.AddAttribute(11, "class", "flex items-start gap-3 p-4 mb-8 animate-fade-in border");
				__builder.AddAttribute(12, "style", "border-color: " + (message.StartsWith("✅") ? "#006191" : "#ba1a1a") + "; background: " + (message.StartsWith("✅") ? "rgba(0,97,145,0.04)" : "rgba(186,26,26,0.04)") + ";");
				__builder.OpenElement(13, "span");
				__builder.AddAttribute(14, "class", "material-symbols-outlined text-[20px] shrink-0");
				__builder.AddAttribute(15, "style", "color: " + (message.StartsWith("✅") ? "#006191" : "#ba1a1a") + ";");
				__builder.AddContent(16, message.StartsWith("✅") ? "check_circle" : "warning");
				__builder.CloseElement();
				__builder.AddMarkupContent(17, "\n                ");
				__builder.OpenElement(18, "span");
				__builder.AddAttribute(19, "class", "font-body-md text-[14px] text-on-surface");
				__builder.AddContent(20, message.Replace("✅ ", "").Replace("❌ ", ""));
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (isLoading)
			{
				__builder.AddMarkupContent(21, "<div class=\"text-center py-20\"><span class=\"material-symbols-outlined text-5xl text-primary block mb-6 animate-spin\">refresh</span>\n                <p class=\"font-label-caps text-label-caps text-on-surface-variant\">Loading available courses...</p></div>");
			}
			else if (courses.Count == 0)
			{
				__builder.AddMarkupContent(22, "<div class=\"border border-on-surface bg-surface text-center p-12 animate-fade-in\"><span class=\"material-symbols-outlined text-5xl text-primary block mb-4\">assignment_late</span>\n                <h4 class=\"font-headline-md text-headline-md text-on-surface mb-2\">No Courses Available</h4>\n                <p class=\"font-body-md text-body-md text-on-surface-variant\">No courses have been created yet. Check back later.</p></div>");
			}
			else
			{
				__builder.OpenElement(23, "div");
				__builder.AddAttribute(24, "class", "flex gap-3 flex-wrap mb-8");
				string[] array = new string[5] { "All", "None", "Pending", "Approved", "Rejected" };
				foreach (string f in array)
				{
					__builder.OpenElement(25, "button");
					__builder.AddAttribute(26, "class", (activeFilter == f) ? "btn-neo-primary" : "btn-neo-outline");
					__builder.AddAttribute(27, "style", "padding: 0.5rem 1rem;");
					__builder.AddAttribute(28, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
					{
						activeFilter = f;
					}));
					if (f == "All")
					{
						__builder.AddMarkupContent(29, "<span class=\"material-symbols-outlined text-[16px]\">grid_view</span>");
					}
					else if (f == "None")
					{
						__builder.AddMarkupContent(30, "<span class=\"material-symbols-outlined text-[16px]\">add_circle</span>");
					}
					else if (f == "Pending")
					{
						__builder.AddMarkupContent(31, "<span class=\"material-symbols-outlined text-[16px]\">hourglass</span>");
					}
					else if (f == "Approved")
					{
						__builder.AddMarkupContent(32, "<span class=\"material-symbols-outlined text-[16px]\">check_circle</span>");
					}
					else
					{
						__builder.AddMarkupContent(33, "<span class=\"material-symbols-outlined text-[16px]\">cancel</span>");
					}
					__builder.AddContent(34, f);
					__builder.AddMarkupContent(35, "\n                        ");
					__builder.OpenElement(36, "span");
					__builder.AddAttribute(37, "class", "badge-neo text-[10px] py-0.5 px-2 ml-1");
					__builder.AddContent(38, CountFor(f));
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.OpenElement(39, "div");
				__builder.AddAttribute(40, "class", "grid grid-cols-1 md:grid-cols-2 gap-6");
				foreach (CourseDto course in FilteredCourses)
				{
					__builder.OpenElement(41, "div");
					__builder.AddAttribute(42, "class", "animate-fade-in");
					__builder.OpenElement(43, "div");
					__builder.AddAttribute(44, "class", "card-neo-hover h-full flex flex-col justify-between relative overflow-hidden");
					__builder.OpenElement(45, "div");
					__builder.AddAttribute(46, "class", "mb-6");
					__builder.OpenElement(47, "div");
					__builder.AddAttribute(48, "class", "flex justify-between items-start flex-wrap gap-2 mb-3");
					__builder.OpenElement(49, "div");
					__builder.OpenElement(50, "h3");
					__builder.AddAttribute(51, "class", "font-headline-md text-[22px] text-on-surface mb-1");
					__builder.AddContent(52, course.Name);
					__builder.CloseElement();
					__builder.AddMarkupContent(53, "\n                                        ");
					__builder.OpenElement(54, "span");
					__builder.AddAttribute(55, "class", "badge-neo text-[11px]");
					__builder.AddContent(56, course.CourseCode);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(57, "\n                                    ");
					__builder.AddContent(58, StatusBadge(course.EnrollmentRequestStatus));
					__builder.CloseElement();
					__builder.AddMarkupContent(59, "\n\n                                ");
					__builder.OpenElement(60, "div");
					__builder.AddAttribute(61, "class", "flex items-center gap-2 mt-3 font-body-md text-[14px] text-on-surface-variant");
					__builder.AddMarkupContent(62, "<span class=\"material-symbols-outlined text-[16px] text-primary shrink-0\">person</span>\n                                    ");
					__builder.OpenElement(63, "span");
					__builder.AddContent(64, course.ProfessorName);
					__builder.CloseElement();
					__builder.AddMarkupContent(65, "\n                                    ");
					__builder.AddMarkupContent(66, "<span class=\"text-outline-variant\">·</span>\n                                    ");
					__builder.OpenElement(67, "span");
					__builder.AddAttribute(68, "class", "text-on-surface-variant/70");
					__builder.AddContent(69, course.Department);
					__builder.CloseElement();
					__builder.CloseElement();
					if (course.EnrollmentRequestStatus == "Rejected" && !string.IsNullOrEmpty(course.Note))
					{
						__builder.OpenElement(70, "div");
						__builder.AddAttribute(71, "class", "mt-4 p-3 border border-primary/30 flex items-start gap-2 text-sm bg-surface-container-low");
						__builder.AddMarkupContent(72, "<span class=\"material-symbols-outlined text-[18px] text-primary shrink-0\">chat</span>\n                                        ");
						__builder.OpenElement(73, "div");
						__builder.AddMarkupContent(74, "<span class=\"font-label-sm text-label-sm text-on-surface-variant\">Professor note: </span>\n                                            ");
						__builder.OpenElement(75, "span");
						__builder.AddAttribute(76, "class", "font-body-md text-[14px] text-on-surface");
						__builder.AddContent(77, course.Note);
						__builder.CloseElement();
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(78, "\n\n                            ");
					__builder.OpenElement(79, "div");
					__builder.AddAttribute(80, "class", "mt-auto");
					if (course.EnrollmentRequestStatus == "Approved")
					{
						__builder.AddMarkupContent(81, "<div class=\"flex items-center gap-2 py-3 font-label-caps text-label-caps text-tertiary\"><span class=\"material-symbols-outlined text-[18px]\">check_circle</span>\n                                        <span>Enrolled</span></div>");
					}
					else if (course.EnrollmentRequestStatus == "Pending")
					{
						__builder.AddMarkupContent(82, "<div class=\"flex items-center gap-2 py-3 font-label-caps text-label-caps\" style=\"color: #f1c40f;\"><span class=\"material-symbols-outlined text-[18px]\">hourglass</span>\n                                        <span>Awaiting Professor Approval</span></div>");
					}
					else
					{
						__builder.OpenElement(83, "button");
						__builder.AddAttribute(84, "class", "btn-neo-primary w-full flex items-center justify-center gap-2");
						__builder.AddAttribute(85, "id", "enroll-btn-" + course.Id);
						__builder.AddAttribute(86, "disabled", submittingCourseId == course.Id);
						__builder.AddAttribute(87, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RequestEnrollment(course.Id, course.Name)));
						if (submittingCourseId == course.Id)
						{
							__builder.AddMarkupContent(88, "<span class=\"material-symbols-outlined text-[18px] animate-spin\">refresh</span>\n                                            ");
							__builder.AddMarkupContent(89, "<span>Submitting...</span>");
						}
						else if (course.EnrollmentRequestStatus == "Rejected")
						{
							__builder.AddMarkupContent(90, "<span class=\"material-symbols-outlined text-[18px]\">repeat</span>\n                                            ");
							__builder.AddMarkupContent(91, "<span>Request Again</span>");
						}
						else
						{
							__builder.AddMarkupContent(92, "<span class=\"material-symbols-outlined text-[18px]\">send</span>\n                                            ");
							__builder.AddMarkupContent(93, "<span>Request Enrollment</span>");
						}
						__builder.CloseElement();
					}
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}

		private int CountFor(string filter)
		{
			if (!(filter == "All"))
			{
				return courses.Count((CourseDto c) => c.EnrollmentRequestStatus == filter);
			}
			return courses.Count;
		}

		protected override async Task OnInitializedAsync()
		{
			string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (string.IsNullOrEmpty(token) || token == "null")
			{
				Nav.NavigateTo("/login");
				return;
			}
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
			httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
			if (httpResponseMessage.IsSuccessStatusCode)
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				string text = jsonDocument.RootElement.GetProperty("data").GetProperty("role").GetString();
				if (text != null && text == "Professor")
				{
					Nav.NavigateTo("/professor-dashboard");
					return;
				}
			}
			await LoadCourses(token);
		}

		private async Task LoadCourses(string token)
		{
			isLoading = true;
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/courses");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				if (res.IsSuccessStatusCode)
				{
					courses = (await res.Content.ReadFromJsonAsync<ApiResponse<List<CourseDto>>>())?.Data ?? new List<CourseDto>();
					return;
				}
				message = "❌ " + ((await res.Content.ReadFromJsonAsync<ApiResponse<string>>())?.Message ?? $"Failed to load courses ({res.StatusCode}).");
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

		private async Task Logout()
		{
			await JS.InvokeVoidAsync("authStorage.removeToken");
			Nav.NavigateTo("/login", forceLoad: true);
		}

		private async Task RequestEnrollment(int courseId, string courseName)
		{
			submittingCourseId = courseId;
			message = null;
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"api/courses/{courseId}/enrollment-requests");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode && (object)apiResponse != null && apiResponse.Success)
				{
					message = "✅ " + apiResponse.Message;
					await LoadCourses(token);
				}
				else
				{
					message = "❌ " + (apiResponse?.Message ?? "Request failed.");
				}
			}
			catch (Exception ex)
			{
				message = "❌ Connection error: " + ex.Message;
			}
			finally
			{
				submittingCourseId = null;
			}
		}

		private RenderFragment StatusBadge(string status)
		{
			return status switch
			{
				"Approved" => delegate(RenderTreeBuilder __builder2)
				{
					__builder2.AddMarkupContent(94, "<span class=\"badge-glass-success d-inline-flex align-items-center gap-1 px-2 py-1\" style=\"font-size: 10px;\"><i class=\"bi bi-check-circle-fill\"></i>Enrolled\n                      </span>");
				}, 
				"Pending" => delegate(RenderTreeBuilder __builder2)
				{
					__builder2.AddMarkupContent(95, "<span class=\"badge-glass-warning d-inline-flex align-items-center gap-1 px-2 py-1\" style=\"font-size: 10px;\"><i class=\"bi bi-hourglass-split\"></i>Pending\n                      </span>");
				}, 
				"Rejected" => delegate(RenderTreeBuilder __builder2)
				{
					__builder2.AddMarkupContent(96, "<span class=\"badge-glass-error d-inline-flex align-items-center gap-1 px-2 py-1\" style=\"font-size: 10px;\"><i class=\"bi bi-x-circle-fill\"></i>Rejected\n                      </span>");
				}, 
				_ => delegate(RenderTreeBuilder __builder2)
				{
					__builder2.AddMarkupContent(97, "<span></span>\n");
				}, 
			};
		}
	}
}
