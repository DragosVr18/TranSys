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
    /// Interaction logic for VehicleMenu.xaml
    /// </summary>
    public partial class VehicleMenu : Page
    {
        private MainWindow _mainWindow;
        public MainWindow MainWindow { get { return _mainWindow; } }
        public VehicleMenu(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            int i = 0;
            int margin = 0;
            foreach (var vehicle in _mainWindow._service.getAllVehicles())
            {
                Button btn = new Button();
                btn.Tag = vehicle.Id;
                //btn.HorizontalContentAlignment = HorizontalAlignment.Left;
                btn.BorderBrush = Brushes.Transparent;
                btn.Foreground = new SolidColorBrush(Color.FromArgb(0xD8, 0xFF, 0xFF, 0xFF));
                btn.FontSize = 18;
                btn.Content = vehicle.Model +"  "+ vehicle.Id;
                if (vehicle.Type == "bus")
                    btn.Background = Brushes.DarkMagenta;
                else if (vehicle.Type == "trolley")
                    btn.Background = Brushes.DarkSlateBlue;
                else
                    btn.Background = Brushes.DarkGreen;
                btn.VerticalAlignment = VerticalAlignment.Top;
                btn.Width = 270;
                btn.Height = 50;

                btn.Margin = new Thickness(0, i * 50 + margin, 0, 0);
                //btn.Click += LineButton_Click;
                margin += 10;
                i++;
                VehiclesGrid.Children.Add(btn);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleMenu addVehicleMenu = new AddVehicleMenu(MainWindow);
            MainWindow.MenuFrame.Content = addVehicleMenu;
        }

        private void StationsButton_Click(object sender, RoutedEventArgs e)
        {
            StationMenu stationMenu = new StationMenu(MainWindow);
            MainWindow.MenuFrame.Content = stationMenu;
        }

        private void LinesButton_Click(object sender, RoutedEventArgs e)
        {
            LineMenu lineMenu = new LineMenu(_mainWindow);
            _mainWindow.MenuFrame.Content = lineMenu;
        }
    }
}
