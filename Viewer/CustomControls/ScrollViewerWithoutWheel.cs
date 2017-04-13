using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Viewer.CustomControls
{
    public class ScrollViewerWithoutWheel : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
        }
    }
}
