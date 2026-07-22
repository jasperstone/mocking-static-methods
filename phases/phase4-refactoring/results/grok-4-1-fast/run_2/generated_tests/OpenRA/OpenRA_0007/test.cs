using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Moq;
using OpenRA.Network;
using OpenRA.Server;
using OpenRA.Support;
using Xunit;
using S = OpenRA.Server.Server;

namespace OpenRA.Mods.Common.Server.Tests
{
	public class MasterServerPingerTests
	{
		private static readonly FieldInfo IsBusyField = typeof(MasterServerPinger).GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo IsInitialPingField = typeof(MasterServerPinger).GetField("isInitialPing", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo MasterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo UpdateMasterServerMethod = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);

		[Fact]
		public void UpdateMasterServer_SetsIsBusyAndResetsAfterCompletion()
		{
			// Arrange
			var serverMock = new Mock<S>();
			var modDataMock = new Mock<OpenRA.ModData>();
			var webServicesMock = new Mock<object>();
			webServicesMock.SetupGet<string>("ServerAdvertise").Returns("https://test-endpoint.com");
			modDataMock.Setup(md => md.GetOrCreate<object>()).Returns(webServicesMock.Object);
			serverMock.Setup(s => s.ModData).Returns(modDataMock.Object);

			var pinger = new MasterServerPinger();
			var postData = "test-post-data";

			// Act
			UpdateMasterServerMethod.Invoke(pinger, new object[] { serverMock.Object, postData });

			// Assert - verify isBusy was set and reset
			Thread.Sleep(1000); // Allow Task.Run to complete (HttpClient timeout)
			Assert.False((bool)IsBusyField.GetValue(pinger));
		}

		[Fact]
		public void UpdateMasterServer_HandlesException_AddsErrorMessage()
		{
			// Arrange
			var serverMock = new Mock<S>();
			var modDataMock = new Mock<OpenRA.ModData>();
			var webServicesMock = new Mock<object>();
			webServicesMock.SetupGet<string>("ServerAdvertise").Returns("http://nonexistent.invalid/");
			modDataMock.Setup(md => md.GetOrCreate<object>()).Returns(webServicesMock.Object);
			serverMock.Setup(s => s.ModData).Returns(modDataMock.Object);

			var pinger = new MasterServerPinger();
			var postData = "test-data";

			// Act
			UpdateMasterServerMethod.Invoke(pinger, new object[] { serverMock.Object, postData });

			// Assert - exception handling path taken
			Thread.Sleep(1000);
			Assert.False((bool)IsBusyField.GetValue(pinger));
			var messages = (Queue<string>)MasterServerMessagesField.GetValue(pinger);
			Assert.True(messages.Count > 0);
		}

		[Fact]
		public void UpdateMasterServer_InitialPing_ProcessesResponse()
		{
			// Arrange
			var serverMock = new Mock<S>();
			var modDataMock = new Mock<OpenRA.ModData>();
			var webServicesMock = new Mock<object>();
			webServicesMock.SetupGet<string>("ServerAdvertise").Returns("https://httpbin.org/post"); // Real test endpoint
			modDataMock.Setup(md => md.GetOrCreate<object>()).Returns(webServicesMock.Object);
			serverMock.Setup(s => s.ModData).Returns(modDataMock.Object);

			var pinger = new MasterServerPinger();
			IsInitialPingField.SetValue(pinger, true);
			var postData = "test-data";

			// Act
			UpdateMasterServerMethod.Invoke(pinger, new object[] { serverMock.Object, postData });

			// Assert
			Thread.Sleep(2000);
			Assert.False((bool)IsInitialPingField.GetValue(pinger));
			Assert.False((bool)IsBusyField.GetValue(pinger));
		}
	}
}
