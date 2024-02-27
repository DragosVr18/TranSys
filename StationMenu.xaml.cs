using Accessibility;
using Mapsui;
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
    /// Interaction logic for StationMenu.xaml
    /// </summary>
    public partial class StationMenu : Page
    {
        private MainWindow _mainWindow;
        //private AddStationPage _addStationPage;
        public StationMenu(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            //loadStations();
            _mainWindow._mapControl.Map.Info += showStationData;
            //_addStationPage = new AddStationPage(_mainWindow);
        }

        private async void showStationData(object sender, MapInfoEventArgs e)
        {
            var stationFeature = e.MapInfo?.Feature;
            if (stationFeature == null)
                return;
            if (e.MapInfo.Layer.Name != "StationsLayer" || _mainWindow.MenuFrame.Content.GetType() != typeof(StationMenu))
                return;
            _mainWindow.MenuFrame.Content = new LoadingSpinnerControl.LoadingSpinner { IsLoading = true, Color = Brushes.White };
            var name = stationFeature["Name"].ToString();
            var station = await Task.Run(() => _mainWindow._service.findStationByName(name));
            StationPanel stationPanel = new StationPanel(_mainWindow, station);
            _mainWindow.MenuFrame.Content = stationPanel;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //_mainWindow.MenuFrame.NavigationService.RemoveBackEntry();
            _mainWindow._mapControl.Map.Info -= showStationData;
            AddStationPage _addStationPage = new AddStationPage(_mainWindow);
            _mainWindow.MenuFrame.Content = _addStationPage;
        }

        private void LinesButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow._mapControl.Map.Info -= showStationData;
            LineMenu lineMenu = new LineMenu(_mainWindow);
            _mainWindow.MenuFrame.Content = lineMenu;
        }

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow._mapControl.Map.Info -= showStationData;
            VehicleMenu vehicleMenu = new VehicleMenu(_mainWindow);
            _mainWindow.MenuFrame.Content = vehicleMenu;
        }
    }
}
