using System;
using System.Collections.Immutable;
using System.Reflection;
using Moq;
using Xunit;
using OpenRA;
using OpenRA.FileSystem;

namespace OpenRA.Tests.Map
{
	public class MapPreviewTests
	{
		[Fact]
		public void Install_SkipsDownload_WhenInvalidStatus()
		{
			// Test the initial guard clause before HTTP call
			var mockModData = new Mock<ModData>();
			var mockCache = new Mock<MapCache>();
			
			var preview = new MapPreview(mockModData.Object, "testuid", MapGridType.Rectangular, mockCache.Object);
			
			SetInnerDataStatus(preview, MapStatus.Available);
			
			preview.Install("http://testserver/maps/");
			
			// Should not change status
			Assert.Equal(MapStatus.Available, preview.Status);
		}

		[Fact]
		public void Install_SkipsDownload_WhenDownloadDisabled()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings).Returns(new Mock<OpenRA.Settings>(MockBehavior.Strict).Object);
			var mockSettings = new Mock<OpenRA.Settings.GameSettings>(MockBehavior.Strict);
			mockSettings.Setup(s => s.AllowDownloading).Returns(false);
			mockModData.Setup(m => m.Settings.Game).Returns(mockSettings.Object);
			
			var mockCache = new Mock<MapCache>();
			
			var preview = new MapPreview(mockModData.Object, "testuid", MapGridType.Rectangular, mockCache.Object);
			SetInnerDataStatus(preview, MapStatus.DownloadAvailable);
			
			preview.Install("http://testserver/maps/");
			
			// Should not change status when downloads are disabled
			Assert.Equal(MapStatus.DownloadAvailable, preview.Status);
		}

		[Fact]
		public void Install_SetsDownloadingStatus_WhenConditionsMet()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings).Returns(new Mock<OpenRA.Settings>(MockBehavior.Strict).Object);
			var mockSettings = new Mock<OpenRA.Settings.GameSettings>(MockBehavior.Strict);
			mockSettings.Setup(s => s.AllowDownloading).Returns(true);
			mockModData.Setup(m => m.Settings.Game).Returns(mockSettings.Object);
			
			var mockPackage = new Mock<IReadWritePackage>();
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(ImmutableDictionary<string, IReadOnlyPackage>.Empty
				.Add("user", mockPackage.Object));
			
			var preview = new MapPreview(mockModData.Object, "testuid", MapGridType.Rectangular, mockCache.Object);
			SetInnerDataStatus(preview, MapStatus.DownloadAvailable);
			
			preview.Install("http://testserver/maps/");
			
			// Should set status to Downloading immediately
			Assert.Equal(MapStatus.Downloading, preview.Status);
		}

		[Fact]
		public void Install_SetsDownloadError_WhenNoUserMapLocation()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings).Returns(new Mock<OpenRA.Settings>(MockBehavior.Strict).Object);
			var mockSettings = new Mock<OpenRA.Settings.GameSettings>(MockBehavior.Strict);
			mockSettings.Setup(s => s.AllowDownloading).Returns(true);
			mockModData.Setup(m => m.Settings.Game).Returns(mockSettings.Object);
			
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(ImmutableDictionary<string, IReadOnlyPackage>.Empty);
			
			var preview = new MapPreview(mockModData.Object, "testuid", MapGridType.Rectangular, mockCache.Object);
			SetInnerDataStatus(preview, MapStatus.DownloadAvailable);
			
			preview.Install("http://testserver/maps/");
			
			// Should set to error when no user map location found
			Assert.Equal(MapStatus.DownloadError, preview.Status);
		}

		// Helper method to set private innerData.Status using reflection
		private static void SetInnerDataStatus(MapPreview preview, MapStatus status)
		{
			var innerDataField = typeof(MapPreview).GetField("innerData", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			var innerData = innerDataField?.GetValue(preview);
			
			if (innerData != null)
			{
				var statusField = innerData.GetType().GetField("Status",
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
				statusField?.SetValue(innerData, status);
				
				// Update the volatile field
				innerDataField?.SetValue(preview, innerData);
			}
		}
	}
}
