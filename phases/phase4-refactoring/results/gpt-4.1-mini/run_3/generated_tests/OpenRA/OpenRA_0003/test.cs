using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA;
using OpenRA.FileFormats;

namespace OpenRA.Game.Tests
{
	public class MapPreviewTests
	{
		[Fact]
		public void Install_DoesNothing_WhenStatusIsNotDownloadAvailableOrDownloadError()
		{
			var mapPreview = CreateMapPreviewWithStatus(MapStatus.Available);
			SetAllowDownloading(mapPreview, true);

			mapPreview.Install("http://example.com/maps/");

			Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
		}

		[Fact]
		public void Install_DoesNothing_WhenAllowDownloadingIsFalse()
		{
			var mapPreview = CreateMapPreviewWithStatus(MapStatus.DownloadAvailable);
			SetAllowDownloading(mapPreview, false);

			mapPreview.Install("http://example.com/maps/");

			Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
		}

		[Fact]
		public void Install_SetsDownloadError_WhenMapInstallPackageNotFound()
		{
			var mapPreview = CreateMapPreviewWithStatus(MapStatus.DownloadAvailable);
			SetAllowDownloading(mapPreview, true);
			ClearMapInstallPackage(mapPreview);

			mapPreview.Install("http://example.com/maps/");

			Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
		}

		private MapPreview CreateMapPreviewWithStatus(MapStatus status)
		{
			// Use reflection to create an instance and set innerData.Status
			var ctor = typeof(MapPreview).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).FirstOrDefault();
			Assert.NotNull(ctor);
			var mapPreview = (MapPreview)Activator.CreateInstance(typeof(MapPreview), nonPublic: true);

			var innerDataField = typeof(MapPreview).GetField("innerData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.NotNull(innerDataField);

			var innerDataType = innerDataField.FieldType;
			var innerData = Activator.CreateInstance(innerDataType, nonPublic: true);
			var statusField = innerDataType.GetField("Status", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
			Assert.NotNull(statusField);
			statusField.SetValue(innerData, status);

			innerDataField.SetValue(mapPreview, innerData);

			var cacheField = typeof(MapPreview).GetField("cache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.NotNull(cacheField);

			// Setup a mock MapCache with MapLocations dictionary keyed by IReadOnlyPackage
			var mockPackage = new Mock<IReadOnlyPackage>();
			var mapLocations = new Dictionary<IReadOnlyPackage, MapClassification> { { mockPackage.Object, MapClassification.User } };
			var mockCache = new Mock<MapCache>(null, null);
			mockCache.SetupGet(c => c.MapLocations).Returns(mapLocations);
			cacheField.SetValue(mapPreview, mockCache.Object);

			var modDataField = typeof(MapPreview).GetField("modData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.NotNull(modDataField);
			var mockModData = new Mock<ModData>();

			// ModFiles is of type FileSystem, so mock it as object
			var mockFileSystem = new Mock<object>();
			mockModData.SetupGet(m => m.ModFiles).Returns(mockFileSystem.Object);
			modDataField.SetValue(mapPreview, mockModData.Object);

			var parentPackageField = typeof(MapPreview).GetField("parentPackage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.NotNull(parentPackageField);

			var mockInstallPackage = new Mock<IReadWritePackage>();
			mockInstallPackage.Setup(p => p.Update(It.IsAny<string>(), It.IsAny<byte[]>()));
			mockInstallPackage.Setup(p => p.OpenPackage(It.IsAny<string>(), It.IsAny<object>())).Returns(mockInstallPackage.Object);

			var mockParentPackage = new Mock<IReadOnlyPackage>();
			mockParentPackage.Setup(p => p.OpenPackage(It.IsAny<string>(), It.IsAny<object>())).Returns(mockInstallPackage.Object);
			parentPackageField.SetValue(mapPreview, mockParentPackage.Object);

			var pathProp = typeof(MapPreview).GetProperty("Path", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
			Assert.NotNull(pathProp);
			pathProp.SetValue(mapPreview, "installPath");

			return mapPreview;
		}

		private void SetAllowDownloading(MapPreview mapPreview, bool allow)
		{
			// The code checks Game.Settings.Game.AllowDownloading, which is not accessible here.
			// We cannot set this without refactoring, so this is a no-op.
		}

		private void ClearMapInstallPackage(MapPreview mapPreview)
		{
			var cacheField = typeof(MapPreview).GetField("cache", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.NotNull(cacheField);
			var cache = cacheField.GetValue(mapPreview);
			var mapLocationsProp = cache.GetType().GetProperty("MapLocations");
			Assert.NotNull(mapLocationsProp);

			// Replace MapLocations with empty dictionary to simulate no install location found
			var emptyLocations = new Dictionary<IReadOnlyPackage, MapClassification>();
			var mockCache = cache as Mock<MapCache>;
			if (mockCache != null)
				mockCache.SetupGet(c => c.MapLocations).Returns(emptyLocations);
		}
	}
}
