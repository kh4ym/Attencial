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
namespace Attencial.Client
{
	public class App : ComponentBase
	{
		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			__builder.OpenComponent<Router>(0);
			__builder.AddComponentParameter(1, "AppAssembly", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(typeof(App).Assembly));
			__builder.AddAttribute(2, "Found", (RenderFragment<RouteData>)((RouteData routeData) => delegate(RenderTreeBuilder renderTreeBuilder)
			{
				renderTreeBuilder.OpenComponent<RouteView>(3);
				renderTreeBuilder.AddComponentParameter(4, "RouteData", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(routeData));
				renderTreeBuilder.AddComponentParameter(5, "DefaultLayout", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(typeof(MainLayout)));
				renderTreeBuilder.CloseComponent();
				renderTreeBuilder.AddMarkupContent(6, "\r\n        ");
				renderTreeBuilder.OpenComponent<FocusOnNavigate>(7);
				renderTreeBuilder.AddComponentParameter(8, "RouteData", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(routeData));
				renderTreeBuilder.AddComponentParameter(9, "Selector", "h1");
				renderTreeBuilder.CloseComponent();
			}));
			__builder.CloseComponent();
		}
	}
}
