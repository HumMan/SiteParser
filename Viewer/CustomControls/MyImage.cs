using System.Windows;
using System.Windows.Controls;

namespace Viewer.CustomControls
{
    public class MyImage : Image
    {
        public static readonly RoutedEvent SourceChangedEvent = EventManager.RegisterRoutedEvent(
            "SourceChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MyImage));

        static MyImage()
        {
            SourceProperty.OverrideMetadata(typeof(MyImage), new FrameworkPropertyMetadata(SourcePropertyChanged));
        }

        public event RoutedEventHandler SourceChanged
        {
            add { AddHandler(SourceChangedEvent, value); }
            remove { RemoveHandler(SourceChangedEvent, value); }
        }

        private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Image image = obj as Image;
            if (image != null)
            {
                image.RaiseEvent(new RoutedEventArgs(SourceChangedEvent));
            }
        }
    }
}
