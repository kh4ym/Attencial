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
using System.Text.Json;
using System.Threading.Tasks;
namespace Attencial.Client.Layout
{
	public class MainLayout : LayoutComponentBase, IDisposable
	{
		private bool isAuthPage;

		private bool isEnrollGatePage;

		private bool showEnrollGate;

		[Inject]
		private HttpClient Http { get; set; }

		[Inject]
		private IJSRuntime JS { get; set; }

		[Inject]
		private NavigationManager Nav { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder __builder)
		{
			if (isAuthPage)
			{
				__builder.AddContent(0, base.Body);
			}
			else if (isEnrollGatePage)
			{
				__builder.AddContent(1, base.Body);
			}
			else if (!showEnrollGate)
			{
				__builder.OpenComponent<NavMenu>(2);
				__builder.CloseComponent();
				__builder.AddContent(3, base.Body);
			}
		}

		protected override async Task OnInitializedAsync()
		{
			UpdatePageType(Nav.Uri);
			Nav.LocationChanged += OnLocationChanged;
			await CheckEnrollmentGate();
		}

		private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
		{
			UpdatePageType(e.Location);
			await CheckEnrollmentGate();
			StateHasChanged();
		}

		private void UpdatePageType(string uri)
		{
			string text = Nav.ToBaseRelativePath(uri).Split('?')[0].ToLower();
			bool flag = ((text == "login" || text == "register") ? true : false);
			isAuthPage = flag;
			isEnrollGatePage = text == "enroll-face";
		}

		private async Task CheckEnrollmentGate()
		{
			if (isAuthPage || isEnrollGatePage)
			{
				showEnrollGate = false;
				return;
			}
			try
			{
				string token = await JSRuntimeExtensions.InvokeAsync<string>(JS, "authStorage.getToken", Array.Empty<object>());
				if (string.IsNullOrEmpty(token) || token == "null" || token == "undefined")
				{
					return;
				}
				HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
				httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				HttpResponseMessage httpResponseMessage = await Http.SendAsync(httpRequestMessage);
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					return;
				}
				using JsonDocument doc = JsonDocument.Parse(await httpResponseMessage.Content.ReadAsStringAsync());
				var role = doc.RootElement.GetProperty("data").GetProperty("role").GetString() ?? "";

				// Only enforce face enrollment for students — professors skip this gate
				if (!role.Equals("Professor", StringComparison.OrdinalIgnoreCase))
				{
					HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(HttpMethod.Get, "api/enrollment/status");
					httpRequestMessage2.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
					HttpResponseMessage httpResponseMessage2 = await Http.SendAsync(httpRequestMessage2);
					if (httpResponseMessage2.IsSuccessStatusCode)
					{
						using JsonDocument jsonDocument = JsonDocument.Parse(await httpResponseMessage2.Content.ReadAsStringAsync());
						if (!jsonDocument.RootElement.GetProperty("data").GetProperty("isEnrolled").GetBoolean())
						{
							Nav.NavigateTo("/enroll-face", forceLoad: true);
						}
					}
				}
			}
			catch (Exception ex)
			{
				// Enrollment gate check is non-critical — log and allow navigation
				Console.Error.WriteLine($"[MainLayout] Enrollment gate check failed: {ex.Message}");
			}
		}

		public void Dispose()
		{
			Nav.LocationChanged -= OnLocationChanged;
		}
	}
}
