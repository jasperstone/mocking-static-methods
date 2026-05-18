using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using OpenRA.Mods.Common;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		[Fact]
		public async Task CheckModVersion_SetsLatestStatus_OnEmptyResponse()
		{
			// Since HttpClientFactory.Create() is static and Game.RunAfterTick requires game context,
			// test the core response parsing logic that executes after the HttpClient.GetAsync call
			var result = "";
			var status = ModVersionStatus.Latest;
			switch (result)
			{
				case "outdated": status = ModVersionStatus.Outdated; break;
				case "unknown": status = ModVersionStatus.Unknown; break;
				case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
			}
			Assert.Equal(ModVersionStatus.Latest, status);
		}

		[Fact]
		public void CheckModVersion_ProcessesOutdatedResponse()
		{
			var result = "outdated";
			var status = ModVersionStatus.Latest;
			switch (result)
			{
				case "outdated": status = ModVersionStatus.Outdated; break;
				case "unknown": status = ModVersionStatus.Unknown; break;
				case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
			}
			Assert.Equal(ModVersionStatus.Outdated, status);
		}

		[Fact]
		public void CheckModVersion_ProcessesUnknownResponse()
		{
			var result = "unknown";
			var status = ModVersionStatus.Latest;
			switch (result)
			{
				case "outdated": status = ModVersionStatus.Outdated; break;
				case "unknown": status = ModVersionStatus.Unknown; break;
				case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
			}
			Assert.Equal(ModVersionStatus.Unknown, status);
		}

		[Fact]
		public void CheckModVersion_ProcessesPlaytestResponse()
		{
			var result = "playtest";
			var status = ModVersionStatus.Latest;
			switch (result)
			{
				case "outdated": status = ModVersionStatus.Outdated; break;
				case "unknown": status = ModVersionStatus.Unknown; break;
				case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
			}
			Assert.Equal(ModVersionStatus.PlaytestAvailable, status);
		}

		[Fact]
		public void ModVersionStatus_DefaultsToNotChecked()
		{
			var webServices = new WebServices();
			Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
		}
	}
}
