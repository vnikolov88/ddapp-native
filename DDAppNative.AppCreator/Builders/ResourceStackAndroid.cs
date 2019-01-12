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

            var resourcePackage = new ResourceDefinition[]
            {
                // Drawable
                new ResourceDefinition{ Source = $"{appResourceDir}/40x40.png",      Destination = $"{appDrawableDir}/icon.png" },
            };

            var missingResources = resourcePackage.Where(x => !FileUtils.PathExists(x.Source))
                .Select(x => $"Resource file is missing [{x.Source}]").ToList();

            foreach (var resource in missingResources) Console.WriteLine(resource);

            foreach (var resource in resourcePackage)
                FileUtils.CopyFile(resource.Source, resource.Destination);
        }
    }
}
