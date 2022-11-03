using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace SML.UWP.Dependencies
{
    public class DisableScreen
    {
        [Obsolete]
        public async Task Disable()
        {
            var currentSize = ApplicationView.GetForCurrentView();
            if (!currentSize.IsFullScreen)
            {
                currentSize.TryEnterFullScreenMode();
            }
            else
            { }
        }
    }
}
