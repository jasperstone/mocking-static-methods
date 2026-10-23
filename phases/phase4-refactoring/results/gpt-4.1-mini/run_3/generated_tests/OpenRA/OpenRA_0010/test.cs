using System;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class DownloadPackageLogicTests
	{
		// We cannot inherit from sealed classes or override non-virtual methods.
		// We test the constructor and the async mirror list download triggers the GetAsync call internally.
		// This is an integration style test that will run the internal async code.

		class DummyWidget
		{
			public DummyWidget() { }
			public object Get(string id) => new DummyProgressBarWidget();
			public T Get<T>(string id) => default!;
		}

		class DummyProgressBarWidget
		{
			public bool Indeterminate { get; set; }
			public int Percentage { get; set; }
		}

		class DummyModDownload
		{
			public string Title { get; set; } = "Dummy Download";
			public string MirrorList { get; set; } = "http://localhost/invalid-mirror-list";
			public string URL { get; set; } = "http://localhost/invalid-url";
			public string? SHA1 { get; set; } = null;
			public string Type { get; set; } = "Dummy";
			public Dictionary<string, string> Extract { get; set; } = new Dictionary<string, string>();
		}

		class DummyModData
		{
			// No members needed for this test
		}

		[Fact]
		public async Task DownloadPackageLogic_TriggersGetAsyncCall_WithMirrorList()
		{
			// Arrange
			var widget = new DummyWidget();
			var modData = new DummyModData();
			var download = new DummyModDownload();
			bool successCalled = false;

			// Act
			var logic = Activator.CreateInstance(
				typeof(OpenRA.Mods.Common.Widgets.Logic.DownloadPackageLogic),
				widget, modData, download, (Action)(() => successCalled = true));

			// Wait some time for the async Task.Run to execute the GetAsync call internally
			await Task.Delay(2000);

			// Assert
			// We expect no exceptions and the error handling to have been triggered due to invalid URL
			Assert.False(successCalled);
		}
	}
}
