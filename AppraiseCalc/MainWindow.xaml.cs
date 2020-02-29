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

        private static Settings singleton;
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

        public Action Clone()
        {
            return new Action()
            {
                appraisalType = this.appraisalType,
                singleMind = this.singleMind,
                utmostCaution = this.utmostCaution
            };
        }
    }

    public class Chain
    {
        public List<Action> actions;
        public Chain()
        {
            actions = new List<Action>();
        }
        public Chain Clone()
        {
            Chain clone = new Chain();
            foreach (Action a in actions) clone.actions.Add(a.Clone());
            return clone;
        }

        public string ChainDescription
        {
            get
            {
                StringBuilder desc = new StringBuilder();
                bool firstAction = true;
                foreach (Action a in actions)
                {
                    if (!firstAction) desc.Append(" - ");

                    if (a.singleMind) desc.Append("(SM)");
                    if (a.utmostCaution) desc.Append("(UC)");
                    desc.Append(a.appraisalType.ToString());

                    firstAction = false;
                }

                return desc.ToString();
            }
        }
        public int TotalWear
        {
            get
            {
                int wear = 0;
                foreach (Action a in actions)
                {
                    if (a.appraisalType == AppraisalType.Stickler) continue;
                    if (a.utmostCaution) continue;
                    wear += 10;
                }
                return wear;
            }
        }
        public int NumGatherAttempts
        {
            get
            {
                int remainingAttempts = Settings.Get().maxAttempts;
                foreach (Action a in actions)
                {
                    if (a.singleMind) continue;
                    remainingAttempts--;
                }
                return remainingAttempts;
            }
        }
        public float ExpectedRarity
        {
            get
            {
                float rarity = 0f;
                float multiplier = 1f;
                foreach (Action a in actions)
                {
                    switch (a.appraisalType)
                    {
                        case AppraisalType.Instinctual:
                            {
                                rarity += 1.15f * multiplier;
                                multiplier = 1f;
                            }
                            break;
                        case AppraisalType.Impulsive:
                            {
                                rarity += 0.9f * multiplier;
                                multiplier = 1f + Settings.Get().procChance * 0.5f;
                            }
                            break;
                        case AppraisalType.Stickler:
                            {
                                rarity += 0.5f * multiplier;
                                multiplier = 1f;
                            }
                            break;
                    }
                }
                return rarity;
            }
        }
        public int GpCost
        {
            get
            {
                int cost = 0;
                foreach (Action a in actions)
                {
                    if (a.singleMind) cost += 200;
                    if (a.utmostCaution) cost += 200;
                }
                return cost;
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
            
            int maxBuffs = Settings.Get().maxGp / 200;
            // There are 2 factors that limit the maximum number of actions we can perform.
            // 1. Wear. Aside from 10 wear per appraisal, we get 1 free Stickler, and free actions from Utmost Caution buffs.
            int maxActionsLimitedByWear = Settings.Get().maxWear / 10 + 1 + maxBuffs;
            // 2. Attempts. We get free actions from Single Mind buffs. We also must have at least 1 attempt remain after all appraisals.
            int maxActionsLimitedByAttempt = Settings.Get().maxAttempts - 1 + maxBuffs;

            int maxActions = Math.Min(maxActionsLimitedByWear, maxActionsLimitedByAttempt);

            // Start building chains.
            System.Action<Chain> extendChain = null;  // In-line definition doesn't allow recursion for some reason
            extendChain = (Chain c) => 
            {
                if (c.actions.Count > 0
                    && c.NumGatherAttempts >= 1
                    && c.TotalWear <= Settings.Get().maxWear)
                {
                    results.Add(c.Clone());
                }

                if (c.actions.Count >= maxActions) return;
                foreach (AppraisalType type in Enum.GetValues(typeof(AppraisalType)))
                {
                    Action a = new Action();
                    a.appraisalType = type;
                    c.actions.Add(a);
                    extendChain(c);
                    c.actions.RemoveAt(c.actions.Count - 1);
                }
            };
            extendChain(new Chain());
        }

        #region Event handling and input parsing
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
