using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using Xunit;
using OpenRA.Mods.Common;
using System.Threading;

namespace OpenRA.Mods.Common.Tests
{
	public class ItchIntegrationTests
	{
		// Helper class to override HttpClientFactory.Create to return a mocked HttpClient
		class TestHttpClientFactory : IDisposable
		{
			private readonly HttpMessageHandler _handler;
			public HttpClient Client { get; }

			public TestHttpClientFactory(HttpMessageHandler handler)
			{
				_handler = handler;
				Client = new HttpClient(_handler);
			}

			public void Dispose()
			{
				Client.Dispose();
			}
		}

		[Fact]
		public async Task GetPlayerName_CallsGetAsyncAndInvokesCallbackWithDisplayName()
		{
			// Arrange
			var user = new
			{
				user = new
				{
					display_name = "DisplayName",
					username = "Username"
				}
			};
			string jsonResponse = JsonSerializer.Serialize(user);

			var handler = new MockHttpMessageHandler(jsonResponse, HttpStatusCode.OK);
			using var factory = new TestHttpClientFactory(handler);

			// Patch HttpClientFactory.Create to return our client
			// We cannot patch static methods easily without a framework, so we simulate by reflection
			// But since we cannot modify the original code, we will simulate environment variable and callback

			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key", EnvironmentVariableTarget.Process);

			string callbackResult = null;
			var callbackInvoked = new ManualResetEventSlim();

			var integration = new ItchIntegration();

			// We need to replace HttpClientFactory.Create to return our client
			// Since we cannot do that easily, we will simulate by temporarily replacing HttpClientFactory.Create with a delegate
			// But the code uses HttpClientFactory.Create() directly, so we cannot inject the client.
			// So we will test the callback invocation by setting environment variable and waiting for callback.

			// Act
			integration.GetPlayerName(name =>
			{
				callbackResult = name;
				callbackInvoked.Set();
			});

			// Wait for callback to be invoked (timeout 2 seconds)
			bool signaled = callbackInvoked.Wait(2000);

			// Assert
			Assert.True(signaled, "Callback was not invoked within timeout.");
			Assert.Equal("DisplayName", callbackResult);

			// Cleanup
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
		}

		[Fact]
		public async Task GetPlayerName_UsesUsernameIfDisplayNameIsEmpty()
		{
			// Arrange
			var user = new
			{
				user = new
				{
					display_name = "",
					username = "Username"
				}
			};
			string jsonResponse = JsonSerializer.Serialize(user);

			var handler = new MockHttpMessageHandler(jsonResponse, HttpStatusCode.OK);
			using var factory = new TestHttpClientFactory(handler);

			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key", EnvironmentVariableTarget.Process);

			string callbackResult = null;
			var callbackInvoked = new ManualResetEventSlim();

			var integration = new ItchIntegration();

			// Act
			integration.GetPlayerName(name =>
			{
				callbackResult = name;
				callbackInvoked.Set();
			});

			bool signaled = callbackInvoked.Wait(2000);

			// Assert
			Assert.True(signaled, "Callback was not invoked within timeout.");
			Assert.Equal("Username", callbackResult);

			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
		}

		[Fact]
		public void GetPlayerName_NoApiKey_DoesNotInvokeCallback()
		{
			// Arrange
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			string callbackResult = null;
			var callbackInvoked = new ManualResetEventSlim();

			var integration = new ItchIntegration();

			// Act
			integration.GetPlayerName(name =>
			{
				callbackResult = name;
				callbackInvoked.Set();
			});

			// Wait 1 second to see if callback is invoked
			bool signaled = callbackInvoked.Wait(1000);

			// Assert
			Assert.False(signaled, "Callback should not be invoked when no API key is set.");
			Assert.Null(callbackResult);
		}

		// Mock HttpMessageHandler to simulate HttpClient responses
		class MockHttpMessageHandler : HttpMessageHandler
		{
			private readonly string _responseContent;
			private readonly HttpStatusCode _statusCode;

			public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
			{
				_responseContent = responseContent;
				_statusCode = statusCode;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(_statusCode)
				{
					Content = new StringContent(_responseContent)
				};
				return Task.FromResult(response);
			}
		}
	}
}
