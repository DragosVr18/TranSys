using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace TranSys
{
    /// <summary>
    /// Interaction logic for LineMenu.xaml
    /// </summary>
    public partial class LineMenu : Page
    {
        private MainWindow _mainWindow;
        public MainWindow MainWindow { get { return _mainWindow; } }
        public LineMenu(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            int i = 0;
            int margin = 0;
            resetLines(_mainWindow._service.Lines);
            FilterTextBox.TextChanged += filterLines;
        }

        private async void filterLines(object sender, RoutedEventArgs e)
        {
            var text = FilterTextBox.Text;
            if(text.Length == 0)
            {
                resetLines(_mainWindow._service.Lines);
                return;
            }
            var filteredLines = await Task.Run(() => MainWindow._service.Lines.FindAll(x => x.Name.Contains(text)));
            resetLines(filteredLines);
        }

        private void resetLines(List<Line> lines)
        {
            LinesGrid.Children.Clear();
            int i = 0;
            int margin = 0;
            foreach (var line in lines)
            {
                var btn = new LineButton(line.Name, line.Description);
                btn.Tag = line.Name;
                if (line.VehicleType == "bus")
                    btn.Background = Brushes.DarkMagenta;
                else if (line.VehicleType == "trolley")
                    btn.Background = Brushes.DarkSlateBlue;
                else
                    btn.Background = Brushes.DarkGreen;
                btn.VerticalAlignment = VerticalAlignment.Top;

                btn.Margin = new Thickness(0, i * 55 + margin, 0, 0);
                btn.Click += LineButton_Click;
                margin += 10;
                i++;
                LinesGrid.Children.Add(btn);
            }
        }

        public async void LineButton_Click(object sender, RoutedEventArgs e)
        {
            Button? button = sender as Button;
            string lineName = button.Tag.ToString();
            _mainWindow.MenuFrame.Content = new LoadingSpinnerControl.LoadingSpinner { IsLoading = true, Color = Brushes.White };

            var lineTask = Task.Run(() => LayersCreator.getLineLayer(_mainWindow._service.getRoute(lineName), "red", 4));
            await lineTask;
            //_mainWindow._mapControl.Map.Layers.Add(lineTask.Result);

            var linePanel = await LinePanel.AsyncCreate(_mainWindow, button.Tag.ToString());

            _mainWindow._mapControl.Map.Layers.Add(lineTask.Result);
            _mainWindow.MenuFrame.Content = linePanel;
        }

        private void StationsButton_Click(object sender, RoutedEventArgs e)
        {
            StationMenu stationMenu = new StationMenu(MainWindow);
            MainWindow.MenuFrame.Content = stationMenu;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddLinePage addLinePage = new AddLinePage(MainWindow);
            MainWindow.MenuFrame.Content = addLinePage;
        }

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleMenu vehicleMenu = new VehicleMenu(MainWindow);
            MainWindow.MenuFrame.Content = vehicleMenu;
        }
    }
}
