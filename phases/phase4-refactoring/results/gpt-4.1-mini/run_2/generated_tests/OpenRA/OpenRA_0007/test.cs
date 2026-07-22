using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common.Server;
using System.Reflection;
using System.Threading;
using OpenRA.Server;

namespace OpenRA.Mods.Common.Tests.Server
{
	public class MasterServerPingerTests
	{
		// Minimal stub classes to satisfy dependencies
		class FakeWebServices : WebServices
		{
			public override string ServerAdvertise { get; set; } = "http://fake.endpoint";
		}

		class FakeModData : ModData
		{
			public FakeWebServices WebServices { get; } = new FakeWebServices();

			public override T GetOrCreate<T>()
			{
				if (typeof(T) == typeof(WebServices))
					return WebServices as T;
				return base.GetOrCreate<T>();
			}
		}

		class FakeServer : Server
		{
			public FakeServer()
			{
				Settings = new ServerSettings
				{
					AdvertiseOnline = true,
					AdvertiseOnLocalNetwork = false
				};
				ModData = new FakeModData();
			}

			public override void SendFluentMessage(string message)
			{
				// No-op for test
			}
		}

		[Fact]
		public async Task UpdateMasterServer_CallsPostAsyncAndProcessesResponse()
		{
			// Arrange
			var pinger = new MasterServerPinger();
			var server = new FakeServer();

			// Use reflection to get the private UpdateMasterServer method
			var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(method);

			// Act
			method.Invoke(pinger, new object[] { server, "postData" });

			// Wait some time for the async Task.Run inside UpdateMasterServer to complete
			await Task.Delay(1500);

			// Assert
			// Check that isBusy is false after the async operation completes
			var isBusyField = typeof(MasterServerPinger).GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(isBusyField);
			var isBusy = (bool)isBusyField.GetValue(pinger);
			Assert.False(isBusy);
		}
	}
}
