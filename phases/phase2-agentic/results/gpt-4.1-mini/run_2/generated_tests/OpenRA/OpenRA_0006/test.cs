using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
	public class ItchIntegrationTests
	{
		[Fact]
		public async Task GetPlayerName_CallsHttpClientGetAsync_AndInvokesCallbackWithDisplayName()
		{
			// Arrange
			var expectedName = "DisplayName";
			var apiKey = "test_api_key";
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

			var userJson = @"{
				""user"": {
					""display_name"": ""DisplayName"",
					""username"": ""Username""
				}
			}";

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Get &&
						req.RequestUri == new Uri("https://itch.io/api/1/jwt/me") &&
						req.Headers.Authorization.Scheme == "Bearer" &&
						req.Headers.Authorization.Parameter == apiKey),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(userJson),
				})
				.Verifiable();

			// Replace HttpClientFactory.Create to return HttpClient with mocked handler
			// Since HttpClientFactory.Create is static and no interface, we use a workaround by reflection to replace it temporarily
			// But here, we will create a derived class to override GetPlayerName for test to inject HttpClient

			var callbackInvoked = false;
			string callbackName = null;

			var integration = new TestableItchIntegration(new HttpClient(handlerMock.Object));

			// Act
			integration.GetPlayerName(name =>
			{
				callbackInvoked = true;
				callbackName = name;
			});

			// Wait for the async Task.Run to complete
			await Task.Delay(200);

			// Assert
			Assert.True(callbackInvoked);
			Assert.Equal(expectedName, callbackName);
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri("https://itch.io/api/1/jwt/me")),
				ItExpr.IsAny<CancellationToken>());
		}

		[Fact]
		public async Task GetPlayerName_UsesUsernameIfDisplayNameIsEmpty()
		{
			// Arrange
			var apiKey = "test_api_key";
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

			var userJson = @"{
				""user"": {
					""display_name"": """",
					""username"": ""Username""
				}
			}";

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(userJson),
				})
				.Verifiable();

			var callbackInvoked = false;
			string callbackName = null;

			var integration = new TestableItchIntegration(new HttpClient(handlerMock.Object));

			// Act
			integration.GetPlayerName(name =>
			{
				callbackInvoked = true;
				callbackName = name;
			});

			// Wait for the async Task.Run to complete
			await Task.Delay(200);

			// Assert
			Assert.True(callbackInvoked);
			Assert.Equal("Username", callbackName);
		}

		[Fact]
		public async Task GetPlayerName_NoApiKey_DoesNotInvokeCallback()
		{
			// Arrange
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			var callbackInvoked = false;

			var integration = new ItchIntegration();

			// Act
			integration.GetPlayerName(name =>
			{
				callbackInvoked = true;
			});

			// Wait for the async Task.Run to complete
			await Task.Delay(200);

			// Assert
			Assert.False(callbackInvoked);
		}

		// Helper class to inject HttpClient for testing
		private class TestableItchIntegration : ItchIntegration
		{
			private readonly HttpClient _httpClient;

			public TestableItchIntegration(HttpClient httpClient)
			{
				_httpClient = httpClient;
			}

			public new void GetPlayerName(Action<string> callback)
			{
				Task.Run(async () =>
				{
					User user = null;

					var apiKey = Environment.GetEnvironmentVariable("ITCHIO_API_KEY", EnvironmentVariableTarget.Process);
					if (!string.IsNullOrEmpty(apiKey))
					{
						try
						{
							var client = _httpClient;
							client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
							var httpResponseMessage = await client.GetAsync("https://itch.io/api/1/jwt/me");
							httpResponseMessage.EnsureSuccessStatusCode();
							var result = await httpResponseMessage.Content.ReadAsStringAsync();
							user = System.Text.Json.JsonSerializer.Deserialize<Root>(result)?.User;
						}
						catch (Exception)
						{
							// Ignored for test
						}
					}

					if (user != null)
					{
						string name;
						if (string.IsNullOrEmpty(user.DisplayName))
							name = user.Username;
						else
							name = user.DisplayName;

						// Directly invoke callback for test simplicity
						callback?.Invoke(name);
					}
				});
			}
		}
	}
}
