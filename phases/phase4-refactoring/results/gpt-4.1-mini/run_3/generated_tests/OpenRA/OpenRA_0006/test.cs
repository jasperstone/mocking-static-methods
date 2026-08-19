using System;
using System.Threading;
using Xunit;
using OpenRA.Mods.Common;

namespace OpenRA.Mods.Common.Tests
{
	public class ItchIntegrationTests
	{
		[Fact]
		public void GetPlayerName_NoApiKey_DoesNotInvokeCallback()
		{
			// Arrange
			var integration = new ItchIntegration();
			string result = null;
			var callbackInvoked = new ManualResetEventSlim(false);

			// Act
			integration.GetPlayerName(name =>
			{
				result = name;
				callbackInvoked.Set();
			});

			// Wait a short time to see if callback is invoked
			bool invoked = callbackInvoked.Wait(500);

			// Assert
			Assert.False(invoked);
			Assert.Null(result);
		}

		// Note: We cannot easily test the HttpClient.GetAsync call directly or mock it without refactoring.
		// This test covers the code path where the API key is missing, so the call to GetAsync is not made.
	}
}
