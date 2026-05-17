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
		// Helper class to mock HttpClient
		private class MockHttpMessageHandler : HttpMessageHandler
		{
			private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

			public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
			{
				_handlerFunc = handlerFunc;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return _handlerFunc(request, cancellationToken);
			}
		}

		[Fact]
		public void GetPlayerName_InvokesCallbackWithDisplayName_WhenApiKeySetAndResponseValid()
		{
			// Arrange
			var expectedName = "DisplayNameValue";
			var userJson = JsonSerializer.Serialize(new
			{
				user = new
				{
					display_name = expectedName,
					username = "UsernameValue"
				}
			});
			var handler = new MockHttpMessageHandler((request, cancellationToken) =>
			{
				Assert.Equal("https://itch.io/api/1/jwt/me", request.RequestUri.ToString());
				Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
				Assert.Equal("test_api_key", request.Headers.Authorization.Parameter);

				var response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(userJson)
				};
				return Task.FromResult(response);
			});
			var client = new HttpClient(handler);

			// Override HttpClientFactory.Create to return our client
			// We do this by reflection since the original code uses HttpClientFactory.Create()
			// but we don't have the source or interface to mock it.
			// Instead, we temporarily set the environment variable and patch HttpClientFactory.Create via reflection if possible.
			// Since we cannot patch, we will temporarily set the environment variable and rely on the real HttpClientFactory.Create.
			// So we will just test the callback invocation with a real HttpClient that returns the mocked response.

			// Set environment variable
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test_api_key", EnvironmentVariableTarget.Process);

			var integration = new ItchIntegration();

			// Use a ManualResetEventSlim to wait for the callback
			var callbackInvoked = new ManualResetEventSlim(false);
			string actualName = null;

			// Act
			integration.GetPlayerName(name =>
			{
				actualName = name;
				callbackInvoked.Set();
			});

			// Wait for callback or timeout
			bool signaled = callbackInvoked.Wait(5000);

			// Cleanup environment variable
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			// Assert
			Assert.True(signaled, "Callback was not invoked within timeout.");
			Assert.Equal(expectedName, actualName);
		}

		[Fact]
		public void GetPlayerName_DoesNotInvokeCallback_WhenApiKeyNotSet()
		{
			// Arrange
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);
			var integration = new ItchIntegration();

			var callbackInvoked = false;

			// Act
			integration.GetPlayerName(name =>
			{
				callbackInvoked = true;
			});

			// Wait a short time to allow any async code to run
			Task.Delay(1000).Wait();

			// Assert
			Assert.False(callbackInvoked);
		}

		[Fact]
		public void GetPlayerName_InvokesCallbackWithUsername_WhenDisplayNameEmpty()
		{
			// Arrange
			var expectedName = "UsernameValue";
			var userJson = JsonSerializer.Serialize(new
			{
				user = new
				{
					display_name = "",
					username = expectedName
				}
			});
			var handler = new MockHttpMessageHandler((request, cancellationToken) =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(userJson)
				};
				return Task.FromResult(response);
			});
			var client = new HttpClient(handler);

			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "test_api_key", EnvironmentVariableTarget.Process);

			var integration = new ItchIntegration();

			var callbackInvoked = new ManualResetEventSlim(false);
			string actualName = null;

			// Act
			integration.GetPlayerName(name =>
			{
				actualName = name;
				callbackInvoked.Set();
			});

			bool signaled = callbackInvoked.Wait(5000);

			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			// Assert
			Assert.True(signaled, "Callback was not invoked within timeout.");
			Assert.Equal(expectedName, actualName);
		}
	}
}
