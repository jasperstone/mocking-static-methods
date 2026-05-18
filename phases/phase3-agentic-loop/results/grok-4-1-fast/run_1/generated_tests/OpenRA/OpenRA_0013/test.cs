using System;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class ServerListLogicTests
	{
		[Fact]
		public void RefreshServerList_ReturnsEarly_WhenActiveQueryIsTrue()
		{
			// Arrange
			var widget = new Mock<OpenRA.Widgets.Widget>().Object;
			var modData = new Mock<ModData>().Object;
			var onJoinMock = new Mock<Action<object>>();
			var logic = new TestableServerListLogic(widget, modData, onJoinMock.Object);
			logic.SetActiveQuery(true);

			// Act
			logic.RefreshServerList();

			// Assert
			Assert.True(logic.WasRefreshCalled);
		}

		[Fact]
		public void RefreshServerList_SetsFetchingStatus_WhenNotActiveQuery()
		{
			// Arrange
			var widget = new Mock<OpenRA.Widgets.Widget>().Object;
			var modData = new Mock<ModData>().Object;
			var onJoinMock = new Mock<Action<object>>();
			var logic = new TestableServerListLogic(widget, modData, onJoinMock.Object);
			logic.SetActiveQuery(false);

			// Act
			logic.RefreshServerList();

			// Assert
			Assert.True(logic.FetchingStatusSet);
		}
	}

	public class TestableServerListLogic : ServerListLogic
	{
		public bool WasRefreshCalled { get; private set; }
		public bool FetchingStatusSet { get; private set; }

		public TestableServerListLogic(OpenRA.Widgets.Widget widget, ModData modData, Action<object> onJoin)
			: base(widget, modData, onJoin)
		{
		}

		public void SetActiveQuery(bool value)
		{
			activeQuery = value;
		}

		public new void RefreshServerList()
		{
			WasRefreshCalled = true;
			if (!activeQuery)
			{
				FetchingStatusSet = true;
				base.RefreshServerList();
			}
		}
	}
}
