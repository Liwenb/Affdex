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
using Visifire.Charts;

namespace SMGExpression.Charts
{
    /// <summary>
    /// JoyChart.xaml 的交互逻辑
    /// </summary>
    public partial class JoyChart : UserControl
    {
        private static DataPoints dataPoints = null;
        public DispatcherTimer TimeLine;//男女平均线进程
        Chart chartTest;
        MainWindow mw;
        private double length = 0.0167;//一秒内滑动条走过的长度
        private List<Time_seg> list = new List<Time_seg>();
        public DataSeries dsman;
        public DataSeries dswoman;
        public DataSeries dsavg;
        public DataSeries dsjob;
        public DataSeries dsedu;
        public DataSeries dsincome;
        double boymax { get; set; }//男所对应x轴的长度
        double girlmax { get; set; }//女所对应x轴的长度
        double avgmax { get; set; }
        double edumax { get; set; }
        double jobmax { get; set; }
        double incomemax { get; set; }

        public JoyChart()
        {
            InitializeComponent();
        }
        public JoyChart(MainWindow mm)
        {
            InitializeComponent();
            mw = mm;
            dataPoints = new DataPoints();
            chartGrid.DataContext = dataPoints;
            CreateChart();
            InitData();
        }
        private DataPoint getDataPoint(double avg, double max)
        {
            DataPoint avgpointOne = new DataPoint();
            avgpointOne.YValue = avg;
            avgpointOne.XValue = max;
            return avgpointOne;
        }
        private double GetAvg(string str)
        {
            double value = 0;
            switch (str)
            {
                case "Joy":
                    value = ((mw.canvas.ee.Joy + mw.canvas1.ee.Joy + mw.canvas2.ee.Joy + mw.canvas3.ee.Joy) / 4) * 5 >= 100 ? 100 : ((mw.canvas.ee.Joy + mw.canvas1.ee.Joy + mw.canvas2.ee.Joy + mw.canvas3.ee.Joy) / 4) * 5;
                    break;
                case "Sadness":
                    value = (mw.canvas.ee.Sadness + mw.canvas1.ee.Sadness + mw.canvas2.ee.Sadness + mw.canvas3.ee.Sadness) / 10;
                    break;
                case "Anger":
                    value = (mw.canvas.ee.Anger + mw.canvas1.ee.Anger + mw.canvas2.ee.Anger + mw.canvas3.ee.Anger) / 4;
                    break;
                case "Disgust":
                    value = (mw.canvas.ee.Disgust + mw.canvas1.ee.Disgust + mw.canvas2.ee.Disgust + mw.canvas3.ee.Disgust) / 4;
                    break;
                case "Suprise":
                    value = (mw.canvas.ee.Surprise + mw.canvas1.ee.Surprise + mw.canvas2.ee.Surprise + mw.canvas3.ee.Surprise) / 10;
                    break;
                case "Fear":
                    value = (mw.canvas.ee.Fear + mw.canvas1.ee.Fear + mw.canvas2.ee.Fear + mw.canvas3.ee.Fear) / 4;
                    break;
                default:
                    break;
            }
            return value;
        }
        private void InitData()
        {
            InitPrimary();
            TimeLine = new DispatcherTimer();
            TimeLine.Interval = TimeSpan.FromSeconds(1);
            TimeLine.Tick += ((sender, args) =>
            {

                dataPoints.BoyPoint.Add(getDataPoint(GetAvg("Joy"), boymax));
                boymax += length;

                dataPoints.GirlPoint.Add(getDataPoint(GetAvg("Anger"), girlmax));
                girlmax += length;

                dataPoints.AvgPoint.Add(getDataPoint(GetAvg("Sadness"), avgmax));
                avgmax += length;

                dataPoints.User_edu.Add(getDataPoint(GetAvg("Suprise"), edumax));
                edumax += length;

                dataPoints.Income.Add(getDataPoint(GetAvg("Fear"), incomemax));
                incomemax += length;

                dataPoints.User_job.Add(getDataPoint(GetAvg("Disgust"), jobmax));
                jobmax += length;
                MySqlHelper.ExecuteDataSet("insert into expressionactualscore(test_id,Anger,Disgust,Fear,Joy,Sadness,Surprise,intime) VALUES(" + mw.testid + "," + GetAvg("Anger") + "," + GetAvg("Disgust") + "," + GetAvg("Fear") + "," + GetAvg("Joy") + "," + GetAvg("Sadness") + "," + GetAvg("Suprise") + ",'" + DateTime.Now + "');");
            });
            TimeLine.Start();
        }
        private double getTotalSecode()
        {
            list = MySqlHelper.GetTime_seg();
            for (int i = 0; i < list.Count - 1; i++)
            {
                AddSplitTime(Math.Round(double.Parse(list[i].Period) / 60, 1));
            }
            double totalsecode = double.Parse(list[list.Count - 1].Period);
            double aa = totalsecode / 60;
            return aa;
        }
        private void InitPrimary()
        {
            double Primary = 0;
            double count = getTotalSecode();
            for (int i = 0; i < count; i++)
            {
                DataPoint primarypoint = new DataPoint();
                primarypoint.YValue = 100;
                primarypoint.XValue = Primary;
                Primary += length;
                dataPoints.PrimaryPoint.Add(primarypoint);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
        private DataSeries InitTimeDataseries(double content)
        {
            DataSeries Onesplit = new DataSeries();
            Onesplit.ShadowEnabled = false;
            Onesplit.ShowInLegend = false;
            Onesplit.RenderAs = RenderAs.Line;
            Onesplit.MarkerEnabled = false;
            Onesplit.SelectionEnabled = false;
            Onesplit.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5DEB3"));
            DataPoint dp = new DataPoint();
            dp.XValue = content;
            dp.YValue = 0;
            DataPoint dpSplit = new DataPoint();
            dpSplit.XValue = content;
            dpSplit.YValue = 100;
            Onesplit.DataPoints.Add(dp);
            Onesplit.DataPoints.Add(dpSplit);
            return Onesplit;
        }
        private void AddSplitTime(double content)
        {
            DataSeries Onesplit = InitTimeDataseries(content);//获取环节分割线的时间和颜色
            chartTest.Series.Add(Onesplit);
        }
        private void InitOtherDataSeries(DataSeries ds, string pathText, string legend, Color color)
        {
            ds.ShadowEnabled = false;
            ds.LegendText = legend;
            ds.RenderAs = RenderAs.Line;
            ds.MarkerEnabled = false;
            ds.SelectionEnabled = false;
            ds.Color = new SolidColorBrush(color);

            Binding binding = new Binding()
            {
                Path = new PropertyPath(pathText),
                Source = dataPoints
            };
            ds.SetBinding(DataSeries.DataPointsProperty, binding);
            chartTest.Series.Add(ds);
        }
        public Dictionary<int, DataSeries> dicLineGroup = new Dictionary<int, DataSeries>();//新增线
        public int groupNumber { get; set; }
        private void CreateChart()
        {
            chartTest = new Chart();
            chartTest.Background = new SolidColorBrush(Colors.Transparent);
            chartTest.Theme = "Theme4";
            chartTest.Height = mw.ChartTwo.ActualHeight;
            chartTest.Width = mw.ChartTwo.ActualWidth;
            chartTest.ScrollingEnabled = false;
            chartTest.ShadowEnabled = false;

            Title title = new Visifire.Charts.Title();
            title.Text = "实时折线图";
            chartTest.Titles.Add(title);

            PlotArea plotarea = new PlotArea();
            plotarea.ShadowEnabled = false;
            plotarea.BorderBrush = new SolidColorBrush(Colors.Gray);
            plotarea.BorderThickness = new Thickness(0, 0, 0.5, 0);
            chartTest.PlotArea = plotarea;

            Axis xaxis = new Axis();
            xaxis.IntervalType = IntervalTypes.Auto;
            xaxis.StartFromZero = true;
            chartTest.AxesX.Add(xaxis);

            Axis yaxis = new Axis();
            yaxis.AxisMaximum = 100;
            chartTest.AxesY.Add(yaxis);

            DataSeries dsprimary = new DataSeries();
            dsprimary.RenderAs = RenderAs.Line;
            dsprimary.ShadowEnabled = false;
            dsprimary.SelectionEnabled = false;
            //dsprimary.Background = new SolidColorBrush(Colors.Transparent);
            dsprimary.Opacity = 0;
            dsprimary.ShowInLegend = false;
            dsprimary.Visibility = System.Windows.Visibility.Collapsed;
            //dsprimary.Color = new SolidColorBrush(Colors.Transparent);

            Binding binding = new Binding()
            {
                Path = new PropertyPath("PrimaryPoint"),
                Source = dataPoints
            };
            dsprimary.SetBinding(DataSeries.DataPointsProperty, binding);
            chartTest.Series.Add(dsprimary);

            dsman = new DataSeries();
            InitOtherDataSeries(dsman, "BoyPoint", "开心", (Color)ColorConverter.ConvertFromString("#0073ff"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dsman);
            //女
            dswoman = new DataSeries();
            dswoman.Opacity = 0;
            InitOtherDataSeries(dswoman, "GirlPoint", "悲伤", (Color)ColorConverter.ConvertFromString("#ff3d38"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dswoman);
            //平均
            dsavg = new DataSeries();
            dsavg.Opacity = 0;
            InitOtherDataSeries(dsavg, "AvgPoint", "愤怒", (Color)ColorConverter.ConvertFromString("#caff5f"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dsavg);

            dsjob = new DataSeries();
            dsjob.Opacity = 0;
            InitOtherDataSeries(dsjob, "User_job", "失望", (Color)ColorConverter.ConvertFromString("#BA55D3"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dsjob);

            dsedu = new DataSeries();
            dsedu.Opacity = 0;
            InitOtherDataSeries(dsedu, "User_edu", "惊讶", (Color)ColorConverter.ConvertFromString("#737373"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dsedu);

            dsincome = new DataSeries();
            dsincome.Opacity = 0;
            InitOtherDataSeries(dsincome, "Income", "害怕", (Color)ColorConverter.ConvertFromString("#66CDAA"));
            groupNumber++;
            dicLineGroup.Add(groupNumber, dsincome);
            chartGrid.Children.Add(chartTest);
        }
    }
}
