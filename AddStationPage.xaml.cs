using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TranSys
{
    /// <summary>
    /// Interaction logic for AddStationPage.xaml
    /// </summary>
    public partial class AddStationPage : Page
    {
        private MainWindow _mainWindow;
        private MapControl _mapControl;
        private double _latitude;
        private double _longitude;
        private bool[] _vehicles;
        public AddStationPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            //_service = mainWindow._service;
            _mapControl = mainWindow._mapControl;
            _latitude = 0;
            _longitude = 0;
            _vehicles = new bool[3];
            _mapControl.MouseLeftButtonDown += getLocationFromMouse;
        }
        public void getLocationFromMouse(object sender, RoutedEventArgs e)
        {
            var point = _mapControl.Map?.Navigator.Viewport.ScreenToWorld(Mouse.GetPosition(_mapControl).X, Mouse.GetPosition(_mapControl).Y);
            var coord = SphericalMercator.ToLonLat(point.X, point.Y);
            _latitude = coord.lat;
            _longitude = coord.lon;
            LatLabel.Content = "Latitude: " + _latitude;
            LongLabel.Content = "Longitude: " + _longitude;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text;
            int capacity = 0;
            try
            {
                capacity = int.Parse(CapacityTextBox.Text);
            }
            catch (Exception) { }
            List<string> vehicles = new List<string>();
            if (_vehicles[0] == true)
                vehicles.Add("bus");
            if (_vehicles[1] == true)
                vehicles.Add("trolley");
            if (_vehicles[2] == true)
                vehicles.Add("tram");
            try
            {
                StationValidator.validateStation(name, vehicles, capacity, _longitude, _latitude);
                _mainWindow._service.addStation(name, vehicles, capacity, _longitude, _latitude);
                var stationsLayer = _mainWindow._mapControl.Map.Layers.FindLayer("StationsLayer").First();
                //stationsLayer.DataHasChanged();
                _mainWindow._mapControl.Map.Layers.Remove(stationsLayer);
                _mainWindow._mapControl.Map.Layers.Add(LayersCreator.getStationsLayer(_mainWindow._service.Stations));
                NameTextBox.Text = "";
                CapacityTextBox.Text = "";
                if (_vehicles[0] == true)
                {
                    _vehicles[0] = false;
                    BusButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
                }
                if (_vehicles[1] == true)
                {
                    _vehicles[1] = false;
                    TrolleyButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
                }
                if (_vehicles[2] == true)
                {
                    _vehicles[2] = false;
                    TramButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
                }
                _longitude = 0;
                _latitude = 0;
                LatLabel.Content = "Latitude: not set";
                LongLabel.Content = "Longitude: not set";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BusButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vehicles[0] == false)
            {
                _vehicles[0] = true;
                BusButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            }
            else
            {
                _vehicles[0] = false;
                BusButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
                if (_vehicles[1] == true)
                {
                    _vehicles[1] = false;
                    TrolleyButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
                }
            }
        }

        private void TrolleyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vehicles[1] == false)
            {
                _vehicles[1] = true;
                TrolleyButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
                if (_vehicles[0] == false)
                {
                    _vehicles[0] = true;
                    BusButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
                }
            }
            else
            {
                _vehicles[1] = false;
                TrolleyButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            }
        }

        private void TramButton_Click(object sender, RoutedEventArgs e)
        {
            if (_vehicles[2] == false)
            {
                _vehicles[2] = true;
                TramButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            }
            else
            {
                _vehicles[2] = false;
                TramButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StationMenu stationMenu = new StationMenu(_mainWindow);
            _mainWindow.MenuFrame.Content = stationMenu;
        }
    }
}
