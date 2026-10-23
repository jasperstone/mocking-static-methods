using System;
using Xunit;
using OpenRA.Mods.Common.Server;

namespace OpenRA.Mods.Common.Tests.Server
{
	// Minimal fake server-like class with necessary members for MasterServerPinger.Tick
	class FakeServer
	{
		public bool IsMultiplayer => true;
		public FakeSettings Settings { get; } = new FakeSettings();
		public FakeModData ModData { get; } = new FakeModData();

		public void SendFluentMessage(string message)
		{
			// No-op
		}
	}

	class FakeSettings
	{
		public bool AdvertiseOnline { get; set; } = true;
		public bool AdvertiseOnLocalNetwork { get; set; } = false;
	}

	class FakeModData
	{
		public WebServices GetOrCreate<WebServices>() where WebServices : new()
		{
			return new WebServices();
		}
	}

	class WebServices
	{
		public string ServerAdvertise { get; set; } = "http://localhost";
	}

	public class MasterServerPingerTests
	{
		[Fact]
		public void Tick_DoesNotThrow()
		{
			var pinger = new MasterServerPinger();
			var server = new FakeServer();

			// Call Tick which triggers UpdateMasterServer and the PostAsync call internally
			pinger.Tick(server);

			// No exception means the code path was exercised
			Assert.True(true);
		}
	}
}
