using Grasshopper.Kernel;
using SiteReader.UI.UiElements;
using System;
using System.Collections.Generic;

namespace SiteReader.UI
{
    public class UiImportCloud : UiBase
    {
        //FIELDS ======================================================================================================
        private readonly ReleaseButton _importButton;
        private readonly Action _importAction;

        private readonly ReleaseButton _zoomButton;
        private readonly Action _zoomAction;
        
        //CONSTRUCTORS ================================================================================================
        public UiImportCloud(GH_Component owner, Action importAction, Action zoomAction) : base(owner)
        {
            _importAction = importAction;
            _importButton = new ReleaseButton("import", 30);
            _importButton.ClickAction = importAction;

            _zoomAction = zoomAction;
            _zoomButton = new ReleaseButton("zoom", 30);
            _zoomButton.ClickAction = zoomAction;

            ComponentList = new List<IUi>()
            {
                _importButton,
                _zoomButton
            };
        }
    }
}
