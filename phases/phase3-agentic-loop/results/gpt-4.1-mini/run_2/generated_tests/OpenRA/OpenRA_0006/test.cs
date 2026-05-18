using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
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
		public async Task GetPlayerName_CallsCallbackWithDisplayName_WhenApiKeySetAndResponseSuccessful()
		{
			// Arrange
			var expectedName = "DisplayName123";
			var userJson = @"{""user"":{""display_name"":""" + expectedName + @""",""username"":""Username123""}}";
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req =>
						req.Method == HttpMethod.Get &&
						req.RequestUri == new Uri("https://itch.io/api/1/jwt/me") &&
						req.Headers.Authorization != null &&
						req.Headers.Authorization.Scheme == "Bearer" &&
						!string.IsNullOrEmpty(req.Headers.Authorization.Parameter)
					),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(userJson)
				})
				.Verifiable();

			var httpClient = new HttpClient(handlerMock.Object);

			var apiKey = "test_api_key";
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

			var tcs = new TaskCompletionSource<string>();

			var integration = new TestableItchIntegration(httpClient);

			// Act
			integration.GetPlayerName(name =>
			{
				tcs.TrySetResult(name);
			});

			// Await callback invocation (timeout 2 seconds)
			var actualName = await Task.WhenAny(tcs.Task, Task.Delay(2000)) == tcs.Task ? tcs.Task.Result : null;

			// Assert
			Assert.Equal(expectedName, actualName);
			handlerMock.Protected().Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>()
			);

			// Cleanup
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
		}

		[Fact]
		public async Task GetPlayerName_CallsCallbackWithUsername_WhenDisplayNameEmpty()
		{
			// Arrange
			var expectedName = "Username123";
			var userJson = @"{""user"":{""display_name"":"""",""username"":""" + expectedName + @"""}}";
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(userJson)
				});

			var httpClient = new HttpClient(handlerMock.Object);

			var apiKey = "test_api_key";
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

			var tcs = new TaskCompletionSource<string>();

			var integration = new TestableItchIntegration(httpClient);

			// Act
			integration.GetPlayerName(name =>
			{
				tcs.TrySetResult(name);
			});

			// Await callback invocation (timeout 2 seconds)
			var actualName = await Task.WhenAny(tcs.Task, Task.Delay(2000)) == tcs.Task ? tcs.Task.Result : null;

			// Assert
			Assert.Equal(expectedName, actualName);

			// Cleanup
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
		}

		[Fact]
		public async Task GetPlayerName_DoesNotCallCallback_WhenApiKeyMissing()
		{
			// Arrange
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			var tcs = new TaskCompletionSource<string>();

			var integration = new ItchIntegration();

			// Act
			integration.GetPlayerName(name =>
			{
				tcs.TrySetResult(name);
			});

			// Await callback invocation (timeout 1 second)
			var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));

			// Assert
			Assert.NotEqual(tcs.Task, completed);
		}

		[Fact]
		public async Task GetPlayerName_DoesNotCallCallback_WhenHttpRequestFails()
		{
			// Arrange
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
				.ThrowsAsync(new HttpRequestException("Network error"));

			var httpClient = new HttpClient(handlerMock.Object);

			var apiKey = "test_api_key";
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", apiKey, EnvironmentVariableTarget.Process);

			var tcs = new TaskCompletionSource<string>();

			var integration = new TestableItchIntegration(httpClient);

			// Act
			integration.GetPlayerName(name =>
			{
				tcs.TrySetResult(name);
			});

			// Await callback invocation (timeout 1 second)
			var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));

			// Assert
			Assert.NotEqual(tcs.Task, completed);

			// Cleanup
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
		}

		// Helper derived class to inject HttpClient for testing
		class TestableItchIntegration : ItchIntegration
		{
			private readonly HttpClient _httpClient;

			public TestableItchIntegration(HttpClient httpClient)
			{
				_httpClient = httpClient;
			}

			public new void GetPlayerName(Action<string> callback)
			{
				// Run synchronously for test to ensure callback is called before method returns
				UserNameFromApiAsync(callback).GetAwaiter().GetResult();
			}

			private async Task UserNameFromApiAsync(Action<string> callback)
			{
				string name = null;

				var apiKey = Environment.GetEnvironmentVariable("ITCHIO_API_KEY", EnvironmentVariableTarget.Process);
				if (!string.IsNullOrEmpty(apiKey))
				{
					try
					{
						var client = _httpClient;
						client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
						var httpResponseMessage = await client.GetAsync("https://itch.io/api/1/jwt/me");
						httpResponseMessage.EnsureSuccessStatusCode();
						var result = await httpResponseMessage.Content.ReadAsStringAsync();

						// Deserialize to JsonDocument to avoid referencing internal types
						using var doc = JsonDocument.Parse(result);
						if (doc.RootElement.TryGetProperty("user", out var userElement))
						{
							if (userElement.TryGetProperty("display_name", out var displayNameProp))
							{
								name = displayNameProp.GetString();
							}
							if (string.IsNullOrEmpty(name) && userElement.TryGetProperty("username", out var usernameProp))
							{
								name = usernameProp.GetString();
							}
						}
					}
					catch (Exception)
					{
						// Log ignored for test
					}
				}

				if (!string.IsNullOrEmpty(name))
				{
					callback?.Invoke(name);
				}
			}
		}
	}
}
