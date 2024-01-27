using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using SiteReader.UI.UiElements;

namespace SiteReader.UI
{
    public class UiZoomEnhance : UiBase
    {
        //CONSTRUCTORS ================================================================================================
        public UiZoomEnhance(GH_Component owner, Action importAction) : base(owner)
        {
            var importButton = new ReleaseButton("Zoom & Enhance!", 30)
            {
                ClickAction = importAction
            };

            ComponentList = new List<IUi>()
            {
                importButton,
            };
        }
    }
}
