using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Charts;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Windows.Threading;

namespace SMGExpression.Charts
{
    /// <summary>
    /// BarSideBySide2DControl.xaml 的交互逻辑
    /// </summary>
    public partial class BarSideBySide2DControl : UserControl
    {
        public DispatcherTimer time1;
        private MainWindow mw;
        public BarSideBySide2DControl()
        {
            InitializeComponent();
        }
        //重载函数传入主对象和环节个数
        public BarSideBySide2DControl(MainWindow mm, int SegmentCount)
        {
            InitializeComponent();
            mw = mm;
            GetSocket(SegmentCount);
        }
        System.Timers.Timer aTimer = new System.Timers.Timer();

        void ChartsDemoModule_ModuleAppear(object sender, RoutedEventArgs e)
        {
            chart.Animate();
        }
        private double SumSuprise = 0;
        private double SumDisgust = 0;
        private double SumFear = 0;
        private double SumAnger = 0;
        private double SumSadness = 0;
        private double SumJoy = 0;
        /// <summary>
        /// 动态设置不同环节数值
        /// </summary>
        /// <param name="i">代表当前环节</param>
        private void GetCustome(int i)
        {
            SumSuprise += GetAvg("Suprise");
            SumDisgust += GetAvg("Disgust");
            SumFear += GetAvg("Disgust");
            SumAnger += GetAvg("Anger");
            SumSadness += GetAvg("Sadness");
            SumJoy += GetAvg("Joy");
            SURPRISE.Points[i].Value = SumSuprise;
            DISGUST.Points[i].Value = SumDisgust;
            FEAR.Points[i].Value = SumFear;
            ANGER.Points[i].Value = SumAnger;
            SADENESS.Points[i].Value = SumSadness;
            JOY.Points[i].Value = SumJoy;
        }
        private void ResetVal()
        {
            SumSuprise = 0;
            SumDisgust = 0;
            SumFear = 0;
            SumAnger = 0;
            SumSadness = 0;
            SumJoy = 0;
        }
        private int SegmentTwoCount { get; set; }
        private int SegmentThreeCount { get; set; }
        private int SegmentFourCount { get; set; }
        private int SegmentFiveCount { get; set; }
        private int SegmentSexCount { get; set; }
        private int SegmentSevenCount { get; set; }
        public void GetSocket(int count)
        {
            for (int i = 0; i < chart.Diagram.Series.Count; i++)
            {
                for (int r = 1; r <= count; r++)
                {
                    SeriesPoint seriesPoint = new SeriesPoint();
                    seriesPoint.Argument = "环节" + r;
                    seriesPoint.Value = 0;
                    chart.Diagram.Series[i].Points.Add(seriesPoint);
                }
            }
            time1 = new DispatcherTimer();
            time1.Interval = TimeSpan.FromSeconds(1);
            //默认最多显示七个环节
            time1.Tick += (ssender, args) =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    switch (mw.content)
                    {
                        case "环节开始":
                            GetCustome(0);
                            break;
                        case "环节2":
                            if (SegmentTwoCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(1);
                            SegmentTwoCount++;
                            break;
                        case "环节3":
                            if (SegmentThreeCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(2);
                            SegmentThreeCount++;
                            break;
                        case "环节4":
                            if (SegmentFourCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(3);
                            SegmentFourCount++;
                            break;
                        case "环节5":
                            if (SegmentFiveCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(4);
                            SegmentFiveCount++;
                            break;
                        case "环节6":
                            if (SegmentSexCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(5);
                            SegmentSexCount++;
                            break;
                        case "环节7":
                            if (SegmentSevenCount == 0)//第一次制0
                                ResetVal();
                            GetCustome(6);
                            SegmentSevenCount++;
                            break;
                        default:
                            break;
                    }
                }));

            };
            time1.Start();
        }
        private double GetAvg(string str)
        {
            double value = 0;
            switch (str)
            {
                case "Joy":
                    value = ((mw.canvas.ee.Joy + mw.canvas1.ee.Joy + mw.canvas2.ee.Joy + mw.canvas3.ee.Joy) / 4)*5;
                    break;
                case "Sadness":
                    value = (mw.canvas.ee.Sadness + mw.canvas1.ee.Sadness + mw.canvas2.ee.Sadness + mw.canvas3.ee.Sadness) / 40;
                    break;
                case "Anger":
                    value = (mw.canvas.ee.Anger + mw.canvas1.ee.Anger + mw.canvas2.ee.Anger + mw.canvas3.ee.Anger) / 4;
                    break;
                case "Disgust":
                    value = (mw.canvas.ee.Disgust + mw.canvas1.ee.Disgust + mw.canvas2.ee.Disgust + mw.canvas3.ee.Disgust) / 40;
                    break;
                case "Suprise":
                    value = (mw.canvas.ee.Surprise + mw.canvas1.ee.Surprise + mw.canvas2.ee.Surprise + mw.canvas3.ee.Surprise) / 4;
                    break;
                case "Fear":
                    value = (mw.canvas.ee.Fear + mw.canvas1.ee.Fear + mw.canvas2.ee.Fear + mw.canvas3.ee.Fear) / 4;
                    break;
                default:
                    break;
            }
            return value;
        }
    }
}
