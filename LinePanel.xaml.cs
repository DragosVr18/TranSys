using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui;
using NetTopologySuite.Geometries;
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
using Mapsui.Projections;
using Neo4j.Driver;
using Itinero;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Profiles;
using Mapsui.Layers;
using System.Threading;

namespace TranSys
{
    /// <summary>
    /// Interaction logic for LinePanel.xaml
    /// </summary>
    public partial class LinePanel : Page
    {
        private MainWindow _mainWindow;
        private string _lineName;
        private List<Vehicle> vehicles = new List<Vehicle>();
        public MainWindow MainWindow { get { return _mainWindow; } }

        public LinePanel(MainWindow mainWindow, string lineName)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _lineName = lineName;
            NameLabel.Content = "Line "+_lineName;

        }

        public static async Task<LinePanel> AsyncCreate(MainWindow mainWindow, string lineName)
        {
            var linePanel = new LinePanel(mainWindow, lineName);
            linePanel.vehicles = await Task.Run(() => mainWindow._service.GetVehiclesAssignedToLine(lineName));
            linePanel.updateTimeTable();
            return linePanel;
        }

        private void updateTimeTable()
        {
            List<Tuple<OffsetTime, string>> tourTimes = new List<Tuple<OffsetTime, string>>();
            List<Tuple<OffsetTime, string>> backTimes = new List<Tuple<OffsetTime, string>>();
            foreach(Vehicle vehicle in vehicles)
            {
                foreach(var time in vehicle.TourDepartures)
                {
                    tourTimes.Add(new Tuple<OffsetTime, string>(time, vehicle.Id.ToString()));
                }
                foreach (var time in vehicle.BackDepartures)
                {
                    backTimes.Add(new Tuple<OffsetTime, string>(time, vehicle.Id.ToString()));
                }
            }
            tourTimes.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            backTimes.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            foreach (var time in tourTimes)
                TourTimeTable.Items.Add(time.Item1.Hour.ToString("D2") + ":" + time.Item1.Minute.ToString("D2") + String.Format("{0,13}", time.Item2));
            foreach (var time in backTimes)
                BackTimeTable.Items.Add(time.Item1.Hour.ToString("D2") + ":" + time.Item1.Minute.ToString("D2") + String.Format("{0,13}", time.Item2));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var lineLayer = MainWindow._mapControl.Map.Layers.FindLayer("LineLayer").First();
            _mainWindow._mapControl.Map.Layers.Remove(lineLayer);
            MainWindow.MenuFrame.Content = new LineMenu(MainWindow);
        }
    }
}
