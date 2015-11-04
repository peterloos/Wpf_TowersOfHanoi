using System;
using System.Windows;
using System.Windows.Controls;

using System.Threading;
using System.Threading.Tasks;

using TowersOfHanoiModel;
using WpfTowerControl;

namespace TowersOfHanoi
{
    public partial class MainWindow : Window
    {
        // number of discs
        public const int MaxDiscs = 4;

        // model
        private ITowersOfHanoi model;
        private int discSpeed;

        // threading utils
        private Task t;
        private bool isActive;

        public MainWindow()
        {
            this.InitializeComponent();

            // create model
            this.model = new HanoiTowerModelImpl();
            this.model.Discs = MaxDiscs;
            this.model.DiscMoved += new DiscMovedHandler(this.Model_DiscMoved);

            // adjust views
            this.TowerLeft.Create(this.model.Discs);
            this.TowerMiddle.Create(0);
            this.TowerRight.Create(0);

            // setup controller
            for (int i = 0; i < 7; i++)
                this.ComboBox_Discs.Items.Add(i + 1);

            this.ComboBox_DiscSpeed.Items.Add(10);
            this.ComboBox_DiscSpeed.Items.Add(50);
            this.ComboBox_DiscSpeed.Items.Add(75);
            this.ComboBox_DiscSpeed.Items.Add(100);
            this.ComboBox_DiscSpeed.Items.Add(150);

            this.ComboBox_Discs.SelectedItem = this.model.Discs;
            this.ComboBox_DiscSpeed.SelectedItem = 50;
        }

        // event handlers
        private void ButtonTest_Click(Object sender, RoutedEventArgs e)
        {
            if (sender == this.ButtonStart)
            {
                // any animation active
                if (this.t != null)
                    return;

                this.isActive = true;
                this.t = Task.Factory.StartNew(() => {
                    this.model.DoSimulation();
                    this.t = null;
                });
            }
            else if (sender == this.ButtonStop)
            {
                this.isActive = false;
            }
            else if (sender == this.ButtonClear)
            {
                this.TowerLeft.Create(0);
                this.TowerMiddle.Create(0);
                this.TowerRight.Create(0);
            }
        }

        private void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            int value = (int) comboBox.SelectedItem;

            if (sender == this.ComboBox_Discs)
            {
                this.TowerLeft.Create(value);
                this.TowerMiddle.Create(0);
                this.TowerRight.Create(0);

                this.model.Discs = value;

            }
            else if (sender == this.ComboBox_DiscSpeed)
            {
                this.discSpeed = value;
                this.TowerLeft.DiscSpeed = this.discSpeed;
                this.TowerMiddle.DiscSpeed = this.discSpeed;
                this.TowerRight.DiscSpeed = this.discSpeed;
            }
        }

        private void Model_DiscMoved(int from, int to)
        {
            // premature end of simulation
            if (!this.isActive)
                return;

            Console.WriteLine(">>> Moving disk from {0} to {1}", from, to);

            // pop disc
            int size = -1;
            switch (from)
            {
                case 1:  // Left
                    size = this.TowerLeft.PopAnimated();
                    this.TowerLeft.WaitForEndOfSimulation();
                    break;

                case 2:  // Middle
                    size = this.TowerMiddle.PopAnimated();
                    this.TowerMiddle.WaitForEndOfSimulation();
                    break;

                case 3:  // Right
                    size = this.TowerRight.PopAnimated();
                    this.TowerRight.WaitForEndOfSimulation();
                    break;
            }

            // Thread.Sleep(10 * this.discSpeed);

            // push disc
            switch (to)
            {
                case 1:  // Left
                    this.TowerLeft.PushAnimated(size);
                    this.TowerLeft.WaitForEndOfSimulation();
                    break;

                case 2:  // Middle
                    this.TowerMiddle.PushAnimated(size);
                    this.TowerMiddle.WaitForEndOfSimulation();
                    break;

                case 3:  // Right
                    this.TowerRight.PushAnimated(size);
                    this.TowerRight.WaitForEndOfSimulation();
                    break;
            }

            // Thread.Sleep(10 * this.discSpeed);

            Console.WriteLine("<<< Moving disk from {0} to {1}", from, to);
        }
    }
}
