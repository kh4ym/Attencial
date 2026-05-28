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
using System.Threading.Tasks;
namespace Attencial.Client.Pages
{
	[Route("/parallax-demo")]
	public class ParallaxDemo : ComponentBase, IAsyncDisposable
	{
		private bool _initialized;

		[Inject]
		private IJSRuntime JS { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenComponent<PageTitle>(0);
			__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder2)
			{
				renderTreeBuilder2.AddMarkupContent(2, "Parallax Demo — Attencial");
			});
			__builder.CloseComponent();
			__builder.AddMarkupContent(3, "\n\n");
			__builder.OpenElement(4, "div");
			__builder.AddAttribute(5, "id", "parallax-container");
			__builder.AddAttribute(6, "class", "canvas-bg");
			__builder.AddMarkupContent(7, "<section class=\"relative min-h-screen flex items-center justify-center overflow-hidden\"><div data-parallax-bg data-speed=\"0.5\" class=\"absolute inset-0\" style=\"background: radial-gradient(ellipse at 30% 50%, rgba(176,37,43,0.08) 0%, transparent 60%), radial-gradient(ellipse at 70% 30%, rgba(0,97,145,0.06) 0%, transparent 50%); z-index: 0;\"></div>\n        <div class=\"relative z-10 text-center px-margin-mobile max-w-2xl\"><span class=\"font-label-caps text-label-caps text-primary tracking-[0.3em] block mb-6\" data-fade-up data-delay=\"0\">ACADEMIC EXCELLENCE</span>\n            <h1 class=\"font-display-lg text-display-lg text-on-surface mb-8\" data-fade-up data-delay=\"0.1\">Smooth Parallax</h1>\n            <p class=\"font-body-lg text-body-lg text-secondary mb-12\" data-fade-up data-delay=\"0.2\">Scroll-triggered animations powered by GSAP & ScrollTrigger. Cards stagger, backgrounds parallax, everything scrubs.</p>\n            <div data-scale-in class=\"mt-6\"><span class=\"badge-neo badge-neo-active inline-flex items-center gap-2 text-sm px-4 py-2\"><span class=\"live-dot\"></span> Scroll down</span></div></div></section>\n\n    ");
			__builder.OpenElement(8, "section");
			__builder.AddAttribute(9, "class", "py-24 px-margin-desktop max-w-max-width mx-auto");
			__builder.AddMarkupContent(10, "<div class=\"flex justify-between items-end mb-16 border-b border-outline-variant/20 pb-8\"><div><h2 class=\"font-headline-lg text-headline-lg mb-2\" data-fade-up>Staggered Card Reveal</h2>\n                <p class=\"font-body-md text-body-md text-secondary\">Each card animates 200ms after the last.</p></div>\n            <div class=\"flex gap-2\"><div class=\"w-2 h-2 rounded-full bg-primary\"></div>\n                <div class=\"w-2 h-2 rounded-full bg-outline\"></div>\n                <div class=\"w-2 h-2 rounded-full bg-outline\"></div></div></div>\n\n        ");
			__builder.OpenElement(11, "div");
			__builder.AddAttribute(12, "data-stagger-cards");
			__builder.AddAttribute(13, "class", "grid grid-cols-1 md:grid-cols-3 gap-gutter");
			for (int num = 1; num <= 6; num++)
			{
				__builder.OpenElement(14, "div");
				__builder.AddAttribute(15, "data-card");
				__builder.AddAttribute(16, "class", "card-neo-hover text-center");
				__builder.OpenElement(17, "span");
				__builder.AddAttribute(18, "class", "font-label-caps text-[10px] text-primary mb-8 block");
				__builder.AddContent(19, "MODULE 0");
				__builder.AddContent(20, num);
				__builder.CloseElement();
				__builder.AddMarkupContent(21, "\n                    ");
				__builder.OpenElement(22, "span");
				__builder.AddAttribute(23, "class", "material-symbols-outlined text-[64px] mb-6 block text-on-surface");
				RenderTreeBuilder renderTreeBuilder = __builder;
				renderTreeBuilder.AddContent(24, num switch
				{
					1 => "fingerprint", 
					2 => "insights", 
					3 => "shield", 
					4 => "star", 
					5 => "extension", 
					_ => "favorite", 
				});
				__builder.CloseElement();
				__builder.AddMarkupContent(25, "\n                    ");
				__builder.OpenElement(26, "h3");
				__builder.AddAttribute(27, "class", "font-headline-md text-headline-md mb-4");
				renderTreeBuilder = __builder;
				renderTreeBuilder.AddContent(28, num switch
				{
					1 => "Biometrics", 
					2 => "Analytics", 
					3 => "Security", 
					4 => "Quality", 
					5 => "Modular", 
					_ => "Trusted", 
				});
				__builder.CloseElement();
				__builder.AddMarkupContent(29, "\n                    ");
				__builder.OpenElement(30, "p");
				__builder.AddAttribute(31, "class", "font-body-md text-body-md text-secondary");
				__builder.AddContent(32, "Card ");
				__builder.AddContent(33, num);
				__builder.AddMarkupContent(34, " — staggered 200ms reveal.");
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(35, "\n\n    ");
			__builder.AddMarkupContent(36, "<section class=\"relative h-[80vh] overflow-hidden\"><div data-parallax-bg data-speed=\"0.3\" class=\"absolute inset-0\" style=\"background: radial-gradient(ellipse at 60% 40%, rgba(176,37,43,0.06) 0%, transparent 55%), radial-gradient(ellipse at 30% 60%, rgba(0,97,145,0.05) 0%, transparent 50%); z-index: 0;\"></div>\n        <div class=\"relative z-10 flex items-center justify-center h-full text-center px-margin-mobile\"><div><h2 class=\"font-headline-lg text-headline-lg text-on-surface mb-4\" data-fade-up>Vertical Card Stack</h2>\n                <p class=\"font-body-md text-body-md text-secondary\" data-fade-up data-delay=\"0.1\">Cards fan out on scroll with scrub: 0.5</p></div></div></section>\n\n    ");
			__builder.OpenElement(37, "section");
			__builder.AddAttribute(38, "class", "py-24 px-margin-desktop max-w-max-width mx-auto");
			__builder.AddAttribute(39, "style", "min-height: 120vh;");
			__builder.OpenElement(40, "div");
			__builder.AddAttribute(41, "data-card-stack");
			__builder.AddAttribute(42, "class", "relative mx-auto");
			__builder.AddAttribute(43, "style", "max-width: 500px; min-height: 450px;");
			for (int num2 = 1; num2 <= 4; num2++)
			{
				__builder.OpenElement(44, "div");
				__builder.AddAttribute(45, "data-stack-card");
				__builder.AddAttribute(46, "class", "absolute top-0 left-0 w-full card-neo");
				__builder.AddAttribute(47, "style", "background: #fff;");
				__builder.OpenElement(48, "div");
				__builder.AddAttribute(49, "class", "flex items-center gap-4 mb-4");
				__builder.OpenElement(50, "span");
				__builder.AddAttribute(51, "class", "material-symbols-outlined text-[40px]");
				__builder.AddAttribute(52, "style", "color: " + num2 switch
				{
					3 => "#5f5e5e", 
					2 => "#006191", 
					1 => "#b0252b", 
					_ => "#1b1c1a", 
				} + ";");
				RenderTreeBuilder renderTreeBuilder = __builder;
				renderTreeBuilder.AddContent(53, num2 switch
				{
					1 => "looks_one", 
					2 => "looks_two", 
					3 => "looks_3", 
					_ => "looks_4", 
				});
				__builder.CloseElement();
				__builder.AddMarkupContent(54, "\n                        ");
				__builder.OpenElement(55, "div");
				__builder.OpenElement(56, "h3");
				__builder.AddAttribute(57, "class", "font-headline-md text-headline-md text-on-surface");
				renderTreeBuilder = __builder;
				renderTreeBuilder.AddContent(58, num2 switch
				{
					1 => "Initialize", 
					2 => "Process", 
					3 => "Validate", 
					_ => "Complete", 
				});
				__builder.CloseElement();
				__builder.AddMarkupContent(59, "\n                            ");
				__builder.OpenElement(60, "span");
				__builder.AddAttribute(61, "class", "font-label-caps text-label-caps text-on-surface-variant");
				__builder.AddContent(62, "Step ");
				__builder.AddContent(63, num2);
				__builder.AddContent(64, " of 4");
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.CloseElement();
				__builder.AddMarkupContent(65, "\n                    ");
				__builder.OpenElement(66, "p");
				__builder.AddAttribute(67, "class", "font-body-md text-body-md text-secondary");
				renderTreeBuilder = __builder;
				renderTreeBuilder.AddContent(68, num2 switch
				{
					1 => "The system captures and preprocesses the input data.", 
					2 => "Core algorithms process through validation layers.", 
					3 => "Results cross-checked against business rules.", 
					_ => "Final output delivered with full audit trail.", 
				});
				__builder.CloseElement();
				__builder.CloseElement();
			}
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.AddMarkupContent(69, "\n\n    ");
			__builder.AddMarkupContent(70, "<section class=\"bg-inverse-surface text-inverse-on-surface py-32 relative overflow-hidden\"><div class=\"px-margin-desktop max-w-max-width mx-auto text-center relative z-10\"><span class=\"font-label-caps text-label-caps text-primary tracking-[0.4em] mb-8 block\" data-fade-up>READY TO BUILD?</span>\n            <h2 class=\"font-headline-lg text-display-lg mb-8\" data-fade-up data-delay=\"0.1\">Animations Clean Up on Navigation</h2>\n            <a href=\"/\" class=\"btn-neo-primary text-lg px-10 py-4 no-underline inline-flex\" data-scale-in>Go to Dashboard</a></div></section>");
			__builder.CloseElement();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (!_initialized)
			{
				_initialized = true;
				await JS.InvokeVoidAsync("attencialParallax.initStaggeredParallax", "#parallax-container");
			}
		}

		public async ValueTask DisposeAsync()
		{
			try
			{
				await JS.InvokeVoidAsync("attencialParallax.cleanupParallax");
			}
			catch
			{
			}
		}
	}
}
