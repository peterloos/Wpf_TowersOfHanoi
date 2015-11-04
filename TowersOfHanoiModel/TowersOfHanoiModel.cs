using System;

namespace TowersOfHanoiModel
{
    public delegate void DiscMovedHandler(int from, int to);

    public interface ITowersOfHanoi
    {
        int Discs { get; set; }
        void DoSimulation();
        event DiscMovedHandler DiscMoved;
    }

    public class HanoiTowerModelImpl : ITowersOfHanoi
    {
        public event DiscMovedHandler DiscMoved;

        private int discs;

        // c'tor
        public HanoiTowerModelImpl()
        {
            this.discs = 3;
        }

        // properties
        public int Discs
        {
            get { return this.discs; }
            set { this.discs = value; }
        }

        // public interface
        public void DoSimulation()
        {
            Console.WriteLine("Simulation started:");
            this.MoveTower(this.discs, 1, 2, 3);
            Console.WriteLine("Simulation stopped.");
        }

        // private helper methods
        private void MoveTower(int discs, int from, int tmp, int to)
        {
            if (discs > 0)
            {
                this.MoveTower(discs - 1, from, to, tmp);  // move tower of height n-1
                this.MoveDisc (from, to);                  // move lowest disc
                this.MoveTower(discs - 1, tmp, from, to);  // move remaining tower
            }
        }

        private void MoveDisc(int from, int to)
        {
            if (this.DiscMoved != null)
                this.DiscMoved(from, to);
        }
    }
}
