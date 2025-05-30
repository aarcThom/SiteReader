namespace SiteReader.Components.Plants
{
    public abstract class PlantBase : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        protected PlantBase(string name, string nickname, string description)
            : base(name, nickname, description, "Plants")
        {
        }
    }
}
