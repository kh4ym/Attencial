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
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/")]
	public class Home : ComponentBase
	{
		private bool isLoggedIn;

		[Inject]
		private NavigationManager Nav { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenComponent<PageTitle>(0);
			__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.AddContent(2, "Attencial | Neo-Classical Academic Management");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n<canvas id=\"particleCanvas\" style=\"display: none;\"></canvas>\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "class", "canvas-bg font-body-md text-on-surface min-h-screen overflow-x-hidden selection:bg-primary-container selection:text-on-primary-container");
			__builder.OpenElement(6, "header");
			__builder.AddAttribute(7, "class", "bg-background border-b border-on-surface-variant/20 fixed top-0 left-0 right-0 z-50");
			__builder.OpenElement(8, "nav");
			__builder.AddAttribute(9, "class", "flex justify-between items-center w-full px-margin-mobile md:px-margin-desktop h-20 max-w-max-width mx-auto");
			__builder.OpenElement(10, "div");
			__builder.AddAttribute(11, "class", "flex items-center gap-6 md:gap-12");
			__builder.AddMarkupContent(12, "<a href=\"/\" class=\"flex items-center gap-3 no-underline group\"><div class=\"w-8 h-8 md:w-10 md:h-10 border-2 border-primary flex items-center justify-center relative\"><span class=\"material-symbols-outlined text-primary text-2xl group-hover:scale-110 transition-transform\">visibility</span>\n                        <div class=\"absolute -top-[2px] -right-[2px] w-2 h-2 bg-primary rounded-full\"></div></div>\n                    <div class=\"flex flex-col leading-none\"><span class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter\">Attencial</span>\n                        <span class=\"font-label-caps text-[8px] text-primary tracking-[0.2em] mt-0.5\">ACADEMIC</span></div></a>");
			if (isLoggedIn)
			{
				__builder.AddMarkupContent(13, "<div class=\"hidden md:flex gap-8\"><a class=\"text-on-surface-variant font-label-caps text-label-caps hover:text-primary transition-colors duration-300 no-underline\" href=\"dashboard\">Dashboard</a>\n                        <a class=\"text-on-surface-variant font-label-caps text-label-caps hover:text-primary transition-colors duration-300 no-underline\" href=\"session\">Start Session</a>\n                        <a class=\"text-on-surface-variant font-label-caps text-label-caps hover:text-primary transition-colors duration-300 no-underline\" href=\"professor-dashboard\">Analytics</a></div>");
			}
			__builder.CloseElement();
			__builder.AddMarkupContent(14, "\n            ");
			__builder.OpenElement(15, "div");
			__builder.AddAttribute(16, "class", "flex items-center gap-4 md:gap-6");
			if (isLoggedIn)
			{
				__builder.AddMarkupContent(18, "<a class=\"material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors no-underline\" href=\"profile\" title=\"Profile\">person</a>");
				__builder.OpenElement(185, "button");
				__builder.AddAttribute(186, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Logout));
				__builder.AddAttribute(187, "class", "material-symbols-outlined text-on-surface-variant hover:text-primary transition-colors bg-transparent border-0 cursor-pointer");
				__builder.AddAttribute(188, "title", "Logout");
				__builder.AddContent(189, "logout");
				__builder.CloseElement();
			}
			else
			{
				__builder.AddMarkupContent(19, "<a class=\"font-label-caps text-label-caps bg-primary text-surface px-3 py-2 md:px-5 md:py-3 no-underline transition-all duration-300 hover:scale-[1.03] inline-block animate-pulse-subtle border border-primary text-xs md:text-label-caps\" href=\"login\">LOGIN</a>\n                    ");
				__builder.AddMarkupContent(20, "<a class=\"font-label-caps text-label-caps bg-surface text-on-surface border border-on-surface px-3 py-2 md:px-5 md:py-3 no-underline transition-all duration-300 hover:scale-[1.03] hover:bg-[#fdf5f5] hover:border-primary/40 inline-block text-xs md:text-label-caps\" href=\"register\">SIGN UP</a>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(21, "\n\n    ");
			__builder.OpenElement(22, "main");
			__builder.AddAttribute(23, "class", "pt-5");
			__builder.OpenElement(24, "section");
			__builder.AddAttribute(25, "class", "relative min-h-[80vh] flex flex-col justify-start pt-8 md:pt-16 px-margin-mobile md:px-margin-desktop max-w-max-width mx-auto overflow-visible");
			__builder.AddMarkupContent(26, "<div data-spin data-spin-speed=\"180\" data-spin-dir=\"1\" class=\"absolute top-40 right-20 w-24 h-24 border-[3px] border-primary rounded-full geometric-accent hidden md:block opacity-20\"></div>\n            <div data-spin data-spin-speed=\"120\" data-spin-dir=\"-1\" class=\"absolute bottom-20 left-1/4 w-0 h-0 border-l-[40px] border-l-transparent border-r-[40px] border-r-transparent border-b-[70px] border-b-tertiary geometric-accent opacity-10\"></div>\n\n            ");
			__builder.OpenElement(27, "div");
			__builder.AddAttribute(28, "class", "grid grid-cols-12 gap-gutter relative z-10");
			__builder.OpenElement(29, "div");
			__builder.AddAttribute(30, "class", "col-span-12 lg:col-span-7 flex flex-col justify-center pt-4 animate-gentle-rise");
			__builder.AddMarkupContent(31, "<span class=\"font-label-caps text-label-caps text-primary mb-4 block tracking-[0.3em]\" style=\"animation: gentle-rise 0.8s 0.1s ease both;\">ACADEMIC EXCELLENCE</span>\n                    ");
			__builder.AddMarkupContent(32, "<h1 class=\"font-display-lg text-headline-lg-mobile md:text-display-lg text-on-surface mb-6 max-w-2xl\" style=\"animation: gentle-rise 0.9s 0.25s ease both;\">\n                        Streamline Your <span class=\"text-primary italic\">Academic</span> Attendance\n                    </h1>\n                    ");
			__builder.AddMarkupContent(33, "<p class=\"font-body-lg text-body-lg text-secondary mb-10 max-w-lg leading-relaxed\" style=\"animation: gentle-rise 0.8s 0.4s ease both;\">\n                        A smart, biometric-powered platform designed for high-end cultural institutions and modern universities. Bridge the gap between classical administration and avant-garde security.\n                    </p>\n                    ");
			__builder.OpenElement(34, "div");
			__builder.AddAttribute(35, "class", "flex items-center gap-8 flex-wrap");
			if (isLoggedIn)
			{
				__builder.AddMarkupContent(36, "<a href=\"dashboard\" class=\"bg-primary text-surface px-10 py-4 rounded-full font-label-caps text-label-caps tracking-widest hover:bg-[#f05454] transition-colors active:scale-95 transition-transform no-underline\">\n                                OPEN DASHBOARD\n                            </a>");
			}
			else
			{
				__builder.AddMarkupContent(37, "<a href=\"register\" class=\"relative overflow-hidden bg-primary text-surface px-10 py-4 rounded-full font-label-caps text-label-caps tracking-widest hover:bg-[#f05454] transition-all duration-300 active:scale-95 transition-transform no-underline group\"><span class=\"relative z-10 flex items-center gap-2\">\n                                    GET STARTED\n                                    <span class=\"material-symbols-outlined text-lg group-hover:translate-x-1 transition-transform\">arrow_forward</span></span>\n                                <span class=\"absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-700\"></span></a>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(38, "\n                ");
			__builder.AddMarkupContent(39, "<div class=\"col-span-12 lg:col-span-5 relative flex items-center justify-center min-h-[500px]\"><div data-spin data-spin-speed=\"45\" data-spin-dir=\"1\" class=\"absolute w-[450px] h-[450px] geometric-bg-triangle rotate-12 opacity-90 bg-primary-container\"></div>\n                    <div data-spin data-spin-speed=\"200\" data-spin-dir=\"-1\" class=\"absolute top-10 right-0 font-label-caps text-display-lg text-on-surface-variant opacity-10 select-none\">X</div>\n                    <div data-spin data-spin-speed=\"160\" data-spin-dir=\"1\" class=\"absolute bottom-0 left-0 font-label-caps text-headline-lg text-tertiary opacity-20 select-none\">X</div>\n                    <div class=\"absolute right-[-40px] top-1/2 -translate-y-1/2 hidden xl:flex flex-col items-center gap-4\"><span class=\"vertical-text font-label-caps text-label-caps text-secondary tracking-widest opacity-40\">SCROLL DOWN</span>\n                        <div class=\"w-[1px] h-24 bg-outline-variant/50\"></div></div></div>");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(40, "\n\n        ");
			__builder.AddMarkupContent(41, "<section id=\"features\" class=\"py-24 px-margin-mobile md:px-margin-desktop max-w-max-width mx-auto\"><div class=\"flex justify-between items-end mb-16 border-b border-outline-variant/20 pb-8\"><div><h2 class=\"font-headline-lg text-headline-lg text-on-surface mb-2\">Architectural Features</h2>\n                    <p class=\"font-body-md text-body-md text-secondary\">A trifecta of precision, data, and management.</p></div></div>\n\n            <div class=\"grid grid-cols-1 md:grid-cols-3 gap-8\"><div class=\"group border border-on-surface p-10 relative overflow-hidden transition-all duration-500 hover:bg-surface-container-low hover:-translate-y-2 hover:shadow-[16px_16px_0px_0px_rgba(27,28,26,0.06)]\"><div class=\"absolute top-0 right-0 w-24 h-24 bg-primary-container/5 -translate-y-12 translate-x-12 rounded-full transition-all duration-700 group-hover:scale-[2] group-hover:opacity-30\"></div>\n                    <span class=\"font-label-caps text-[10px] text-primary mb-12 block relative z-10\">MODULE 01</span>\n                    <span class=\"material-symbols-outlined text-display-lg mb-8 block text-on-surface relative z-10 transition-transform duration-500 group-hover:scale-110 group-hover:text-primary\">fingerprint</span>\n                    <h3 class=\"font-headline-md text-headline-md text-on-surface mb-6 relative z-10\">Smart Biometrics</h3>\n                    <p class=\"font-body-md text-body-md text-secondary leading-relaxed relative z-10\">\n                        Advanced facial recognition powered by AWS Rekognition ensures 99.9% accuracy in student identification.\n                    </p></div>\n                <div class=\"group border border-on-surface p-10 relative overflow-hidden transition-all duration-500 hover:bg-surface-container-low hover:-translate-y-2 hover:shadow-[16px_16px_0px_0px_rgba(27,28,26,0.06)]\"><div class=\"absolute top-0 right-0 font-label-caps text-[60px] text-on-surface-variant/5 select-none -translate-y-4 translate-x-4 transition-all duration-700 group-hover:text-primary/10 group-hover:text-[80px]\">X</div>\n                    <span class=\"font-label-caps text-[10px] text-primary mb-12 block relative z-10\">MODULE 02</span>\n                    <span class=\"material-symbols-outlined text-display-lg mb-8 block text-on-surface relative z-10 transition-transform duration-500 group-hover:scale-110 group-hover:text-primary\">how_to_reg</span>\n                    <h3 class=\"font-headline-md text-headline-md text-on-surface mb-6 relative z-10\">Enrollment Control</h3>\n                    <p class=\"font-body-md text-body-md text-secondary leading-relaxed relative z-10\">\n                        Streamlined course enrollment with professor approval workflows and real-time status tracking.\n                    </p></div>\n                <div class=\"group border border-on-surface p-10 relative overflow-hidden transition-all duration-500 hover:bg-surface-container-low hover:-translate-y-2 hover:shadow-[16px_16px_0px_0px_rgba(27,28,26,0.06)]\"><div class=\"absolute -bottom-8 -right-8 w-32 h-32 border border-primary/20 rounded-full transition-all duration-700 group-hover:scale-[2] group-hover:border-primary/40\"></div>\n                    <span class=\"font-label-caps text-[10px] text-primary mb-12 block relative z-10\">MODULE 03</span>\n                    <span class=\"material-symbols-outlined text-display-lg mb-8 block text-on-surface relative z-10 transition-transform duration-500 group-hover:scale-110 group-hover:text-primary\">insights</span>\n                    <h3 class=\"font-headline-md text-headline-md text-on-surface mb-6 relative z-10\">Real-time Analytics</h3>\n                    <p class=\"font-body-md text-body-md text-secondary leading-relaxed relative z-10\">\n                        Visual reports and heatmaps that highlight attendance trends across faculties and departments.\n                    </p></div></div></section>\n\n        ");
			__builder.OpenElement(42, "section");
			__builder.AddAttribute(43, "class", "bg-inverse-surface text-inverse-on-surface py-32 mt-20 relative overflow-hidden");
			__builder.OpenElement(44, "div");
			__builder.AddAttribute(45, "class", "absolute inset-0 opacity-5");
			__builder.OpenElement(46, "div");
			__builder.AddAttribute(47, "class", "grid grid-cols-12 h-full");
			for (int num = 0; num < 8; num++)
			{
				__builder.AddMarkupContent(48, "<div class=\"border-r border-on-surface col-span-1 h-full\"></div>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(49, "\n            ");
			__builder.OpenElement(50, "div");
			__builder.AddAttribute(51, "class", "px-margin-mobile md:px-margin-desktop max-w-max-width mx-auto text-center relative z-10");
			__builder.AddMarkupContent(52, "<span class=\"font-label-caps text-label-caps text-primary tracking-[0.4em] mb-8 block\">READY FOR TRANSFORMATION?</span>\n                ");
			__builder.AddMarkupContent(53, "<h2 data-reveal-top class=\"font-headline-lg text-headline-lg-mobile md:text-display-lg mb-12 max-w-3xl mx-auto\" style=\"opacity: 0;\">Modernize Your Academic Infrastructure Today</h2>");
			if (isLoggedIn)
			{
				__builder.AddMarkupContent(54, "<a href=\"dashboard\" class=\"bg-primary text-surface px-12 py-5 rounded-full font-label-caps text-label-caps tracking-widest hover:bg-[#f05454] transition-all no-underline inline-block\">\n                        GO TO DASHBOARD\n                    </a>");
			}
			else
			{
				__builder.AddMarkupContent(55, "<a href=\"register\" class=\"bg-primary text-surface px-12 py-5 rounded-full font-label-caps text-label-caps tracking-widest hover:bg-[#f05454] transition-all no-underline inline-block\">\n                        GET STARTED\n                    </a>");
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(56, "\n\n    ");
			__builder.OpenElement(57, "footer");
			__builder.AddAttribute(58, "class", "bg-background border-t border-on-surface-variant/20 py-16");
			__builder.OpenElement(59, "div");
			__builder.AddAttribute(60, "class", "w-full px-margin-mobile md:px-margin-desktop max-w-max-width mx-auto flex flex-col md:flex-row justify-between items-center gap-8");
			__builder.OpenElement(61, "div");
			__builder.AddAttribute(62, "class", "flex flex-col items-center md:items-start");
			__builder.AddMarkupContent(63, "<a href=\"/\" class=\"flex items-center gap-3 no-underline group mb-4\"><div class=\"w-8 h-8 md:w-10 md:h-10 border-2 border-primary flex items-center justify-center relative\"><span class=\"material-symbols-outlined text-primary text-2xl group-hover:scale-110 transition-transform\">visibility</span>\n                        <div class=\"absolute -top-[2px] -right-[2px] w-2 h-2 bg-primary rounded-full\"></div></div>\n                    <span class=\"font-display-lg text-headline-md font-bold text-on-surface tracking-tighter\">Attencial</span></a>\n                ");
			__builder.OpenElement(64, "p");
			__builder.AddAttribute(65, "class", "font-body-md text-on-surface-variant text-sm");
			__builder.AddContent(66, "© ");
			__builder.AddContent(67, DateTime.Now.Year);
			__builder.AddContent(68, " Attencial Academic. All rights reserved.");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(69, "\n            ");
			__builder.AddMarkupContent(70, "<nav class=\"flex flex-wrap justify-center gap-6\"><a class=\"font-label-caps text-label-caps text-on-surface-variant hover:text-primary transition-colors no-underline\" href=\"#\">Academic Integrity</a>\n                <a class=\"font-label-caps text-label-caps text-on-surface-variant hover:text-primary transition-colors no-underline\" href=\"#\">Privacy Policy</a>\n                <a class=\"font-label-caps text-label-caps text-on-surface-variant hover:text-primary transition-colors no-underline\" href=\"#\">Terms of Service</a></nav>");
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}

		protected override async Task OnInitializedAsync()
		{
			string text = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
			isLoggedIn = !string.IsNullOrEmpty(text) && text != "null" && text != "undefined";
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await JS.InvokeVoidAsync("attencialParallax.initGeometricSpin");
				await JS.InvokeVoidAsync("attencialAnimations.startParticles", "particleCanvas");
				await JS.InvokeVoidAsync("attencialAnimations.initAll");
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statAccuracy", 99, 2000);
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statSpeed", 800, 1800);
				await JS.InvokeVoidAsync("attencialAnimations.animateCounter", "statUptime", 99, 2200);
			}
		}

		private async Task Logout()
		{
			await JS.InvokeVoidAsync("authStorage.removeToken");
			Nav.NavigateTo("/login", forceLoad: true);
		}
	}
}
