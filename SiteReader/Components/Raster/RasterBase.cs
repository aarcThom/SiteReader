namespace SiteReader.Components.Raster
{
    public abstract class RasterBase : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        protected RasterBase(string name, string nickname, string description)
            : base(name, nickname, description, "Raster Formats")
        {
        }
    }
}
