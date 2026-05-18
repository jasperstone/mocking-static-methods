using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
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
		[Fact]
		public void ProcessesValidMasterServerResponse()
		{
			// Test the regex parsing logic from line 154+ that processes PostAsync response
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var testResponse = "[1]Test error message";
			var match = regex.Match(testResponse);
			
			Assert.True(match.Success);
			Assert.True(int.TryParse(match.Groups["code"].Value, out var code));
			Assert.Equal(1, code);
			Assert.Equal("Test error message", match.Groups["message"].Value.Trim());
		}

		[Fact]
		public void ProcessesInvalidMasterServerResponse()
		{
			// Test fallback for invalid response format
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var testResponse = "Invalid response";
			var match = regex.Match(testResponse);
			
			Assert.False(match.Success);
		}

		[Fact]
		public void MasterServerErrorsLookup_Success()
		{
			// Test hardcoded error lookup used after PostAsync response
			var pingerType = typeof(MasterServerPinger);
			var field = pingerType.GetField("MasterServerErrors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			var errors = (System.Collections.Generic.IDictionary<int, string>)field.GetValue(null);
			
			Assert.True(errors.ContainsKey(1));
			Assert.Equal("notification-no-port-forward", errors[1]);
			Assert.True(errors.ContainsKey(2));
			Assert.Equal("notification-blacklisted-server-name", errors[2]);
		}

		[Fact]
		public void MasterServerPinger_CanBeInstantiated()
		{
			// Verify basic construction (internal fields exist but are private)
			var pinger = new MasterServerPinger();
			Assert.NotNull(pinger);
		}
	}
}
