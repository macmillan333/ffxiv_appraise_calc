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
    public class Settings
    {
        public int maxWear;
        public int maxAttempts;
        public int maxGp;
        public float procChance;

        public static Settings singleton;
        public static Settings Get()
        {
            if (singleton == null)
            {
                singleton = new Settings();
            }
            return singleton;
        }
    }

    // Each action costs 1 gathering attempt, unless Single Mind is active.
    public enum AppraisalType
    {
        Instinctual,  // 0.8-1.5x, expects 1.15x
        Impulsive,  // 0.9x, some chance for Discerning Eye, which makes effect of next appraisal +50%
        Stickler,  // 0.5x, doesn't generate wear, can only be used once
    }

    // Each action costs 1 gathering attempt and generates 10 wear, unless buffed
    public class Action
    {
        public AppraisalType appraisalType;
        public bool singleMind;  // costs 200 GP; if true, costs no gathering attempt
        public bool utmostCaution;  // costs 200 GP; if true, doesn't generate wear
    }

    public class Chain
    {
        public List<Action> actions;
        public Chain()
        {
            actions = new List<Action>();
        }

        public string ChainDescription
        {
            get
            {
                return "chain";
            }
        }
        public int NumGatherAttempts
        {
            get
            {
                return Settings.Get().maxAttempts;
            }
        }
        public float ExpectedRarity
        {
            get
            {
                return 3.4f;
            }
        }
        public int GpCost
        {
            get
            {
                return 400;
            }
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SolidColorBrush normalBrush;
        private SolidColorBrush invalidBrush;
        private ObservableCollection<Chain> results;
        public MainWindow()
        {
            results = new ObservableCollection<Chain>();
            normalBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            invalidBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            InitializeComponent();
            resultGrid.DataContext = results;
        }

        private void RefreshResults()
        {
            results.Clear();
            Chain chain = new Chain();
            chain.actions = new List<Action>();
            Action action = new Action();
            chain.actions.Add(action);
            results.Add(chain);
        }

        #region Event handling and settings parsing
        private void MaxWearTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(maxWearTextbox.Text, out Settings.Get().maxWear)
                && Settings.Get().maxWear >= 0)
            {
                maxWearTextbox.Foreground = normalBrush;
            }
            else
            {
                maxWearTextbox.Foreground = invalidBrush;
            }
            RefreshResults();
        }

        private void MaxAttemptTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(maxAttemptTextbox.Text, out Settings.Get().maxAttempts)
               && Settings.Get().maxAttempts >= 0)
            {
                maxAttemptTextbox.Foreground = normalBrush;
            }
            else
            {
                maxAttemptTextbox.Foreground = invalidBrush;
            }
            RefreshResults();
        }

        private void MaxGpTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(maxGpTextbox.Text, out Settings.Get().maxGp)
                && Settings.Get().maxGp >= 0)
            {
                maxGpTextbox.Foreground = normalBrush;
            }
            else
            {
                maxGpTextbox.Foreground = invalidBrush;
            }
            RefreshResults();
        }

        private void ProcChanceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(procChanceTextbox.Text, out Settings.Get().procChance)
                && Settings.Get().procChance >= 0f
                && Settings.Get().procChance <= 1f)
            {
                procChanceTextbox.Foreground = normalBrush;
            }
            else
            {
                procChanceTextbox.Foreground = invalidBrush;
            }
            RefreshResults();
        }
        #endregion
    }
}
