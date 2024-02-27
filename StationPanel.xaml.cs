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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TranSys
{
    /// <summary>
    /// Interaction logic for StationPanel.xaml
    /// </summary>
    public partial class StationPanel : Page
    {
        private MainWindow _mainWindow;
        private Station _station;
        public StationPanel(MainWindow mainWindow, Station station)
        {
            InitializeComponent();
            NameLabel.Content = station._name;
            _mainWindow = mainWindow;
            _station = station;
        }

        public void EditButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MenuFrame.Content = new EditStationPage(_mainWindow, _station, this);
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MenuFrame.Content = new StationMenu(_mainWindow);
        }
    }
}
