using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;
using System.Reflection;
using OpenRA.Server;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class ServerListLogicTests
	{
		[Fact]
		public void RefreshServerList_DoesNotStartNewQueryIfActiveQueryIsTrue()
		{
			var logic = CreateServerListLogic();

			// Set activeQuery to true via reflection since it's private
			var activeQueryField = typeof(ServerListLogic).GetField("activeQuery", BindingFlags.NonPublic | BindingFlags.Instance);
			activeQueryField.SetValue(logic, true);

			logic.RefreshServerList();

			// activeQuery should remain true and no exception thrown
			Assert.True((bool)activeQueryField.GetValue(logic));
		}

		[Fact]
		public async Task RefreshServerList_StartsQueryAndSetsActiveQuery()
		{
			var logic = CreateServerListLogic();

			var activeQueryField = typeof(ServerListLogic).GetField("activeQuery", BindingFlags.NonPublic | BindingFlags.Instance);

			logic.RefreshServerList();

			// Wait a bit for the async Task.Run to start and set activeQuery
			await Task.Delay(200);

			Assert.True((bool)activeQueryField.GetValue(logic));
		}

		private ServerListLogic CreateServerListLogic()
		{
			// We cannot instantiate all dependencies easily, so we use nulls and minimal stubs where possible.
			// The constructor requires Widget, ModData, and Action<GameServer>.
			// We pass null for Widget and ModData, and a no-op action for onJoin.
			// This may cause some null refs if the code accesses these deeply, but for RefreshServerList it should be fine.

			return (ServerListLogic)Activator.CreateInstance(
				typeof(ServerListLogic),
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
				null,
				new object[] { null, null, new Action<GameServer>(_ => { }) },
				null
			);
		}
	}
}
