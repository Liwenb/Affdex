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
    /// SeriesTitlesControl.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesTitlesControl : UserControl
    {
        public DispatcherTimer time1;
        public SeriesTitlesControl()
        {
            InitializeComponent();
        }
        private MainWindow mw;
        public SeriesTitlesControl(MainWindow mm)
        {
            InitializeComponent();            
            
            mw = mm;
            InitTime();
        }
        void ChartsDemoModule_ModuleAppear(object sender, RoutedEventArgs e)
        {
            chart.Animate();
            chartOne.Animate();
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
        public double SumSuprise = 0;
        public double SumDisgust = 0;
        public double SumFear = 0;
        public double SumAnger = 0;
        public double SumSadness = 0;
        public double SumJoy = 0;
        private void GetCustome()
        {
            SumSuprise += GetAvg("Suprise");
            SumDisgust += GetAvg("Disgust");
            SumFear += GetAvg("Fear");
            SumAnger += GetAvg("Anger");
            SumSadness += GetAvg("Sadness");
            SumJoy += GetAvg("Joy");
        }
        private void InitTime()
        {
            time1 = new DispatcherTimer();
            time1.Interval = TimeSpan.FromSeconds(1);
            //默认最多显示七个环节
            time1.Tick += (ssender, args) =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    GetCustome();
                    foreach (PieSeries2D series in chart.Diagram.Series)
                    {
                        series.Points[0].Value = SumJoy;
                        series.Points[1].Value = SumSadness;
                        series.Points[2].Value = SumAnger;
                        series.Points[3].Value = SumDisgust;
                        series.Points[4].Value = SumSuprise;
                        series.Points[5].Value = SumFear;
                    }
                    foreach (PieSeries2D series in chartOne.Diagram.Series)
                    {
                        series.Points[0].Value = GetAvg("Joy");
                        series.Points[1].Value = GetAvg("Sadness");
                        series.Points[2].Value = GetAvg("Anger");
                        series.Points[3].Value = GetAvg("Disgust");
                        series.Points[4].Value = GetAvg("Suprise");
                        series.Points[5].Value = GetAvg("Fear");
                    }
                }));
            };
            time1.Start();
        }

    }
}