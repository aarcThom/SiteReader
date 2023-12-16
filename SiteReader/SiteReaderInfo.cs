using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SiteReader
{
    public class SiteReaderInfo : GH_AssemblyInfo
    {
        public override string Name => "SiteReader";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("3ec7d2c2-de40-4f03-b37d-58eef67c5a5b");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}