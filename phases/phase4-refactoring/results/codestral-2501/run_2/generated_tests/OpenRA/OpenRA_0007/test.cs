using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BeaconLib;
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
        public void UpdateMasterServer_SetsIsBusyFlag()
        {
            // Arrange
            var pinger = new MasterServerPinger();
            var endpoints = new List<IPEndPoint> { new IPEndPoint(IPAddress.Loopback, 1234) };
            var mod = new Manifest();
            var modData = new ModData(mod, new InstalledMods(), true);
            var server = new S(endpoints, new ServerSettings(), modData, ServerType.PrivateLobby);
            var postData = "testPostData";

            // Make UpdateMasterServer public for testing
            var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(pinger, new object[] { server, postData });

            // Assert
            var isBusyField = typeof(MasterServerPinger).GetField("isBusy", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.True((bool)isBusyField.GetValue(pinger));
        }

        [Fact]
        public void UpdateMasterServer_EnqueuesConnectedMessage()
        {
            // Arrange
            var pinger = new MasterServerPinger();
            var endpoints = new List<IPEndPoint> { new IPEndPoint(IPAddress.Loopback, 1234) };
            var mod = new Manifest();
            var modData = new ModData(mod, new InstalledMods(), true);
            var server = new S(endpoints, new ServerSettings(), modData, ServerType.PrivateLobby);
            var postData = "testPostData";

            // Make UpdateMasterServer public for testing
            var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(pinger, new object[] { server, postData });

            // Assert
            var masterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", BindingFlags.NonPublic | BindingFlags.Instance);
            var masterServerMessages = (Queue<string>)masterServerMessagesField.GetValue(pinger);
            Assert.Contains("notification-master-server-connected", masterServerMessages);
        }

        [Fact]
        public void UpdateMasterServer_EnqueuesErrorMessage()
        {
            // Arrange
            var pinger = new MasterServerPinger();
            var endpoints = new List<IPEndPoint> { new IPEndPoint(IPAddress.Loopback, 1234) };
            var mod = new Manifest();
            var modData = new ModData(mod, new InstalledMods(), true);
            var server = new S(endpoints, new ServerSettings(), modData, ServerType.PrivateLobby);
            var postData = "testPostData";

            // Make UpdateMasterServer public for testing
            var method = typeof(MasterServerPinger).GetMethod("UpdateMasterServer", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(pinger, new object[] { server, postData });

            // Assert
            var masterServerMessagesField = typeof(MasterServerPinger).GetField("masterServerMessages", BindingFlags.NonPublic | BindingFlags.Instance);
            var masterServerMessages = (Queue<string>)masterServerMessagesField.GetValue(pinger);
            Assert.Contains("notification-master-server-error", masterServerMessages);
        }
    }
}
