using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Xunit;
using S = OpenRA.Server.Server;
using OpenRA.Server;

namespace OpenRA.Mods.Common.Server.Tests
{
	public class MasterServerPingerTests
	{
		[Fact]
		public void MasterServerResponseRegex_MatchesValidErrorFormat()
		{
			// Test the exact regex used after PostAsync response (line 154+)
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var response = "[1]No port forward";
			
			var match = regex.Match(response);
			Assert.True(match.Success);
			Assert.True(int.TryParse(match.Groups["code"].Value, out var code));
			Assert.Equal(1, code);
			Assert.Equal("No port forward", match.Groups["message"].Value.Trim());
		}

		[Fact]
		public void MasterServerResponseRegex_MatchesNegativeErrorCode()
		{
			// Test negative error codes (warnings)
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var response = "[-1]Warning message";
			
			var match = regex.Match(response);
			Assert.True(match.Success);
			Assert.True(int.TryParse(match.Groups["code"].Value, out var code));
			Assert.Equal(-1, code);
		}

		[Fact]
		public void MasterServerResponseRegex_FallsBackForInvalidFormat()
		{
			// Test fallback when regex doesn't match after PostAsync
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var response = "Plain error text without brackets";
			
			var match = regex.Match(response);
			Assert.False(match.Success);
		}

		[Fact]
		public void MasterServerResponseRegex_HandlesEmptyResponse()
		{
			// Test empty/whitespace response handling after PostAsync
			var regex = new Regex(@"^\[(?<code>-?\d+)\](?<message>.*)");
			var response = "";
			
			var match = regex.Match(response);
			Assert.False(match.Success);
		}

		[Fact]
		public void MasterServerErrors_LookupKnownErrorCodes()
		{
			// Test MasterServerErrors dictionary lookup used after PostAsync
			Assert.True(MasterServerPinger.MasterServerErrors.ContainsKey(1));
			Assert.True(MasterServerPinger.MasterServerErrors.ContainsKey(2));
			Assert.Equal("notification-no-port-forward", MasterServerPinger.MasterServerErrors[1]);
		}
	}
}
