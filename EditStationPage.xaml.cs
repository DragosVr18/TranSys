using Mapsui.Projections;
using Mapsui.Extensions;
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
    /// Interaction logic for EditStationPage.xaml
    /// </summary>
    public partial class EditStationPage : Page
    {
        private Page previous;
        private Station _station;
        private MainWindow _mainWindow;
        private bool[] _vehicles = new bool[3];
        private bool editName = false;
        private bool editCapacity = false;
        private bool editTypes = false;
        private bool editCoordinates = false;
        public EditStationPage(MainWindow mainWindow, Station station, Page previous)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _station = new Station(station._name, station._vehicleTypes, station._capacity, station._long, station._lat);
            CurrentNameLabel.Content = station._name;
            CurrentCapacityLabel.Content = station._capacity;
            CurrentLatLabel.Content = station._lat;
            CurrentLongLabel.Content = station._long;
            LatLabel.Visibility = Visibility.Hidden;
            LongLabel.Visibility = Visibility.Hidden;
            foreach (var vehicleType in station._vehicleTypes)
            {
                switch (vehicleType)
                {
                    case "bus":
                        _vehicles[0] = true;
                        BusButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
                        break;
                    case "trolley":
                        _vehicles[1] = true;
                        TrolleyButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
                        break;
                    case "tram":
                        _vehicles[2] = true;
                        TramButton.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
                        break;
                }
            }
            this.previous = previous;
            _mainWindow._mapControl.MouseLeftButtonDown += getLocationFromMouse;
        }

        public void getLocationFromMouse(object sender, RoutedEventArgs e)
        {
            var point = _mainWindow._mapControl.Map?.Navigator.Viewport.ScreenToWorld(Mouse.GetPosition(_mainWindow._mapControl).X, Mouse.GetPosition(_mainWindow._mapControl).Y);
            var coord = SphericalMercator.ToLonLat(point.X, point.Y);
            _station._lat = coord.lat;
            _station._long = coord.lon;
            LatLabel.Content = "Latitude: " + _station._lat;
            LongLabel.Content = "Longitude: " + _station._long;
            LatLabel.Visibility = Visibility.Visible;
            LongLabel.Visibility = Visibility.Visible;
            editCoordinates = true;
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

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string currentName = _station._name;
            if (NameTextBox.Text.Length > 0)
            {
                _station._name = NameTextBox.Text;
                editName = true;
            }
            try
            {
                _station._capacity = int.Parse(CapacityTextBox.Text);
                editCapacity = true;
            }
            catch (Exception) { }

            List<string> newVehicles = new List<string>();
            if (_vehicles[0] == true)
                newVehicles.Add("bus");
            if (_vehicles[1] == true)
                newVehicles.Add("trolley");
            if (_vehicles[2] == true)
                newVehicles.Add("tram");
            if(newVehicles.SequenceEqual(_station._vehicleTypes) == false)
            {
                _station._vehicleTypes = newVehicles;
                editTypes = true;
            }
            await Task.Run(() =>
            {
                if(editName == true)
                {
                    _mainWindow._service.updateStationName(currentName, _station._name);
                }
                if(editCapacity == true)
                {
                    _mainWindow._service.updateStationCapacity(currentName, _station._capacity);
                }
                if(editCoordinates == true)
                {
                    _mainWindow._service.updateStationCoordinates(currentName, _station._long, _station._lat);
                }
                var stationsLayer = _mainWindow._mapControl.Map.Layers.FindLayer("StationsLayer").First();
                _mainWindow._mapControl.Map.Layers.Remove(stationsLayer);
                _mainWindow._mapControl.Map.Layers.Add(LayersCreator.getStationsLayer(_mainWindow._service.Stations));
            });
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MenuFrame.Content = previous;
        }
    }
}
