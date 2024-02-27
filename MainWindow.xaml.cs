using HarfBuzzSharp;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using Mapsui.UI.Wpf;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidget;
using Mapsui.Widgets.MouseCoordinatesWidget;
using Neo4j.Driver;
using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Service _service { get; }
        public MapControl _mapControl { get; }

        public MainWindow(string uri, string username, string password)
        {
            InitializeComponent();
            _service = new Service(uri, username, password);

            _mapControl = new MapControl();
            _mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
            _mapControl.Map?.Layers.Add(LayersCreator.getStationsLayer(_service.Stations));
            //_mapControl.MouseLeftButtonDown += getLocationFromMouse;

            MapGrid.Children.Add(_mapControl);
            //StationMenu stationMenu = new StationMenu(this);
            StationMenu stationMenu = new StationMenu(this);
            MenuFrame.Content = stationMenu;
        }

        public void centerLocation()
        {
            var dataList = _service.getCenterLocation();
            var resList = _mapControl.Map?.Navigator.Resolutions.ToList();
            if (resList != null && dataList!= null)
            {
                var coords = SphericalMercator.FromLonLat(dataList[0], dataList[1]);
                _mapControl.Map?.Navigator.CenterOnAndZoomTo(new Mapsui.MPoint(coords.x, coords.y), resList[(int)dataList[2]]);
            }
        }
    }
}
