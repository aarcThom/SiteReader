namespace SiteReader.Components.Extras
{
    public abstract class ExtrasBase : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        protected ExtrasBase(string name, string nickname, string description)
            : base(name, nickname, description, "Utility")
        {
        }
    }
}
