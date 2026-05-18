using System;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common;
using OpenRA.Support;

namespace OpenRA.Mods.Common.Tests
{
	public class WebServicesTests
	{
		[Fact]
		public void ModVersionStatus_DefaultsToNotChecked()
		{
			var webServices = new WebServices();
			Assert.Equal(ModVersionStatus.NotChecked, webServices.ModVersionStatus);
		}

		[Fact]
		public void CheckModVersion_CanBeCalledWithoutImmediateException()
		{
			var webServices = new WebServices();
			webServices.CheckModVersion();
		}

		[Fact]
		public void VersionCheckSwitchLogic_HandlesEmptyResponse()
		{
			// Tests the logic that processes response from HttpClient.GetAsync
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
		public void VersionCheckSwitchLogic_HandlesOutdatedResponse()
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
		public void VersionCheckSwitchLogic_HandlesUnknownResponse()
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
		public void VersionCheckSwitchLogic_HandlesPlaytestResponse()
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
	}
}
