using Grasshopper.Kernel;
using SiteReader.UI.UiElements;
using System;
using System.Collections.Generic;

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
