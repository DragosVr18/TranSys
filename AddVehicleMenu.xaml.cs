using Neo4j.Driver;
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
    /// Interaction logic for AddVehicleMenu.xaml
    /// </summary>
    public partial class AddVehicleMenu : Page
    {
        private MainWindow _mainWindow;
        private string vehicleType;
        private List<Line> lines;
        public MainWindow MainWindow { get { return _mainWindow; } }

        public AddVehicleMenu(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            lines = mainWindow._service.Lines;
        }

        private void BusButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "bus";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            updateAssignedLineComboBox();
        }

        private void TrolleyButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "trolley";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            updateAssignedLineComboBox();
        }

        private void TramButton_Click(object sender, RoutedEventArgs e)
        {
            vehicleType = "tram";
            BusButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TrolleyButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x50, 0x50, 0x50));
            TramButton.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x78, 0x78, 0x78));
            updateAssignedLineComboBox();
        }

        private void updateAssignedLineComboBox()
        {
            AssignedLineComboBox.Items.Clear();
            AssignedLineComboBox.Text = "";
            foreach(var line in lines.FindAll(x => x.VehicleType == vehicleType))
            {
                AssignedLineComboBox.Items.Add(line.Name);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string model = ModelTextBox.Text;
            string assignedLine = AssignedLineComboBox.Text;

            List<OffsetTime> tourDepartures = new List<OffsetTime>();
            List<OffsetTime> backDepartures = new List<OffsetTime>();
            foreach(var time in TourTimeTable.Items)
            {
                var t = time.ToString();
                int hour = int.Parse(t.Substring(0, t.IndexOf(':')));
                int minute = int.Parse(t.Substring(t.IndexOf(':') + 1));
                tourDepartures.Add(new OffsetTime(hour, minute, 0, 0));
            }
            foreach (var time in BackTimeTable.Items)
            {
                var t = time.ToString();
                int hour = int.Parse(t.Substring(0, t.IndexOf(':')));
                int minute = int.Parse(t.Substring(t.IndexOf(':') + 1));
                backDepartures.Add(new OffsetTime(hour, minute, 0, 0));
            }

            //to add validator

            int id = MainWindow._service.getFirstAvailableID();
            MainWindow._service.addVehicle(id, model, vehicleType, assignedLine, tourDepartures, backDepartures);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            VehicleMenu vehicleMenu = new VehicleMenu(MainWindow);
            MainWindow.MenuFrame.Content = vehicleMenu;
        }

        private void AddTourButton_Click(object sender, RoutedEventArgs e)
        {
            string time = TourTimePicker.HourComboBox.Text + ":" + TourTimePicker.MinuteComboBox.Text;
            foreach(var t in TourTimeTable.Items)
            {
                if (t as string == time)
                    { MessageBox.Show("This time is already selected"); return; }
            }
            TourTimeTable.Items.Add(time);
        }

        private void AddBackButton_Click(object sender, RoutedEventArgs e)
        {
            string time = BackTimePicker.HourComboBox.Text + ":" + BackTimePicker.MinuteComboBox.Text;
            foreach (var t in BackTimeTable.Items)
            {
                if (t as string == time)
                { MessageBox.Show("This time is already selected"); return; }
            }
            BackTimeTable.Items.Add(time);
        }
    }
}
