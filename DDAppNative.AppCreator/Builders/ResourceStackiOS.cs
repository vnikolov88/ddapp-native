using System;
using System.Collections.Generic;
using System.Linq;

namespace DDAppNative.AppCreator.Builders
{
    class ResourceStackiOS : IResourceStack
    {
        readonly string _resourcesBaseDir;
        public ResourceStackiOS(string resourcesBaseDir)
        {
            _resourcesBaseDir = resourcesBaseDir ?? throw new ArgumentNullException(nameof(resourcesBaseDir));
        }

        public void FillInResources(AppBuildState state)
        {
            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";

            FileUtils.CopyFiles($"{appBuildBaseDir}/Caches", $"{appBuildBaseDir}/iOS/Resources");

            var appResourceDir = $"{_resourcesBaseDir}/{state.AppCode}";
            var appIconsDir = $"{appBuildBaseDir}/iOS/Resources/Media.xcassets/AppIcons.appiconset";
            var launchImageDir = $"{appBuildBaseDir}/iOS/Resources/Media.xcassets/LaunchImage.imageset";
            var launchImagesDir = $"{appBuildBaseDir}/iOS/Resources/Media.xcassets/LaunchImages.launchimage";

            var resourcePackage = new ResourceDefinition[]
            {
                // AppIcons
                new ResourceDefinition{ Source = $"{appResourceDir}/29x29.png",      Destination = $"{appIconsDir}/29x29.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/40x40.png",      Destination = $"{appIconsDir}/40x40.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/40x40.png",      Destination = $"{appIconsDir}/40x401.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/40x40.png",      Destination = $"{appIconsDir}/40x402.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/58x58.png",      Destination = $"{appIconsDir}/58x58.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/58x58.png",      Destination = $"{appIconsDir}/58x581.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/76x76.png",      Destination = $"{appIconsDir}/76x76.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/80x80.png",      Destination = $"{appIconsDir}/80x80.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/80x80.png",      Destination = $"{appIconsDir}/80x801.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/114x114.png",    Destination = $"{appIconsDir}/114x114.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/120x120.png",    Destination = $"{appIconsDir}/120x120.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/120x120.png",    Destination = $"{appIconsDir}/120x1201.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/152x152.png",    Destination = $"{appIconsDir}/152x152.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/1024x1024.png",  Destination = $"{appIconsDir}/1024x1024.png" },
                // LaunchImage
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_640x1136.png",  Destination = $"{launchImageDir}/Splash_Screen_640x1136.png" },
                // LaunchImages
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_320x480.png",   Destination = $"{launchImagesDir}/Splash_Screen_320x480.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_640x960.png",   Destination = $"{launchImagesDir}/Splash_Screen_640x960.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_640x1136.png",  Destination = $"{launchImagesDir}/Splash_Screen_640x1136.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_640x9601.png",  Destination = $"{launchImagesDir}/Splash_Screen_640x9601.png" },
                new ResourceDefinition{ Source = $"{appResourceDir}/Splash_Screen_640x1136.png",  Destination = $"{launchImagesDir}/Splash_Screen_640x11361.png" },
            };

            var missingResources = resourcePackage.Where(x => !FileUtils.PathExists(x.Source))
                .Select(x => $"Resource file is missing [{x.Source}]").ToList();

            foreach (var resource in missingResources) Console.WriteLine(resource);

            foreach (var resource in resourcePackage)
                FileUtils.CopyFile(resource.Source, resource.Destination);
        }
    }
}
