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
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/profile")]
	public class Profile : ComponentBase
	{
		private bool isLoading = true;

		private string? errorMessage;
		private string? faceMessage;
		private string userFullName = string.Empty;

		private string userEmail = string.Empty;

		private string userRole = string.Empty;

		private string rollNumber = string.Empty;

		private bool isEnrolled;

		private DateTime? lastEnrollmentDate;

		private double daysUntilNextUpdate;

		private bool isEditing;

		private bool isSaving;

		private string editFullName = string.Empty;

		private string editEmail = string.Empty;

		private string editRollNumber = string.Empty;

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
				renderTreeBuilder.AddMarkupContent(2, "Profile — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "canvas-bg min-h-screen pb-16");
			__builder.OpenElement(6, "div");
			__builder.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile lg:px-margin-desktop pt-8 animate-fade-in");
			__builder.AddMarkupContent(8, "<div class=\"mb-8\"><span class=\"font-label-caps text-label-caps text-on-surface-variant tracking-[0.2em] block mb-2\">ACCOUNT</span>\n            <h1 class=\"font-display-lg text-display-lg text-on-surface\">Profile</h1></div>");
			if (isLoading)
			{
				__builder.AddMarkupContent(9, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined animate-spin text-primary text-2xl block mb-2\">refresh</span>\n                <p class=\"font-label-sm text-on-surface-variant\">Loading profile...</p></div>");
			}
			else if (!string.IsNullOrEmpty(errorMessage))
			{
				__builder.OpenElement(10, "div");
				__builder.AddAttribute(11, "class", "border border-error/30 p-4 mb-8 flex items-start gap-3");
				__builder.AddAttribute(12, "style", "background: rgba(186,26,26,0.04);");
				__builder.AddMarkupContent(13, "<span class=\"material-symbols-outlined text-error\">error</span>\n                ");
				__builder.OpenElement(14, "span");
				__builder.AddAttribute(15, "class", "text-sm text-on-surface-variant");
				__builder.AddContent(16, errorMessage);
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else
			{
				__builder.OpenElement(17, "div");
				__builder.AddAttribute(18, "class", "grid grid-cols-1 lg:grid-cols-2 gap-gutter");
				__builder.OpenElement(19, "div");
				__builder.AddAttribute(20, "class", "card-neo");
				__builder.OpenElement(21, "div");
				__builder.AddAttribute(22, "class", "flex justify-between items-center mb-4 border-b border-outline-variant/20 pb-3");
				__builder.AddMarkupContent(23, "<h3 class=\"font-label-caps text-label-caps text-on-surface\">Account Details</h3>\n                        ");
				__builder.OpenElement(24, "button");
				__builder.AddAttribute(25, "class", "text-on-surface-variant hover:text-primary transition-colors bg-transparent border-0 cursor-pointer");
				__builder.AddAttribute(26, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)ToggleEdit));
				__builder.OpenElement(27, "span");
				__builder.AddAttribute(28, "class", "material-symbols-outlined text-lg");
				__builder.AddContent(29, isEditing ? "close" : "edit");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(30, "\n\n                    ");
				__builder.OpenElement(31, "div");
				__builder.AddAttribute(32, "class", "relative overflow-hidden");
				__builder.OpenElement(33, "div");
				__builder.AddAttribute(34, "class", "transition-all duration-500 ease-out " + (isEditing ? "opacity-0 max-h-0 overflow-hidden" : "opacity-100 max-h-[500px]"));
				__builder.OpenElement(35, "div");
				__builder.AddAttribute(36, "class", "space-y-4");
				__builder.OpenElement(37, "div");
				__builder.AddMarkupContent(38, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Name</span>\n                                    ");
				__builder.OpenElement(39, "span");
				__builder.AddAttribute(40, "class", "font-body-md text-on-surface");
				__builder.AddContent(41, userFullName);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(42, "\n                                ");
				__builder.OpenElement(43, "div");
				__builder.AddMarkupContent(44, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Email</span>\n                                    ");
				__builder.OpenElement(45, "span");
				__builder.AddAttribute(46, "class", "font-body-md text-on-surface");
				__builder.AddContent(47, userEmail);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(48, "\n                                ");
				__builder.OpenElement(49, "div");
				__builder.AddMarkupContent(50, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Role</span>\n                                    ");
				__builder.OpenElement(51, "span");
				__builder.AddAttribute(52, "class", "badge-neo");
				__builder.AddContent(53, userRole);
				__builder.CloseElement();
				__builder.CloseElement();
				if (userRole == "Student" && !string.IsNullOrEmpty(rollNumber))
				{
					__builder.OpenElement(54, "div");
					__builder.AddMarkupContent(55, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Roll Number</span>\n                                        ");
					__builder.OpenElement(56, "span");
					__builder.AddAttribute(57, "class", "font-mono text-on-surface");
					__builder.AddContent(58, rollNumber);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(59, "\n\n                        ");
				__builder.OpenElement(60, "div");
				__builder.AddAttribute(61, "class", "transition-all duration-500 ease-out " + (isEditing ? "opacity-100 max-h-[500px]" : "opacity-0 max-h-0 overflow-hidden"));
				__builder.OpenElement(62, "div");
				__builder.AddAttribute(63, "class", "space-y-4");
				__builder.OpenElement(64, "div");
				__builder.AddMarkupContent(65, "<label class=\"font-label-caps text-[10px] text-on-surface-variant block mb-1\">Name</label>\n                                    ");
				__builder.OpenElement(66, "input");
				__builder.AddAttribute(67, "class", "w-full bg-transparent border-b border-on-surface-variant/30 py-2 focus:outline-none focus:border-primary text-on-surface font-body-md");
				__builder.AddAttribute(68, "value", BindConverter.FormatValue(editFullName));
				__builder.AddAttribute(69, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					editFullName = __value;
				}, editFullName));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(70, "\n                                ");
				__builder.OpenElement(71, "div");
				__builder.AddMarkupContent(72, "<label class=\"font-label-caps text-[10px] text-on-surface-variant block mb-1\">Email</label>\n                                    ");
				__builder.OpenElement(73, "input");
				__builder.AddAttribute(74, "class", "w-full bg-transparent border-b border-on-surface-variant/30 py-2 focus:outline-none focus:border-primary text-on-surface font-body-md");
				__builder.AddAttribute(75, "value", BindConverter.FormatValue(editEmail));
				__builder.AddAttribute(76, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					editEmail = __value;
				}, editEmail));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(77, "\n                                ");
				__builder.OpenElement(78, "div");
				__builder.AddMarkupContent(79, "<span class=\"font-label-caps text-[10px] text-on-surface-variant block\">Role</span>\n                                    ");
				__builder.OpenElement(80, "span");
				__builder.AddAttribute(81, "class", "badge-neo");
				__builder.AddContent(82, userRole);
				__builder.CloseElement();
				__builder.CloseElement();
				if (userRole == "Student")
				{
					__builder.OpenElement(83, "div");
					__builder.AddMarkupContent(84, "<label class=\"font-label-caps text-[10px] text-on-surface-variant block mb-1\">Roll Number</label>\n                                        ");
					__builder.OpenElement(85, "input");
					__builder.AddAttribute(86, "class", "w-full bg-transparent border-b border-on-surface-variant/30 py-2 focus:outline-none focus:border-primary text-on-surface font-mono");
					__builder.AddAttribute(87, "value", BindConverter.FormatValue(editRollNumber));
					__builder.AddAttribute(88, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
					{
						editRollNumber = __value;
					}, editRollNumber));
					__builder.SetUpdatesAttributeName("value");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(89, "div");
				__builder.AddAttribute(90, "class", "flex gap-3 pt-2");
				__builder.OpenElement(91, "button");
				__builder.AddAttribute(92, "class", "btn-neo-primary text-sm flex-1");
				__builder.AddAttribute(93, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SaveProfile));
				__builder.AddAttribute(94, "disabled", isSaving);
				__builder.AddContent(95, isSaving ? "Saving..." : "Save Changes");
				__builder.CloseElement();
				__builder.AddMarkupContent(96, "\n                                    ");
				__builder.OpenElement(97, "button");
				__builder.AddAttribute(98, "class", "btn-neo-outline text-sm");
				__builder.AddAttribute(99, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)CancelEdit));
				__builder.AddContent(100, "Cancel");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(101, "\n\n                ");
				__builder.OpenElement(102, "div");
				__builder.AddAttribute(103, "class", "card-neo");
				__builder.AddMarkupContent(104, "<h3 class=\"font-label-caps text-label-caps text-on-surface mb-4 border-b border-outline-variant/20 pb-3\">Face Enrollment</h3>");
				if (!string.IsNullOrEmpty(faceMessage))
				{
					__builder.OpenElement(105, "div");
					__builder.AddAttribute(106, "class", "border-l-4 border-tertiary bg-surface-container-low p-3 mb-4 flex items-start gap-2 animate-fade-in");
					__builder.AddMarkupContent(107, "<span class=\"material-symbols-outlined text-tertiary text-sm mt-0.5\">info</span>\n                            ");
					__builder.OpenElement(108, "span");
					__builder.AddAttribute(109, "class", "font-body-md text-sm text-on-surface");
					__builder.AddContent(110, faceMessage);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(111, "div");
				__builder.AddAttribute(112, "class", "flex items-center gap-3 mb-4");
				if (isEnrolled)
				{
					__builder.AddMarkupContent(113, "<span class=\"material-symbols-outlined text-2xl text-tertiary\">check_circle</span>\n                            ");
					__builder.OpenElement(114, "div");
					__builder.AddMarkupContent(115, "<span class=\"font-label-caps text-on-surface block\">Enrolled</span>");
					if (lastEnrollmentDate.HasValue)
					{
						__builder.OpenElement(116, "span");
						__builder.AddAttribute(117, "class", "font-label-sm text-on-surface-variant");
						__builder.AddContent(118, "Last: ");
						__builder.AddContent(119, lastEnrollmentDate.Value.ToLocalTime().ToString("MMM dd, yyyy"));
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
				else
				{
					__builder.AddMarkupContent(120, "<span class=\"material-symbols-outlined text-2xl text-primary\">gpp_bad</span>\n                            ");
					__builder.AddMarkupContent(121, "<div><span class=\"font-label-caps text-on-surface block\">Not Enrolled</span>\n                                <span class=\"font-label-sm text-on-surface-variant\">Required for attendance</span></div>");
				}
				__builder.CloseElement();
				if (userRole == "Student")
				{
				__builder.OpenElement(123, "a");
				__builder.AddAttribute(124, "href", "/enroll-face?returnUrl=/profile");
				__builder.AddAttribute(125, "class", "btn-neo-primary w-full text-sm flex items-center justify-center gap-2 no-underline");
				__builder.OpenElement(126, "span");
				__builder.AddAttribute(127, "class", "material-symbols-outlined text-lg");
				__builder.AddContent(128, isEnrolled ? "refresh" : "face");
				__builder.CloseElement();
				__builder.AddMarkupContent(129, "\n                        ");
				__builder.AddContent(130, isEnrolled ? "Refresh Face" : "Enroll Face");
				__builder.CloseElement();
				__builder.CloseElement();
				}
				__builder.CloseElement();
			}
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
				await LoadProfile();
			}
		}

		protected override void OnParametersSet()
		{
			// Capture face enrollment feedback messages from query string (e.g. ?message=...)
			var uri = new Uri(Nav.Uri);
			var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
			faceMessage = query["message"];
		}

		private async Task LoadProfile()
		{
			isLoading = true;
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					JsonElement property = jsonDocument.RootElement.GetProperty("data");
					userEmail = property.GetProperty("email").GetString() ?? "";
					userRole = property.GetProperty("role").GetString() ?? "";
				}
				if (userRole == "Professor")
				{
					HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "api/seed/me");
					httpRequestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					HttpResponseMessage httpResponseMessage2 = await Http.SendAsync(httpRequestMessage2);
					if (httpResponseMessage2.IsSuccessStatusCode)
					{
						using JsonDocument jsonDocument2 = JsonDocument.Parse(await httpResponseMessage2.Content.ReadAsStringAsync());
						if (jsonDocument2.RootElement.GetProperty("data").TryGetProperty("professorProfile", out var value) && value.ValueKind != JsonValueKind.Null)
						{
							userFullName = value.GetProperty("fullName").GetString() ?? "";
						}
					}
				}
				else
				{
					HttpRequestMessage httpRequestMessage3 = new HttpRequestMessage(HttpMethod.Get, "api/seed/me");
					httpRequestMessage3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					HttpResponseMessage httpResponseMessage3 = await Http.SendAsync(httpRequestMessage3);
					if (httpResponseMessage3.IsSuccessStatusCode)
					{
						using JsonDocument jsonDocument3 = JsonDocument.Parse(await httpResponseMessage3.Content.ReadAsStringAsync());
						if (jsonDocument3.RootElement.GetProperty("data").TryGetProperty("studentProfile", out var value2) && value2.ValueKind != JsonValueKind.Null)
						{
							userFullName = value2.GetProperty("fullName").GetString() ?? "";
							if (value2.TryGetProperty("rollNumber", out var value3))
							{
								rollNumber = value3.GetString() ?? "";
							}
						}
					}
				}
				string requestUri = ((userRole == "Professor") ? "api/faculty/enrollment/status" : "api/enrollment/status");
				HttpRequestMessage httpRequestMessage4 = new HttpRequestMessage(HttpMethod.Get, requestUri);
				httpRequestMessage4.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage4 = await Http.SendAsync(httpRequestMessage4);
				if (!httpResponseMessage4.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument4 = JsonDocument.Parse(await httpResponseMessage4.Content.ReadAsStringAsync());
				JsonElement property2 = jsonDocument4.RootElement.GetProperty("data");
				isEnrolled = property2.GetProperty("isEnrolled").GetBoolean();
				if (property2.TryGetProperty("lastEnrollmentDate", out var value4) && value4.ValueKind != JsonValueKind.Null)
				{
					lastEnrollmentDate = value4.GetDateTime();
				}
				if (property2.TryGetProperty("daysUntilNextUpdate", out var value5))
				{
					daysUntilNextUpdate = value5.GetDouble();
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading profile: " + ex.Message;
			}
			finally
			{
				isLoading = false;
			}
		}

		private void ToggleEdit()
		{
			if (!isEditing)
			{
				editFullName = userFullName;
				editEmail = userEmail;
				editRollNumber = rollNumber;
			}
			isEditing = !isEditing;
			errorMessage = null;
		}

		private void CancelEdit()
		{
			isEditing = false;
			editFullName = userFullName;
			editEmail = userEmail;
			editRollNumber = rollNumber;
		}

		private async Task SaveProfile()
		{
			isSaving = true;
			errorMessage = null;
			try
			{
				string parameter = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "api/auth/update-profile");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					fullName = editFullName.Trim(),
					email = editEmail.Trim(),
					rollNumber = ((userRole == "Student") ? editRollNumber.Trim() : "")
				});
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					userFullName = editFullName.Trim();
					userEmail = editEmail.Trim();
					if (userRole == "Student")
					{
						rollNumber = editRollNumber.Trim();
					}
					isEditing = false;
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				errorMessage = (jsonDocument.RootElement.TryGetProperty("message", out var value) ? value.GetString() : "Update failed.");
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
			finally
			{
				isSaving = false;
			}
		}
	}
}
