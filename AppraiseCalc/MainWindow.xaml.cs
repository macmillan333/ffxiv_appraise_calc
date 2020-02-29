using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AppraiseCalc
{
    public class Chain
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Chain> results;
        public MainWindow()
        {
            results = new ObservableCollection<Chain>();
            InitializeComponent();
            resultGrid.DataContext = results;
        }

        private void RefreshResults()
        {
            int maxWear = 0;
            if (!int.TryParse(maxWearTextbox.Text, out maxWear))
            {
                return;
            }

            results.Clear();
            results.Add(new Chain()
            {
                Name = "name",
                Value = maxWear
            });
        }

        private void MaxWearTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }

        private void MaxAttemptTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }

        private void MaxGpTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }

        private void ProcChanceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshResults();
        }
    }
}
