using DevExpress.Xpf.Charts;
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
using System.Windows.Threading;

namespace SMGExpression.Charts
{
    /// <summary>
    /// CustomMarker2DControl.xaml 的交互逻辑
    /// </summary>
    public partial class CustomMarker2DControl : UserControl
    {
        public DispatcherTimer time2;
        DrawingCanvas dd = new DrawingCanvas();
        private MainWindow mw;
        public CustomMarker2DControl()
        {
            InitializeComponent();
        }
        public CustomMarker2DControl(MainWindow mm,int segmentCount)
        {
            InitializeComponent();
            mw = mm;
            GetSocket(segmentCount);
        }
        public int joyCountSegmentOne = 0;
        public int joyCountSegmentTwo = 0;
        public int joyCountSegmentThree = 0;
        public int joyCountSegmentFour = 0;
        public int joyCountSegmentFive = 0;
        public int joyCountSegmentSex = 0;
        public int joyCountSegmentSeven = 0;
        void ChartsDemoModule_ModuleAppear(object sender, RoutedEventArgs e)
        {
            chart.Animate();
        }
        private void GetSocket(int segmentCount)
        {
            for (int i = 0; i < chart.Diagram.Series.Count; i++)
            {
                for (int r = 1; r <=segmentCount; r++)
                {
                    SeriesPoint seriesPoint = new SeriesPoint();
                    seriesPoint.Argument = "环节" + r;
                    seriesPoint.Value = 0;
                    chart.Diagram.Series[i].Points.Add(seriesPoint);
                }
            }
            time2 = new DispatcherTimer();
            time2.Interval = TimeSpan.FromSeconds(1);
            time2.Tick += (ssender, args) =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {               
                    switch (mw.content)
                    {
                        case "环节开始":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentOne, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(0, joyCountSegmentOne);
                            break;
                        case "环节2":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentTwo, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(1,joyCountSegmentTwo);
                            break;
                        case "环节3":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentThree, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(2, joyCountSegmentThree);
                            break;
                        case "环节4":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentFour, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(3, joyCountSegmentFour);
                            break;
                        case "环节5":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentFive, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(4, joyCountSegmentFive);
                            break;
                        case "环节6":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentSex, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(5, joyCountSegmentSex);
                            break;
                        case "环节7":
                            GetCount(mw.canvas.ee.Joy, mw.canvas1.ee.Joy, mw.canvas2.ee.Joy, mw.canvas3.ee.Joy, ref joyCountSegmentSeven, mw.canvas.Faces.Count, mw.canvas1.Faces.Count, mw.canvas2.Faces.Count, mw.canvas3.Faces.Count);
                            GetCustome(6, joyCountSegmentSeven);
                            break;
                        default:
                            break;
                    }
                }));
               
                
            };
            time2.Start();
        }
        private void GetCustome(int i,int segmentCount)
        {
            foreach (LineSeries2D series in chart.Diagram.Series)
            {
                series.Points[i].Value = segmentCount;
            };
        }
        private void GetCount(double i,double ii,double iii,double iiii,ref int segment,int faceOne,int faceTwo,int faceThree,int faceFour)
        {
            if (i >= 1 && faceOne > 0)
                segment++;
            if (ii >= 1 && faceTwo > 0)
                segment++;
            if (iii >= 1 && faceThree > 0)
                segment++;
            if (iiii >= 1 && faceFour > 0)
                segment++;
        }
    }
}
