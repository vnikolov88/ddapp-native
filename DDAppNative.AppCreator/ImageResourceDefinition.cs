using System;
using System.Collections.Generic;
using System.Text;

namespace DDAppNative.AppCreator
{
    class ImageResourceDefinition : IResourceDefinition
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
