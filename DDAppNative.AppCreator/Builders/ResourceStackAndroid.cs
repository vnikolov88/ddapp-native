using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DDAppNative.AppCreator.Builders
{
    class ResourceStackAndroid : IResourceStack
    {
        readonly string _resourcesBaseDir;
        public ResourceStackAndroid(string resourcesBaseDir)
        {
            _resourcesBaseDir = resourcesBaseDir ?? throw new ArgumentNullException(nameof(resourcesBaseDir));
        }

        public void FillInResources(AppBuildState state)
        {
            var appBaseDir = $"./{state.AppCode}";
            var appBuildBaseDir = $"{appBaseDir}/{state.BuildNumber}";

            FileUtils.CopyFiles($"{appBuildBaseDir}/Caches", $"{appBuildBaseDir}/Android/Resources");

            var appResourceDir = $"{_resourcesBaseDir}/{state.AppCode}";
            var appDrawableDir = $"{appBuildBaseDir}/Android/Resources/drawable";

            var resourcePackage = new ImageResourceDefinition[]
            {
                // Drawable
                new ImageResourceDefinition{ Source = $"{appResourceDir}/icon_s.png",      Destination = $"{appDrawableDir}/icon.png", Width = 40, Height = 40 },
            };

            var missingResources = resourcePackage.Where(x => !FileUtils.PathExists(x.Source))
                .Select(x => $"Resource file is missing [{x.Source}]").ToList();

            foreach (var resource in missingResources) Console.WriteLine(resource);

            foreach (var resource in resourcePackage)
            {
                using (var image = Image.Load(resource.Source))
                {
                    image.Mutate(x => x
                         .Resize(resource.Width, resource.Height));
                    image.Save(resource.Destination);
                }
            }
        }
    }
}
