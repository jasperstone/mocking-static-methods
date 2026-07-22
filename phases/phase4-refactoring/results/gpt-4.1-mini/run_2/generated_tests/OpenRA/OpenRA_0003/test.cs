using System;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using OpenRA;

namespace OpenRA.Game.Tests
{
	public class MapPreviewTests
	{
		[Fact]
		public void Install_DoesNotStartDownload_WhenStatusIsNotDownloadErrorOrDownloadAvailable()
		{
			var mapPreview = CreateMapPreviewWithStatus(MapStatus.Available);
			mapPreview.Install("http://example.com/maps/");
			Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
		}

		[Fact]
		public void Install_ReturnsImmediately_WhenStatusIsNotDownloadErrorOrDownloadAvailable()
		{
			var mapPreview = CreateMapPreviewWithStatus(MapStatus.Generatable);
			mapPreview.Install("http://example.com/maps/");
			Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
		}

		// Due to lack of public constructors and inability to mock HttpClientFactory,
		// deeper testing of Install's async download behavior is not feasible without refactoring.
		// These tests cover the early exit conditions.

		private static MapPreview CreateMapPreviewWithStatus(MapStatus status)
		{
			// Use FormatterServices to create uninitialized MapPreview instance
			var mapPreviewType = typeof(MapPreview);
			var mapPreview = (MapPreview)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(mapPreviewType);

			// Create InnerData instance and set Status
			var innerDataType = mapPreviewType.GetNestedType("InnerData", BindingFlags.NonPublic);
			var innerData = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(innerDataType);
			innerDataType.GetField("Status").SetValue(innerData, status);

			// Set innerData field
			var innerDataField = mapPreviewType.GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
			innerDataField.SetValue(mapPreview, innerData);

			return mapPreview;
		}
	}
}
