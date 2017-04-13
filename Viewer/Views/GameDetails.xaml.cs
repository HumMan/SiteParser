using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Viewer
{
    /// <summary>
    /// Interaction logic for GameDetails.xaml
    /// </summary>
    public partial class GameDetails : Window
    {
        public bool IsExit { get; set; }
        public GameDetails()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsExit)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
            }
        }
    }
}
