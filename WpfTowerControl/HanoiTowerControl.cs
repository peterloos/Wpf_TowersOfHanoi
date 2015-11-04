using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfTowerControl
{
    public class HanoiTowerView : Canvas
    {
        private const int TopPosition = 12;
        private const int WidthScaleFactor = 25;
        private const int DefaultAnimationSpeed = 70;

        // disc sizes of tower (valid elements: 1, 2, ..., 7)
        private List<int> discs;

        // floating disc, if any
        private int floatingDiscPosition;
        private Rectangle floatingDiscRectangle;

        // threading utils
        private Task t;
        private int speed;
        private bool isActive;

        // c'tor
        public HanoiTowerView()
        {
            this.Background = Brushes.LightGray;
            this.Height = 300;
            this.Width = 200;

            // setup empty stack
            Point p1 = new Point(10, this.Height - 10);
            Point p2 = new Point(this.Width - 10, this.Height - 10);
            Point p3 = new Point(this.Width - 10, this.Height - 30);
            Point p4 = new Point(this.Width / 2 + 10, this.Height - 30);
            Point p5 = new Point(this.Width / 2 + 10, 100);
            Point p6 = new Point(this.Width / 2 - 10, 100);
            Point p7 = new Point(this.Width / 2 - 10, this.Height - 30);
            Point p8 = new Point(10, this.Height - 30);

            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(p1);
            pointCollection.Add(p2);
            pointCollection.Add(p3);
            pointCollection.Add(p4);
            pointCollection.Add(p5);
            pointCollection.Add(p6);
            pointCollection.Add(p7);
            pointCollection.Add(p8);

            Polygon poly = new Polygon();
            poly.Fill = SystemColors.ControlDarkBrush;
            poly.Points = pointCollection;
            this.Children.Add(poly);

            // no discs, yet ...
            this.discs = new List<int>();

            // threading utils
            this.t = null;
            this.isActive = false;
            this.speed = DefaultAnimationSpeed;
        }

        // properties
        public int DiscSpeed
        {
            get
            {
                return this.speed;
            }

            set
            {
                this.speed = value;
            }
        }

        // public interface
        public void Create(int levels)
        {
            // add disc sizes according to number of levels
            this.discs.Clear();
            for (int i = 0; i < levels; i++)
                this.discs.Add(levels - i);

            if (this.Children.Count > 1)
                this.Children.RemoveRange(1, this.Children.Count - 1);
            for (int i = 0; i < levels; i++)
                this.AddRectangle(this.discs[i], i + 1, Colors.Yellow);
        }

        public void Push(int size)
        {
            // is tower full
            if (this.discs.Count >= 7)
                return;

            // add disc size
            this.discs.Add(size);

            // create and add a new rectangle
            this.AddRectangle(size, this.discs.Count, Colors.Yellow);
        }

        public void PushAnimated(int size)
        {
            // any animation active
            if (this.t != null)
                return;

            // is tower full
            if (this.discs.Count >= 7)
                return;

            // add disc size
            this.discs.Add(size);

            // create and add a new rectangle
            Rectangle rect = this.AddRectangle(size, this.discs.Count, Colors.Red);

            // set floating recangle's position for animation
            this.floatingDiscPosition = TopPosition;
            this.floatingDiscRectangle = rect;

            // .. and animate this disc
            this.isActive = true;
            this.t = Task.Factory.StartNew(() => { this.PushSimulation(); });
        }

        // task procedure
        private void PushSimulation()
        {
            // position of top most disc in internal stack
            int top = this.discs.Count - 1;

            while (this.floatingDiscPosition != top)
            {
                if (!this.isActive)
                    break;

                this.UpdateDiscLocation(this.floatingDiscRectangle, this.floatingDiscPosition);
                Thread.Sleep(this.speed);
                this.floatingDiscPosition--;
            }

            // change color of disc
            this.ChangeDiscColor(this.floatingDiscRectangle, Colors.Yellow); 

            // task is no more longer needed
            this.t = null;
        }

        public int Pop()
        {
            // empty tower
            if (this.discs.Count == 0)
                return -1;

            // retrieve size of last rectangle
            int size = this.discs[this.discs.Count - 1];

            // remove topmost disc from stack
            this.discs.RemoveAt(this.discs.Count - 1);

            // remove rectangle
            this.Children.RemoveAt(this.Children.Count - 1);

            return size;
        }

        public int PopAnimated()
        {
            // any animation active
            if (this.t != null)
                return -1;

            // empty tower
            if (this.discs.Count == 0)
                return -1;

            // retrieve infos about topmost (floating) rectangle
            this.floatingDiscPosition = this.discs.Count;
            this.floatingDiscRectangle = this.AttachTopMostRectangle();

            // retrieve size of last rectangle
            int size = this.discs[this.discs.Count - 1];

            // remove topmost disc from internal stack
            this.discs.RemoveAt(this.discs.Count - 1);

            // create new task
            this.isActive = true;
            this.t = Task.Factory.StartNew(() => { this.PopSimulation(); });

            return size;
        }

        private void PopSimulation()
        {
            // change color of disc
            this.ChangeDiscColor(this.floatingDiscRectangle, Colors.Red);
            Thread.Sleep(this.speed);

            while (this.floatingDiscPosition != TopPosition)
            {
                if (!this.isActive)
                    break;

                this.UpdateDiscLocation(this.floatingDiscRectangle, this.floatingDiscPosition);
                Thread.Sleep(this.speed);
                this.floatingDiscPosition++;
            }

            // remove disc from tower
            this.RemoveRectangle(this.floatingDiscRectangle);

            // task is no more longer needed
            this.t = null;
        }

        public void WaitForEndOfSimulation()
        {
            if (this.t != null)
                this.t.Wait();
        }

        public void Stop()
        {
            if (this.t != null)
            {
                this.isActive = false;
                this.t.Wait();
            }
        }

        // helper methods
        private void UpdateDiscLocation(Rectangle rect, int offset)
        {
            if (this.Dispatcher.CheckAccess())
            {
                // set dependency property 'Canvas.Top'
                double y = this.Height - 30 - offset * 20;
                rect.SetCurrentValue(Canvas.TopProperty, y);
            }
            else
            {
                this.Dispatcher.Invoke(
                    (Action)(() => { this.UpdateDiscLocation(rect, offset); })
                );
            }
        }

        private void ChangeDiscColor(Rectangle rect, Color color)
        {
            if (this.Dispatcher.CheckAccess())
            {
                rect.Fill = new SolidColorBrush(color);
            }
            else
            {
                this.Dispatcher.Invoke(
                    (Action)(() => { this.ChangeDiscColor(rect, color); })
                );
            }
        }

        private Rectangle AttachTopMostRectangle()
        {
            if (this.Dispatcher.CheckAccess())
            {
                return (Rectangle) this.Children[this.Children.Count - 1];
            }
            else
            {
                Rectangle tmp = null;
                this.Dispatcher.Invoke(
                    (Action)(() => { tmp = this.AttachTopMostRectangle(); })
                );
                return tmp;
            }
        }

        private void RemoveRectangle (Rectangle r)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.Children.Remove(r);
            }
            else
            {
                // this.Dispatcher.BeginInvoke(
                this.Dispatcher.Invoke(
                   (Action)(() => { this.RemoveRectangle(r); })
                );
            }
        }

        private Rectangle AddRectangle(int size, int position, Color color)
        {
            if (this.Dispatcher.CheckAccess())
            {
                int width = WidthScaleFactor * size;
                double x = (this.Width - width) / 2;
                double y = this.Height - 30 - position * 20;
                Rectangle rect = this.CreateRectangle(x, y, width, 20, color);
                this.Children.Add(rect);
                return rect;
            }
            else
            {
                Rectangle rect = null;
                this.Dispatcher.Invoke(
                    (Action)(() => { rect = this.AddRectangle(size, position, color); })
                );
                return rect;
            }
        }

        private Rectangle CreateRectangle(double x, double y, int width, int height, Color color)
        {
            Rectangle rect = new Rectangle();
            rect.Height = 20;
            rect.Width = width;
            rect.Fill = new SolidColorBrush(color);
            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 1;

            // set dependency properties 'Canvas.Left' and 'Canvas.Top'
            rect.SetCurrentValue(Canvas.LeftProperty, x);
            rect.SetCurrentValue(Canvas.TopProperty, y);
            return rect;
        }
    }
}
