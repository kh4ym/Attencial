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

		private int professorActiveCourses;

		private int professorTodaySessions;

		private int professorPendingEnrollments;

		private int professorPendingAppeals;

		private StudentAttendanceSummaryDto? studentSummary;

		private Dictionary<int, bool> expandedCourses = new Dictionary<int, bool>();

		private bool hasAnimated;
		private string greeting = "Good morning";



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
				__builder.AddMarkupContent(3, "<div class=\"canvas-bg flex items-center justify-center\" style=\"min-height: calc(100vh - 4rem);\"><div class=\"text-center\"><div class=\"spinner-ring-lg mb-4\"></div>\n            <p class=\"font-label-caps text-label-caps text-on-surface-variant\">Loading dashboard...</p></div></div>");
			}
			else if (!isAuthorized)
			{
				__builder.AddMarkupContent(4, "<div class=\"canvas-bg flex items-center justify-center\" style=\"min-height: calc(100vh - 4rem);\"><p class=\"font-body-md text-body-md text-on-surface-variant\">Redirecting to login...</p></div>");
			}
			else
			{
				__builder.OpenElement(5, "div");
				__builder.AddAttribute(6, "class", "canvas-bg min-h-screen animate-fade-in");
				__builder.OpenElement(7, "main");
				__builder.AddAttribute(8, "class", "pt-8 pb-20 px-margin-mobile md:px-margin-desktop max-w-max-width mx-auto relative overflow-hidden");
				__builder.AddMarkupContent(9, "");
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
				__builder.AddAttribute(17, "class", "mb-12 md:mb-16 flex flex-col md:flex-row md:justify-between md:items-end gap-6 md:gap-8 relative animate-fade-in");
				__builder.OpenElement(18, "div");
				__builder.AddAttribute(19, "class", "max-w-2xl");
				__builder.AddMarkupContent(20, "<span class=\"font-label-caps text-label-caps text-secondary tracking-widest block mb-4\"><span class=\"live-dot align-middle mr-2\"></span> LIVE DASHBOARD\n                    </span>\n                    ");
				__builder.AddMarkupContent(21, "<h1 class=\"font-display-lg text-headline-lg-mobile md:text-display-lg text-on-surface mb-2 animate-fade-in\">" + greeting + "</h1>\n                    ");
				__builder.OpenElement(22, "p");
				__builder.AddAttribute(23, "class", "font-body-lg text-body-lg text-on-surface-variant");
				__builder.AddContent(24, userEmail);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(25, "\n                ");
				__builder.OpenElement(26, "div");
				__builder.AddAttribute(27, "class", "flex w-full md:w-auto items-center gap-4");
				__builder.OpenElement(28, "button");
				__builder.AddAttribute(29, "class", "btn-neo-primary w-full md:w-auto flex items-center gap-2");
				__builder.AddAttribute(30, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)RefreshData));
				__builder.AddMarkupContent(31, "<span class=\"material-symbols-outlined text-[18px]\">refresh</span>\n                        Refresh\n                    ");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(32, "\n\n            ");
				__builder.OpenElement(33, "div");
				__builder.AddAttribute(34, "class", "grid grid-cols-1 sm:grid-cols-2 " + ((userRole == "Student" || userRole == "Professor") ? "lg:grid-cols-4" : "lg:grid-cols-3") + " gap-6 mb-16 animate-fade-in delay-1");
				if (userRole == "Professor")
				{
					__builder.OpenElement(35, "div");
					__builder.AddAttribute(36, "class", "stat-neo group hover:border-primary transition-colors");
					__builder.AddMarkupContent(37, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">menu_book</span>\n                        Active Courses\n                    </span>\n                    ");
					__builder.OpenElement(38, "div");
					__builder.AddAttribute(39, "class", "stat-neo-value");
					__builder.AddAttribute(40, "id", "statProfCourses");
					__builder.AddContent(41, professorActiveCourses);
					__builder.CloseElement();
					__builder.AddMarkupContent(42, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
					__builder.OpenElement(43, "p");
					__builder.AddAttribute(44, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
					__builder.OpenElement(45, "span");
					__builder.AddAttribute(46, "class", "material-symbols-outlined text-[14px]");
					__builder.AddContent(47, "dashboard");
					__builder.CloseElement();
					__builder.AddMarkupContent(48, "\n                        ");
					__builder.AddContent(49, "Assigned this term");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(50, "\n                ");
					__builder.OpenElement(51, "div");
					__builder.AddAttribute(52, "class", "stat-neo group hover:border-tertiary transition-colors");
					__builder.AddMarkupContent(53, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">event</span>\n                        Today's Sessions\n                    </span>\n                    ");
					__builder.OpenElement(54, "div");
					__builder.AddAttribute(55, "class", "stat-neo-value");
					__builder.AddAttribute(56, "id", "statProfSessions");
					__builder.AddContent(57, professorTodaySessions);
					__builder.CloseElement();
					__builder.AddMarkupContent(58, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
					__builder.OpenElement(59, "p");
					__builder.AddAttribute(60, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
					__builder.OpenElement(61, "span");
					__builder.AddAttribute(62, "class", "material-symbols-outlined text-[14px]");
					__builder.AddContent(63, (professorTodaySessions > 0) ? "play_circle" : "radio_button_unchecked");
					__builder.CloseElement();
					__builder.AddMarkupContent(64, "\n                        ");
					__builder.AddContent(65, (professorTodaySessions > 0) ? "Active today" : "None yet");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(66, "\n                ");
					__builder.OpenElement(67, "div");
					__builder.AddAttribute(68, "class", "stat-neo group hover:border-primary transition-colors");
					__builder.AddMarkupContent(69, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">how_to_reg</span>\n                        Pending Enrollments\n                    </span>\n                    ");
					__builder.OpenElement(70, "div");
					__builder.AddAttribute(71, "class", "stat-neo-value");
					__builder.AddAttribute(72, "id", "statProfPendingEnrollments");
					__builder.AddContent(73, professorPendingEnrollments);
					__builder.CloseElement();
					__builder.AddMarkupContent(74, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
					__builder.OpenElement(75, "p");
					__builder.AddAttribute(76, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
					__builder.OpenElement(77, "span");
					__builder.AddAttribute(78, "class", "material-symbols-outlined text-[14px]");
					__builder.AddContent(79, (professorPendingEnrollments > 0) ? "flag" : "check_circle");
					__builder.CloseElement();
					__builder.AddMarkupContent(80, "\n                        ");
					__builder.AddContent(81, professorPendingEnrollments);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(82, "\n                ");
					__builder.OpenElement(83, "div");
					__builder.AddAttribute(84, "class", "stat-neo group hover:border-primary transition-colors");
					__builder.AddMarkupContent(85, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">rate_review</span>\n                        Pending Appeals\n                    </span>\n                    ");
					__builder.OpenElement(86, "div");
					__builder.AddAttribute(87, "class", "stat-neo-value");
					__builder.AddAttribute(88, "id", "statProfPendingAppeals");
					__builder.AddContent(89, professorPendingAppeals);
					__builder.CloseElement();
					__builder.AddMarkupContent(90, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
					__builder.OpenElement(91, "p");
					__builder.AddAttribute(92, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
					__builder.OpenElement(93, "span");
					__builder.AddAttribute(94, "class", "material-symbols-outlined text-[14px]");
					__builder.AddContent(95, (professorPendingAppeals > 0) ? "warning" : "check_circle");
					__builder.CloseElement();
					__builder.AddMarkupContent(96, "\n                        ");
					__builder.AddContent(97, "Needs review");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				else
				{
					if (userRole == "Student")
					{
						__builder.OpenElement(98, "div");
						__builder.AddAttribute(99, "class", "stat-neo group hover:border-primary transition-colors");
						__builder.AddMarkupContent(100, "<span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">verified</span>\n                        Face Enrollment\n                    </span>\n                    ");
						__builder.OpenElement(101, "div");
						__builder.AddAttribute(102, "class", "stat-neo-value");
						__builder.AddAttribute(103, "id", "statEnrollment");
						__builder.AddContent(104, enrollmentStatus);
						__builder.CloseElement();
						__builder.AddMarkupContent(105, "\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    ");
						__builder.OpenElement(106, "p");
						__builder.AddAttribute(107, "class", "font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1");
						__builder.OpenElement(108, "span");
						__builder.AddAttribute(109, "class", "material-symbols-outlined text-[14px]");
						__builder.AddContent(110, isEnrolled ? "check_circle" : "error");
						__builder.CloseElement();
						__builder.AddMarkupContent(111, "\n                        ");
						__builder.AddContent(112, isEnrolled ? "Ready for attendance" : "Enrollment required");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.AddMarkupContent(113, "<div class=\"stat-neo group hover:border-primary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">menu_book</span>\n                        Total Courses\n                    </span>\n                    <div class=\"stat-neo-value\" id=\"statCourses\">0</div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">arrow_upward</span>\n                        Active this semester\n                    </p></div>\n\n                ");
					__builder.AddMarkupContent(114, "<div class=\"stat-neo group hover:border-tertiary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">trending_up</span>\n                        Attendance %\n                    </span>\n                    <div class=\"stat-neo-value\"><span id=\"statAttendance\">0</span>\n                        <span class=\"text-[32px] font-headline-md text-on-surface\">%</span></div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">check_circle</span>\n                        Across all courses\n                    </p></div>\n\n                ");
					__builder.AddMarkupContent(115, "<div class=\"stat-neo group hover:border-primary transition-colors\"><span class=\"stat-neo-label flex items-center gap-2\"><span class=\"material-symbols-outlined text-[16px]\">calendar_month</span>\n                        Sessions Attended\n                    </span>\n                    <div class=\"stat-neo-value\" id=\"statSessions\">0</div>\n                    <div class=\"w-full h-[1px] bg-outline-variant/30 mt-6 mb-4\"></div>\n                    <p class=\"font-label-sm text-label-sm text-on-surface-variant flex items-center gap-1\"><span class=\"material-symbols-outlined text-[14px]\">schedule</span>\n                        This semester\n                    </p></div>");
				}
				__builder.CloseElement();
				if (userRole == "Student")
				{
					__builder.OpenElement(53, "div");
					__builder.AddAttribute(54, "class", "mb-16 animate-fade-in delay-2");
					__builder.AddMarkupContent(55, "<div class=\"flex items-center gap-3 mb-8 pb-4 border-b border-outline-variant/30\"><span class=\"material-symbols-outlined text-primary\">assignment_turned_in</span>\n                        <h2 class=\"font-headline-md text-headline-md text-on-surface\">My Courses & Attendance</h2></div>");
					if (studentSummary == null || studentSummary.CourseAttendance.Count == 0)
					{
						__builder.AddMarkupContent(56, "<div class=\"border border-on-surface bg-surface p-6 md:p-12 text-center\"><span class=\"material-symbols-outlined text-5xl text-outline block mb-4\">assignment_late</span>\n                            <p class=\"font-body-md text-body-md text-on-surface-variant mb-6\">You are not currently enrolled in any courses.</p>\n                            <a href=\"courses\" class=\"btn-neo-outline inline-flex no-underline\">Browse Courses</a></div>");
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
							__builder.AddAttribute(63, "class", "flex flex-col sm:flex-row sm:justify-between sm:items-start gap-4 mb-4");
							__builder.OpenElement(64, "div");
							__builder.OpenElement(65, "span");
							__builder.AddAttribute(66, "class", "badge-neo mb-3 inline-block");
							__builder.AddAttribute(67, "style", "border-color: " + text + "; color: " + text + "; background: " + text2 + ";");
							__builder.AddContent(68, course.CourseCode);
							__builder.CloseElement();
							__builder.AddMarkupContent(69, "\n                                            ");
							__builder.OpenElement(70, "h3");
							__builder.AddAttribute(71, "class", "font-headline-md text-[22px] text-on-surface break-words");
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
							__builder.AddAttribute(80, "class", "text-left sm:text-right");
							__builder.OpenElement(81, "span");
							__builder.AddAttribute(82, "class", "font-display-lg text-[40px] md:text-[48px] leading-none");
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
							if (course.Sessions.Count(s => !s.IsPresent) == 0)
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
								__builder.AddContent(107, course.Sessions.Count(s => !s.IsPresent));
								__builder.AddContent(108, " ");
								__builder.AddContent(109, (course.Sessions.Count(s => !s.IsPresent) == 1) ? "session" : "sessions");
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
									foreach (AttendanceSessionDto session in course.Sessions.Where(s => !s.IsPresent))
									{
										__builder.OpenElement(119, "div");
										__builder.AddAttribute(120, "class", "flex flex-col sm:flex-row sm:justify-between sm:items-center gap-2 py-2");
										__builder.OpenElement(121, "div");
										__builder.AddMarkupContent(122, "<span class=\"font-label-sm text-label-sm text-primary\"><span class=\"material-symbols-outlined text-[14px] align-middle\">cancel</span>\n                                                                    Missed\n                                                                </span>\n                                                                ");
										__builder.OpenElement(123, "span");
										__builder.AddAttribute(124, "class", "font-body-md text-[13px] text-on-surface-variant ml-3");
										__builder.AddContent(125, session.Date.ToLocalTime().ToString("MMM dd, yyyy · hh:mm tt"));
										__builder.CloseElement();
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
				if (userRole == "Professor")
				{
					await LoadProfessorSummary(token);
					return;
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

		private async Task LoadProfessorSummary(string token)
		{
			professorActiveCourses = 0;
			professorTodaySessions = 0;
			professorPendingEnrollments = 0;
			professorPendingAppeals = 0;
			try
			{
				HttpRequestMessage coursesRequest = new HttpRequestMessage(HttpMethod.Get, "api/attendance/professor/courses");
				coursesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage coursesResponse = await Http.SendAsync(coursesRequest);
				if (!coursesResponse.IsSuccessStatusCode)
				{
					if (coursesResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
					{
						loadError = "Professor profile required.";
						return;
					}
					loadError = $"Failed to load professor courses. Status: {coursesResponse.StatusCode}";
					return;
				}
				using JsonDocument coursesDoc = JsonDocument.Parse(await coursesResponse.Content.ReadAsStringAsync());
				List<int> courseIds = new List<int>();
				foreach (JsonElement item in coursesDoc.RootElement.GetProperty("data").EnumerateArray())
				{
					courseIds.Add(item.GetProperty("id").GetInt32());
				}
				professorActiveCourses = courseIds.Count;
				DateTime today = DateTime.UtcNow.Date;
				foreach (int courseId in courseIds)
				{
					HttpRequestMessage sessionsRequest = new HttpRequestMessage(HttpMethod.Get, $"api/professor/courses/{courseId}/sessions");
					sessionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					HttpResponseMessage sessionsResponse = await Http.SendAsync(sessionsRequest);
					if (!sessionsResponse.IsSuccessStatusCode)
					{
						continue;
					}
					using JsonDocument sessionsDoc = JsonDocument.Parse(await sessionsResponse.Content.ReadAsStringAsync());
					List<ProfessorSessionDto> sessions = JsonSerializer.Deserialize<List<ProfessorSessionDto>>(sessionsDoc.RootElement.GetProperty("data").GetRawText(), new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					}) ?? new List<ProfessorSessionDto>();
					professorTodaySessions += sessions.Count((ProfessorSessionDto s) => s.StartTime.Date == today);
				}
				HttpRequestMessage enrollmentsRequest = new HttpRequestMessage(HttpMethod.Get, "api/courses/enrollment-requests/pending");
				enrollmentsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage enrollmentsResponse = await Http.SendAsync(enrollmentsRequest);
				if (enrollmentsResponse.IsSuccessStatusCode)
				{
					using JsonDocument enrollmentsDoc = JsonDocument.Parse(await enrollmentsResponse.Content.ReadAsStringAsync());
					professorPendingEnrollments = enrollmentsDoc.RootElement.GetProperty("data").EnumerateArray().Count();
				}
				HttpRequestMessage appealsRequest = new HttpRequestMessage(HttpMethod.Get, "api/professor/appeals/pending");
				appealsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage appealsResponse = await Http.SendAsync(appealsRequest);
				if (appealsResponse.IsSuccessStatusCode)
				{
					using JsonDocument appealsDoc = JsonDocument.Parse(await appealsResponse.Content.ReadAsStringAsync());
					professorPendingAppeals = appealsDoc.RootElement.GetProperty("data").EnumerateArray().Count();
				}
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
				if (userRole == "Professor")
				{
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statProfCourses", professorActiveCourses, 1500);
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statProfSessions", professorTodaySessions, 1600);
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statProfPendingEnrollments", professorPendingEnrollments, 1700);
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statProfPendingAppeals", professorPendingAppeals, 1800);
				}
				else
				{
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statCourses", totalCourses, 1500);
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statAttendance", attendanceRate, 1800);
					await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statSessions", sessionsAttended, 1600);
				}
				int hour = DateTime.Now.Hour;
				greeting = (hour < 12) ? "Good morning" : (hour < 17) ? "Good afternoon" : "Good evening";
				StateHasChanged();
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

		private async Task Logout()
		{
			await JS.InvokeVoidAsync("authStorage.removeToken");
			Nav.NavigateTo("/login", forceLoad: true);
		}
	}
}
