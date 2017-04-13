using Shared.Model;
using Shared.ModelSerialize;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Viewer.Assistants;

namespace Viewer
{
    public class CheckedListItem<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isChecked;
        private T item;

        public CheckedListItem()
        { }

        public CheckedListItem(T item, bool isChecked = false)
        {
            this.item = item;
            this.isChecked = isChecked;
        }

        public T Item
        {
            get { return item; }
            set
            {
                item = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
            }
        }


        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
    }

    public class ViewModel
    {
        public string SearchName { get; set; }
    }

    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private GameInfo[] Games { get; set; }

        private ObservableCollection<CheckedListItem<string>> genreFilters = new ObservableCollection<CheckedListItem<string>>();
        private ObservableCollection<CheckedListItem<string>> yearFilters = new ObservableCollection<CheckedListItem<string>>();
        private ObservableCollection<CheckedListItem<string>> platformFilters = new ObservableCollection<CheckedListItem<string>>();
        private CollectionViewSource viewSource = new CollectionViewSource();

        GameDetails dlg;

        TypeAssistant assistant;

        ViewModel viewModel = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();

            LoadSizePosition();

            Games = new JsonModelSerializer().LoadGamesList();
            foreach (var g in Games)
            {
                if (!genreFilters.Any(i => i.Item == g.Genre))
                    genreFilters.Add(new CheckedListItem<string>
                    {
                        IsChecked = true,
                        Item = g.Genre
                    });
                if (!yearFilters.Any(i => i.Item == g.Year))
                    yearFilters.Add(new CheckedListItem<string>
                    {
                        IsChecked = true,
                        Item = g.Year
                    });
                if (!platformFilters.Any(i => i.Item == g.Platform))
                    platformFilters.Add(new CheckedListItem<string>
                    {
                        IsChecked = true,
                        Item = g.Platform
                    });
            }

            viewSource.Filter += viewSource_Filter;

            viewSource.Source = Games;

            GamesList.ItemsSource = viewSource.View;

            dlg = new GameDetails();

            assistant = new TypeAssistant();
            assistant.Idled += assistant_Idled;

            this.DataContext = viewModel;
        }

        private void LoadSizePosition()
        {
            Top = Properties.Settings.Default.Top;
            Left = Properties.Settings.Default.Left;
            Height = Properties.Settings.Default.Height;
            Width = Properties.Settings.Default.Width;
            if (Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void btnGenreFilter_Click(object sender, RoutedEventArgs e)
        {
            filterItems.ItemsSource = genreFilters;
            popCountry.PlacementTarget = btnGenreFilter;
            popCountry.IsOpen = true;
        }

        private void btnYearFilter_Click(object sender, RoutedEventArgs e)
        {
            filterItems.ItemsSource = yearFilters;
            popCountry.PlacementTarget = btnYearFilter;
            popCountry.IsOpen = true;
        }

        private void btnPlatformFilter_Click(object sender, RoutedEventArgs e)
        {
            filterItems.ItemsSource = platformFilters;
            popCountry.PlacementTarget = btnPlatformFilter;
            popCountry.IsOpen = true;
        }

        private void viewSource_Filter(object sender, FilterEventArgs e)
        {
            GameInfo cust = (GameInfo)e.Item;

            var result =
                genreFilters.Where(w => w.IsChecked).Any(w => w.Item == cust.Genre) &&
                yearFilters.Where(w => w.IsChecked).Any(w => w.Item == cust.Year) &&
                platformFilters.Where(w => w.IsChecked).Any(w => w.Item == cust.Platform);
            if(!String.IsNullOrWhiteSpace(viewModel.SearchName))
            {
                result &= cust.Name.IndexOf(viewModel.SearchName, StringComparison.InvariantCultureIgnoreCase)>=0;
            }
            e.Accepted = result;
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckedListItem<string> item in filterItems.ItemsSource)
            {
                item.IsChecked = true;
            }
        }

        private void btnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckedListItem<string> item in filterItems.ItemsSource)
            {
                item.IsChecked = false;
            }
        }

        private void ApplyFilters(object sender, RoutedEventArgs e)
        {
            viewSource.View.Refresh();
        }

        private void GamesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var info = (GameInfo)(GamesList.SelectedItem);
            if (info == null)
                return;
            dlg.DataContext = info;
            dlg.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {            
            dlg.IsExit = true;
            dlg.Close();

            SaveSizePosition();
        }

        private void SaveSizePosition()
        {
            if (WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        void assistant_Idled(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                viewSource.View.Refresh();
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            assistant.TextChanged();
        }
    }
}
