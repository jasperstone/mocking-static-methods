using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using OpenRA.Mods.Common.Server;
using S = OpenRA.Server.Server;

namespace OpenRA.Mods.Common.Tests.ServerTraits
{
	public class MasterServerPingerTests
	{
		private static readonly FieldInfo IsBusyField = typeof(MasterServerPinger).GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo IsInitialPingField = typeof(MasterServerPinger).GetField("isInitialPing", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo MasterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly MethodInfo UpdateMasterServerMethod = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance)!;

		[Fact]
		public void UpdateMasterServer_SetsIsBusyToTrue()
		{
			// Arrange
			var server = Mock.Of<S>();
			var pinger = new MasterServerPinger();

			// Act
			UpdateMasterServerMethod.Invoke(pinger, new object[] { server, "test-data" });

			// Assert
			Assert.True((bool)IsBusyField.GetValue(pinger));
		}

		[Fact]
		public async Task UpdateMasterServer_CallsHttpClientPostAsync()
		{
			// Arrange
			var server = Mock.Of<S>();
			var mockModData = new Mock<OpenRA.ModData>();
			var mockWebServices = new Mock<OpenRA.Mods.Common.Traits.WebServices>();
			mockWebServices.Setup(x => x.ServerAdvertise).Returns("https://test-endpoint");
			mockModData.Setup(x => x.GetOrCreate<OpenRA.Mods.Common.Traits.WebServices>()).Returns(mockWebServices.Object);
			Mock.Get(server).Setup(x => x.ModData).Returns(mockModData.Object);

			var mockClient = new Mock<HttpClient>();
			var mockResponse = new Mock<HttpResponseMessage>(System.Net.HttpStatusCode.OK);
			mockResponse.Setup(r => r.Content).Returns(new StringContent("[0]Success"));
			mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync(mockResponse.Object);

			MockStaticHttpClientFactory(mockClient.Object);

			try
			{
				var pinger = new MasterServerPinger();

				// Act
				UpdateMasterServerMethod.Invoke(pinger, new object[] { server, "test-post-data" });

				// Wait for background task
				await Task.Delay(100);

				// Assert
				mockClient.Verify(c => c.PostAsync(It.Is<string>(u => u == "https://test-endpoint"), It.IsAny<HttpContent>()), Times.Once);
			}
			finally
			{
				RestoreHttpClientFactory();
			}
		}

		[Fact]
		public async Task UpdateMasterServer_HandlesInitialPingResponse()
		{
			// Arrange
			var server = Mock.Of<S>();
			var mockClient = new Mock<HttpClient>();
			var mockResponse = new Mock<HttpResponseMessage>(System.Net.HttpStatusCode.OK);
			mockResponse.Setup(r => r.Content).Returns(new StringContent("[0]Success"));
			mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ReturnsAsync(mockResponse.Object);

			MockStaticHttpClientFactory(mockClient.Object);

			try
			{
				var pinger = new MasterServerPinger();

				// Act
				UpdateMasterServerMethod.Invoke(pinger, new object[] { server, "test-data" });

				// Wait for completion
				await Task.Delay(100);

				// Assert
				Assert.False((bool)IsInitialPingField.GetValue(pinger));
			}
			finally
			{
				RestoreHttpClientFactory();
			}
		}

		[Fact]
		public async Task UpdateMasterServer_HandlesHttpException()
		{
			// Arrange
			var server = Mock.Of<S>();
			var mockClient = new Mock<HttpClient>();
			mockClient.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ThrowsAsync(new HttpRequestException("Test exception"));

			MockStaticHttpClientFactory(mockClient.Object);

			try
			{
				var pinger = new MasterServerPinger();

				// Act
				UpdateMasterServerMethod.Invoke(pinger, new object[] { server, "test-data" });

				// Wait for task to complete and isBusy to reset
				await Task.Delay(100);

				// Assert
				Assert.False((bool)IsBusyField.GetValue(pinger));
			}
			finally
			{
				RestoreHttpClientFactory();
			}
		}

		private static void MockStaticHttpClientFactory(HttpClient fakeClient)
		{
			var factoryType = typeof(OpenRA.Mods.Common.ServerTraits.HttpClientFactory);
			var field = factoryType.GetField("_create", BindingFlags.NonPublic | BindingFlags.Static);
			if (field != null)
			{
				var original = field.GetValue(null);
				field.SetValue(null, new Func<HttpClient>(() => fakeClient));
			}
		}

		private static void RestoreHttpClientFactory()
		{
			var factoryType = typeof(OpenRA.Mods.Common.ServerTraits.HttpClientFactory);
			var field = factoryType.GetField("_create", BindingFlags.NonPublic | BindingFlags.Static);
			if (field != null)
			{
				// Reset to original behavior
				field.SetValue(null, null);
			}
		}
	}
}
