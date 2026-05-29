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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/professor-dashboard")]
	public class ProfessorDashboard : ComponentBase
	{
		private class CourseItem
		{
			public int Id { get; set; }

			public string Name { get; set; } = string.Empty;

			public string CourseCode { get; set; } = string.Empty;

			public int TotalSessions { get; set; }

			public int EnrolledCount { get; set; }

			public DateTime? LastSessionDate { get; set; }

			public string ActiveTab { get; set; } = "sessions";

			public List<SessionHistoryItem>? SessionHistory { get; set; }

			public List<RosterStudent>? Roster { get; set; }

			public List<AbuseLogItem>? AbuseLogs { get; set; }

			public List<AttendanceRow>? AttendanceData { get; set; }
		}

		private class AttendanceRow
		{
			public string StudentName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;

			public int PresentCount { get; set; }

			public double AttendanceRate { get; set; }

			public List<SessionMark> Sessions { get; set; } = new List<SessionMark>();
		}

		private class SessionMark
		{
			public int SessionId { get; set; }

			public bool IsPresent { get; set; }
		}

		private class SessionHistoryItem
		{
			public int SessionId { get; set; }

			public DateTime StartTime { get; set; }

			public DateTime? EndTime { get; set; }

			public bool IsActive { get; set; }
		}

		private class RosterStudent
		{
			public int StudentId { get; set; }

			public string StudentName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;
		}

		private class AbuseLogItem
		{
			public int Id { get; set; }

			public string StudentName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;

			public string AbuseType { get; set; } = string.Empty;

			public string Details { get; set; } = string.Empty;

			public DateTime LoggedAt { get; set; }
		}

		private class EnrolledStudent
		{
			public int Id { get; set; }

			public string FullName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;
		}

		private class AppealItem
		{
			public int Id { get; set; }

			public string StudentName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;

			public string CourseCode { get; set; } = string.Empty;

			public string Reason { get; set; } = string.Empty;

			public string Status { get; set; } = string.Empty;

			public DateTime SessionDate { get; set; }
		}

		private bool isAuthorized;

		private bool isProfileMissing;

		private string? errorMessage;

		private string jwtToken = string.Empty;

		private List<CourseItem> courses = new List<CourseItem>();

		private int expandedCourseId;

		private bool isLoadingCourses = true;

		private int todaySessionCount;

		private int pendingEnrollments;


		private int pendingAppealCount;




		private int sessionToDelete;

		private int courseToDelete;

		private int overrideSessionId;

		private List<EnrolledStudent> overrideStudents = new List<EnrolledStudent>();

		private HashSet<int> overrideSelectedIds = new HashSet<int>();

		private bool isSavingOverride;

		private bool showAppealsModal;

		private List<AppealItem> appealsList = new List<AppealItem>();

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
				renderTreeBuilder.AddMarkupContent(2, "At a Glance — Attencial");
			});
			__builder.CloseComponent();
			if (!isAuthorized)
			{
				__builder.AddMarkupContent(3, "<div class=\"min-h-screen canvas-bg flex items-center justify-center\"><div class=\"spinner-ring-lg\"></div></div>");
			}
			else if (isProfileMissing)
			{
				__builder.AddMarkupContent(4, "<div class=\"min-h-screen canvas-bg flex items-center justify-center\"><div class=\"card-neo-raised p-8 text-center max-w-md\"><span class=\"material-symbols-outlined text-4xl text-primary mb-4 block\">badge</span>\n            <h3 class=\"font-headline-md text-headline-md text-on-surface mb-2\">Professor Profile Required</h3>\n            <p class=\"text-body-md text-on-surface-variant mb-4\">Create your professor profile to access the dashboard.</p>\n            <a href=\"/session\" class=\"btn-neo-primary inline-block no-underline\">Go to Sessions</a></div></div>");
			}
			else
			{
				__builder.OpenElement(5, "div");
				__builder.AddAttribute(6, "class", "canvas-bg min-h-screen pb-16");
				__builder.OpenElement(7, "div");
				__builder.AddAttribute(8, "class", "max-w-max-width mx-auto px-margin-mobile lg:px-margin-desktop pt-8");
				__builder.OpenElement(9, "div");
				__builder.AddAttribute(10, "class", "mb-8 flex flex-col md:flex-row justify-between items-start md:items-end gap-4");
				__builder.OpenElement(11, "div");
				__builder.AddMarkupContent(12, "<div class=\"flex items-center gap-2 mb-2\"><span class=\"red-accent-dot\"></span>\n                        <span class=\"red-accent-dot\"></span>\n                        <span class=\"red-accent-dot\"></span></div>\n                    ");
				__builder.OpenElement(13, "span");
				__builder.AddAttribute(14, "class", "font-label-caps text-label-caps text-on-surface-variant tracking-[0.2em] block mb-2");
				__builder.AddContent(15, DateTime.Now.ToString("MMMM yyyy").ToUpper());
				__builder.CloseElement();
				__builder.AddMarkupContent(16, "\n                    ");
				__builder.AddMarkupContent(17, "<h1 class=\"font-display-lg text-headline-lg-mobile md:text-display-lg text-on-surface\">At a Glance</h1>\n                    <div class=\"red-accent-line mt-3\"></div>");
				__builder.CloseElement();
				__builder.AddMarkupContent(18, "\n                ");
				__builder.AddMarkupContent(19, "<a href=\"session\" class=\"btn-neo-primary no-underline text-lg px-8 py-4 flex items-center gap-3 animate-bob\"><span class=\"material-symbols-outlined text-2xl\">play_circle</span>\n                    Create Session\n                </a>");
				__builder.CloseElement();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					__builder.OpenElement(20, "div");
					__builder.AddAttribute(21, "class", "border border-error/30 p-4 mb-8 flex items-start gap-3 animate-fade-in");
					__builder.AddAttribute(22, "style", "background: rgba(186,26,26,0.04);");
					__builder.AddMarkupContent(23, "<span class=\"material-symbols-outlined text-error\">error</span>\n                    ");
					__builder.OpenElement(24, "span");
					__builder.AddAttribute(25, "class", "text-sm text-on-surface-variant");
					__builder.AddContent(26, errorMessage);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(27, "div");
				__builder.AddAttribute(28, "class", "grid grid-cols-2 lg:grid-cols-4 gap-gutter mb-8");
				__builder.OpenElement(29, "div");
				__builder.AddAttribute(30, "class", "stat-neo stagger-1");
				__builder.AddMarkupContent(31, "<span class=\"stat-neo-label\">Active Courses</span>\n                    ");
				__builder.OpenElement(32, "span");
				__builder.AddAttribute(33, "class", "stat-neo-value");
				__builder.AddContent(34, courses.Count);
				__builder.CloseElement();
				__builder.AddMarkupContent(35, "\n                    ");
				__builder.AddMarkupContent(36, "<div class=\"mt-4 flex items-center gap-1 font-label-caps text-label-sm\"><span class=\"material-symbols-outlined text-sm\">menu_book</span> Assigned\n                    </div>");
				__builder.CloseElement();
				__builder.AddMarkupContent(37, "\n                ");
				__builder.OpenElement(38, "div");
				__builder.AddAttribute(39, "class", "stat-neo stagger-2");
				__builder.AddMarkupContent(40, "<span class=\"stat-neo-label\">Today's Sessions</span>\n                    ");
				__builder.OpenElement(41, "span");
				__builder.AddAttribute(42, "class", "stat-neo-value");
				__builder.AddContent(43, todaySessionCount);
				__builder.CloseElement();
				__builder.AddMarkupContent(44, "\n                    ");
				__builder.OpenElement(45, "div");
				__builder.AddAttribute(46, "class", "mt-4 flex items-center gap-1 font-label-caps text-label-sm");
				__builder.OpenElement(47, "span");
				__builder.AddAttribute(48, "class", "material-symbols-outlined text-sm");
				__builder.AddContent(49, (todaySessionCount > 0) ? "play_circle" : "radio_button_unchecked");
				__builder.CloseElement();
				__builder.AddMarkupContent(50, "\n                        ");
				__builder.AddContent(51, (todaySessionCount > 0) ? "Active today" : "None yet");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(52, "\n                ");
				__builder.OpenElement(53, "div");
				__builder.AddAttribute(54, "class", "stat-neo stagger-3");
				__builder.AddMarkupContent(55, "<span class=\"stat-neo-label\">Pending Enrollments</span>\n                    ");
				__builder.OpenElement(56, "span");
				__builder.AddAttribute(57, "class", "stat-neo-value");
				__builder.AddContent(58, pendingEnrollments);
				__builder.CloseElement();
				__builder.AddMarkupContent(59, "\n                    ");
				__builder.OpenElement(60, "div");
				__builder.AddAttribute(61, "class", "mt-4 flex items-center gap-1 font-label-caps text-label-sm");
				__builder.OpenElement(62, "span");
				__builder.AddAttribute(63, "class", "material-symbols-outlined text-sm");
				__builder.AddContent(64, (pendingEnrollments > 0) ? "flag" : "check_circle");
				__builder.CloseElement();
				__builder.AddMarkupContent(65, "\n                        ");
				__builder.AddContent(66, pendingEnrollments);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(70, "\n                ");
				__builder.CloseElement();
				__builder.AddMarkupContent(88, "\n\n            ");
				__builder.OpenElement(89, "div");
				__builder.AddAttribute(90, "class", "grid grid-cols-1 lg:grid-cols-3 gap-gutter");
				__builder.OpenElement(91, "div");
				__builder.AddAttribute(92, "class", "lg:col-span-2 space-y-gutter");
				__builder.AddMarkupContent(93, "<div class=\"border-b border-outline-variant/20 pb-4\"><h2 class=\"font-headline-md text-headline-md text-on-surface\">Your Courses</h2></div>");
				if (isLoadingCourses)
				{
					__builder.AddMarkupContent(94, "<div class=\"card-neo text-center py-12\"><div class=\"spinner-ring-lg mb-2\"></div>\n                            <p class=\"font-label-sm text-on-surface-variant\">Loading courses...</p></div>");
				}
				else if (courses.Count == 0)
				{
					__builder.AddMarkupContent(95, "<div class=\"card-neo text-center py-16\"><span class=\"material-symbols-outlined text-5xl text-on-surface-variant/20 block mb-4\">menu_book</span>\n                            <p class=\"font-body-md text-on-surface-variant mb-4\">No courses assigned yet.</p>\n                            <a href=\"session\" class=\"btn-neo-primary no-underline inline-block\">Create Your First Course</a></div>");
				}
				else
				{
					foreach (CourseItem course in courses)
					{
						__builder.OpenElement(96, "div");
						__builder.AddAttribute(97, "class", "card-neo group stagger-5 " + ((expandedCourseId == course.Id) ? "border-primary" : "hover:border-primary/50"));
						__builder.OpenElement(98, "div");
						__builder.AddAttribute(99, "class", "flex justify-between items-start mb-4");
						__builder.OpenElement(100, "div");
						__builder.AddAttribute(101, "class", "flex-1");
						__builder.OpenElement(102, "div");
						__builder.AddAttribute(103, "class", "flex items-center justify-between w-full pr-4 mb-2");
						__builder.OpenElement(104, "div");
						__builder.AddAttribute(105, "class", "flex items-center gap-3");
						__builder.OpenElement(106, "span");
						__builder.AddAttribute(107, "class", "badge-neo text-[10px]");
						__builder.AddContent(108, course.CourseCode);
						__builder.CloseElement();
						__builder.CloseElement();
						if (courseToDelete == course.Id)
						{
							__builder.OpenElement(109, "div");
							__builder.AddAttribute(110, "class", "flex items-center gap-2");
							__builder.OpenElement(111, "button");
							__builder.AddAttribute(112, "class", "font-label-caps text-[9px] text-error border border-error px-2 py-0.5 hover:bg-error hover:text-on-error transition-colors cursor-pointer");
							__builder.AddAttribute(113, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => DeleteCourse(course.Id)));
							__builder.AddEventStopPropagationAttribute(114, "onclick", value: true);
							__builder.AddContent(115, "Confirm Delete");
							__builder.CloseElement();
							__builder.OpenElement(116, "button");
							__builder.AddAttribute(117, "class", "font-label-caps text-[9px] text-on-surface-variant border border-on-surface-variant/30 px-2 py-0.5 hover:bg-surface-variant transition-colors cursor-pointer");
							__builder.AddAttribute(118, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
							{
								courseToDelete = 0;
							}));
							__builder.AddEventStopPropagationAttribute(119, "onclick", value: true);
							__builder.AddContent(120, "Cancel");
							__builder.CloseElement();
							__builder.CloseElement();
						}
						else
						{
							__builder.OpenElement(121, "button");
							__builder.AddAttribute(122, "class", "text-error/60 hover:text-error transition-colors bg-transparent border-0 cursor-pointer p-1 flex items-center");
							__builder.AddAttribute(123, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
							{
								courseToDelete = course.Id;
							}));
							__builder.AddEventStopPropagationAttribute(124, "onclick", value: true);
							__builder.AddAttribute(125, "title", "Delete Course");
							__builder.AddMarkupContent(126, "<span class=\"material-symbols-outlined text-[16px]\">delete</span>");
							__builder.CloseElement();
						}
						__builder.CloseElement();
						__builder.AddMarkupContent(107, "\n                                        ");
						__builder.OpenElement(108, "h3");
						__builder.AddAttribute(109, "class", "font-headline-md text-headline-md text-on-surface group-hover:text-primary transition-colors");
						__builder.AddContent(110, course.Name);
						__builder.CloseElement();
						__builder.CloseElement();
						__builder.AddMarkupContent(111, "\n                                    ");
						__builder.OpenElement(112, "button");
						__builder.AddAttribute(113, "class", "relative material-symbols-outlined text-on-surface-variant group-hover:text-primary transition-all duration-300 bg-transparent border-0 cursor-pointer " + ((expandedCourseId == course.Id) ? "rotate-180" : ""));
						__builder.AddAttribute(114, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ToggleCourse(course.Id)));
						__builder.AddAttribute(115, "aria-label", "Toggle details");
						__builder.AddMarkupContent(116, "<span class=\"absolute inset-0 rounded-full bg-red-500/20 opacity-0 group-hover:opacity-100 transition-opacity duration-300 scale-150\"></span>\n                                        <span class=\"absolute inset-0 rounded-full border-2 border-red-500/60 opacity-0 group-hover:opacity-100 transition-opacity duration-300 scale-150\"></span>\n                                        ");
						__builder.AddMarkupContent(117, "<span class=\"relative z-10\">expand_more</span>");
						__builder.CloseElement();
						__builder.CloseElement();
						__builder.AddMarkupContent(118, "\n\n                                ");
						__builder.OpenElement(119, "div");
						__builder.AddAttribute(120, "class", "flex gap-6 font-label-caps text-label-sm text-on-surface-variant");
						__builder.OpenElement(121, "span");
						__builder.AddMarkupContent(122, "<span class=\"material-symbols-outlined text-sm align-middle mr-1\">event</span> ");
						__builder.AddContent(123, course.TotalSessions);
						__builder.AddContent(124, " sessions");
						__builder.CloseElement();
						__builder.AddMarkupContent(125, "\n                                    ");
						__builder.OpenElement(126, "span");
						__builder.AddMarkupContent(127, "<span class=\"material-symbols-outlined text-sm align-middle mr-1\">group</span> ");
						__builder.AddContent(128, course.EnrolledCount);
						__builder.AddContent(129, " students");
						__builder.CloseElement();
						if (course.LastSessionDate.HasValue)
						{
							__builder.OpenElement(130, "span");
							__builder.AddMarkupContent(131, "<span class=\"material-symbols-outlined text-sm align-middle mr-1\">schedule</span> Last: ");
							__builder.AddContent(132, course.LastSessionDate?.ToLocalTime().ToString("MMM dd") ?? "N/A");
							__builder.CloseElement();
						}
						__builder.CloseElement();
						if (expandedCourseId == course.Id)
						{
							__builder.OpenElement(133, "div");
							__builder.AddAttribute(134, "class", "mt-6 pt-6 border-t border-outline-variant/20 animate-fade-in");
							__builder.OpenElement(135, "div");
							__builder.AddAttribute(136, "class", "flex gap-2 mb-4 border-b border-outline-variant/20");
							__builder.OpenElement(137, "button");
							__builder.AddAttribute(138, "class", "font-label-caps text-label-sm px-3 py-2 border-0 bg-transparent cursor-pointer transition-colors " + ((course.ActiveTab == "sessions") ? "text-primary border-b-2 border-primary" : "text-on-surface-variant hover:text-on-surface"));
							__builder.AddAttribute(139, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
							{
								course.ActiveTab = "sessions";
							}));
							__builder.AddMarkupContent(140, "\n                                                Sessions\n                                            ");
							__builder.CloseElement();
							__builder.AddMarkupContent(141, "\n                                            ");
							__builder.OpenElement(142, "button");
							__builder.AddAttribute(143, "class", "font-label-caps text-label-sm px-3 py-2 border-0 bg-transparent cursor-pointer transition-colors " + ((course.ActiveTab == "roster") ? "text-primary border-b-2 border-primary" : "text-on-surface-variant hover:text-on-surface"));
							__builder.AddAttribute(144, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => LoadRoster(course)));
							__builder.AddMarkupContent(145, "\n                                                Roster\n                                            ");
							__builder.CloseElement();
							__builder.AddMarkupContent(146, "\n                                            ");
							__builder.OpenElement(147, "button");
							__builder.AddAttribute(148, "class", "font-label-caps text-label-sm px-3 py-2 border-0 bg-transparent cursor-pointer transition-colors " + ((course.ActiveTab == "abuse") ? "text-primary border-b-2 border-primary" : "text-on-surface-variant hover:text-on-surface"));
							__builder.AddAttribute(149, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => LoadAbuseLogs(course)));
							__builder.AddMarkupContent(150, "\n                                                Abuse Logs\n                                            ");
							__builder.CloseElement();
							__builder.AddMarkupContent(151, "\n                                            ");
							__builder.OpenElement(152, "button");
							__builder.AddAttribute(153, "class", "font-label-caps text-label-sm px-3 py-2 border-0 bg-transparent cursor-pointer transition-colors " + ((course.ActiveTab == "attendance") ? "text-primary border-b-2 border-primary" : "text-on-surface-variant hover:text-on-surface"));
							__builder.AddAttribute(154, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => LoadAttendanceView(course)));
							__builder.AddMarkupContent(155, "\n                                                Attendance\n                                            ");
							__builder.CloseElement();
							__builder.CloseElement();
							if (course.ActiveTab == "sessions")
							{
								if (course.SessionHistory == null)
								{
									__builder.AddMarkupContent(161, "<div class=\"text-center py-4\"><div class=\"spinner-ring-sm mb-2\"></div>\n                                                    <span class=\"font-label-sm text-on-surface-variant block\">Loading sessions...</span></div>");
								}
								else if (course.SessionHistory.Count == 0)
								{
									__builder.AddMarkupContent(162, "<p class=\"font-label-sm text-on-surface-variant text-center py-4\">No sessions held yet for this course.</p>");
								}
								else
								{
									__builder.OpenElement(163, "div");
									__builder.AddAttribute(164, "class", "space-y-3");
									foreach (SessionHistoryItem session in course.SessionHistory.OrderByDescending((SessionHistoryItem sessionHistoryItem) => sessionHistoryItem.StartTime))
									{
										__builder.OpenElement(165, "div");
										__builder.AddAttribute(166, "class", "border border-outline-variant/20 p-4 " + ((sessionToDelete == session.SessionId) ? "border-primary border-2" : ""));
										__builder.OpenElement(167, "div");
										__builder.AddAttribute(168, "class", "flex justify-between items-center");
										__builder.OpenElement(169, "div");
										__builder.OpenElement(170, "span");
										__builder.AddAttribute(171, "class", "font-label-caps text-label-caps text-on-surface");
										__builder.AddContent(172, "Session #");
										__builder.AddContent(173, session.SessionId);
										__builder.CloseElement();
										__builder.AddMarkupContent(174, "\n                                                                    ");
										__builder.OpenElement(175, "span");
										__builder.AddAttribute(176, "class", "font-label-sm text-on-surface-variant ml-3");
										__builder.AddContent(177, session.StartTime.ToLocalTime().ToString("MMM dd, yyyy · hh:mm tt"));
										__builder.CloseElement();
										if (session.IsActive)
										{
											__builder.AddMarkupContent(178, "<span class=\"badge-neo badge-neo-active text-[10px] ml-2\">Active</span>");
										}
										__builder.CloseElement();
										__builder.CloseElement();
										__builder.AddMarkupContent(179, "\n                                                            ");
										__builder.OpenElement(180, "div");
										__builder.AddAttribute(181, "class", "flex gap-2 mt-3");
										__builder.OpenElement(182, "button");
										__builder.AddAttribute(183, "class", "font-label-caps text-[10px] text-on-surface-variant border border-on-surface-variant/30 px-2 py-1 hover:border-primary hover:text-primary transition-colors cursor-pointer");
										__builder.AddAttribute(184, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => ToggleManualOverride(session.SessionId)));
										__builder.AddEventStopPropagationAttribute(185, "onclick", value: true);
										__builder.AddMarkupContent(186, "<span class=\"material-symbols-outlined text-[14px] align-middle mr-1\">edit_note</span>Manual Override\n                                                                ");
										__builder.CloseElement();
										if (sessionToDelete == session.SessionId)
										{
											__builder.OpenElement(187, "button");
											__builder.AddAttribute(188, "class", "font-label-caps text-[10px] text-error border border-error px-2 py-1 hover:bg-error hover:text-on-error transition-colors cursor-pointer");
											__builder.AddAttribute(189, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => DeleteSession(session.SessionId)));
											__builder.AddEventStopPropagationAttribute(190, "onclick", value: true);
											__builder.AddMarkupContent(191, "<span class=\"material-symbols-outlined text-[14px] align-middle mr-1\">delete_forever</span>Confirm Delete\n                                                                    ");
											__builder.CloseElement();
											__builder.AddMarkupContent(192, "\n                                                                    ");
											__builder.OpenElement(193, "button");
											__builder.AddAttribute(194, "class", "font-label-caps text-[10px] text-on-surface-variant border border-on-surface-variant/30 px-2 py-1 hover:bg-surface-variant transition-colors cursor-pointer");
											__builder.AddAttribute(195, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
											{
												sessionToDelete = 0;
											}));
											__builder.AddEventStopPropagationAttribute(196, "onclick", value: true);
											__builder.AddContent(197, "Cancel");
											__builder.CloseElement();
										}
										else
										{
											__builder.OpenElement(198, "button");
											__builder.AddAttribute(199, "class", "font-label-caps text-[10px] text-on-surface-variant/50 border border-on-surface-variant/20 px-2 py-1 hover:border-error hover:text-error transition-colors cursor-pointer");
											__builder.AddAttribute(200, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
											{
												sessionToDelete = session.SessionId;
											}));
											__builder.AddEventStopPropagationAttribute(201, "onclick", value: true);
											__builder.AddMarkupContent(202, "<span class=\"material-symbols-outlined text-[14px] align-middle mr-1\">delete</span>Delete\n                                                                    ");
											__builder.CloseElement();
										}
										__builder.CloseElement();
										if (overrideSessionId == session.SessionId)
										{
											__builder.OpenElement(203, "div");
											__builder.AddAttribute(204, "class", "mt-4 pt-4 border-t border-outline-variant/20");
											__builder.AddMarkupContent(205, "<h4 class=\"font-label-caps text-label-caps text-on-surface mb-3\">Manual Override — Select Present Students</h4>");
											if (overrideStudents.Count == 0)
											{
												__builder.AddMarkupContent(206, "<p class=\"font-label-sm text-on-surface-variant\">Loading enrolled students...</p>");
											}
											else
											{
												__builder.OpenElement(207, "div");
												__builder.AddAttribute(208, "class", "max-h-48 overflow-y-auto space-y-1 mb-4");
												foreach (EnrolledStudent s in overrideStudents.OrderBy((EnrolledStudent enrolledStudent) => enrolledStudent.RollNumber))
												{
													__builder.OpenElement(209, "label");
													__builder.AddAttribute(210, "class", "flex items-center gap-2 py-1 cursor-pointer hover:bg-surface-container-low px-2");
													__builder.OpenElement(211, "input");
													__builder.AddAttribute(212, "type", "checkbox");
													__builder.AddAttribute(213, "checked", overrideSelectedIds.Contains(s.Id));
													__builder.AddAttribute(214, "onchange", EventCallback.Factory.Create(this, delegate(ChangeEventArgs e)
													{
														ToggleOverrideStudent(s.Id, (bool)(e.Value ?? ((object)false)));
													}));
													__builder.CloseElement();
													__builder.AddMarkupContent(215, "\n                                                                                    ");
													__builder.OpenElement(216, "span");
													__builder.AddAttribute(217, "class", "font-mono text-[11px] text-on-surface-variant w-20");
													__builder.AddContent(218, s.RollNumber);
													__builder.CloseElement();
													__builder.AddMarkupContent(219, "\n                                                                                    ");
													__builder.OpenElement(220, "span");
													__builder.AddAttribute(221, "class", "font-body-md text-sm text-on-surface");
													__builder.AddContent(222, s.FullName);
													__builder.CloseElement();
													__builder.CloseElement();
												}
												__builder.CloseElement();
												__builder.AddMarkupContent(223, "\n                                                                        ");
												__builder.OpenElement(224, "div");
												__builder.AddAttribute(225, "class", "flex gap-2");
												__builder.OpenElement(226, "button");
												__builder.AddAttribute(227, "class", "btn-neo-primary text-sm flex-1");
												__builder.AddAttribute(228, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => SaveOverride(session.SessionId)));
												__builder.AddEventStopPropagationAttribute(229, "onclick", value: true);
												__builder.AddAttribute(230, "disabled", isSavingOverride);
												__builder.AddContent(231, isSavingOverride ? "Saving..." : "Save Attendance");
												__builder.CloseElement();
												__builder.AddMarkupContent(232, "\n                                                                            ");
												__builder.OpenElement(233, "button");
												__builder.AddAttribute(234, "class", "btn-neo-outline text-sm");
												__builder.AddAttribute(235, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
												{
													overrideSessionId = 0;
												}));
												__builder.AddEventStopPropagationAttribute(236, "onclick", value: true);
												__builder.AddContent(237, "Cancel");
												__builder.CloseElement();
												__builder.CloseElement();
											}
											__builder.CloseElement();
										}
										__builder.CloseElement();
									}
									__builder.CloseElement();
								}
							}
							if (course.ActiveTab == "roster")
							{
								if (course.Roster == null)
								{
									__builder.AddMarkupContent(238, "<div class=\"text-center py-4\"><div class=\"spinner-ring-sm mb-2\"></div>\n                                                    <span class=\"font-label-sm text-on-surface-variant block\">Loading roster...</span></div>");
								}
								else if (course.Roster.Count == 0)
								{
									__builder.AddMarkupContent(239, "<p class=\"font-label-sm text-on-surface-variant text-center py-4\">No students enrolled in this course.</p>");
								}
								else
								{
									__builder.OpenElement(240, "div");
									__builder.AddAttribute(241, "class", "overflow-x-auto");
									__builder.OpenElement(242, "table");
									__builder.AddAttribute(243, "class", "table-neo text-sm");
									__builder.AddMarkupContent(244, "<thead><tr><th>Roll No.</th>\n                                                                <th>Name</th></tr></thead>\n                                                        ");
									__builder.OpenElement(245, "tbody");
									foreach (RosterStudent item in course.Roster.OrderBy((RosterStudent rosterStudent) => rosterStudent.RollNumber))
									{
										__builder.OpenElement(246, "tr");
										__builder.OpenElement(247, "td");
										__builder.AddAttribute(248, "class", "font-mono text-[11px]");
										__builder.AddContent(249, item.RollNumber);
										__builder.CloseElement();
										__builder.AddMarkupContent(250, "\n                                                                    ");
										__builder.OpenElement(251, "td");
										__builder.AddContent(252, item.StudentName);
										__builder.CloseElement();
										__builder.CloseElement();
									}
									__builder.CloseElement();
									__builder.CloseElement();
									__builder.CloseElement();
								}
							}
							if (course.ActiveTab == "abuse")
							{
								if (course.AbuseLogs == null)
								{
									__builder.AddMarkupContent(253, "<div class=\"text-center py-4\"><div class=\"spinner-ring-sm mb-2\"></div>\n                                                    <span class=\"font-label-sm text-on-surface-variant block\">Loading abuse logs...</span></div>");
								}
								else if (course.AbuseLogs.Count == 0)
								{
									__builder.AddMarkupContent(254, "<p class=\"font-label-sm text-on-surface-variant text-center py-4\">No security flags for this course.</p>");
								}
								else
								{
									__builder.OpenElement(255, "div");
									__builder.AddAttribute(256, "class", "overflow-x-auto");
									__builder.OpenElement(257, "table");
									__builder.AddAttribute(258, "class", "table-neo text-xs");
									__builder.AddMarkupContent(259, "<thead><tr><th>Student</th>\n                                                                <th>Roll No.</th>\n                                                                <th>Type</th>\n                                                                <th>Details</th>\n                                                                <th class=\"text-right\">Time</th></tr></thead>\n                                                        ");
									__builder.OpenElement(260, "tbody");
									foreach (AbuseLogItem abuseLog in course.AbuseLogs)
									{
										__builder.OpenElement(261, "tr");
										__builder.OpenElement(262, "td");
										__builder.AddContent(263, abuseLog.StudentName);
										__builder.CloseElement();
										__builder.AddMarkupContent(264, "\n                                                                    ");
										__builder.OpenElement(265, "td");
										__builder.AddAttribute(266, "class", "font-mono text-[11px]");
										__builder.AddContent(267, abuseLog.RollNumber);
										__builder.CloseElement();
										__builder.AddMarkupContent(268, "\n                                                                    ");
										__builder.OpenElement(269, "td");
										__builder.OpenElement(270, "span");
										__builder.AddAttribute(271, "class", "badge-neo text-[10px]");
										__builder.AddContent(272, abuseLog.AbuseType);
										__builder.CloseElement();
										__builder.CloseElement();
										__builder.AddMarkupContent(273, "\n                                                                    ");
										__builder.OpenElement(274, "td");
										__builder.AddAttribute(275, "class", "text-on-surface-variant max-w-[200px] truncate");
										__builder.AddContent(276, abuseLog.Details);
										__builder.CloseElement();
										__builder.AddMarkupContent(277, "\n                                                                    ");
										__builder.OpenElement(278, "td");
										__builder.AddAttribute(279, "class", "text-right text-on-surface-variant");
										__builder.AddContent(280, abuseLog.LoggedAt.ToLocalTime().ToString("MMM dd, hh:mm tt"));
										__builder.CloseElement();
										__builder.CloseElement();
									}
									__builder.CloseElement();
									__builder.CloseElement();
									__builder.CloseElement();
								}
							}
							if (course.ActiveTab == "attendance")
							{
								if (course.AttendanceData == null)
								{
									__builder.AddMarkupContent(281, "<div class=\"text-center py-4\"><div class=\"spinner-ring-sm mb-2\"></div>\n                                                    <span class=\"font-label-sm text-on-surface-variant block\">Loading attendance...</span></div>");
								}
								else if (course.AttendanceData.Count == 0)
								{
									__builder.AddMarkupContent(282, "<p class=\"font-label-sm text-on-surface-variant text-center py-4\">No attendance data for this course.</p>");
								}
								else
								{
									__builder.OpenElement(283, "div");
									__builder.AddAttribute(284, "class", "overflow-x-auto");
									__builder.OpenElement(285, "table");
									__builder.AddAttribute(286, "class", "table-neo text-xs w-full");
									__builder.OpenElement(287, "thead");
									__builder.OpenElement(288, "tr");
									__builder.AddMarkupContent(289, "<th>Student</th>\n                                                                ");
									__builder.AddMarkupContent(290, "<th>Roll No.</th>");
									foreach (SessionMark session2 in course.AttendanceData.First().Sessions)
									{
										__builder.OpenElement(291, "th");
										__builder.AddAttribute(292, "class", "text-center text-[10px]");
										__builder.AddContent(293, "S#");
										__builder.AddContent(294, session2.SessionId);
										__builder.CloseElement();
									}
									__builder.AddMarkupContent(295, "<th class=\"text-center\">Present</th>\n                                                                ");
									__builder.AddMarkupContent(296, "<th class=\"text-center\">Rate</th>");
									__builder.CloseElement();
									__builder.CloseElement();
									__builder.AddMarkupContent(297, "\n                                                        ");
									__builder.OpenElement(298, "tbody");
									foreach (AttendanceRow attendanceDatum in course.AttendanceData)
									{
										__builder.OpenElement(299, "tr");
										__builder.OpenElement(300, "td");
										__builder.AddAttribute(301, "class", "font-label-caps text-xs");
										__builder.AddContent(302, attendanceDatum.StudentName);
										__builder.CloseElement();
										__builder.AddMarkupContent(303, "\n                                                                    ");
										__builder.OpenElement(304, "td");
										__builder.AddAttribute(305, "class", "font-mono text-[11px]");
										__builder.AddContent(306, attendanceDatum.RollNumber);
										__builder.CloseElement();
										foreach (SessionMark session3 in attendanceDatum.Sessions)
										{
											__builder.OpenElement(307, "td");
											__builder.AddAttribute(308, "class", "text-center");
											__builder.OpenElement(309, "span");
											__builder.AddAttribute(310, "class", (session3.IsPresent ? "text-tertiary" : "text-primary") + " font-bold");
											__builder.AddContent(311, session3.IsPresent ? "P" : "A");
											__builder.CloseElement();
											__builder.CloseElement();
										}
										__builder.OpenElement(312, "td");
										__builder.AddAttribute(313, "class", "text-center font-bold");
										__builder.AddContent(314, attendanceDatum.PresentCount);
										__builder.CloseElement();
										__builder.AddMarkupContent(315, "\n                                                                    ");
										__builder.OpenElement(316, "td");
										__builder.AddAttribute(317, "class", "text-center font-bold " + ((attendanceDatum.AttendanceRate >= 75.0) ? "text-tertiary" : "text-primary"));
										__builder.AddContent(318, $"{attendanceDatum.AttendanceRate:F0}%");
										__builder.CloseElement();
										__builder.CloseElement();
									}
									__builder.CloseElement();
									__builder.CloseElement();
									__builder.CloseElement();
								}
							}
							__builder.CloseElement();
						}
						__builder.CloseElement();
					}
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(341, "\n\n                ");
				__builder.OpenElement(342, "div");
				__builder.AddAttribute(343, "class", "space-y-gutter");
				__builder.AddMarkupContent(370, "\n\n                    ");
				__builder.OpenElement(371, "button");
				__builder.AddAttribute(372, "class", "card-neo stagger-6 w-full text-left no-underline hover:border-primary transition-colors group cursor-pointer " + (pendingAppealCount > 0 ? "border-2 border-primary" : ""));
				__builder.AddAttribute(373, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)LoadAppeals));
				__builder.OpenElement(374, "div");
				__builder.AddAttribute(375, "class", "flex justify-between items-center");
				__builder.AddMarkupContent(376, "<div class=\"flex items-center gap-3\"><span class=\"material-symbols-outlined text-2xl text-primary group-hover:scale-110 transition-transform\">rate_review</span>\n                                <div><h3 class=\"font-label-caps text-label-caps text-on-surface\">Attendance Appeals</h3>\n                                    <p class=\"font-label-sm text-on-surface-variant\">Review student appeals</p></div></div>\n                            ");
				__builder.OpenElement(377, "span");
				__builder.AddAttribute(378, "class", "badge-neo " + ((pendingAppealCount > 0) ? "badge-neo-active" : ""));
				__builder.AddContent(379, pendingAppealCount);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(380, "\n\n                    ");
				__builder.OpenElement(381, "a");
				__builder.AddAttribute(382, "href", "enrollment-review");
				__builder.AddAttribute(383, "class", "card-neo no-underline block w-full text-left " + (pendingEnrollments > 0 ? "border-2 border-primary" : ""));
				__builder.OpenElement(384, "div");
				__builder.AddAttribute(385, "class", "flex justify-between items-center");
				__builder.AddMarkupContent(386, "<div class=\"flex items-center gap-3\"><span class=\"material-symbols-outlined text-2xl text-primary group-hover:scale-110 transition-transform\">person_add</span>\n                                <div><h3 class=\"font-label-caps text-label-caps text-on-surface\">Enrollment Requests</h3>\n                                    <p class=\"font-label-sm text-on-surface-variant\">Review student enrollments</p></div></div>\n                            ");
				__builder.OpenElement(387, "span");
				__builder.AddAttribute(388, "class", "badge-neo " + ((pendingEnrollments > 0) ? "badge-neo-active" : ""));
				__builder.AddContent(389, pendingEnrollments);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
					__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			if (!showAppealsModal)
			{
				return;
			}
			__builder.OpenElement(402, "div");
			__builder.AddAttribute(403, "class", "fixed inset-0 z-50 flex items-center justify-center bg-black/30");
			__builder.AddAttribute(404, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
			{
				showAppealsModal = false;
			}));
			__builder.OpenElement(405, "div");
			__builder.AddAttribute(406, "class", "card-neo-raised p-6 max-w-2xl w-full mx-4 max-h-[80vh] overflow-y-auto bg-surface");
			__builder.AddEventStopPropagationAttribute(407, "onclick", value: true);
			__builder.OpenElement(408, "div");
			__builder.AddAttribute(409, "class", "flex justify-between items-center mb-4");
			__builder.AddMarkupContent(410, "<h3 class=\"font-headline-md text-headline-md text-on-surface\">Attendance Appeals</h3>\n                ");
			__builder.OpenElement(411, "button");
			__builder.AddAttribute(412, "class", "material-symbols-outlined text-on-surface-variant hover:text-primary cursor-pointer bg-transparent border-0");
			__builder.AddAttribute(413, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
			{
				showAppealsModal = false;
			}));
			__builder.AddContent(414, "close");
			__builder.CloseElement();
			__builder.CloseElement();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					__builder.AddMarkupContent(415, "<p class=\"font-body-md text-error text-center py-8\">" + errorMessage + "</p>");
				}
				else
			if (appealsList.Count == 0)
			{
				__builder.AddMarkupContent(415, "<p class=\"font-body-md text-on-surface-variant text-center py-8\">No appeals found.</p>");
			}
			else
			{
				__builder.OpenElement(416, "div");
				__builder.AddAttribute(417, "class", "space-y-4");
				foreach (AppealItem appeal in appealsList)
				{
					__builder.OpenElement(418, "div");
					__builder.AddAttribute(419, "class", "border border-outline-variant/20 p-4");
					__builder.OpenElement(420, "div");
					__builder.AddAttribute(421, "class", "flex justify-between items-start mb-2");
					__builder.OpenElement(422, "div");
					__builder.OpenElement(423, "span");
					__builder.AddAttribute(424, "class", "font-label-caps text-label-caps text-on-surface");
					__builder.AddContent(425, appeal.StudentName);
					__builder.CloseElement();
					__builder.AddMarkupContent(426, "\n                                    ");
					__builder.OpenElement(427, "span");
					__builder.AddAttribute(428, "class", "font-label-sm text-on-surface-variant ml-3");
					__builder.AddContent(429, appeal.RollNumber);
					__builder.CloseElement();
					__builder.AddMarkupContent(430, "\n                                    ");
					__builder.OpenElement(431, "span");
					__builder.AddAttribute(432, "class", "font-label-sm text-on-surface-variant ml-3");
					__builder.AddContent(433, appeal.CourseCode);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(434, "\n                                ");
					__builder.OpenElement(435, "span");
					__builder.AddAttribute(436, "class", "badge-neo " + ((appeal.Status == "Pending") ? "badge-neo-pending" : ((appeal.Status == "Approved") ? "badge-neo-success" : "")) + " text-[10px]");
					__builder.AddContent(437, appeal.Status);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(438, "\n                            ");
					__builder.OpenElement(439, "p");
					__builder.AddAttribute(440, "class", "font-body-md text-sm text-on-surface-variant mb-2");
					__builder.AddContent(441, appeal.Reason);
					__builder.CloseElement();
					__builder.AddMarkupContent(442, "\n                            ");
					__builder.OpenElement(443, "p");
					__builder.AddAttribute(444, "class", "font-label-sm text-on-surface-variant/60 mb-3");
					__builder.AddContent(445, "Session: ");
					__builder.AddContent(446, appeal.SessionDate.ToLocalTime().ToString("MMM dd, yyyy · hh:mm tt"));
					__builder.CloseElement();
					if (appeal.Status == "Pending")
					{
						__builder.OpenElement(447, "div");
						__builder.AddAttribute(448, "class", "flex gap-2");
						__builder.OpenElement(449, "button");
						__builder.AddAttribute(450, "class", "btn-neo-primary text-sm px-4 py-1");
						__builder.AddAttribute(451, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => HandleAppeal(appeal.Id, approve: true)));
						__builder.AddContent(452, "Approve");
						__builder.CloseElement();
						__builder.AddMarkupContent(453, "\n                                    ");
						__builder.OpenElement(454, "button");
						__builder.AddAttribute(455, "class", "btn-neo-danger text-sm px-4 py-1");
						__builder.AddAttribute(456, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => HandleAppeal(appeal.Id, approve: false)));
						__builder.AddContent(457, "Reject");
						__builder.CloseElement();
						__builder.CloseElement();
					}
					__builder.CloseElement();
				}
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
			jwtToken = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			if (string.IsNullOrEmpty(jwtToken) || jwtToken == "null")
			{
				Nav.NavigateTo("/login");
				return;
			}
			isAuthorized = true;
			StateHasChanged();
			await LoadAllData();
		}

		private async Task LoadAllData()
		{
			isLoadingCourses = true;
			errorMessage = null;
			isProfileMissing = false;
			courses = new List<CourseItem>();
			todaySessionCount = 0;
			pendingEnrollments = 0;
			pendingAppealCount = 0;
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/attendance/professor/courses");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					isProfileMissing = false;
					using (JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync()))
					{
						JsonElement property = jsonDocument.RootElement.GetProperty("data");
						courses = new List<CourseItem>();
						foreach (JsonElement item2 in property.EnumerateArray())
						{
							CourseItem item = new CourseItem
							{
								Id = item2.GetProperty("id").GetInt32(),
								Name = (item2.GetProperty("name").GetString() ?? ""),
								CourseCode = (item2.GetProperty("courseCode").GetString() ?? "")
							};
							courses.Add(item);
						}
					}
					DateTime today = DateTime.UtcNow.Date;
					Task<int>[] courseTasks = courses.Select((CourseItem course) => LoadCourseSummaryAsync(course, jwtToken, today)).ToArray();
					Task<int> pendingEnrollmentsTask = CountArrayItemsAsync(jwtToken, "api/courses/enrollment-requests/pending");
					Task<int> pendingAppealsTask = CountArrayItemsAsync(jwtToken, "api/professor/appeals/pending");
					Task[] allTasks = courseTasks.Cast<Task>().Concat(new Task[2] { pendingEnrollmentsTask, pendingAppealsTask }).ToArray();
					await Task.WhenAll(allTasks);
					todaySessionCount = courseTasks.Sum((Task<int> task) => task.Result);
					pendingEnrollments = pendingEnrollmentsTask.Result;
					pendingAppealCount = pendingAppealsTask.Result;
				}
				else if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
				{
					isProfileMissing = true;
					isLoadingCourses = false;
				}
				else
				{
					errorMessage = $"Failed to load courses. Status: {httpResponseMessage.StatusCode}";
					isLoadingCourses = false;
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Connection error: " + ex.Message;
			}
			finally
			{
				isLoadingCourses = false;
				StateHasChanged();
			}
		}


		private async Task ToggleCourse(int courseId)
		{
			if (expandedCourseId == courseId)
			{
				expandedCourseId = 0;
				return;
			}
			expandedCourseId = courseId;
			CourseItem course = courses.FirstOrDefault((CourseItem c) => c.Id == courseId);
			if (course == null || course.SessionHistory != null)
			{
				return;
			}
			await LoadCourseSessions(course);
		}

		private async Task LoadCourseSessions(CourseItem course)
		{
			course.SessionHistory = new List<SessionHistoryItem>();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/professor/courses/{course.Id}/sessions");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				foreach (ProfessorSessionDto item in JsonSerializer.Deserialize<List<ProfessorSessionDto>>(jsonDocument.RootElement.GetProperty("data").GetRawText(), new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				}) ?? new List<ProfessorSessionDto>())
				{
					course.SessionHistory.Add(new SessionHistoryItem
					{
						SessionId = item.SessionId,
						StartTime = item.StartTime,
						EndTime = item.EndTime,
						IsActive = item.IsActive
					});
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading sessions: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private async Task<int> LoadCourseSummaryAsync(CourseItem course, string token, DateTime today)
		{
			HttpRequestMessage sessionsRequest = new HttpRequestMessage(HttpMethod.Get, $"api/professor/courses/{course.Id}/sessions");
			sessionsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			HttpRequestMessage enrolledRequest = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/courses/{course.Id}/enrolled-students");
			enrolledRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			Task<HttpResponseMessage> sessionsTask = Http.SendAsync(sessionsRequest);
			Task<HttpResponseMessage> enrolledTask = Http.SendAsync(enrolledRequest);
			await Task.WhenAll(sessionsTask, enrolledTask);
			int todayCount = 0;
			if (sessionsTask.Result.IsSuccessStatusCode)
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(await sessionsTask.Result.Content.ReadAsStringAsync());
				List<ProfessorSessionDto> list = JsonSerializer.Deserialize<List<ProfessorSessionDto>>(jsonDocument.RootElement.GetProperty("data").GetRawText(), new JsonSerializerOptions
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				}) ?? new List<ProfessorSessionDto>();
				course.TotalSessions = list.Count;
				course.LastSessionDate = ((list.Count > 0) ? new DateTime?(list.Max((ProfessorSessionDto s) => s.StartTime)) : ((DateTime?)null));
				todayCount = list.Count((ProfessorSessionDto s) => s.StartTime.Date == today);
			}
			if (enrolledTask.Result.IsSuccessStatusCode)
			{
				using JsonDocument jsonDocument2 = JsonDocument.Parse(await enrolledTask.Result.Content.ReadAsStringAsync());
				course.EnrolledCount = jsonDocument2.RootElement.GetProperty("data").EnumerateArray().Count();
			}
			return todayCount;
		}

		private async Task<int> CountArrayItemsAsync(string token, string url)
		{
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return 0;
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
			return jsonDocument.RootElement.GetProperty("data").EnumerateArray().Count();
		}

		private async Task ReloadDashboard(bool preserveExpandedCourse = false)
		{
			int preservedExpandedCourseId = preserveExpandedCourse ? expandedCourseId : 0;
			string preservedActiveTab = "sessions";
			if (preserveExpandedCourse)
			{
				CourseItem course = courses.FirstOrDefault((CourseItem c) => c.Id == preservedExpandedCourseId);
				if (course != null)
				{
					preservedActiveTab = course.ActiveTab;
				}
			}
			await LoadAllData();
			if (!preserveExpandedCourse || preservedExpandedCourseId == 0)
			{
				return;
			}
			CourseItem refreshedCourse = courses.FirstOrDefault((CourseItem c) => c.Id == preservedExpandedCourseId);
			if (refreshedCourse == null)
			{
				expandedCourseId = 0;
				return;
			}
			expandedCourseId = preservedExpandedCourseId;
			refreshedCourse.ActiveTab = preservedActiveTab;
			refreshedCourse.SessionHistory = null;
			refreshedCourse.Roster = null;
			refreshedCourse.AbuseLogs = null;
			refreshedCourse.AttendanceData = null;
			switch (preservedActiveTab)
			{
				case "roster":
					await LoadRoster(refreshedCourse);
					break;
				case "abuse":
					await LoadAbuseLogs(refreshedCourse);
					break;
				case "attendance":
					await LoadAttendanceView(refreshedCourse);
					break;
				default:
					await LoadCourseSessions(refreshedCourse);
					break;
			}
		}

		private async Task LoadRoster(CourseItem course)
		{
			course.ActiveTab = "roster";
			if (course.Roster != null)
			{
				return;
			}
			course.Roster = new List<RosterStudent>();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/courses/{course.Id}/enrolled-students");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				foreach (JsonElement item in jsonDocument.RootElement.GetProperty("data").EnumerateArray())
				{
					course.Roster.Add(new RosterStudent
					{
						StudentId = item.GetProperty("id").GetInt32(),
						StudentName = (item.GetProperty("fullName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? "")
					});
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading roster: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private async Task LoadAbuseLogs(CourseItem course)
		{
			course.ActiveTab = "abuse";
			if (course.AbuseLogs != null)
			{
				return;
			}
			course.AbuseLogs = new List<AbuseLogItem>();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/professor/courses/{course.Id}/abuselogs");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				foreach (JsonElement item in jsonDocument.RootElement.GetProperty("data").EnumerateArray())
				{
					course.AbuseLogs.Add(new AbuseLogItem
					{
						Id = item.GetProperty("id").GetInt32(),
						StudentName = (item.GetProperty("studentName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? ""),
						AbuseType = (item.GetProperty("abuseType").GetString() ?? ""),
						Details = (item.GetProperty("details").GetString() ?? ""),
						LoggedAt = item.GetProperty("loggedAt").GetDateTime()
					});
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading abuse logs: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private async Task LoadAttendanceView(CourseItem course)
		{
			course.ActiveTab = "attendance";
			if (course.AttendanceData != null)
			{
				return;
			}
			course.AttendanceData = new List<AttendanceRow>();
			StateHasChanged();
			try
			{
				string parameter = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/professor/courses/{course.Id}/export");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				var csv = (await httpResponseMessage.Content.ReadAsStringAsync()).Replace("\r", "");
		string[] array = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
				if (array.Length < 2)
				{
					return;
				}
				string[] array2 = array[0].Split(',');
				List<(int, string)> list = new List<(int, string)>();
				for (int i = 3; i < array2.Length - 3; i++)
				{
					string text = array2[i].Trim('"');
					int num = text.IndexOf('#');
					int num2 = text.IndexOf('(');
					if (num >= 0 && num2 > num && int.TryParse(text.Substring(num + 1, num2 - num - 1).Trim(), out var result))
					{
						list.Add((result, text));
					}
				}
				for (int j = 1; j < array.Length; j++)
				{
					string[] array3 = array[j].Split(',');
					if (array3.Length < 4)
					{
						continue;
					}
					AttendanceRow attendanceRow = new AttendanceRow
					{
						StudentName = array3[0].Trim('"'),
						RollNumber = array3[1].Trim('"')
					};
					int num3 = 0;
					for (int k = 0; k < list.Count; k++)
					{
						int num4 = 3 + k;
						bool flag = num4 < array3.Length && array3[num4].Trim('"', ' ', '\r') == "P";
						attendanceRow.Sessions.Add(new SessionMark
						{
							SessionId = list[k].Item1,
							IsPresent = flag
						});
						if (flag)
						{
							num3++;
						}
					}
					int count = list.Count;
					attendanceRow.PresentCount = num3;
					attendanceRow.AttendanceRate = ((count > 0) ? Math.Round((double)num3 / (double)count * 100.0, 1) : 0.0);
					course.AttendanceData.Add(attendanceRow);
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading attendance: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private async Task DeleteSession(int sessionId)
		{
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/attendance/sessions/{sessionId}");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					sessionToDelete = 0;
					await ReloadDashboard(preserveExpandedCourse: true);
				}
				else
				{
					errorMessage = "Failed to delete session: " + await httpResponseMessage.Content.ReadAsStringAsync();
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error deleting session: " + ex.Message;
			}
		}

		private async Task ToggleManualOverride(int sessionId)
		{
			if (overrideSessionId == sessionId)
			{
				overrideSessionId = 0;
				return;
			}
			overrideSessionId = sessionId;
			overrideSelectedIds.Clear();
			overrideStudents.Clear();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/courses/{expandedCourseId}/enrolled-students");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument doc = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				foreach (JsonElement item in doc.RootElement.GetProperty("data").EnumerateArray())
				{
					overrideStudents.Add(new EnrolledStudent
					{
						Id = item.GetProperty("id").GetInt32(),
						FullName = (item.GetProperty("fullName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? "")
					});
				}
				HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/sessions/{sessionId}/records");
				httpRequestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage2 = await Http.SendAsync(httpRequestMessage2);
				if (!httpResponseMessage2.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage2.Content.ReadAsStringAsync());
				foreach (JsonElement item2 in jsonDocument.RootElement.GetProperty("data").EnumerateArray())
				{
					overrideSelectedIds.Add(item2.GetProperty("studentId").GetInt32());
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading students: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private void ToggleOverrideStudent(int studentId, bool selected)
		{
			if (selected)
			{
				overrideSelectedIds.Add(studentId);
			}
			else
			{
				overrideSelectedIds.Remove(studentId);
			}
		}

		private async Task SaveOverride(int sessionId)
		{
			isSavingOverride = true;
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"api/attendance/sessions/{sessionId}/records");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					presentStudentIds = overrideSelectedIds.ToList()
				});
				if ((await Http.SendAsync(httpRequestMessage)).IsSuccessStatusCode)
				{
					overrideSessionId = 0;
					await ReloadDashboard(preserveExpandedCourse: true);
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error saving: " + ex.Message;
			}
			finally
			{
				isSavingOverride = false;
				StateHasChanged();
			}
		}

		private async Task LoadAppeals()
		{
			showAppealsModal = true;
			appealsList.Clear();
			StateHasChanged();
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/professor/appeals/pending");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				foreach (JsonElement item in jsonDocument.RootElement.GetProperty("data").EnumerateArray())
				{
					appealsList.Add(new AppealItem
					{
						Id = item.GetProperty("id").GetInt32(),
						StudentName = (item.GetProperty("studentName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? ""),
						CourseCode = (item.GetProperty("courseName").GetString() ?? ""),
						Reason = (item.GetProperty("reason").GetString() ?? ""),
						Status = (item.GetProperty("status").GetString() ?? ""),
						SessionDate = item.GetProperty("sessionDate").GetDateTime()
					});
				}
				pendingAppealCount = appealsList.Count((AppealItem a) => a.Status == "Pending");
			}
			catch (Exception ex)
			{
				errorMessage = "Error loading appeals: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}

		private async Task HandleAppeal(int appealId, bool approve)
		{
			_ = 1;
			try
			{
				string requestUri = (approve ? $"api/professor/appeals/{appealId}/approve" : $"api/professor/appeals/{appealId}/reject");
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				if (!approve)
				{
					httpRequestMessage.Content = JsonContent.Create(new
					{
						note = "Rejected by professor."
					});
				}
				if ((await Http.SendAsync(httpRequestMessage)).IsSuccessStatusCode)
				{
					await LoadAppeals();
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
		}

		private async Task DeleteCourse(int courseId)
		{
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/courses/{courseId}");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				var response = await Http.SendAsync(httpRequestMessage);
				if (response.IsSuccessStatusCode)
				{
					courseToDelete = 0;
					expandedCourseId = 0;
					await ReloadDashboard();
				}
				else
				{
					errorMessage = "Failed to delete course: " + response.StatusCode;
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error deleting course: " + ex.Message;
			}
			finally
			{
				StateHasChanged();
			}
		}
	}
}
