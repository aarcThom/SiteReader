using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using SiteReader.UI.Components;
using Grasshopper.GUI;
using Button = SiteReader.UI.Components.Button;

namespace SiteReader.UI
{
    public class UiImportCloud : UIBase
    {
        //FIELDS ======================================================================================================
        private readonly Button _importButton;
        private readonly Action _importAction;
        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        public UiImportCloud(GH_Component owner, Action importAction) : base(owner)
        {

            _importAction = importAction;
             _importButton = new Button("import", 30);

            ComponentList = new List<IUi>()
            {
                _importButton
            };
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _importButton.Bounds.Contains(e.CanvasLocation)) 
            {
                _importButton.Clicked = true;

                // expire layout, but not solution
                base.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Capture;
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {

            if (e.Button == MouseButtons.Left && _importButton.Clicked)
            {
                _importButton.Clicked = false;

                // expire layout, but not solution
                base.ExpireLayout();
                sender.Refresh();
                _importAction();
                return GH_ObjectResponse.Release;
            }
            return base.RespondToMouseUp(sender, e);
        }


    }
}
