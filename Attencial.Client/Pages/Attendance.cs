using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Attencial.Client.Pages;

[Route("/attendance")]
public class Attendance : ComponentBase
{
    private bool isLoading = true;
    private bool isAuthorized;
    private string? errorMessage;
    private string? appealMessage;
    private string jwtToken = string.Empty;
    private StudentAttendanceSummaryDto? summary;
    private readonly Dictionary<int, bool> expanded = new();
    private bool showAppealForm;
    private int appealSessionId;
    private string appealCourseName = string.Empty;
    private string appealReason = string.Empty;
    private bool isAppealSubmitted;

    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        jwtToken = await JS.InvokeAsync<string>("authStorage.getToken");
        if (string.IsNullOrEmpty(jwtToken) || jwtToken == "null" || jwtToken == "undefined")
        {
            Nav.NavigateTo("/login");
            return;
        }
        isAuthorized = true;
        StateHasChanged();
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        errorMessage = null;
        StateHasChanged();
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/students/me/attendance");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            var response = await Http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(json))
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<StudentAttendanceSummaryDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    summary = result?.Data;
                }
            }
            else
            {
                errorMessage = $"Failed to load attendance ({(int)response.StatusCode})";
            }
        }
        catch (Exception ex)
        {
            errorMessage = "Connection error: " + ex.Message;
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SubmitAppeal()
    {
        if (string.IsNullOrWhiteSpace(appealReason)) return;
        appealMessage = null;
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/students/me/appeal");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            request.Content = JsonContent.Create(new
            {
                sessionId = appealSessionId,
                courseName = appealCourseName,
                reason = appealReason.Trim()
            });
            var response = await Http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                appealReason = string.Empty;
                isAppealSubmitted = true;
                await LoadData();
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                appealMessage = $"Failed ({(int)response.StatusCode}): {body}";
            }
        }
        catch (Exception ex)
        {
            appealMessage = "Error: " + ex.Message;
        }
        StateHasChanged();
    }

    private void CloseAppeal()
    {
        showAppealForm = false;
        isAppealSubmitted = false;
        appealMessage = null;
        appealReason = string.Empty;
		StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder b)
    {
        b.OpenComponent<PageTitle>(0);
        b.AddAttribute(1, "ChildContent", (RenderFragment)((b2) => b2.AddMarkupContent(2, "My Attendance — Attencial")));
        b.CloseComponent();

        if (!isAuthorized)
        {
            b.AddMarkupContent(3, "<div class=\"min-h-screen canvas-bg flex items-center justify-center\"><div class=\"spinner-ring-lg\"></div></div>");
            return;
        }

        if (isLoading)
        {
            b.AddMarkupContent(9, "<div class=\"canvas-bg flex items-center justify-center\" style=\"min-height: calc(100vh - 4rem);\"><div class=\"text-center\"><div class=\"spinner-ring-lg mb-4\"></div><p class=\"font-label-caps text-on-surface-variant\">Loading attendance data...</p></div></div>");
            return;
        }

        b.OpenElement(4, "div");
        b.AddAttribute(5, "class", "canvas-bg min-h-screen pb-24 animate-fade-in");

        b.OpenElement(6, "div");
        b.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile md:px-margin-desktop pt-8");
        b.AddMarkupContent(8, "<div class=\"mb-8\"><h1 class=\"font-display-lg text-headline-lg-mobile md:text-display-lg text-on-surface\">My Attendance</h1><div class=\"red-accent-line mt-3\"></div></div>");

        if (!string.IsNullOrEmpty(errorMessage))
        {
            b.OpenElement(10, "div");
            b.AddAttribute(11, "class", "border border-error/30 p-4 mb-8 flex items-start gap-3");
            b.AddAttribute(12, "style", "background: rgba(186,26,26,0.04);");
            b.AddMarkupContent(13, "<span class=\"material-symbols-outlined text-error\">error</span>");
            b.OpenElement(14, "span");
            b.AddAttribute(15, "class", "text-sm text-on-surface-variant");
            b.AddContent(16, errorMessage);
            b.CloseElement();
            b.CloseElement();
        }

        if (summary == null || summary.CourseAttendance.Count == 0)
        {
            b.AddMarkupContent(17, "<div class=\"card-neo text-center py-12\"><span class=\"material-symbols-outlined text-5xl text-outline block mb-4\">assignment_late</span><p class=\"font-body-md text-on-surface-variant mb-4\">No attendance data available.</p><a href=\"courses\" class=\"btn-neo-outline inline-block no-underline\">Browse Courses</a></div>");
            b.CloseElement();
            b.CloseElement();
            return;
        }

        // Summary stats
        b.OpenElement(18, "div");
        b.AddAttribute(19, "class", "grid grid-cols-2 md:grid-cols-4 gap-4 mb-8");
        BuildStatCard(b, 20, "Overall", $"{summary.OverallPercentage:F1}%", "trending_up");
        BuildStatCard(b, 30, "Courses", summary.TotalCourses.ToString(), "menu_book");
        BuildStatCard(b, 40, "Present", summary.PresentSessions.ToString(), "check_circle");
        BuildStatCard(b, 50, "Total Sessions", summary.TotalSessions.ToString(), "calendar_month");
        b.CloseElement();

        // Course list
        b.AddMarkupContent(60, "<div class=\"border-b border-outline-variant/20 pb-3 mb-6\"><h2 class=\"font-headline-md text-headline-md text-on-surface\">Course Breakdown</h2></div>");

        b.OpenElement(61, "div");
        b.AddAttribute(62, "class", "space-y-4");

        for (int i = 0; i < summary.CourseAttendance.Count; i++)
        {
            var course = summary.CourseAttendance[i];
            var isOpen = expanded.ContainsKey(course.CourseId) && expanded[course.CourseId];
            var baseIdx = 63 + (i * 40); // unique index range per course

            b.OpenElement(baseIdx, "div");
            b.AddAttribute(baseIdx + 1, "class", "card-neo cursor-pointer transition-all overflow-visible " + (isOpen ? "border-primary" : ""));
            b.AddAttribute(baseIdx + 2, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ToggleCourse(course.CourseId)));

            b.OpenElement(baseIdx + 3, "div");
            b.AddAttribute(baseIdx + 4, "class", "flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between");

            b.OpenElement(baseIdx + 5, "div");
            b.AddAttribute(baseIdx + 6, "class", "flex min-w-0 items-start gap-3");
            b.OpenElement(baseIdx + 7, "span");
            b.AddAttribute(baseIdx + 8, "class", "mt-2 h-3 w-3 flex-shrink-0 rounded-full inline-block");
            b.AddAttribute(baseIdx + 9, "style", course.Status == "Green" ? "background-color: #2ecc71;" : course.Status == "Yellow" ? "background-color: #f1c40f;" : "background-color: #b0252b;");
            b.CloseElement();
            b.OpenElement(baseIdx + 9, "div");
            b.AddAttribute(baseIdx + 10, "class", "min-w-0");
            b.OpenElement(baseIdx + 10, "h3");
            b.AddAttribute(baseIdx + 11, "class", "font-headline-md text-headline-md text-on-surface text-base break-words");
            b.AddContent(baseIdx + 12, course.CourseName);
            b.CloseElement();
            b.OpenElement(baseIdx + 13, "p");
            b.AddAttribute(baseIdx + 14, "class", "font-label-sm text-on-surface-variant break-words");
            b.AddContent(baseIdx + 15, $"{course.CourseCode} — {course.ProfessorName}");
            b.CloseElement();
            b.CloseElement();
            b.CloseElement();

            b.OpenElement(baseIdx + 16, "div");
            b.AddAttribute(baseIdx + 17, "class", "flex flex-shrink-0 items-center justify-between gap-4 sm:justify-end");
            b.OpenElement(baseIdx + 18, "span");
            b.AddAttribute(baseIdx + 19, "class", "font-headline-md text-xl text-on-surface");
            b.AddContent(baseIdx + 20, $"{course.Percentage:F1}%");
            b.CloseElement();

				if (course.Percentage < 60.0)
				{
					b.OpenElement(baseIdx + 36, "span");
					b.AddAttribute(baseIdx + 37, "class", "material-symbols-outlined text-primary text-lg");
					b.AddAttribute(baseIdx + 38, "title", "Below 60% - at risk");
					b.AddContent(baseIdx + 39, "warning");
					b.CloseElement();
				}
            b.OpenElement(baseIdx + 21, "span");
            b.AddAttribute(baseIdx + 22, "class", "material-symbols-outlined text-on-surface-variant chevron-icon " + (isOpen ? "open" : ""));
            b.AddContent(baseIdx + 23, "expand_more");
            b.CloseElement();
            b.CloseElement();

            b.CloseElement();

            b.OpenElement(baseIdx + 24, "div");
            b.AddAttribute(baseIdx + 25, "class", "flex flex-wrap items-center gap-2 mt-3 font-label-sm text-on-surface-variant");
            b.OpenElement(baseIdx + 26, "span");
            b.AddContent(baseIdx + 27, $"{course.AttendedSessions}/{course.TotalSessions} sessions attended");
            b.CloseElement();
            b.OpenElement(baseIdx + 28, "span");
            b.AddAttribute(baseIdx + 29, "class", "badge-neo");
            b.AddAttribute(baseIdx + 31, "style", course.Status == "Green" ? "color: #2ecc71; border-color: #2ecc71; background: rgba(46,204,113,0.08);" : course.Status == "Yellow" ? "color: #b0252b; border-color: #b0252b; background: #fbf9f6;" : "color: #b0252b; border-color: #b0252b; background: #b0252b;");
            b.AddContent(baseIdx + 30, course.Status);
            b.CloseElement();
            b.CloseElement();

            if (isOpen)
            {
                b.OpenElement(baseIdx + 31, "div");
                b.AddAttribute(baseIdx + 32, "class", "mt-4 border-t border-outline-variant/40 bg-surface-container-low p-3 sm:p-4 animate-slide-down");

                b.OpenElement(baseIdx + 33, "div");
                b.AddAttribute(baseIdx + 34, "class", "space-y-2");
                for (int j = 0; j < course.Sessions.Count; j++)
                {
                    var s = course.Sessions[j];
                    var mIdx = baseIdx + 35 + (j * 10);

                    b.OpenElement(mIdx, "div");
                    b.AddAttribute(mIdx + 1, "class", "flex flex-col gap-2 py-3 border-b border-outline-variant/20 sm:flex-row sm:items-center sm:justify-between");
                    b.OpenElement(mIdx + 2, "div");
                    b.AddAttribute(mIdx + 3, "class", "flex min-w-0 items-center gap-3");

                    if (s.IsPresent)
                    {
                        b.AddMarkupContent(mIdx + 4, "<span class=\"material-symbols-outlined text-tertiary\">check_circle</span>");
                    }
                    else
                    {
                        b.AddMarkupContent(mIdx + 4, "<span class=\"material-symbols-outlined text-primary\">event_busy</span>");
                    }

                    b.OpenElement(mIdx + 5, "span");
                    b.AddAttribute(mIdx + 6, "class", "font-body-md text-sm text-on-surface break-words");
                    b.AddContent(mIdx + 7, s.Date.ToLocalTime().ToString("MMM dd, yyyy — hh:mm tt"));
                    b.CloseElement();
                    b.CloseElement();

                    if (!s.IsPresent)
                    {
                        if (string.IsNullOrEmpty(s.AppealStatus))
                        {
                            b.OpenElement(mIdx + 8, "button");
                            b.AddAttribute(mIdx + 9, "class", "btn-neo-outline w-full text-xs py-1.5 px-3 flex items-center gap-1 sm:w-auto");
                            b.AddAttribute(mIdx + 10, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ShowAppealForm(s.SessionId, course.CourseName)));
                            b.AddAttribute(mIdx + 12, "onclick:stopPropagation", true);
                            b.AddMarkupContent(mIdx + 11, "<span class=\"material-symbols-outlined text-sm\">rate_review</span> Appeal");
                            b.CloseElement();
                        }
                        else if (s.AppealStatus == "Pending")
                        {
                            b.OpenElement(mIdx + 8, "span");
                            b.AddAttribute(mIdx + 9, "class", "badge-neo badge-neo-pending text-xs py-1 px-3");
                            b.AddAttribute(mIdx + 98, "style", "width: 140px; justify-content: center;");
                            b.AddContent(mIdx + 10, "Appeal Pending");
                            b.CloseElement();
                        }
                        else if (s.AppealStatus == "Rejected")
                        {
                            b.OpenElement(mIdx + 8, "span");
                            b.AddAttribute(mIdx + 9, "class", "badge-neo badge-neo-active text-xs py-1 px-3");
                            b.AddAttribute(mIdx + 98, "style", "width: 140px; justify-content: center;");
                            b.AddContent(mIdx + 10, "Appeal Rejected");
                            b.CloseElement();
                        }
                        else if (s.AppealStatus == "Approved" || s.AppealStatus == "Accepted")
                        {
                            b.OpenElement(mIdx + 8, "span");
                            b.AddAttribute(mIdx + 9, "class", "badge-neo text-xs py-1 px-3");
                            b.AddAttribute(mIdx + 98, "style", "width: 140px; justify-content: center; color: #2ecc71; border-color: #2ecc71; background: rgba(46,204,113,0.08);");
                            b.AddContent(mIdx + 10, "Appeal Accepted");
                            b.CloseElement();
                        }
                    }

                    b.CloseElement();
                }
                b.CloseElement();
                b.CloseElement();
            }

            b.CloseElement();
        }
        b.CloseElement();
        b.CloseElement();
        b.CloseElement();

        // Appeal modal
        if (showAppealForm)
        {
            b.OpenElement(2000, "div");
            b.AddAttribute(2001, "class", "fixed inset-0 z-50 flex items-center justify-center bg-black/30");

            if (isAppealSubmitted)
            {
                b.OpenElement(2003, "div");
                b.AddAttribute(2004, "class", "card-neo bg-surface max-w-sm w-full mx-4 relative p-6 text-center animate-scale-up");
                b.AddMarkupContent(2005, "<span class=\"material-symbols-outlined text-tertiary text-5xl mb-4 block\">check_circle</span>");
                b.AddMarkupContent(2006, "<h3 class=\"font-headline-md text-headline-md text-on-surface mb-2\">Appeal Submitted</h3>");
                b.AddMarkupContent(2007, "<p class=\"font-body-md text-on-surface-variant text-sm mb-6\">Your appeal has been successfully submitted to your professor for review.</p>");
                b.OpenElement(2008, "button");
                b.AddAttribute(2009, "class", "btn-neo-primary w-full text-sm");
                b.AddAttribute(2010, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, CloseAppeal));
                b.AddContent(2011, "Great!");
                b.CloseElement();
                b.CloseElement();
            }
            else
            {
                b.OpenElement(2003, "div");
                b.AddAttribute(2004, "class", "card-neo bg-surface max-w-md w-full mx-4 relative");


                b.OpenElement(2009, "h3");
                b.AddAttribute(2010, "class", "font-headline-md text-headline-md text-on-surface mb-2 pr-8");
                b.AddContent(2011, "Appeal Missed Session");
                b.CloseElement();

                b.OpenElement(2012, "p");
                b.AddAttribute(2013, "class", "font-label-sm text-on-surface-variant mb-4");
                b.AddContent(2014, "Course: ");
                b.OpenElement(2015, "strong");
                b.AddContent(2016, appealCourseName);
                b.CloseElement();
                b.CloseElement();

                if (!string.IsNullOrEmpty(appealMessage))
                {
                    b.OpenElement(2017, "div");
                    b.AddAttribute(2018, "class", "border border-tertiary bg-surface-container-low p-3 mb-4");
                    b.AddContent(2019, appealMessage);
                    b.CloseElement();
                }

                b.OpenElement(2020, "textarea");
                b.AddAttribute(2021, "class", "form-neo w-full mb-4 focus:outline-none focus:ring-0");
                b.AddAttribute(2022, "placeholder", "Explain why you missed this session...");
                b.AddAttribute(2023, "rows", "3");
                b.AddAttribute(2024, "value", appealReason);
                b.AddAttribute(2025, "onchange", EventCallback.Factory.CreateBinder(this, (string? v) => appealReason = v ?? "", appealReason));
                b.SetUpdatesAttributeName("value");
                b.CloseElement();

                b.OpenElement(2024, "div");
                b.AddAttribute(2026, "class", "flex gap-3 justify-end");
                b.OpenElement(2026, "button");
                b.AddAttribute(2027, "class", "btn-neo-outline text-sm");
                b.AddAttribute(2028, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, CloseAppeal));
                b.AddContent(2029, "Cancel");
                b.CloseElement();
                b.OpenElement(2030, "button");
                b.AddAttribute(2031, "class", "btn-neo-primary text-sm");
                b.AddAttribute(2032, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, SubmitAppeal));
                b.AddContent(2033, "Submit Appeal");
                b.CloseElement();
                b.CloseElement();

                b.CloseElement();
            }

            b.CloseElement();
        }
    }

    private void BuildStatCard(RenderTreeBuilder b, int idx, string label, string value, string icon)
    {
        b.OpenElement(idx, "div");
        b.AddAttribute(idx + 1, "class", "stat-neo text-center");
        b.OpenElement(idx + 2, "span");
        b.AddAttribute(idx + 3, "class", "material-symbols-outlined text-on-surface-variant text-xl mb-2 block");
        b.AddContent(idx + 4, icon);
        b.CloseElement();
        b.OpenElement(idx + 5, "div");
        b.AddAttribute(idx + 6, "class", "stat-neo-value text-xl");
        b.AddContent(idx + 7, value);
        b.CloseElement();
        b.OpenElement(idx + 8, "span");
        b.AddAttribute(idx + 9, "class", "stat-neo-label");
        b.AddContent(idx + 10, label);
        b.CloseElement();
        b.CloseElement();
    }

    private void ToggleCourse(int courseId)
    {
        if (expanded.ContainsKey(courseId))
            expanded[courseId] = !expanded[courseId];
        else
            expanded[courseId] = true;
        StateHasChanged();
    }

    private void ShowAppealForm(int sessionId, string courseName)
    {
        appealSessionId = sessionId;
        appealCourseName = courseName;
        appealReason = string.Empty;
        appealMessage = null;
        showAppealForm = true;
        StateHasChanged();
    }
}
