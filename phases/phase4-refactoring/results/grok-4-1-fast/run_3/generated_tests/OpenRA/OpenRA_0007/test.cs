using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		private static readonly FieldInfo IsBusyField = typeof(MasterServerPinger).GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo IsInitialPingField = typeof(MasterServerPinger).GetField("isInitialPing", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo MasterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo LastChangedField = typeof(MasterServerPinger).GetField("lastChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;
		private static readonly FieldInfo LastPingField = typeof(MasterServerPinger).GetField("lastPing", BindingFlags.NonPublic | BindingFlags.Instance)!;

		private Server CreateMinimalServer()
		{
			var modData = new ModData(null!);
			var webServices = new WebServices();
			modData.AddOverride<WebServices>(webServices);
			var server = new Server(modData, new ServerSettings(null!, null!, null!));
			return server;
		}

		[Fact]
		public void ErrorResponseParsing_ExtractsErrorCodeAndMessage()
		{
			var responseText = "[1]No port forwarding detected";
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var match = regex.Match(responseText);

			Assert.True(match.Success);
			Assert.True(int.TryParse(match.Groups["code"].Value, out var code));
			Assert.Equal(1, code);
			Assert.Equal("No port forwarding detected", match.Groups["message"].Value.Trim());
		}

		[Fact]
		public void ErrorResponseParsing_FallbackToInvalidErrorCode()
		{
			var responseText = "Invalid response format";
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var match = regex.Match(responseText);

			var errorMessage = match.Success && int.TryParse(match.Groups["code"].Value, out _) ?
				match.Groups["message"].Value.Trim() : "notification-invalid-error-code";

			Assert.Equal("notification-invalid-error-code", errorMessage);
		}

		[Fact]
		public void MasterServerErrors_LookupKnownErrorCodes()
		{
			var errorField = typeof(MasterServerPinger).GetField("MasterServerErrors", BindingFlags.NonPublic | BindingFlags.Static)!;
			var errors = (System.Collections.Generic.IDictionary<int, string>)errorField.GetValue(null)!;

			Assert.True(errors.ContainsKey(1));
			Assert.Equal("notification-no-port-forward", errors[1]);
			Assert.True(errors.ContainsKey(2));
			Assert.Equal("notification-blacklisted-server-name", errors[2]);
		}

		[Fact]
		public async Task UpdateMasterServer_SetsIsBusyTrueThenFalse()
		{
			var server = CreateMinimalServer();
			var pinger = new MasterServerPinger();
			var updateMethod = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance)!;
			
			updateMethod.Invoke(pinger, [server, "test-data"]);

			// Wait for Task.Run to execute and complete
			await Task.Delay(1500);

			// Verify isBusy was set to true then back to false after completion
			Assert.False((bool)IsBusyField.GetValue(pinger));
		}

		[Fact]
		public async Task UpdateMasterServer_FirstCall_InitializesAndAddsMessages()
		{
			var server = CreateMinimalServer();
			var pinger = new MasterServerPinger();
			var updateMethod = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance)!;
			
			updateMethod.Invoke(pinger, [server, "test-data"]);

			await Task.Delay(1500);

			// Verify initial ping processed (isInitialPing = false) and messages enqueued
			Assert.False((bool)IsInitialPingField.GetValue(pinger));
			
			var messages = (Queue<string>)MasterServerMessagesField.GetValue(pinger)!;
			Assert.NotEmpty(messages);
		}

		[Fact]
		public void Tick_SkipsNonMultiplayerServers()
		{
			var server = CreateMinimalServer();
			server.Settings = new ServerSettings(null!, null!, null!) { AdvertiseOnline = false };
			
			var pinger = new MasterServerPinger();
			
			pinger.Tick(server);

			Assert.False((bool)IsBusyField.GetValue(pinger));
		}

		[Fact]
		public void Tick_TriggersUpdate_OnInitialPing()
		{
			var server = CreateMinimalServer();
			server.Settings = new ServerSettings(null!, null!, null!) { AdvertiseOnline = true };
			
			var pinger = new MasterServerPinger();
			LastPingField.SetValue(pinger, 0);
			
			pinger.Tick(server);

			Assert.True((bool)IsBusyField.GetValue(pinger));
		}
	}
}
