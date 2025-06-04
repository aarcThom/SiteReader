using System.Reflection;
using Grasshopper.Kernel;
using System.Drawing;

namespace SiteReader.Components
{
    public abstract class SiteReaderBase : GH_Component
    {
        //FIELDS ======================================================================================================

        // NOTE: See james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/
        // for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        //grabbing embedded resources
        protected readonly Assembly GHAssembly = Assembly.GetExecutingAssembly();

        protected string IconPath;

        //CONSTRUCTORS ================================================================================================
        protected SiteReaderBase(string name, string nickname, string description, string subCategory)
            : base(name, nickname, description, "SiteReader", subCategory)
        {
        }

        /// <summary>
        /// Provides an Icon for the component. Defaults to generic icon if none provided.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                if (IconPath == null)
                {
                    IconPath = "SiteReader.Resources.generic.png";
                }

                System.IO.Stream stream = GHAssembly.GetManifestResourceStream(IconPath);
                return new Bitmap(stream);
            }
        }
    }
}
