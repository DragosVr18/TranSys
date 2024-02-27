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
    /// Interaction logic for LineButton.xaml
    /// </summary>
    public partial class LineButton : Button
    {
        public LineButton(string name, string description)
        {
            InitializeComponent();
            NameLabel.Content = name;
            DescriptionTextBlock.Text = description;
        }
    }
}
