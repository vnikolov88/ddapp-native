using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

            var resourcePackage = new ImageResourceDefinition[]
            {
                // AppIcons
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_s.png",  Destination = $"{appIconsDir}/29x29.png", Width = 29, Height = 29 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_s.png",  Destination = $"{appIconsDir}/40x40.png", Width = 40, Height = 40 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_s.png",  Destination = $"{appIconsDir}/58x58.png", Width = 58, Height = 58 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/76x76.png", Width = 76, Height = 76 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/80x80.png", Width = 80, Height = 80 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/114x114.png", Width = 114, Height = 114 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/120x120.png", Width = 120, Height = 120 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/152x152.png", Width = 152, Height = 152 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/167x167.png", Width = 167, Height = 167 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_l.png",  Destination = $"{appIconsDir}/1024x1024.png", Width = 1024, Height = 1024 },
                // LaunchImage
                new ImageResourceDefinition{ Source = $"{appResourceDir}/splash_screen_l.png",  Destination = $"{launchImageDir}/Splash_Screen_640x1136.png", Width = 640, Height = 1136 },
                // LaunchImages
                new ImageResourceDefinition{ Source = $"{appResourceDir}/splash_screen_s.png",  Destination = $"{launchImagesDir}/Splash_Screen_320x480.png", Width = 320, Height = 480 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/splash_screen_l.png",  Destination = $"{launchImagesDir}/Splash_Screen_640x960.png", Width = 640, Height = 960 },
                new ImageResourceDefinition{ Source = $"{appResourceDir}/splash_screen_l.png",  Destination = $"{launchImagesDir}/Splash_Screen_640x1136.png", Width = 640, Height = 1136 },
            };

            var missingResources = resourcePackage.Where(x => !FileUtils.PathExists(x.Source))
                .Select(x => $"Resource file is missing [{x.Source}]").ToList();

            foreach (var resource in missingResources) Console.WriteLine(resource);

            var encoder = new PngEncoder();
            encoder.ColorType = PngColorType.Rgb;

            foreach (var resource in resourcePackage) {
                using (var image = Image.Load(resource.Source))
                {
                    image.Mutate(x => x
                         .Resize(resource.Width, resource.Height));
                    image.Save(resource.Destination, encoder);
                }
            }
        }
    }
}
