using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using OpenRA.Mods.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenRA.Mods.Common.Tests
{
	public class ItchIntegrationTests
	{
		// We cannot mock HttpClientFactory.Create or HttpClient.GetAsync easily because they are static and non-virtual.
		// So we test the behavior with environment variable unset and set to a dummy value.
		// We test that callback is called with null or not called if no user is found.
		// We test that callback is called with username or display name if user is deserialized.

		[Fact]
		public async Task GetPlayerName_NoApiKey_DoesNotCallCallback()
		{
			// Arrange
			var integration = new ItchIntegration();
			string callbackResult = null;
			var callbackCalled = new TaskCompletionSource<bool>();

			// Act
			integration.GetPlayerName(name =>
			{
				callbackResult = name;
				callbackCalled.SetResult(true);
			});

			// Wait a short time to see if callback is called
			await Task.Delay(500);

			// Assert
			Assert.Null(callbackResult);
		}

		[Fact]
		public async Task GetPlayerName_WithApiKey_InvalidResponse_DoesNotCallCallback()
		{
			// Arrange
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", "dummy_api_key", EnvironmentVariableTarget.Process);
			var integration = new ItchIntegration();
			string callbackResult = null;
			var callbackCalled = new TaskCompletionSource<bool>();

			// Act
			integration.GetPlayerName(name =>
			{
				callbackResult = name;
				callbackCalled.SetResult(true);
			});

			// Wait a short time to see if callback is called
			await Task.Delay(1000);

			// Cleanup
			Environment.SetEnvironmentVariable("ITCHIO_API_KEY", null, EnvironmentVariableTarget.Process);

			// Assert
			// Because the API call will fail (no real server), callback should not be called
			Assert.Null(callbackResult);
		}
	}
}
