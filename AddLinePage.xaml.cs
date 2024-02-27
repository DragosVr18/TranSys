using Itinero;
using Itinero.Exceptions;
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Collections.Specialized.BitVector32;

namespace TranSys
{
    /// <summary>
    /// Interaction logic for AddLinePage.xaml
    /// </summary>
    public partial class AddLinePage : Page
    {
        private MainWindow _mainWindow;
        private List<Station> stations = new List<Station>();
        private string vehicleType = "";
        private int modifyState = 0;
        private List<Midpoint> midpoints = new List<Midpoint>();
        private List<MPoint> points = new List<MPoint>();
        private int modifierIndex = -1;
        public MainWindow mainWindow { get { return _mainWindow; } }
        public AddLinePage(MainWindow mainWindow)
        {
            InitializeComponent();
            StatusTextBlock.Visibility = Visibility.Hidden;
            _mainWindow = mainWindow;
            _mainWindow._mapControl.Map.Info += updateLine;
            _mainWindow._mapControl.MouseLeftButtonDown += updateWithMidpoint;
            _mainWindow._mapControl.MouseRightButtonDown += cancelMidpoint;
        }

        private void displayStatus(string message)
        {
            var timer = new Timer(4000);
            StatusTextBlock.Text = message;
            StatusTextBlock.Visibility = Visibility.Visible;
            timer.Elapsed += hideStatus;
            timer.Start();
        }

        private void hideStatus(object sender, ElapsedEventArgs e)
        {
            //StatusTextBlock.Visibility = Visibility.Hidden;
        }

        private void updateLine(object sender, MapInfoEventArgs e)
        {
            var feature = e.MapInfo?.Feature;
            if (feature == null)
                return;
            if (e.MapInfo?.Layer.Name == "LineLayer")
            {
                modifyState = 1;
                updateLineLayer("yellow");
            }
            else if (e.MapInfo?.Layer.Name == "StationsLayer")
                updateWithStation(feature);
            else if (e.MapInfo?.Layer.Name == "MidpointsLayer" && modifyState == 0)
            {
                removeMidpoint(feature);
            }
        }

        private void updateLineLayer(string color)
        {
            var lineLayer = _mainWindow._mapControl.Map.Layers.FindLayer("LineLayer");
            if (lineLayer.Count() > 0)
            {
                _mainWindow._mapControl.Map.Layers.Remove(lineLayer.First());
            }
            if (stations.Count > 1)
            {
                RouterDb routerDB = _mainWindow._service.RouterDb;
                var router = new Router(routerDB);
                var profile = DynamicVehicle.LoadFromStream(File.OpenRead("RouterData/bus.lua")).Shortest();

                try
                {
                    List<MPoint> pts = new List<MPoint>();
                    int l = points.Count;
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        var converted_1 = SphericalMercator.ToLonLat(points[i]);
                        var converted_2 = SphericalMercator.ToLonLat(points[(i + 1) % l]);
                        var start = router.Resolve(profile, (float)converted_1.Y, (float)converted_1.X);
                        var end = router.Resolve(profile, (float)converted_2.Y, (float)converted_2.X);
                        var path = router.Calculate(profile, start, end);
                        foreach (var p in path.Shape)
                        {
                            pts.Add(new MPoint(p.Longitude, p.Latitude));
                        }
                    }
                    _mainWindow._mapControl.Map.Layers.Insert(1,LayersCreator.getLineLayer(pts, color, 5));
                }
                catch(RouteNotFoundException)
                {
                    MessageBox.Show("Error on calculating route");
                    if (modifyState == 1)
                    {
                        midpoints.Remove(midpoints.Last());
                        points.RemoveAt(points.Count - 2);
                        updateLineLayer("yellow");
                    }
                    else
                    {
                        stations.Remove(stations.Last());
                        points.Remove(points.Last());
                        updateLineLayer("blue");
                    }
                }
            }
        }

        private void updateWithStation(IFeature stationFeature)
        {
            var station = mainWindow._service.Stations.Find(x => x._name == stationFeature["Name"]);
            int index = stations.FindIndex(x => x._name == station._name);
            if(modifyState == 0)
            {
                if(index == -1)
                {
                    if (station._vehicleTypes.Contains(vehicleType))
                    {
                        stations.Add(station);
                        displayStatus("Station " + station._name + " has been added to route");
                        var p = SphericalMercator.FromLonLat(station._long, station._lat);
                        points.Add(new MPoint(p.x, p.y));
                    }
                    else
                    {
                        if (vehicleType == "bus")
                            MessageBox.Show("This station doesn't support buses!");
                        else if (vehicleType == "trolley")
                            MessageBox.Show("This station doesn't support trolleys!");
                        else if (vehicleType == "tram")
                            MessageBox.Show("This station doesn't support trams!");
                        else
                            MessageBox.Show("You must select a vehicle type first!");
                        return;
                    }
                }
                else
                {
                    stations.RemoveAt(index);
                    displayStatus("Station " + station._name + " has been removed from route");
                    var p = SphericalMercator.FromLonLat(station._long, station._lat);
                    points.Remove(points.Find(x => x.X == p.x && x.Y == p.y));
                    foreach (var pt in midpoints.FindAll(x => x.begin._name == station._name || x.end._name == station._name))
                    {
                        points.Remove(points.Find(x => x.X == pt.point.X && x.Y == pt.point.Y));
                        midpoints.Remove(pt);
                    }
                    var midpointLayer = mainWindow._mapControl.Map.Layers.FindLayer("MidpointsLayer");
                    if (midpointLayer.Count() > 0)
                    {
                        mainWindow._mapControl.Map.Layers.Remove(midpointLayer.First());
                    }

                }
                TourStationComboBox.Items.Clear();
                BackStationComboBox.Items.Clear();
                foreach (var s in stations)
                {
                    TourStationComboBox.Items.Add(s._name);
                    BackStationComboBox.Items.Add(s._name);
                }
                updateLineLayer("blue");
            }
            else if(modifyState == 1)
            {
                if(index == -1)
                {
                    MessageBox.Show("Cannot modify the route for a station that's not part of the line");
                    return;
                }
                modifierIndex = index;
                modifyState = 2;
                displayStatus("Modifying after selected station...");
                updateLineLayer("red");
            }
        }

        private void updateWithMidpoint(object sender, RoutedEventArgs e)
        {
            if (modifyState != 2)
                return;
            var p = _mainWindow._mapControl.Map?.Navigator.Viewport.ScreenToWorld(Mouse.GetPosition(_mainWindow._mapControl).X, Mouse.GetPosition(_mainWindow._mapControl).Y);
            if(midpoints.FindAll(x => x.end == stations.Last()).Count > 0)
            {
                MessageBox.Show("Cannot have more than one midpoint between stations");
                return;
            }
            if (modifierIndex == -1)
            {
                midpoints.Add(new Midpoint { begin = stations[stations.Count - 2], end = stations.Last(), point = p });
                points.Insert(points.Count - 1, new MPoint(p.X, p.Y));
            }
            else if (modifierIndex != stations.Count - 1)
            {
                midpoints.Add(new Midpoint { begin = stations[modifierIndex], end = stations[modifierIndex + 1], point = p });
                var pt = SphericalMercator.FromLonLat(stations[modifierIndex]._long, stations[modifierIndex]._lat);
                var stationPtIndex = points.FindIndex(x => x.X == pt.x && x.Y == pt.y);
                points.Insert(stationPtIndex + 1, new MPoint(p.X, p.Y));
            }
            displayStatus("Midpoint has been added to route");
            var midpointLayer = mainWindow._mapControl.Map.Layers.FindLayer("MidpointsLayer");
            if(midpointLayer.Count() > 0)
            {
                mainWindow._mapControl.Map.Layers.Remove(midpointLayer.First());
            }
            mainWindow._mapControl.Map.Layers.Add(LayersCreator.getMidpointsLayer(midpoints));
            modifyState = 1;
            updateLineLayer("yellow");
        }

        private void removeMidpoint(IFeature midpoint)
        {
            var mp = midpoints.Find(x => x.begin == midpoint["begin"] && x.end == midpoint["end"]);
            midpoints.Remove(mp);
            points.Remove(points.Find(x => x.X == mp.point.X && x.Y == mp.point.Y));
            var midpointLayer = mainWindow._mapControl.Map.Layers.FindLayer("MidpointsLayer");
            if (midpointLayer.Count() > 0)
            {
                mainWindow._mapControl.Map.Layers.Remove(midpointLayer.First());
            }
            mainWindow._mapControl.Map.Layers.Add(LayersCreator.getMidpointsLayer(midpoints));
            updateLineLayer("blue");
        }

        private void cancelMidpoint(object sender, RoutedEventArgs e)
        {
            if(modifyState == 2)
            {
                modifyState = 1;
                modifierIndex = -1;
                updateLineLayer("yellow");
            }
            else if(modifyState == 1)
            {
                modifyState = 0;
                updateLineLayer("blue");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var lineLayer = _mainWindow._mapControl.Map.Layers.FindLayer("LineLayer");
            if (lineLayer.Count() > 0)
            {
                _mainWindow._mapControl.Map.Layers.Remove(lineLayer.First());
            }
            stations.Clear();
            _mainWindow._mapControl.Map.Info -= updateLine;
            _mainWindow._mapControl.MouseLeftButtonDown -= updateWithMidpoint;
            _mainWindow._mapControl.MouseRightButtonDown -= cancelMidpoint;
            LineMenu lineMenu = new LineMenu(mainWindow);
            mainWindow.MenuFrame.Content = lineMenu;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LineValidator.validateLine(NameTextBox.Text,
                    vehicleType,
                    DescriptionTextBox.Text,
                    TourStationComboBox.Text,
                    BackStationComboBox.Text);
                _mainWindow._service.addLine(NameTextBox.Text,
                    vehicleType,
                    DescriptionTextBox.Text,
                    TourStationComboBox.Text,
                    BackStationComboBox.Text,
                    stations,
                    midpoints);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void BusButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "bus";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
        }

        private void TrolleyButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "trolley";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
        }

        private void TramButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "tram";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
        }
    }

    public struct Midpoint
    {
        public Station begin;
        public Station end;
        public MPoint point;
    }
}
