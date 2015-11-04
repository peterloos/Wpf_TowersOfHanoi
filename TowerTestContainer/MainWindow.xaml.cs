using System;
using System.Windows;
using TowersOfHanoiModel;

namespace TowerTestContainer
{
    public partial class MainWindow : Window
    {
        // just for testing towers with different sized discs
        private Random rand;

        public MainWindow()
        {
            // TestModelConsoleBased();

            this.InitializeComponent();
            this.rand = new Random();
        }

        private void TestModelConsoleBased()
        {
            ITowersOfHanoi model = new HanoiTowerModelImpl();
            model.Discs = 4;
            model.DiscMoved += new DiscMovedHandler(this.DiscMovedHandler);
            model.DoSimulation();
            return;
        }

        private void DiscMovedHandler(int from, int to)
        {
            Console.WriteLine(">>> Moving disk from {0} to {1}", (int)from, (int)to);
        }

        // event handlers
        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            if (sender == this.ButtonTestPush)
            {
                this.TestingPush();
            }
            else if (sender == this.ButtonTestPop)
            {
                this.TestingPop();
            }
            else if (sender == this.ButtonTestPushAnimated)
            {
                this.TestingPushAnimated();
            }
            else if (sender == this.ButtonTestPopAnimated)
            {
                this.TestingPopAnimated();
            }
        }

        // testing methods
        private void TestingPush()
        {
            int size = 1 + this.rand.Next(6);
            this.TowerLeft.Push(size);
        }

        private void TestingPop()
        {
            this.TowerLeft.Pop();
        }

        private void TestingPushAnimated()
        {
            int size = 1 + this.rand.Next(6);
            this.TowerLeft.PushAnimated(size);
        }

        private void TestingPopAnimated()
        {
            this.TowerLeft.PopAnimated();
        }
    }
}
