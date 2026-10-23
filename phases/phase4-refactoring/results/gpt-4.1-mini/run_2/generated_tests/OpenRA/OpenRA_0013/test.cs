using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class ServerListLogicTests
	{
		[Fact]
		public void RefreshServerList_SetsSearchStatusFetching()
		{
			// Arrange
			var modData = new DummyModData();
			var logic = new ServerListLogic(new DummyWidget(), modData, _ => { });

			// Act
			logic.RefreshServerList();

			// Assert
			var searchStatusField = typeof(ServerListLogic).GetField("searchStatus", BindingFlags.NonPublic | BindingFlags.Instance);
			var searchStatusValue = searchStatusField.GetValue(logic).ToString();
			Assert.Equal("Fetching", searchStatusValue);
		}

		[Fact]
		public async Task RefreshServerList_DoesNotThrow()
		{
			var modData = new DummyModData();
			var logic = new ServerListLogic(new DummyWidget(), modData, _ => { });

			// Act
			logic.RefreshServerList();

			// Wait a bit for the async task to run
			await Task.Delay(1000);
		}

		// Dummy implementations to satisfy constructor dependencies
		class DummyWidget : OpenRA.Widgets.IWidget
		{
			public T Get<T>(string name) => default!;
			public T? GetOrNull<T>(string name) => default;
		}

		class DummyModData
		{
			public T GetOrCreate<T>()
			{
				if (typeof(T) == typeof(WebServices))
					return (T)(object)new WebServices();
				throw new NotImplementedException();
			}
		}

		class WebServices
		{
			public string ServerList => "http://localhost";
			public ModVersionStatus ModVersionStatus => ModVersionStatus.Latest;
		}

		enum ModVersionStatus
		{
			NotChecked,
			Latest,
			Outdated,
			Unknown,
			PlaytestAvailable
		}
	}
}
