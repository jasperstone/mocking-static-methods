using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Xunit;
using OpenRA;

namespace OpenRA.Game.Tests
{
    public class MapPreviewTests
    {
        // Test that Install returns early if Status is not DownloadError or DownloadAvailable
        [Fact]
        public void Install_ReturnsEarly_IfStatusNotDownloadErrorOrDownloadAvailable()
        {
            var mapPreview = CreateMapPreviewWithStatus(MapStatus.Available);
            mapPreview.Install("http://example.com/");
            Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
        }

        // Test that Install returns early if AllowDownloading is false
        [Fact]
        public void Install_ReturnsEarly_IfAllowDownloadingIsFalse()
        {
            var original = Game.Settings.AllowDownloading;
            Game.Settings.AllowDownloading = false;
            try
            {
                var mapPreview = CreateMapPreviewWithStatus(MapStatus.DownloadAvailable);
                mapPreview.Install("http://example.com/");
                Assert.NotEqual(MapStatus.Downloading, mapPreview.Status);
            }
            finally
            {
                Game.Settings.AllowDownloading = original;
            }
        }

        // Test that Install sets Status to DownloadError if map install directory not found
        [Fact]
        public void Install_SetsDownloadError_IfMapInstallDirectoryNotFound()
        {
            var mapPreview = CreateMapPreviewWithStatus(MapStatus.DownloadAvailable, hasMapInstallPackage: false);
            mapPreview.Install("http://example.com/");
            Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
        }

        // Helper to create a MapPreview with given status and optionally with map install package
        private MapPreview CreateMapPreviewWithStatus(MapStatus status, bool hasMapInstallPackage = true)
        {
            // Create a MapPreview instance via reflection (since constructor is internal or complex)
            var ctor = typeof(MapPreview).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);
            MapPreview mapPreview;
            if (ctor != null)
                mapPreview = (MapPreview)ctor.Invoke(null);
            else
                throw new InvalidOperationException("No accessible constructor found for MapPreview");

            // Set innerData field
            var innerDataType = typeof(MapPreview).GetNestedType("InnerData", BindingFlags.NonPublic);
            var innerData = Activator.CreateInstance(innerDataType);
            var statusField = innerDataType.GetField("Status", BindingFlags.Public | BindingFlags.Instance);
            statusField.SetValue(innerData, status);

            var innerDataField = typeof(MapPreview).GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
            innerDataField.SetValue(mapPreview, innerData);

            // Set cache field with MapLocations dictionary
            var cacheField = typeof(MapPreview).GetField("cache", BindingFlags.NonPublic | BindingFlags.Instance);
            var cacheType = cacheField.FieldType;
            var cache = Activator.CreateInstance(cacheType);
            var mapLocationsProp = cacheType.GetProperty("MapLocations");
            var mapLocations = mapLocationsProp.GetValue(cache) as IDictionary<string, MapClassification>;
            if (hasMapInstallPackage)
            {
                mapLocations.Add("dummyPath", MapClassification.User);
            }
            cacheField.SetValue(mapPreview, cache);

            // Set Game.Settings.AllowDownloading to true to allow download
            Game.Settings.AllowDownloading = true;

            return mapPreview;
        }
    }
}
