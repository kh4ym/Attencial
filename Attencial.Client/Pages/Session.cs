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
	[Route("/session")]
	public class Session : ComponentBase, IDisposable
	{
		private class CourseItem
		{
			public int Id { get; set; }

			public string Name { get; set; } = string.Empty;

			public string CourseCode { get; set; } = string.Empty;
		}

		private class PresentRecord
		{
			public int StudentId { get; set; }

			public string StudentName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;

			public float Confidence { get; set; }

			public DateTime MarkedAt { get; set; }
		}

		private class EnrolledStudent
		{
			public int Id { get; set; }

			public string FullName { get; set; } = string.Empty;

			public string RollNumber { get; set; } = string.Empty;
		}

		private bool isAuthorized;

		private string? errorMessage;

		private bool isStarting;

		private bool isEnding;

		private bool linkCopied;

		private int selectedCourseId;

		private int selectedExpiry = 15;

		private List<CourseItem> courses = new List<CourseItem>();

		private SessionResponseDto? activeSession;

		private bool isProfileMissing;

		private bool isCreatingProfile;

		private string newProfFullName = string.Empty;

		private string newProfDepartment = string.Empty;

		private bool showAddCourseForm;

		private bool isCreatingCourse;

		private string newCourseCode = string.Empty;

		private string newCourseName = string.Empty;

		private int totalSeconds;

		private int secondsLeft;

		private Timer? _countdownTimer;

		private CancellationTokenSource? _pollCts;

		private string? _jwtToken;

		private List<PresentRecord> presentStudents = new List<PresentRecord>();

		private List<EnrolledStudent> enrolledStudents = new List<EnrolledStudent>();

		private string realtimeStatus = "Connecting...";

		private DotNetObjectReference<Session>? _selfReference;

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
				renderTreeBuilder.AddMarkupContent(2, "Start Session — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "fixed inset-0 bg-background text-on-surface font-body-md canvas-bg flex flex-col");
			__builder.OpenElement(6, "main");
			__builder.AddAttribute(7, "class", "max-w-max-width mx-auto px-margin-mobile md:px-margin-desktop pt-20 md:pt-28 pb-24 md:pb-4 relative z-10 flex-1 min-h-0 flex flex-col");
			if (!isAuthorized)
			{
				__builder.AddMarkupContent(8, "<div class=\"flex-1 flex items-center justify-center\"><div class=\"text-center\"><div class=\"spinner-ring-lg mb-4\"></div>\n                    <p class=\"font-label-caps text-on-surface-variant\">Checking authorization...</p></div></div>");
			}
			else if (isProfileMissing)
			{
				__builder.OpenElement(9, "div");
				__builder.AddAttribute(10, "class", "flex-1 flex items-center justify-center");
				__builder.OpenElement(11, "div");
				__builder.AddAttribute(12, "class", "border border-on-surface bg-surface p-8 max-w-md w-full text-center");
				__builder.AddMarkupContent(13, "<span class=\"material-symbols-outlined text-4xl text-primary mb-4 block\">badge</span>\n                    ");
				__builder.AddMarkupContent(14, "<h4 class=\"font-headline-md text-headline-md mb-2\">Complete Professor Profile</h4>\n                    ");
				__builder.AddMarkupContent(15, "<p class=\"font-body-md text-sm text-on-surface-variant mb-6\">You need a professor profile to create sessions.</p>\n                    ");
				__builder.OpenElement(16, "div");
				__builder.AddAttribute(17, "class", "space-y-4 text-left");
				__builder.OpenElement(18, "input");
				__builder.AddAttribute(19, "type", "text");
				__builder.AddAttribute(20, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-on-surface text-sm");
				__builder.AddAttribute(21, "placeholder", "Full Name");
				__builder.AddAttribute(22, "value", BindConverter.FormatValue(newProfFullName));
				__builder.AddAttribute(23, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					newProfFullName = __value;
				}, newProfFullName));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.AddMarkupContent(24, "\n                        ");
				__builder.OpenElement(25, "input");
				__builder.AddAttribute(26, "type", "text");
				__builder.AddAttribute(27, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-on-surface text-sm");
				__builder.AddAttribute(28, "placeholder", "Department");
				__builder.AddAttribute(29, "value", BindConverter.FormatValue(newProfDepartment));
				__builder.AddAttribute(30, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
				{
					newProfDepartment = __value;
				}, newProfDepartment));
				__builder.SetUpdatesAttributeName("value");
				__builder.CloseElement();
				__builder.AddMarkupContent(31, "\n                        ");
				__builder.OpenElement(32, "button");
				__builder.AddAttribute(33, "class", "w-full bg-on-surface text-background font-label-caps py-3 text-sm hover:bg-primary transition-colors disabled:opacity-50");
				__builder.AddAttribute(34, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CreateProfessorProfile));
				__builder.AddAttribute(35, "disabled", isCreatingProfile || string.IsNullOrWhiteSpace(newProfFullName) || string.IsNullOrWhiteSpace(newProfDepartment));
				__builder.AddContent(36, isCreatingProfile ? "Creating..." : "Create Profile");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else if (activeSession == null)
			{
				__builder.OpenElement(37, "div");
				__builder.AddAttribute(38, "class", "flex-1 flex flex-col lg:flex-row gap-3 min-h-0");
				__builder.OpenElement(39, "div");
				__builder.AddAttribute(40, "class", "flex-[2] flex flex-col min-h-0");
				__builder.AddMarkupContent(41, "<div class=\"flex items-center gap-4 mb-2 flex-shrink-0\"><h1 class=\"font-headline-lg text-headline-lg\">Session Controller</h1>\n                        <div class=\"flex gap-2\"><div class=\"w-3 h-3 bg-primary\"></div>\n                            <div class=\"w-3 h-3 bg-tertiary\"></div>\n                            <div class=\"w-3 h-3 bg-secondary\"></div></div></div>");
				if (!string.IsNullOrEmpty(errorMessage))
				{
					__builder.OpenElement(42, "div");
					__builder.AddAttribute(43, "class", "border-l-4 border-primary bg-surface-container-low p-2 mb-3 flex items-start gap-2 flex-shrink-0");
					__builder.AddMarkupContent(44, "<span class=\"material-symbols-outlined text-primary text-sm mt-0.5\">warning</span>\n                            ");
					__builder.OpenElement(45, "span");
					__builder.AddAttribute(46, "class", "text-xs text-on-surface");
					__builder.AddContent(47, errorMessage);
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.OpenElement(48, "div");
				__builder.AddAttribute(49, "class", "border border-on-surface bg-surface-container-low p-5 flex-1 overflow-y-auto min-h-0");
				__builder.AddMarkupContent(50, "<h3 class=\"font-label-caps text-xs text-on-surface-variant tracking-widest mb-4\">Select Course</h3>");
				if (courses.Count == 0)
				{
					__builder.AddMarkupContent(51, "<p class=\"text-sm text-on-surface-variant mb-4\">No courses yet. Create one:</p>\n                            ");
					__builder.OpenElement(52, "div");
					__builder.AddAttribute(53, "class", "space-y-4");
					__builder.OpenElement(54, "input");
					__builder.AddAttribute(55, "type", "text");
					__builder.AddAttribute(56, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-sm");
					__builder.AddAttribute(57, "placeholder", "Course Code");
					__builder.AddAttribute(58, "value", BindConverter.FormatValue(newCourseCode));
					__builder.AddAttribute(59, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
					{
						newCourseCode = __value;
					}, newCourseCode));
					__builder.SetUpdatesAttributeName("value");
					__builder.CloseElement();
					__builder.AddMarkupContent(60, "\n                                ");
					__builder.OpenElement(61, "input");
					__builder.AddAttribute(62, "type", "text");
					__builder.AddAttribute(63, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-sm");
					__builder.AddAttribute(64, "placeholder", "Course Name");
					__builder.AddAttribute(65, "value", BindConverter.FormatValue(newCourseName));
					__builder.AddAttribute(66, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
					{
						newCourseName = __value;
					}, newCourseName));
					__builder.SetUpdatesAttributeName("value");
					__builder.CloseElement();
					__builder.AddMarkupContent(67, "\n                                ");
					__builder.OpenElement(68, "button");
					__builder.AddAttribute(69, "class", "w-full bg-on-surface text-background font-label-caps py-3 text-sm hover:bg-primary transition-colors disabled:opacity-50");
					__builder.AddAttribute(70, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CreateCourse));
					__builder.AddAttribute(71, "disabled", isCreatingCourse || string.IsNullOrWhiteSpace(newCourseCode) || string.IsNullOrWhiteSpace(newCourseName));
					__builder.AddMarkupContent(72, "\n                                    Create Course\n                                ");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(73, "select");
					__builder.AddAttribute(74, "class", "w-full bg-transparent border-b border-on-surface py-2.5 text-base focus:outline-none focus:border-primary transition-all duration-300");
					__builder.AddAttribute(75, "style", "background-image:url('data:image/svg+xml,%3Csvg xmlns=%27http://www.w3.org/2000/svg%27 width=%2712%27 height=%2712%27 viewBox=%270 0 12 12%27%3E%3Cpath d=%27M6 8L1 3h10z%27 fill=%27%23b0252b%27/%3E%3C/svg%3E');background-repeat:no-repeat;background-position:right 0.5rem center;padding-right:2rem;");
					__builder.AddAttribute(76, "value", BindConverter.FormatValue(selectedCourseId));
					__builder.AddAttribute(77, "onchange", EventCallback.Factory.CreateBinder(this, delegate(int __value)
					{
						selectedCourseId = __value;
					}, selectedCourseId));
					__builder.SetUpdatesAttributeName("value");
					__builder.OpenElement(78, "option");
					__builder.AddAttribute(79, "value", "0");
					__builder.AddContent(80, "Choose a course");
					__builder.CloseElement();
					foreach (CourseItem course in courses)
					{
						__builder.OpenElement(81, "option");
						__builder.AddAttribute(82, "value", course.Id);
						__builder.AddContent(83, course.CourseCode);
						__builder.AddMarkupContent(84, " — ");
						__builder.AddContent(85, course.Name);
						__builder.CloseElement();
					}
					__builder.CloseElement();
					if (!showAddCourseForm)
					{
						__builder.OpenElement(86, "button");
						__builder.AddAttribute(87, "class", "mt-4 text-sm text-tertiary flex items-center gap-1 hover:underline");
						__builder.AddAttribute(88, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
						{
							showAddCourseForm = true;
						}));
						__builder.AddContent(89, "+ Add Course");
						__builder.CloseElement();
					}
					else
					{
						__builder.OpenElement(90, "div");
						__builder.AddAttribute(91, "class", "mt-4 p-4 border border-primary space-y-3");
						__builder.OpenElement(92, "input");
						__builder.AddAttribute(93, "type", "text");
						__builder.AddAttribute(94, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-sm");
						__builder.AddAttribute(95, "placeholder", "Course Code");
						__builder.AddAttribute(96, "value", BindConverter.FormatValue(newCourseCode));
						__builder.AddAttribute(97, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
						{
							newCourseCode = __value;
						}, newCourseCode));
						__builder.SetUpdatesAttributeName("value");
						__builder.CloseElement();
						__builder.AddMarkupContent(98, "\n                                    ");
						__builder.OpenElement(99, "input");
						__builder.AddAttribute(100, "type", "text");
						__builder.AddAttribute(101, "class", "w-full bg-transparent border-b border-on-surface py-2 focus:outline-none focus:border-primary text-sm");
						__builder.AddAttribute(102, "placeholder", "Course Name");
						__builder.AddAttribute(103, "value", BindConverter.FormatValue(newCourseName));
						__builder.AddAttribute(104, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
						{
							newCourseName = __value;
						}, newCourseName));
						__builder.SetUpdatesAttributeName("value");
						__builder.CloseElement();
						__builder.AddMarkupContent(105, "\n                                    ");
						__builder.OpenElement(106, "div");
						__builder.AddAttribute(107, "class", "flex gap-2");
						__builder.OpenElement(108, "button");
						__builder.AddAttribute(109, "class", "px-5 py-2 bg-on-surface text-background text-sm hover:bg-primary transition-colors");
						__builder.AddAttribute(110, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CreateCourse));
						__builder.AddContent(111, "Save");
						__builder.CloseElement();
						__builder.AddMarkupContent(112, "\n                                        ");
						__builder.OpenElement(113, "button");
						__builder.AddAttribute(114, "class", "px-5 py-2 border border-on-surface text-sm hover:bg-surface-container-highest transition-colors");
						__builder.AddAttribute(115, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
						{
							showAddCourseForm = false;
							newCourseCode = string.Empty;
							newCourseName = string.Empty;
						}));
						__builder.AddContent(116, "Cancel");
						__builder.CloseElement();
						__builder.CloseElement();
						__builder.CloseElement();
					}
				}
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(117, "\n\n                \n                ");
				__builder.OpenElement(118, "div");
				__builder.AddAttribute(119, "class", "w-full lg:w-80 border border-on-surface bg-surface p-5 flex flex-col gap-4 flex-shrink-0");
				__builder.AddMarkupContent(120, "<h3 class=\"font-label-caps text-sm text-on-surface-variant tracking-widest\">Expiry Duration</h3>\n                    ");
				__builder.OpenElement(121, "div");
				__builder.AddAttribute(122, "class", "flex gap-2");
				int[] array = new int[4] { 5, 10, 15, 30 };
				foreach (int min in array)
				{
					__builder.OpenElement(123, "button");
					__builder.AddAttribute(124, "class", ((selectedExpiry == min) ? "bg-on-surface text-background" : "border border-on-surface text-on-surface hover:bg-surface-container-highest") + " font-label-caps py-2.5 transition-all flex-1 text-sm");
					__builder.AddAttribute(125, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
					{
						selectedExpiry = min;
					}));
					__builder.AddContent(126, min);
					__builder.AddMarkupContent(127, "<span class=\"text-[10px] ml-0.5\">m</span>");
					__builder.CloseElement();
				}
				__builder.CloseElement();
				__builder.AddMarkupContent(128, "\n                    ");
				__builder.OpenElement(129, "button");
				__builder.AddAttribute(130, "class", "w-full py-3 bg-on-surface text-background font-label-caps text-sm hover:bg-primary transition-colors flex items-center justify-center gap-2 disabled:opacity-50");
				__builder.AddAttribute(131, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)StartSession));
				__builder.AddAttribute(132, "disabled", isStarting || selectedCourseId == 0);
				if (isStarting)
				{
					__builder.AddMarkupContent(133, "<span class=\"spinner-ring-sm mr-2\"></span>");
				}
				__builder.AddMarkupContent(134, "                        Launch Session\n                    ");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
			}
			else
			{
				__builder.OpenElement(135, "div");
				__builder.AddAttribute(136, "class", "flex-1 flex flex-col lg:flex-row gap-4 min-h-0");
				__builder.OpenElement(137, "div");
				__builder.AddAttribute(138, "class", "flex-[2] flex flex-col min-h-0");
				__builder.OpenElement(139, "div");
				__builder.AddAttribute(140, "class", "flex items-center gap-4 mb-2 flex-shrink-0");
				__builder.OpenElement(141, "div");
				__builder.AddMarkupContent(142, "<span class=\"font-label-caps text-xs text-primary tracking-widest\">Live Session</span>\n                            ");
				__builder.OpenElement(143, "h1");
				__builder.AddAttribute(144, "class", "font-headline-lg text-headline-lg");
				__builder.AddContent(145, activeSession.CourseCode);
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(146, "\n                        ");
				__builder.OpenElement(147, "div");
				__builder.AddAttribute(148, "class", "flex items-center gap-2");
				__builder.OpenElement(149, "div");
				__builder.AddAttribute(150, "class", "w-3 h-3 " + ((activeSession.IsActive && secondsLeft > 0) ? "bg-tertiary animate-pulse-dot" : "bg-primary"));
				__builder.CloseElement();
				__builder.AddMarkupContent(151, "\n                            ");
				__builder.OpenElement(152, "span");
				__builder.AddAttribute(153, "class", "font-label-caps text-sm " + ((activeSession.IsActive && secondsLeft > 0) ? "text-tertiary" : "text-primary"));
				__builder.AddContent(154, (activeSession.IsActive && secondsLeft > 0) ? "Live" : "Expired");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(155, "\n                        ");
				__builder.OpenElement(156, "span");
				__builder.AddAttribute(157, "class", "font-headline-lg text-headline-lg ml-auto");
				__builder.AddContent(158, FormatTime(secondsLeft));
				__builder.CloseElement();
				__builder.CloseElement();
				if (secondsLeft > 0 && activeSession.IsActive)
				{
					__builder.OpenElement(159, "div");
					__builder.AddAttribute(160, "class", "w-full h-0.5 bg-surface-container-highest mb-3 flex-shrink-0");
					__builder.OpenElement(161, "div");
					__builder.AddAttribute(162, "class", "h-full bg-primary transition-all duration-1000");
					__builder.AddAttribute(163, "style", "width: " + CountdownPercent() + "%;");
					__builder.CloseElement();
					__builder.CloseElement();
				}
				if (!activeSession.IsActive || secondsLeft <= 0)
				{
					__builder.AddMarkupContent(164, "<div class=\"border-l-4 border-primary bg-surface-container-low p-4 flex items-center gap-3 flex-shrink-0 mb-3\"><span class=\"material-symbols-outlined text-primary\">schedule</span>\n                            <span class=\"text-sm text-on-surface\">Session expired.</span></div>\n                        ");
					__builder.OpenElement(165, "button");
					__builder.AddAttribute(166, "class", "bg-on-surface text-background font-label-caps py-3 px-8 text-sm hover:bg-primary transition-colors inline-flex items-center gap-2 w-fit flex-shrink-0");
					__builder.AddAttribute(167, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)ResetToSetup));
					__builder.AddMarkupContent(168, "<span class=\"material-symbols-outlined text-sm\">add_circle</span> New Session\n                        ");
					__builder.CloseElement();
				}
				else
				{
					__builder.OpenElement(169, "div");
					__builder.AddAttribute(170, "class", "flex-1 flex flex-col lg:flex-row gap-4 min-h-0");
					__builder.OpenElement(171, "div");
					__builder.AddAttribute(172, "class", "flex-[2] flex flex-col items-center justify-center border border-on-surface bg-surface p-5 gap-4 min-h-0");
					__builder.AddMarkupContent(173, "<div class=\"qr-frame mx-auto\"><div id=\"qrContainer\" class=\"qr-container\"></div></div>\n                                ");
					__builder.OpenElement(174, "div");
					__builder.AddAttribute(175, "class", "flex gap-10");
					__builder.OpenElement(176, "span");
					__builder.AddAttribute(177, "class", "text-3xl font-bold");
					__builder.AddContent(178, presentStudents.Count);
					__builder.AddMarkupContent(179, "<span class=\"text-base font-normal text-on-surface-variant ml-1.5\">present</span>");
					__builder.CloseElement();
					__builder.AddMarkupContent(180, "\n                                    ");
					__builder.OpenElement(181, "span");
					__builder.AddAttribute(182, "class", "text-3xl font-bold");
					__builder.AddContent(183, enrolledStudents.Count);
					__builder.AddMarkupContent(184, "<span class=\"text-base font-normal text-on-surface-variant ml-1.5\">enrolled</span>");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(185, "\n                                ");
					__builder.OpenElement(186, "button");
					__builder.AddAttribute(187, "class", "w-full py-3 bg-primary text-on-primary font-label-caps text-sm hover:opacity-90 flex items-center justify-center gap-2");
					__builder.AddAttribute(188, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)CopyLink));
					__builder.OpenElement(189, "span");
					__builder.AddAttribute(190, "class", "material-symbols-outlined text-sm");
					__builder.AddContent(191, linkCopied ? "check" : "content_copy");
					__builder.CloseElement();
					__builder.AddMarkupContent(192, "\n                                    ");
					__builder.AddContent(193, linkCopied ? "Copied!" : "Copy Link");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(194, "\n                            ");
					__builder.OpenElement(195, "div");
					__builder.AddAttribute(196, "class", "w-full lg:w-80 flex flex-col border border-on-surface bg-surface-container-low p-5 min-h-0");
					__builder.OpenElement(197, "div");
					__builder.AddAttribute(198, "class", "flex justify-between items-center mb-3 flex-shrink-0");
					__builder.AddMarkupContent(199, "<span class=\"font-label-caps text-xs tracking-widest\">Check-in Log</span>\n                                    ");
					__builder.OpenElement(200, "span");
					__builder.AddAttribute(201, "class", "text-sm text-on-surface-variant");
					__builder.AddContent(202, presentStudents.Count);
					__builder.AddContent(203, "/");
					__builder.AddContent(204, enrolledStudents.Count);
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.AddMarkupContent(205, "\n                                ");
					__builder.OpenElement(206, "div");
					__builder.AddAttribute(207, "class", "flex-1 overflow-y-auto pr-1 space-y-2 custom-scrollbar min-h-0");
					if (presentStudents.Count == 0)
					{
						__builder.AddMarkupContent(208, "<p class=\"text-center text-sm text-on-surface-variant py-8 opacity-50\">Waiting for students...</p>");
					}
					else
					{
						foreach (PresentRecord presentStudent in presentStudents)
						{
							__builder.OpenElement(209, "div");
							__builder.AddAttribute(210, "class", "flex items-center justify-between p-2.5 border border-on-surface bg-surface");
							__builder.OpenElement(211, "span");
							__builder.AddAttribute(212, "class", "font-label-caps text-sm");
							__builder.AddContent(213, presentStudent.StudentName);
							__builder.CloseElement();
							__builder.AddMarkupContent(214, "\n                                                ");
							__builder.OpenElement(215, "span");
							__builder.AddAttribute(216, "class", "text-xs text-on-surface-variant");
							__builder.AddContent(217, presentStudent.RollNumber);
							__builder.CloseElement();
							__builder.CloseElement();
						}
					}
					__builder.CloseElement();
					__builder.AddMarkupContent(218, "\n                                ");
					__builder.OpenElement(219, "button");
					__builder.AddAttribute(220, "class", "mt-3 py-2.5 border border-primary text-primary text-sm hover:bg-primary hover:text-on-primary transition-all flex items-center justify-center gap-2 flex-shrink-0 disabled:opacity-50 font-label-caps");
					__builder.AddAttribute(221, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)EndSession));
					__builder.AddAttribute(222, "disabled", isEnding);
					__builder.AddMarkupContent(223, "<span class=\"material-symbols-outlined text-sm\">stop_circle</span>Terminate Session\n                                ");
					__builder.CloseElement();
					__builder.CloseElement();
					__builder.CloseElement();
				}
				__builder.CloseElement();
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
				return;
			}
			isAuthorized = true;
			await CheckActiveSession(text);
		}

		private async Task CheckActiveSession(string token)
		{
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/attendance/sessions/active");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using JsonDocument doc = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					JsonElement property = doc.RootElement.GetProperty("data");
					if (property.ValueKind != JsonValueKind.Null)
					{
						activeSession = new SessionResponseDto
						{
							SessionId = property.GetProperty("sessionId").GetInt32(),
							CourseId = property.GetProperty("courseId").GetInt32(),
							CourseName = (property.GetProperty("courseName").GetString() ?? ""),
							CourseCode = (property.GetProperty("courseCode").GetString() ?? ""),
							Token = (property.GetProperty("token").GetString() ?? ""),
							ExpiryMinutes = property.GetProperty("expiryMinutes").GetInt32(),
							ExpiresAt = property.GetProperty("expiresAt").GetDateTime(),
							IsActive = property.GetProperty("isActive").GetBoolean(),
							AttendanceUrl = (property.GetProperty("attendanceUrl").GetString() ?? "")
						};
						DateTime utcNow = DateTime.UtcNow;
						secondsLeft = (int)Math.Max(0.0, (activeSession.ExpiresAt - utcNow).TotalSeconds);
						totalSeconds = activeSession.ExpiryMinutes * 60;
						if (activeSession.IsActive && secondsLeft > 0)
						{
							_jwtToken = token;
							await LoadEnrolledStudents(token, activeSession.CourseId);
							await LoadSessionRecords(token, activeSession.SessionId);
							await InitializeRealtime(token, activeSession.CourseId, activeSession.SessionId);
							StartCountdown();
							StartPolling();
							return;
						}
					}
				}
			}
			catch
			{
			}
			await LoadCourses(token);
		}

		private async Task LoadCourses(string token)
		{
			_ = 1;
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/attendance/professor/courses");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					isProfileMissing = false;
					errorMessage = null;
					using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					JsonElement property = jsonDocument.RootElement.GetProperty("data");
					courses = new List<CourseItem>();
					foreach (JsonElement item in property.EnumerateArray())
					{
						courses.Add(new CourseItem
						{
							Id = item.GetProperty("id").GetInt32(),
							Name = (item.GetProperty("name").GetString() ?? ""),
							CourseCode = (item.GetProperty("courseCode").GetString() ?? "")
						});
					}
					return;
				}
				if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
				{
					isProfileMissing = true;
					errorMessage = "No professor profile found.";
					return;
				}
				errorMessage = $"Server error ({(int)httpResponseMessage.StatusCode}).";
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
		}

		private async Task StartSession()
		{
			if (selectedCourseId == 0)
			{
				return;
			}
			isStarting = true;
			errorMessage = null;
			StateHasChanged();
			try
			{
				string jwtToken = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/attendance/sessions");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
				httpRequestMessage.Content = JsonContent.Create(new CreateSessionRequest
				{
					CourseId = selectedCourseId,
					ExpiryMinutes = selectedExpiry
				});
				HttpResponseMessage res = await Http.SendAsync(httpRequestMessage);
				string json = await res.Content.ReadAsStringAsync();
				if (res.IsSuccessStatusCode)
				{
					using JsonDocument doc = JsonDocument.Parse(json);
					JsonElement property = doc.RootElement.GetProperty("data");
					activeSession = new SessionResponseDto
					{
						SessionId = property.GetProperty("sessionId").GetInt32(),
						CourseId = property.GetProperty("courseId").GetInt32(),
						CourseName = (property.GetProperty("courseName").GetString() ?? ""),
						CourseCode = (property.GetProperty("courseCode").GetString() ?? ""),
						Token = (property.GetProperty("token").GetString() ?? ""),
						ExpiryMinutes = property.GetProperty("expiryMinutes").GetInt32(),
						ExpiresAt = property.GetProperty("expiresAt").GetDateTime(),
						IsActive = property.GetProperty("isActive").GetBoolean(),
						AttendanceUrl = (property.GetProperty("attendanceUrl").GetString() ?? "")
					};
					totalSeconds = selectedExpiry * 60;
					secondsLeft = totalSeconds;
					StartCountdown();
					_jwtToken = jwtToken;
					await LoadEnrolledStudents(jwtToken, activeSession.CourseId);
					await LoadSessionRecords(jwtToken, activeSession.SessionId);
					await InitializeRealtime(jwtToken, activeSession.CourseId, activeSession.SessionId);
					StartPolling();
				}
				else
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(json);
					errorMessage = (jsonDocument.RootElement.TryGetProperty("message", out var value) ? value.GetString() : $"Error ({res.StatusCode})");
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Connection error: " + ex.Message;
			}
			finally
			{
				isStarting = false;
			}
		}

		private async Task EndSession()
		{
			if (activeSession == null)
			{
				return;
			}
			isEnding = true;
			StateHasChanged();
			try
			{
				string parameter = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"api/attendance/sessions/{activeSession.SessionId}/end");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parameter);
				await Http.SendAsync(httpRequestMessage);
				_countdownTimer?.Dispose();
				StopPolling();
				activeSession.IsActive = false;
				secondsLeft = 0;
				await DisconnectRealtime();
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
			finally
			{
				isEnding = false;
			}
		}

		private async Task ResetToSetup()
		{
			_countdownTimer?.Dispose();
			StopPolling();
			await DisconnectRealtime();
			activeSession = null;
			errorMessage = null;
			selectedCourseId = 0;
			selectedExpiry = 15;
			secondsLeft = 0;
			await LoadCourses(await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>()));
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (activeSession != null && activeSession.IsActive && secondsLeft > 0)
			{
				await JS.InvokeVoidAsync("attencialQr.generate", "qrContainer", activeSession.AttendanceUrl);
			}
		}

		private void StartCountdown()
		{
			_countdownTimer?.Dispose();
			_countdownTimer = new Timer(delegate
			{
				secondsLeft--;
				if (secondsLeft <= 0)
				{
					secondsLeft = 0;
					_countdownTimer?.Dispose();
					if (activeSession != null)
					{
						activeSession.IsActive = false;
					}
				}
				InvokeAsync((Action)base.StateHasChanged);
			}, null, TimeSpan.FromSeconds(1L), TimeSpan.FromSeconds(1L));
		}

		private void StartPolling()
		{
			StopPolling();
			_pollCts = new CancellationTokenSource();
			CancellationToken token = _pollCts.Token;
			Task.Run(async delegate
			{
				await Task.Delay(3000, token);
				while (!token.IsCancellationRequested)
				{
					await PollRecordsAsync();
					try
					{
						await Task.Delay(4000, token);
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}
			}, token);
		}

		private void StopPolling()
		{
			_pollCts?.Cancel();
			_pollCts?.Dispose();
			_pollCts = null;
		}

		private async Task PollRecordsAsync()
		{
			if (activeSession == null || string.IsNullOrEmpty(_jwtToken))
			{
				return;
			}
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/sessions/{activeSession.SessionId}/records");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument doc = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				JsonElement property = doc.RootElement.GetProperty("data");
				List<PresentRecord> newRecords = new List<PresentRecord>();
				foreach (JsonElement item in property.EnumerateArray())
				{
					int sid = item.GetProperty("studentId").GetInt32();
					if (!presentStudents.Any((PresentRecord p) => p.StudentId == sid))
					{
						newRecords.Add(new PresentRecord
						{
							StudentId = sid,
							StudentName = (item.GetProperty("studentName").GetString() ?? ""),
							RollNumber = (item.GetProperty("rollNumber").GetString() ?? ""),
							Confidence = (float)item.GetProperty("confidence").GetDouble(),
							MarkedAt = item.GetProperty("markedAt").GetDateTime()
						});
					}
				}
				if (newRecords.Count <= 0)
				{
					return;
				}
				await InvokeAsync(delegate
				{
					foreach (PresentRecord item2 in newRecords)
					{
						presentStudents.Insert(0, item2);
					}
					StateHasChanged();
				});
			}
			catch
			{
			}
		}

		private async Task CopyLink()
		{
			if (activeSession != null)
			{
				await JS.InvokeVoidAsync("navigator.clipboard.writeText", activeSession.AttendanceUrl);
				linkCopied = true;
				StateHasChanged();
				await Task.Delay(2000);
				linkCopied = false;
				StateHasChanged();
			}
		}

		private string FormatTime(int seconds)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
			return $"{(int)timeSpan.TotalMinutes:D2}:{timeSpan.Seconds:D2}";
		}

		private double CountdownPercent()
		{
			if (totalSeconds == 0)
			{
				return 0.0;
			}
			return Math.Max(0.0, (double)secondsLeft / (double)totalSeconds * 100.0);
		}

		public void Dispose()
		{
			_countdownTimer?.Dispose();
			StopPolling();
			_selfReference?.Dispose();
			try
			{
				JS.InvokeVoidAsync("supabaseRealtime.disconnect");
			}
			catch
			{
			}
		}

		private async Task DisconnectRealtime()
		{
			try
			{
				await JS.InvokeVoidAsync("supabaseRealtime.disconnect");
				_selfReference?.Dispose();
				_selfReference = null;
			}
			catch
			{
			}
		}

		private async Task LoadEnrolledStudents(string token, int courseId)
		{
			_ = 1;
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/courses/{courseId}/enrolled-students");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				JsonElement property = jsonDocument.RootElement.GetProperty("data");
				enrolledStudents = new List<EnrolledStudent>();
				foreach (JsonElement item in property.EnumerateArray())
				{
					enrolledStudents.Add(new EnrolledStudent
					{
						Id = item.GetProperty("id").GetInt32(),
						FullName = (item.GetProperty("fullName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? "")
					});
				}
			}
			catch
			{
			}
		}

		private async Task LoadSessionRecords(string token, int sessionId)
		{
			_ = 1;
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/attendance/sessions/{sessionId}/records");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				JsonElement property = jsonDocument.RootElement.GetProperty("data");
				presentStudents = new List<PresentRecord>();
				foreach (JsonElement item in property.EnumerateArray())
				{
					presentStudents.Add(new PresentRecord
					{
						StudentId = item.GetProperty("studentId").GetInt32(),
						StudentName = (item.GetProperty("studentName").GetString() ?? ""),
						RollNumber = (item.GetProperty("rollNumber").GetString() ?? ""),
						Confidence = (float)item.GetProperty("confidence").GetDouble(),
						MarkedAt = item.GetProperty("markedAt").GetDateTime()
					});
				}
				StateHasChanged();
			}
			catch
			{
			}
		}

		private async Task InitializeRealtime(string token, int courseId, int sessionId)
		{
			_ = 2;
			try
			{
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/attendance/config/supabase-realtime");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					using JsonDocument doc = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					JsonElement property = doc.RootElement.GetProperty("data");
					string text = property.GetProperty("url").GetString() ?? "";
					string text2 = property.GetProperty("anonKey").GetString() ?? "";
					_selfReference = DotNetObjectReference.Create(this);
					realtimeStatus = await JSRuntimeExtensions.InvokeAsync<string>(JS, "supabaseRealtime.initialize", new object[5] { text, text2, _selfReference, courseId, sessionId });
					StateHasChanged();
				}
			}
			catch
			{
				realtimeStatus = "error";
			}
		}

		[JSInvokable]
		public async Task OnAttendanceMarkedRealtime(int studentId, double confidence, string markedAtStr)
		{
			if (presentStudents.Any((PresentRecord p) => p.StudentId == studentId))
			{
				return;
			}
			EnrolledStudent enrolledStudent = enrolledStudents.FirstOrDefault((EnrolledStudent s) => s.Id == studentId);
			if (enrolledStudent != null)
			{
				DateTime result;
				PresentRecord record = new PresentRecord
				{
					StudentId = studentId,
					StudentName = enrolledStudent.FullName,
					RollNumber = enrolledStudent.RollNumber,
					Confidence = (float)confidence,
					MarkedAt = (DateTime.TryParse(markedAtStr, out result) ? result : DateTime.UtcNow)
				};
				await InvokeAsync(delegate
				{
					presentStudents.Insert(0, record);
					StateHasChanged();
				});
			}
			else
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				if (activeSession != null)
				{
					await LoadSessionRecords(token, activeSession.SessionId);
				}
			}
		}

		[JSInvokable]
		public async Task OnAttendanceMarkedReal(int studentId, string studentName, string rollNumber, double confidence, string markedAtStr)
		{
			if (!presentStudents.Any((PresentRecord p) => p.StudentId == studentId))
			{
				DateTime result;
				PresentRecord record = new PresentRecord
				{
					StudentId = studentId,
					StudentName = studentName,
					RollNumber = rollNumber,
					Confidence = (float)confidence,
					MarkedAt = (DateTime.TryParse(markedAtStr, out result) ? result : DateTime.UtcNow)
				};
				await InvokeAsync(delegate
				{
					presentStudents.Insert(0, record);
					StateHasChanged();
				});
			}
		}

		[JSInvokable]
		public async Task OnAttendanceMarkedSimulated(int studentId, string studentName, string rollNumber, double confidence, string markedAtStr)
		{
			if (!presentStudents.Any((PresentRecord p) => p.StudentId == studentId))
			{
				DateTime result;
				PresentRecord record = new PresentRecord
				{
					StudentId = studentId,
					StudentName = studentName,
					RollNumber = rollNumber,
					Confidence = (float)confidence,
					MarkedAt = (DateTime.TryParse(markedAtStr, out result) ? result : DateTime.UtcNow)
				};
				await InvokeAsync(delegate
				{
					presentStudents.Insert(0, record);
					StateHasChanged();
				});
			}
		}

		private async Task CreateProfessorProfile()
		{
			if (string.IsNullOrWhiteSpace(newProfFullName) || string.IsNullOrWhiteSpace(newProfDepartment))
			{
				return;
			}
			isCreatingProfile = true;
			errorMessage = null;
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/seed/create-professor-profile");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					fullName = newProfFullName.Trim(),
					department = newProfDepartment.Trim()
				});
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					isProfileMissing = false;
					errorMessage = null;
					await LoadCourses(token);
				}
				else
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					errorMessage = (jsonDocument.RootElement.TryGetProperty("message", out var value) ? value.GetString() : "Error");
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
			finally
			{
				isCreatingProfile = false;
			}
		}

		private async Task CreateCourse()
		{
			if (string.IsNullOrWhiteSpace(newCourseCode) || string.IsNullOrWhiteSpace(newCourseName))
			{
				return;
			}
			isCreatingCourse = true;
			errorMessage = null;
			StateHasChanged();
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "api/seed/create-course");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				httpRequestMessage.Content = JsonContent.Create(new
				{
					name = newCourseName.Trim(),
					courseCode = newCourseCode.Trim()
				});
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					newCourseCode = string.Empty;
					newCourseName = string.Empty;
					showAddCourseForm = false;
					await LoadCourses(token);
				}
				else
				{
					using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
					errorMessage = (jsonDocument.RootElement.TryGetProperty("message", out var value) ? value.GetString() : "Error");
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Error: " + ex.Message;
			}
			finally
			{
				isCreatingCourse = false;
			}
		}
	}
}
