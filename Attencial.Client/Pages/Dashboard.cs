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
	[Route("/dashboard")]
	public class Dashboard : ComponentBase
	{
		private bool isAuthorized;

		private bool isLoading = true;

		private string userEmail = string.Empty;

		private string userRole = "User";

		private string enrollmentStatus = "Unknown";

		private bool isEnrolled;

		private string? loadError;

		private int totalCourses;

		private int attendanceRate;

		private int sessionsAttended;

		private StudentAttendanceSummaryDto? studentSummary;

		private Dictionary<int, bool> expandedCourses = new Dictionary<int, bool>();

		private bool hasAnimated;

		private bool showAppealForm;

		private int appealSessionId;

		private string appealCourseName = string.Empty;

		private string appealReason = string.Empty;

		private string appealMessage = string.Empty;

		private bool isSubmittingAppeal;

		private int monthlyAppealCount;

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
				renderTreeBuilder.AddMarkupContent(2, "Dashboard — Attencial");
			});
			__builder.CloseComponent();
			if (isLoading)
			{
				__builder.AddMarkupContent(3, "<div class=\"canvas-bg min-h-screen flex items-center justify-center\"><div class=\"text-center\"><span class=\"material-symbols-outlined animate-spin text-primary text-4xl mb-4 block\">progress_activity</span>\n            <p class=\"font-label-caps text-label-caps text-on-surface-variant\">Loading dashboard...</p></div></div>");
			}
			else if (!isAuthorized)
			{
				__builder.AddMarkupContent(4, "<div class=\"canvas-bg min-h-screen flex items-center justify-center\"><p class=\"font-body-md text-body-md text-on-surface-variant\">Redirecting to login...</p></div>");
			}
			else
			{
				__builder.OpenElement(5, "div");
				__builder.AddAttribute(6, "class", "canvas-bg min-h-screen");
				__builder.OpenElement(7, "main");
				__builder.AddAttribute(8, "class", "pt-8 pb-20 px-margin-desktop max-w-max-width mx-auto relative overflow-hidden");
				__builder.AddMarkupContent(9, "<div class=\"absolute top-20 right-[-5%] w-96 h-96 bg-primary/5 rounded-full blur-3xl -z-10\"></div>\n            <div class=\"absolute bottom-[-10%] left-[-5%] w-64 h-64 border-[40px] border-tertiary/5 rounded-full -z-10\"></div>");
				if (!string.IsNullOrEmpty(loadError))
				{
					__builder.OpenElement(10, "div");
					__builder.AddAttribute(11, "class", "border border-primary p-6 mb-8 bg-surface flex items-center gap-4 animate-fade-in");
					__builder.AddMarkupContent(12, "<span class=\"material-symbols-outlined text-primary\">warning</span>\n                    ");
					__builder.OpenElement(13, "span");
					__builder.AddAttribute(14, "class", "font-body-md text-body-md text-on-surface");
					__builder.AddContent(15, loadError);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(16, "div");
				__builder.AddAttribute(17, "class", "mb-16 flex flex-col md:flex-row justify-between items-end gap-8 relative animate-fade-in");
				__builder.OpenElement(18, "div");
				__builder.AddAttribute(19, "class", "max-w-2xl");
				__builder.AddMarkupContent(20, "<span class=\"font-label-caps text-label-caps text-secondary tracking-widest block mb-4\"><span class=\"live-dot align-middle mr-2\"></span> LIVE DASHBOARD\n                    </span>\n                    ");
				__builder.AddMarkupContent(21, "<h1 class=\"font-display-lg text-display-lg text-on-surface mb-2\" id=\"greetingText\">Good morning</h1>\n                    ");
				__builder.OpenElement(22, "p");
				__builder.AddAttribute(23, "class", "font-body-lg text-body-lg text-on-surface-variant");
				__builder.AddContent(24, userEmail);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(25, "\n                ");
				__builder.OpenElement(26, "div");
				__builder.AddAttribute(27, "class", "flex items-center gap-4");
				__builder.OpenElement(28, "button");
				__builder.AddAttribute(29, "class", "btn-neo-primary flex items-center gap-2");
				__builder.AddAttribute(30, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)RefreshData));
				__builder.AddMarkupContent(31, "<span class=\"material-symbols-outlined text-[18px]\">refresh</span>\n                        Refresh\n                    ");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(32, "\n\n            ");
				__builder.OpenElement(33, "div");
				__builder.AddAttribute(34, "class", "grid grid-cols-1 sm:grid-cols-2 " + ((userRole == "Student") ? "lg:grid-cols-4" : "lg:grid-cols-3") + " gap-6 mb-16 animate-fade-in delay-1");
				if (userRole == "Student")
				{
					__builder.OpenElement(35, "div");
					__builder.AddAttribute(36, "class", "stat-neo group hover:border-primary transition-colors");
					__builder.AddMarkupContent(37, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">verified</span>\n                        Face Enrollment\n                    </span>\n                    ");
					__builder.OpenElement(38, "div");
					__builder.AddAttribute(39, "class", "stat-neo-value");
					__builder.AddAttribute(40, "id", "statEnrollment");
					__builder.AddContent(41, enrollmentStatus);
					__builder.CloseElement();
					__builder.AddMarkupContent(42, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
					__builder.OpenElement(43, "p");
					__builder.AddAttribute(44, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
					__builder.OpenElement(45, "span");
					__builder.AddAttribute(46, "class", "material-symbols-outlined text-[14px]");
					__builder.AddContent(47, isEnrolled ? "check_circle" : "error");
					__builder.CloseElement();
					__builder.AddMarkupContent(48, "\n                        ");
					__builder.AddContent(49, isEnrolled ? "Ready for attendance" : "Enrollment required");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.AddMarkupContent(50, "<div class=\"stat-neo group hover:border-primary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">menu_book</span>\n                        Total Courses\n                    </span>\n                    <div class=\"stat-neo-value\" id=\"statCourses\">0</div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">arrow_upward</span>\n                        Active this semester\n                    </p></div>\n\n                ");
				__builder.AddMarkupContent(51, "<div class=\"stat-neo group hover:border-tertiary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">trending_up</span>\n                        Attendance %\n                    </span>\n                    <div class=\"stat-neo-value\"><span id=\"statAttendance\">0</span>\n                        <span class=\"text-[32px] font-headline-md text-on-surface\">%</span></div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">check_circle</span>\n                        Across all courses\n                    </p></div>\n\n                ");
				__builder.AddMarkupContent(52, "<div class=\"stat-neo group hover:border-primary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">calendar_month</span>\n                        Sessions Attended\n                    </span>\n                    <div class=\"stat-neo-value\" id=\"statSessions\">0</div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">schedule</span>\n                        This semester\n                    </p></div>");
				__builder.CloseElement();
				if (userRole == "Student")
				{
					__builder.OpenElement(53, "div");
					__builder.AddAttribute(54, "class", "mb-16 animate-fade-in delay-2");
					__builder.AddMarkupContent(55, "<div class=\"flex items-center gap-3 mb-8 pb-4 border-b border-outline-variant/30\"><span class=\"material-symbols-outlined text-primary\">assignment_turned_in</span>\n                        <h2 class=\"font-headline-md text-headline-md text-on-surface\">My Courses & Attendance</h2></div>");
					if (studentSummary == null || studentSummary.CourseAttendance.Count == 0)
					{
						__builder.AddMarkupContent(56, "<div class=\"border border-on-surface bg-surface p-12 text-center\"><span class=\"material-symbols-outlined text-5xl text-outline block mb-4\">assignment_late</span>\n                            <p class=\"font-body-md text-body-md text-on-surface-variant mb-6\">You are not currently enrolled in any courses.</p>\n                            <a href=\"courses\" class=\"btn-neo-outline inline-block no-underline\">Browse Courses</a></div>");
					}
					else
					{
						__builder.OpenElement(57, "div");
						__builder.AddAttribute(58, "class", "grid grid-cols-1 md:grid-cols-2 gap-6");
						foreach (StudentCourseAttendanceDto course in studentSummary.CourseAttendance)
						{
							string text = ((course.Status == "Green") ? "#2ecc71" : ((course.Status == "Yellow") ? "#f1c40f" : "#ba1a1a"));
							string text2 = ((course.Status == "Green") ? "rgba(46,204,113,0.06)" : ((course.Status == "Yellow") ? "rgba(241,196,15,0.06)" : "rgba(186,26,26,0.06)"));
							bool flag = expandedCourses.ContainsKey(course.CourseId) && expandedCourses[course.CourseId];
							__builder.OpenElement(59, "div");
							__builder.AddAttribute(60, "class", "card-neo-hover");
							__builder.AddAttribute(61, "style", "border-left: 4px solid " + text + ";");
							__builder.OpenElement(62, "div");
							__builder.AddAttribute(63, "class", "flex justify-between items-start mb-4");
							__builder.OpenElement(64, "div");
							__builder.OpenElement(65, "span");
							__builder.AddAttribute(66, "class", "badge-neo mb-3 inline-block");
							__builder.AddAttribute(67, "style", "border-color: " + text + "; color: " + text + "; background: " + text2 + ";");
							__builder.AddContent(68, course.CourseCode);
							__builder.CloseElement();
							__builder.AddMarkupContent(69, "\n                                            ");
							__builder.OpenElement(70, "h3");
							__builder.AddAttribute(71, "class", "font-headline-md text-[22px] text-on-surface");
							__builder.AddContent(72, course.CourseName);
							__builder.CloseElement();
							__builder.AddMarkupContent(73, "\n                                            ");
							__builder.OpenElement(74, "p");
							__builder.AddAttribute(75, "class", "font-body-md text-[14px] text-on-surface-variant");
							__builder.AddContent(76, "Prof. ");
							__builder.AddContent(77, course.ProfessorName);
							__builder.CloseElement();
							__builder.CloseElement();
							__builder.AddMarkupContent(78, "\n                                        ");
							__builder.OpenElement(79, "div");
							__builder.AddAttribute(80, "class", "text-right");
							__builder.OpenElement(81, "span");
							__builder.AddAttribute(82, "class", "font-display-lg text-[48px] leading-none");
							__builder.AddAttribute(83, "style", "color: " + text + ";");
							__builder.AddContent(84, course.Percentage);
							__builder.AddContent(85, "%");
							__builder.CloseElement();
							__builder.AddMarkupContent(86, "\n                                            ");
							__builder.OpenElement(87, "p");
							__builder.AddAttribute(88, "class", "font-label-sm text-label-sm text-on-surface-variant");
							__builder.AddContent(89, course.AttendedSessions);
							__builder.AddContent(90, " / ");
							__builder.AddContent(91, course.TotalSessions);
							__builder.CloseElement();
							__builder.CloseElement();
							__builder.CloseElement();
							__builder.AddMarkupContent(92, "\n\n                                    ");
							__builder.OpenElement(93, "div");
							__builder.AddAttribute(94, "class", "w-full h-1 bg-outline-variant/30 mb-4");
							__builder.OpenElement(95, "div");
							__builder.AddAttribute(96, "class", "h-full transition-all");
							__builder.AddAttribute(97, "style", "width: " + course.Percentage + "%; background: " + text + ";");
							__builder.CloseElement();
							__builder.CloseElement();
							__builder.AddMarkupContent(98, "\n\n                                    ");
							__builder.OpenElement(99, "div");
							__builder.AddAttribute(100, "class", "pt-4 border-t border-outline-variant/20");
							if (course.MissedSessions.Count == 0)
							{
								__builder.AddMarkupContent(101, "<div class=\"flex items-center gap-2 font-label-sm text-label-sm\" style=\"color: #2ecc71;\"><span class=\"material-symbols-outlined text-[16px]\">verified</span>\n                                                100% Perfect Attendance\n                                            </div>");
							}
							else
							{
								__builder.OpenElement(102, "button");
								__builder.AddAttribute(103, "class", "w-full flex justify-between items-center font-label-sm text-label-sm text-on-surface-variant hover:text-primary bg-transparent border-0 cursor-pointer p-0");
								__builder.AddAttribute(104, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
								{
									ToggleCourse(course.CourseId);
								}));
								__builder.OpenElement(105, "span");
								__builder.AddMarkupContent(106, "<span class=\"material-symbols-outlined text-[16px] align-middle\" style=\"color: #f1c40f;\">warning</span>\n                                                    Missed ");
								__builder.AddContent(107, course.MissedSessions.Count);
								__builder.AddContent(108, " ");
								__builder.AddContent(109, (course.MissedSessions.Count == 1) ? "session" : "sessions");
								__builder.CloseElement();
								__builder.AddMarkupContent(110, "\n                                                ");
								__builder.OpenElement(111, "span");
								__builder.AddContent(112, flag ? "Hide" : "View");
								__builder.AddMarkupContent(113, "\n                                                    ");
								__builder.OpenElement(114, "span");
								__builder.AddAttribute(115, "class", "material-symbols-outlined text-[16px] align-middle");
								__builder.AddContent(116, flag ? "expand_less" : "expand_more");
								__builder.CloseElement();
								__builder.CloseElement();
								__builder.CloseElement();
								if (flag)
								{
									__builder.OpenElement(117, "div");
									__builder.AddAttribute(118, "class", "mt-3 p-4 bg-surface-container-low text-sm max-h-48 overflow-y-auto space-y-2");
									foreach (MissedSessionDto session in course.MissedSessions)
									{
										__builder.OpenElement(119, "div");
										__builder.AddAttribute(120, "class", "flex justify-between items-center py-1");
										__builder.OpenElement(121, "div");
										__builder.AddMarkupContent(122, "<span class=\"font-label-sm text-label-sm text-primary\"><span class=\"material-symbols-outlined text-[14px] align-middle\">cancel</span>\n                                                                    Missed\n                                                                </span>\n                                                                ");
										__builder.OpenElement(123, "span");
										__builder.AddAttribute(124, "class", "font-body-md text-[13px] text-on-surface-variant ml-3");
										__builder.AddContent(125, session.Date.ToLocalTime().ToString("MMM dd, yyyy · hh:mm tt"));
										__builder.CloseElement();
										__builder.CloseElement();
										__builder.AddMarkupContent(126, "\n                                                            ");
										__builder.OpenElement(127, "button");
										__builder.AddAttribute(128, "class", "font-label-caps text-[10px] text-primary border border-primary px-2 py-1 hover:bg-primary hover:text-surface transition-colors cursor-pointer");
										__builder.AddAttribute(129, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
										{
											ShowAppealForm(session.SessionId, course.CourseName);
										}));
										__builder.AddEventStopPropagationAttribute(130, "onclick", value: true);
										__builder.AddMarkupContent(131, "\n                                                                APPEAL\n                                                            ");
										__builder.CloseElement();
										__builder.CloseElement();
									}
									__builder.CloseElement();
								}
							}
							__builder.CloseElement();
							__builder.CloseElement();
						}
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
				__builder.OpenElement(132, "div");
				__builder.AddAttribute(133, "class", "grid grid-cols-1 md:grid-cols-2 gap-6 animate-fade-in delay-3");
				__builder.OpenElement(134, "div");
				__builder.AddAttribute(135, "class", "card-neo");
				__builder.AddMarkupContent(136, "<div class=\"flex items-center gap-3 mb-6 pb-4 border-b border-outline-variant/30\"><span class=\"material-symbols-outlined text-primary\">bolt</span>\n                        <h3 class=\"font-headline-md text-[22px] text-on-surface\">Quick Actions</h3></div>\n                    ");
				__builder.OpenElement(137, "div");
				__builder.AddAttribute(138, "class", "space-y-3");
				__builder.OpenElement(139, "button");
				__builder.AddAttribute(140, "class", "btn-neo-outline w-full flex justify-between items-center");
				__builder.AddAttribute(141, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)RefreshData));
				__builder.AddMarkupContent(142, "<span><span class=\"material-symbols-outlined text-[16px] align-middle mr-2\">person_check</span> Refresh Stats</span>\n                            ");
				__builder.AddMarkupContent(143, "<span class=\"material-symbols-outlined text-[16px]\">chevron_right</span>");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(144, "\n\n                ");
				__builder.OpenElement(145, "div");
				__builder.AddAttribute(146, "class", "card-neo");
				__builder.AddMarkupContent(147, "<div class=\"flex items-center gap-3 mb-6 pb-4 border-b border-outline-variant/30\"><span class=\"material-symbols-outlined text-primary\">database</span>\n                        <h3 class=\"font-headline-md text-[22px] text-on-surface\">System Status</h3></div>\n                    ");
				__builder.OpenElement(148, "div");
				__builder.AddAttribute(149, "class", "space-y-4");
				__builder.AddMarkupContent(150, "<div class=\"flex justify-between items-center\"><span class=\"font-body-md text-[14px] text-on-surface-variant flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px] text-tertiary\">cloud</span>\n                                AWS Rekognition\n                            </span>\n                            <span class=\"badge-neo badge-neo-success\">Online</span></div>\n                        ");
				__builder.AddMarkupContent(151, "<div class=\"flex justify-between items-center\"><span class=\"font-body-md text-[14px] text-on-surface-variant flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px] text-tertiary\">storage</span>\n                                Database\n                            </span>\n                            <span class=\"badge-neo badge-neo-success\">Connected</span></div>\n                        ");
				__builder.AddMarkupContent(152, "<div class=\"flex justify-between items-center\"><span class=\"font-body-md text-[14px] text-on-surface-variant flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px] text-tertiary\">lock</span>\n                                API Status\n                            </span>\n                            <span class=\"badge-neo badge-neo-success flex items-center gap-2\"><span class=\"live-dot\"></span> Operational</span></div>\n                        ");
				__builder.OpenElement(153, "div");
				__builder.AddAttribute(154, "class", "flex justify-between items-center");
				__builder.AddMarkupContent(155, "<span class=\"font-body-md text-[14px] text-on-surface-variant flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">badge</span>\n                                Your Role\n                            </span>\n                            ");
				__builder.OpenElement(156, "span");
				__builder.AddAttribute(157, "class", "badge-neo");
				__builder.AddContent(158, userRole);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(159, "\n\n            ");
				__builder.AddMarkupContent(160, "<p class=\"text-center font-label-sm text-label-sm text-on-surface-variant/50 mt-16 flex items-center justify-center gap-2\"><span class=\"material-symbols-outlined text-[14px]\">verified</span>\n                Secured by AWS Rekognition · JWT Authentication\n            </p>");
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (showAppealForm)
			{
				__builder.OpenElement(161, "div");
				__builder.AddAttribute(162, "class", "fixed inset-0 z-50 flex items-center justify-center bg-black/30");
				__builder.AddAttribute(163, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)CloseAppealForm));
				__builder.OpenElement(164, "div");
				__builder.AddAttribute(165, "class", "card-neo-raised p-8 max-w-md w-full mx-4 bg-surface");
				__builder.AddEventStopPropagationAttribute(166, "onclick", value: true);
				__builder.AddMarkupContent(167, "<h3 class=\"font-headline-md text-headline-md text-on-surface mb-2\">Appeal Attendance</h3>\n            ");
				__builder.OpenElement(168, "p");
				__builder.AddAttribute(169, "class", "font-body-md text-on-surface-variant mb-4");
				__builder.AddContent(170, "Course: ");
				__builder.AddContent(171, appealCourseName);
				__builder.CloseElement();
				__builder.AddMarkupContent(172, "\n            ");
				__builder.OpenElement(173, "p");
				__builder.AddAttribute(174, "class", "font-label-sm text-on-surface-variant mb-6");
				__builder.AddContent(175, "Appeals used this month: ");
				__builder.AddContent(176, monthlyAppealCount);
				__builder.AddContent(177, " / 5");
				__builder.CloseElement();
				__builder.AddMarkupContent(178, "\n            ");
				__builder.OpenElement(179, "textarea");
				__builder.AddAttribute(180, "class", "w-full border border-on-surface bg-transparent p-3 font-body-md text-on-surface placeholder:text-on-surface-variant/50 mb-4");
				__builder.AddAttribute(181, "rows", "4");
				__builder.AddAttribute(182, "placeholder", "Explain why your absence should be excused...");
				__builder.AddAttribute(183, "value", BindConverter.FormatValue(appealReason));
				__builder.AddAttribute(184, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					appealReason = __value;
				}, appealReason));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				if (!string.IsNullOrEmpty(appealMessage))
				{
					__builder.OpenElement(185, "div");
					__builder.AddAttribute(186, "class", "border border-error/30 p-3 mb-4 text-sm");
					__builder.AddAttribute(187, "style", "background: rgba(186,26,26,0.04); color: var(--brand-error);");
					__builder.AddContent(188, appealMessage);
					__builder.CloseElement();
				}
				__builder.OpenElement(189, "div");
				__builder.AddAttribute(190, "class", "flex gap-3");
				__builder.OpenElement(191, "button");
				__builder.AddAttribute(192, "class", "btn-neo-primary flex-1");
				__builder.AddAttribute(193, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)SubmitAppeal));
				__builder.AddAttribute(194, "disabled", isSubmittingAppeal || monthlyAppealCount >= 5);
				__builder.AddContent(195, isSubmittingAppeal ? "Submitting..." : "Submit Appeal");
				__builder.CloseElement();
				__builder.AddMarkupContent(196, "\n                ");
				__builder.OpenElement(197, "button");
				__builder.AddAttribute(198, "class", "btn-neo-outline flex-1");
				__builder.AddAttribute(199, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)CloseAppealForm));
				__builder.AddContent(200, "Cancel");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
		}

		private void ToggleCourse(int courseId)
		{
			if (expandedCourses.ContainsKey(courseId))
			{
				expandedCourses[courseId] = !expandedCourses[courseId];
			}
			else
			{
				expandedCourses[courseId] = true;
			}
		}

		protected override async Task OnInitializedAsync()
		{
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (string.IsNullOrEmpty(text) || text == "null" || text == "undefined")
			{
				Nav.NavigateTo("/login");
				return;
			}
			isAuthorized = true;
			await LoadDashboardData(text);
		}

		private async Task LoadDashboardData(string token)
		{
			isLoading = true;
			loadError = null;
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					JsonElement property = jsonDocument.RootElement.GetProperty("data");
					userEmail = property.GetProperty("email").GetString() ?? string.Empty;
					userRole = property.GetProperty("role").GetString() ?? "User";
				}
				if (userRole == "Student")
				{
					HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "api/enrollment/status");
					httpRequestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					HttpResponseMessage httpResponseMessage2 = await Http.SendAsync(httpRequestMessage2);
					if (httpResponseMessage2.IsSuccessStatusCode)
					{
						using JsonDocument jsonDocument2 = JsonDocument.Parse(await httpResponseMessage2.Content.ReadAsStringAsync());
						isEnrolled = jsonDocument2.RootElement.GetProperty("data").GetProperty("isEnrolled").GetBoolean();
						enrollmentStatus = (isEnrolled ? "Active" : "Pending");
						if (!isEnrolled)
						{
							Nav.NavigateTo("/enroll-face", forceLoad: true);
							return;
						}
					}
				}
				if (!(userRole == "Student"))
				{
					return;
				}
				HttpRequestMessage httpRequestMessage3 = new HttpRequestMessage(HttpMethod.Get, "api/students/me/attendance");
				httpRequestMessage3.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage3 = await Http.SendAsync(httpRequestMessage3);
				if (httpResponseMessage3.IsSuccessStatusCode)
				{
					studentSummary = (await httpResponseMessage3.Content.ReadFromJsonAsync<ApiResponse<StudentAttendanceSummaryDto>>())?.Data;
					if (studentSummary != null)
					{
						totalCourses = studentSummary.TotalCourses;
						attendanceRate = (int)Math.Round(studentSummary.OverallPercentage);
						sessionsAttended = studentSummary.PresentSessions;
					}
				}
				else
				{
					loadError = $"Failed to load student attendance. Status: {httpResponseMessage3.StatusCode}";
				}
				HttpRequestMessage httpRequestMessage4 = new HttpRequestMessage(HttpMethod.Get, "api/students/appeals");
				httpRequestMessage4.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage4 = await Http.SendAsync(httpRequestMessage4);
				if (!httpResponseMessage4.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument3 = JsonDocument.Parse(await httpResponseMessage4.Content.ReadAsStringAsync());
				monthlyAppealCount = jsonDocument3.RootElement.GetProperty("data").EnumerateArray().Count(delegate(JsonElement a)
				{
					DateTime dateTime = a.GetProperty("createdAt").GetDateTime();
					return dateTime.Year == DateTime.UtcNow.Year && dateTime.Month == DateTime.UtcNow.Month;
				});
			}
			catch (Exception ex)
			{
				loadError = "Connection error: " + ex.Message;
			}
			finally
			{
				isLoading = false;
			StateHasChanged();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!isLoading && isAuthorized && !hasAnimated)
			{
				hasAnimated = true;
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statCourses", totalCourses, 1500);
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statAttendance", attendanceRate, 1800);
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statSessions", sessionsAttended, 1600);
				int hour = DateTime.Now.Hour;
				string text = ((hour < 12) ? "Good morning" : ((hour < 17) ? "Good afternoon" : "Good evening"));
				await JS.InvokeVoidAsync("attencialAnimations.scrambleText", "greetingText", text, 800);
			}
		}

		private async Task RefreshData()
		{
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (!string.IsNullOrEmpty(text))
			{
				hasAnimated = false;
				await LoadDashboardData(text);
			}
		}

		private void ShowAppealForm(int sessionId, string courseName)
		{
			appealSessionId = sessionId;
			appealCourseName = courseName;
			appealReason = string.Empty;
			appealMessage = string.Empty;
			showAppealForm = true;
		}

		private void CloseAppealForm()
		{
			showAppealForm = false;
		}

		private async Task Logout()
		{
			await JS.InvokeVoidAsync("authStorage.removeToken");
			Nav.NavigateTo("/login", forceLoad: true);
		}

		private async Task SubmitAppeal()
		{
			if (string.IsNullOrWhiteSpace(appealReason))
			{
				appealMessage = "Please provide a reason.";
				return;
			}
			isSubmittingAppeal = true;
			appealMessage = string.Empty;
			StateHasChanged();
			try
			{
				string parameter = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/students/appeal");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					sessionId = appealSessionId,
					reason = appealReason.Trim()
				});
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				ApiResponse<string> apiResponse = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
				if (res.IsSuccessStatusCode)
				{
					showAppealForm = false;
					monthlyAppealCount++;
				}
				else
				{
					appealMessage = apiResponse?.Message ?? "Failed to submit appeal.";
				}
			}
			catch (Exception ex)
			{
				appealMessage = "Error: " + ex.Message;
			}
			finally
			{
				isSubmittingAppeal = false;
				StateHasChanged();
			}
		}
	}
}
