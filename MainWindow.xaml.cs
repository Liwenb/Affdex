using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Shapes;
using System.IO;
using Visifire.Charts;
using SMGExpression.Charts;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Net;
using System.Text;
using System.Management;
using System.Text.RegularExpressions;

namespace SMGExpression
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Affdex.ImageListener, Affdex.ProcessStatusListener
    {
        #region
        private Socket socket;//控制套socket
        private Socket socketjiank;//监控端的socket
        private readonly static string ip = "192.168.1.85";
        private DispatcherTimer TestConnectTimer;//实时连接socket线程
        private bool isSuccess { get; set; }
        public string content { get; set; }
        //private CustomMarker2DControl cc = new CustomMarker2DControl();//线图实例
        private JoyChart cc = new JoyChart();
        private SeriesTitlesControl ss = new SeriesTitlesControl();//饼图实例
        private BarSideBySide2DControl bb = new BarSideBySide2DControl();//柱状图实例


        private void AsyncReveive()
        {
            byte[] data = new byte[1024];
            try
            {
                //开始接收消息  
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None,
            IAsyncResult =>
            {
                try
                {
                    int length = socket.EndReceive(IAsyncResult);
                    if (length > 0)
                    {
                        content = Encoding.UTF8.GetString(data);
                        content = content.TrimEnd('\0');
                        this.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            InitReceive();
                        }));
                        AsyncReveive();
                    }
                    else
                    {
                        dispose();
                    }
                }
                catch (Exception)
                {
                    //进入异常后重新连接
                    ConnectSocket();
                }

            }, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public string SegmentCount { get; set; }//环节总数
        public string testid = string.Empty;
        public List<Time_seg> seg = new List<Time_seg>();
        /// <summary>
        /// 异步刷新UI
        /// </summary>
        private void InitReceive()
        {
            if (content.Contains("测试编号"))
            {
                testid = content.Substring(4);
            }
            else
            {
                switch (content)
                {
                    case "环节开始":
                        radioStp.Visibility = System.Windows.Visibility.Visible;
                        InsertTime.Start();
                        seg = MySqlHelper.GetTime_seg();
                        cc = new JoyChart(this);
                        ss = new SeriesTitlesControl(this);
                        bb = new BarSideBySide2DControl(this, seg.Count);
                        ChartTwo.Children.Add(cc);
                        Chartc.Children.Add(ss);
                        ChartOne.Children.Add(bb);
                        initsocket("smg,3," + ipadrlist[0] + ",11,1:2," + DateTime.Now + "\r\n");
                        initsocket("smg,3," + ipadrlist[0] + ",11,2:2," + DateTime.Now + "\r\n");
                        initsocket("smg,3," + ipadrlist[0] + ",11,3:2," + DateTime.Now + "\r\n");
                        initsocket("smg,3," + ipadrlist[0] + ",11,4:2," + DateTime.Now + "\r\n");
                        Chartc.Visibility = System.Windows.Visibility.Visible;
                        ChartOne.Visibility = System.Windows.Visibility.Visible;
                        ChartTwo.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "暂停":
                        InsertTime.Stop();
                        cc.TimeLine.Stop();
                        ss.time1.Stop();
                        bb.time1.Stop();
                        break;
                    case "继续":
                        InsertTime.Start();
                        cc.TimeLine.Start();
                        ss.time1.Start();
                        bb.time1.Start();
                        break;
                    case "结束":
                        InsertSegCount();
                        InsertTime.Stop();
                        cc.TimeLine.Stop();
                        ss.time1.Stop();
                        bb.time1.Stop();
                        this.Close();
                        Application.Current.Shutdown();
                        break;
                    default:
                        break;
                }
            }
        }
        private void InsertSegCount()
        {
            MySqlHelper.ExecuteDataSet("insert into expressiontotalscore(test_id,Anger,Disgust,Fear,Joy,Sadness,Surprise,intime) VALUES(" + testid + "," + ss.SumAnger + "," + ss.SumDisgust + "," + ss.SumFear + "," + ss.SumJoy + "," + ss.SumSadness + "," + ss.SumSuprise + ",'" + DateTime.Now + "');");
            //List<int> list = new List<int>();
            //list.Add(cc.joyCountSegmentOne);
            //list.Add(cc.joyCountSegmentTwo);
            //list.Add(cc.joyCountSegmentThree);
            //list.Add(cc.joyCountSegmentFour);
            //list.Add(cc.joyCountSegmentFive);
            //list.Add(cc.joyCountSegmentSex);
            //list.Add(cc.joyCountSegmentSeven);
            //for (int i = 0; i < int.Parse(SegmentCount); i++)
            //{
            //    MySqlHelper.ExecuteDataSet("insert into smiletotalscore(test_id,huanjieno,huanjiename,score,intime) VALUES(" + testid + "," + i + "," + "环节" + i + "," + list[i] + ",'" + DateTime.Now + "');");
            //}

        }
        /// <summary>
        /// 释放Socket
        /// </summary>
        private void dispose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// 开始连接socket 如果连接未成功线程开启持续连接，直到连上为止
        /// </summary>
        private void ConnectSocket()
        {
            try
            {
                socketjiank = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketjiank.Connect(IPAddress.Parse(ip), 8082);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(IPAddress.Parse(ip), 8083);
            }
            catch (Exception ex)
            {
                MessageBox.Show("服务未连接，请先开服务");
                TestConnectTimer = new DispatcherTimer();//如果进入异常新增线程重新连接
                isSuccess = false;
                TestConnectTimer.Interval = TimeSpan.FromSeconds(1);
                TestConnectTimer.Tick += (sender, f) =>
                {
                    try
                    {
                        if (isSuccess == false)
                        {
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socket.Connect(IPAddress.Parse(ip), 8083);
                            socketjiank = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            socketjiank.Connect(IPAddress.Parse(ip), 8082);
                            TestConnectTimer.Stop();
                            TestConnectTimer = null;
                            isSuccess = true;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                };
            }
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            CenterWindowOnScreen();
            ConnectSocket();
            AsyncReveive();
            string name = Dns.GetHostName();
            ipadrlist = Dns.GetHostAddresses(name);//获取ipv4唯一地址
        }
        IPAddress[] ipadrlist;
        private TestCameraT tt;//第二个摄像头对象
        private TestCameraTR ttt;//第三个摄像头对象
        private TestCameraFour Four;//第四个摄像头对象
        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetDeviceID();
            //ChartTwo.Children.Add(cc);
            Chartc.Children.Add(ss);
            ChartOne.Children.Add(bb);
            Detector = null;
            DetectorOne = null;
            cornerLogo.Visibility = Visibility.Hidden;
            EnabledClassifiers = SMGExpression.Settings.Default.Classifiers;
            canvas.MetricNames = EnabledClassifiers;
            canvas1.MetricNames = EnabledClassifiers;
            canvas2.MetricNames = EnabledClassifiers;
            canvas3.MetricNames = EnabledClassifiers;

            // Enable/Disable buttons on start
            btnStopCamera.IsEnabled =
            btnExit.IsEnabled = true;

            btnStartCamera.IsEnabled =
            btnResetCamera.IsEnabled =
            Points.IsEnabled =
            Metrics.IsEnabled =
            Appearance.IsEnabled =
            Emojis.IsEnabled =
            btnResetCamera.IsEnabled =
            btnAppShot.IsEnabled =
            btnStopCamera.IsEnabled = false;

            // Initialize Button Click Handlers
            btnStartCamera.Click += btnStartCamera_Click;
            btnStopCamera.Click += btnStopCamera_Click;
            Points.Click += Points_Click;
            Emojis.Click += Emojis_Click;
            Appearance.Click += Appearance_Click;
            Metrics.Click += Metrics_Click;
            btnResetCamera.Click += btnResetCamera_Click;
            btnExit.Click += btnExit_Click;
            btnAppShot.Click += btnAppShot_Click;

            ShowEmojis = canvas.DrawEmojis = canvas1.DrawEmojis = canvas2.DrawEmojis = canvas3.DrawEmojis = SMGExpression.Settings.Default.ShowEmojis;
            ShowAppearance = canvas.DrawAppearance = canvas1.DrawAppearance = canvas2.DrawAppearance = canvas3.DrawAppearance = SMGExpression.Settings.Default.ShowAppearance;
            ShowFacePoints = canvas.DrawPoints = canvas1.DrawPoints = canvas2.DrawPoints = canvas3.DrawPoints = SMGExpression.Settings.Default.ShowPoints;
            ShowMetrics = canvas.DrawMetrics = canvas1.DrawMetrics = canvas2.DrawMetrics = canvas3.DrawMetrics = SMGExpression.Settings.Default.ShowMetrics;
            changeButtonStyle(Emojis, SMGExpression.Settings.Default.ShowEmojis);
            changeButtonStyle(Appearance, SMGExpression.Settings.Default.ShowAppearance);
            changeButtonStyle(Points, SMGExpression.Settings.Default.ShowPoints);
            changeButtonStyle(Metrics, SMGExpression.Settings.Default.ShowMetrics);

            this.ContentRendered += MainWindow_ContentRendered;
            tt = new TestCameraT(this);
            ttt = new TestCameraTR(this);
            Four = new TestCameraFour(this);
            InsertDB();
        }
        private void initsocket(string str)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                socketjiank.Send(bytes);//发送服务端消息到客户端  
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }
        private int issendOne = 2;//第一个摄像头是否发送离线状态
        private int issendTwo = 2;//第二个摄像头是否发送离线状态
        private int issendThree = 2;//第三个摄像头是否发送离线状态
        private int issendFour = 2;//第四个摄像头是否发送离线状态
        private void GetStatus(string str)
        {
            switch (str)
            {
                case "One":
                    int onLine = canvas.Faces.Count == 0 ? 2 : 1;
                    if (onLine != issendOne)
                    {
                        initsocket("smg,3," + ipadrlist[0] + ",11,1:" + onLine + "," + DateTime.Now + "\r\n");
                        issendOne = onLine;
                    }
                    break;
                case "Two":
                    int onLineTwo = canvas1.Faces.Count == 0 ? 2 : 1;
                    if (onLineTwo != issendTwo)
                    {
                        initsocket("smg,3," + ipadrlist[0] + ",11,2:" + onLineTwo + "," + DateTime.Now + "\r\n");
                        issendTwo = onLineTwo;
                    }
                    break;
                case "Three":
                    int onLineThree = canvas2.Faces.Count == 0 ? 2 : 1;
                    if (onLineThree != issendThree)
                    {
                        initsocket("smg,3," + ipadrlist[0] + ",11,3:" + onLineThree + "," + DateTime.Now + "\r\n");
                        issendThree = onLineThree;
                    }
                    break;
                case "Four":
                    int onLineFoure = canvas3.Faces.Count == 0 ? 2 : 1;
                    if (onLineFoure != issendFour)
                    {
                        initsocket("smg,3," + ipadrlist[0] + ",11,4:" + onLineFoure + "," + DateTime.Now + "\r\n");
                        issendFour = onLineFoure;
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 表情数据插入数据库 1秒执行一次 判断人员在线和不在线状态
        /// </summary>
        private void InsertDB()
        {
            InsertTime = new DispatcherTimer();
            InsertTime.Interval = TimeSpan.FromSeconds(1);//
            InsertTime.Tick += (Sender, args) =>
            {
                GetStatus("One");
                GetStatus("Two");
                GetStatus("Three");
                GetStatus("Four");
                if (canvas.Faces.Count > 0)
                {
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(1," + testid + "," + canvas.ee.Anger + "," + canvas.ee.Contempt + "," + canvas.ee.Disgust + "," + canvas.ee.Fear + "," + canvas.ee.Engagement + "," + canvas.ee.Joy + "," + canvas.ee.Sadness + "," + canvas.ee.Surprise + "," + canvas.ee.Valence + ",'" + DateTime.Now + "');");
                }
                if (canvas1.Faces.Count > 0)
                {
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(2," + testid + "," + canvas1.ee.Anger + "," + canvas1.ee.Contempt + "," + canvas1.ee.Disgust + "," + canvas1.ee.Fear + "," + canvas1.ee.Engagement + "," + canvas1.ee.Joy + "," + canvas1.ee.Sadness + "," + canvas1.ee.Surprise + "," + canvas1.ee.Valence + ",'" + DateTime.Now + "');");
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(3," + testid + "," + canvas1.ee.Anger + "," + canvas1.ee.Contempt + "," + canvas1.ee.Disgust + "," + canvas1.ee.Fear + "," + canvas1.ee.Engagement + "," + canvas1.ee.Joy + "," + canvas1.ee.Sadness + "," + canvas1.ee.Surprise + "," + canvas1.ee.Valence + ",'" + DateTime.Now + "');");
                }

                if (canvas2.Faces.Count > 0)
                {
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(4," + testid + "," + canvas2.ee.Anger + "," + canvas2.ee.Contempt + "," + canvas2.ee.Disgust + "," + canvas2.ee.Fear + "," + canvas2.ee.Engagement + "," + canvas2.ee.Joy + "," + canvas2.ee.Sadness + "," + canvas2.ee.Surprise + "," + canvas2.ee.Valence + ",'" + DateTime.Now + "');");
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(5," + testid + "," + canvas2.ee.Anger + "," + canvas2.ee.Contempt + "," + canvas2.ee.Disgust + "," + canvas2.ee.Fear + "," + canvas2.ee.Engagement + "," + canvas2.ee.Joy + "," + canvas2.ee.Sadness + "," + canvas2.ee.Surprise + "," + canvas2.ee.Valence + ",'" + DateTime.Now + "');");
                }

                if (canvas3.Faces.Count > 0)
                    MySqlHelper.ExecuteDataSet("insert into AffdexTable(id,test_id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(6," + testid + "," + canvas3.ee.Anger + "," + canvas3.ee.Contempt + "," + canvas3.ee.Disgust + "," + canvas3.ee.Fear + "," + canvas3.ee.Engagement + "," + canvas3.ee.Joy + "," + canvas3.ee.Sadness + "," + canvas3.ee.Surprise + "," + canvas3.ee.Valence + ",'" + DateTime.Now + "');");
            };
        }
        /// <summary>
        /// Once the window las been loaded and the content rendered, the camera
        /// can be initialized and started. This sequence allows for the underlying controls
        /// and watermark logo to be displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (DeviceStr.Contains("USB\\VID_046D&PID_0825\\CC47E540") && DeviceStr.Contains("USB\\VID_046D&PID_0825\\84A7E540") && DeviceStr.Contains("USB\\VID_046D&PID_0825\\ECB09810") && DeviceStr.Contains("USB\\VID_046D&PID_0825\\8DAD8810"))
            {
                StartCameraProcessing();
                tt.StartCameraProcessingOne();
                ttt.StartCameraProcessingTwo();
                Four.StartCameraProcessingThree();
            }
            else
            {
                MessageBox.Show("摄像头不匹配");
            }
        }

        /// <summary>
        /// Center the main window on the screen
        /// </summary>
        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }


        /// <summary>
        /// Handles the Image results event produced by Affdex.Detector
        /// </summary>
        /// <param name="faces">The detected faces.</param>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image analyzed.</param>
        public void onImageResults(Dictionary<int, Affdex.Face> faces, Affdex.Frame image)
        {
            DrawData(image, faces);
            //DrawData1(image, faces);
        }

        /// <summary>
        /// Handles the Image capture from source produced by Affdex.Detector
        /// </summary>
        /// <param name="image">The <see cref="Affdex.Frame"/> instance containing the image captured from camera.</param>
        public void onImageCapture(Affdex.Frame image)
        {
            DrawCapturedImage(image);
            //DrawCapturedImage1(image);
        }

        /// <summary>
        /// Handles occurence of exception produced by Affdex.Detector
        /// </summary>
        /// <param name="ex">The <see cref="Affdex.AffdexException"/> instance containing the exception details.</param>
        public void onProcessingException(Affdex.AffdexException ex)
        {
            String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
            ShowExceptionAndShutDown(message);
        }


        /// <summary>
        /// Handles the end of processing event; not used
        /// </summary>
        public void onProcessingFinished() { }


        /// <summary>
        /// Displays a alert with exception details
        /// </summary>
        /// <param name="exceptionMessage"> contains the exception details.</param>
        public void ShowExceptionAndShutDown(String exceptionMessage)
        {
            MessageBoxResult result = MessageBox.Show(exceptionMessage,
                                                        "SMGExpression Error",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Error);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                StopCameraProcessing();
                ResetDisplayArea();
            }));
        }

        /// <summary>
        /// Constructs the bitmap image from byte array.
        /// </summary>
        /// <param name="imageData">The image data.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        private BitmapSource ConstructImage(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }
        private BitmapSource ConstructImageOne(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }
        private BitmapSource ConstructImageTwo(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }
        private BitmapSource ConstructImageThree(byte[] imageData, int width, int height)
        {
            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    var stride = (width * PixelFormats.Bgr24.BitsPerPixel + 7) / 8;
                    var imageSrc = BitmapSource.Create(width, height, 96d, 96d, PixelFormats.Bgr24, null, imageData, stride);
                    return imageSrc;
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }

            return null;
        }
        /// <summary>
        /// Draws the facial analysis captured by Affdex.Detector.
        /// </summary>
        /// <param name="image">The image analyzed.</param>
        /// <param name="faces">The faces found in the image analyzed.</param>
        private void DrawData(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((Detector != null) && (Detector.isRunning()))
                        {
                            canvas.Faces = faces;
                            canvas.Width = cameraDisplay.ActualWidth;
                            canvas.Height = cameraDisplay.ActualHeight;
                            canvas.XScale = canvas.Width / image.getWidth();
                            canvas.YScale = canvas.Height / image.getHeight();
                            canvas.InvalidateVisual();
                            DrawSkipCount = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }
        public void DrawDataOne(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((DetectorOne != null) && (DetectorOne.isRunning()))
                        {
                            canvas1.Faces = faces;
                            canvas1.Width = cameraDisplay1.ActualWidth;
                            canvas1.Height = cameraDisplay1.ActualHeight;
                            canvas1.XScale = canvas1.Width / image.getWidth();
                            canvas1.YScale = canvas1.Height / image.getHeight();
                            canvas1.InvalidateVisual();
                            DrawSkipCount1 = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }
        public void DrawDataTwo(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((DetectorTwo != null) && (DetectorTwo.isRunning()))
                        {
                            canvas2.Faces = faces;
                            canvas2.Width = cameraDisplay2.ActualWidth;
                            canvas2.Height = cameraDisplay2.ActualHeight;
                            canvas2.XScale = canvas2.Width / image.getWidth();
                            canvas2.YScale = canvas2.Height / image.getHeight();
                            canvas2.InvalidateVisual();
                            DrawSkipCount2 = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }
        public void DrawDataThree(Affdex.Frame image, Dictionary<int, Affdex.Face> faces)
        {
            try
            {
                // Plot Face Points
                if (faces != null)
                {
                    var result = this.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if ((DetectorThree != null) && (DetectorThree.isRunning()))
                        {
                            canvas3.Faces = faces;
                            canvas3.Width = cameraDisplay3.ActualWidth;
                            canvas3.Height = cameraDisplay3.ActualHeight;
                            canvas3.XScale = canvas3.Width / image.getWidth();
                            canvas3.YScale = canvas3.Height / image.getHeight();
                            canvas3.InvalidateVisual();
                            DrawSkipCount3 = 0;
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Draws the image captured from the camera.
        /// </summary>
        /// <param name="image">The image captured.</param>
        public void DrawCapturedImage(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    cameraDisplay.Source = ConstructImage(image.getBGRByteArray(), image.getWidth(), image.getHeight());
                    if (++DrawSkipCount > 4)
                    {
                        canvas.Faces = new Dictionary<int, Affdex.Face>();
                        canvas.InvalidateVisual();
                        DrawSkipCount = 0;
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }
        public void DrawCapturedImageOne(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    cameraDisplay1.Source = ConstructImageOne(image.getBGRByteArray(), image.getWidth(), image.getHeight());
                    if (++DrawSkipCount1 > 4)
                    {
                        canvas1.Faces = new Dictionary<int, Affdex.Face>();
                        canvas1.InvalidateVisual();
                        DrawSkipCount1 = 0;
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }
        public void DrawCapturedImageTwo(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    cameraDisplay2.Source = ConstructImageTwo(image.getBGRByteArray(), image.getWidth(), image.getHeight());
                    if (++DrawSkipCount2 > 4)
                    {
                        canvas2.Faces = new Dictionary<int, Affdex.Face>();
                        canvas2.InvalidateVisual();
                        DrawSkipCount2 = 0;
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }
        public void DrawCapturedImageThree(Affdex.Frame image)
        {
            // Update the Image control from the UI thread
            var result = this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    cameraDisplay3.Source = ConstructImageThree(image.getBGRByteArray(), image.getWidth(), image.getHeight());
                    if (++DrawSkipCount3 > 4)
                    {
                        canvas3.Faces = new Dictionary<int, Affdex.Face>();
                        canvas3.InvalidateVisual();
                        DrawSkipCount3 = 0;
                    }
                    if (image != null)
                    {
                        image.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                    ShowExceptionAndShutDown(message);
                }
            }));
        }
        /// <summary>
        /// Saves the settings.
        /// </summary>
        void SaveSettings()
        {
            SMGExpression.Settings.Default.ShowPoints = ShowFacePoints;
            SMGExpression.Settings.Default.ShowAppearance = ShowAppearance;
            SMGExpression.Settings.Default.ShowEmojis = ShowEmojis;
            SMGExpression.Settings.Default.ShowMetrics = ShowMetrics;
            SMGExpression.Settings.Default.Classifiers = EnabledClassifiers;
            SMGExpression.Settings.Default.Save();
        }

        /// <summary>
        /// Resets the display area.
        /// </summary>
        private void ResetDisplayArea()
        {
            try
            {
                // Hide Camera feed;
                cameraDisplay.Visibility = cornerLogo.Visibility = Visibility.Hidden;

                // Clear any drawn data
                canvas.Faces = new Dictionary<int, Affdex.Face>();
                canvas.InvalidateVisual();

                // Show the logo
                //logoBackground.Visibility = Visibility.Visible;

                Points.IsEnabled =
                Metrics.IsEnabled =
                Appearance.IsEnabled =
                Emojis.IsEnabled = false;
                cameraDisplay1.Visibility = cornerLogo.Visibility = Visibility.Hidden;

                // Clear any drawn data
                canvas1.Faces = new Dictionary<int, Affdex.Face>();
                canvas1.InvalidateVisual();

                cameraDisplay2.Visibility = cornerLogo.Visibility = Visibility.Hidden;

                // Clear any drawn data
                canvas2.Faces = new Dictionary<int, Affdex.Face>();
                canvas2.InvalidateVisual();

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Turns the on classifiers.
        /// </summary>
        private void TurnOnClassifiers()
        {
            Detector.setDetectAllEmotions(false);
            Detector.setDetectAllExpressions(false);
            Detector.setDetectAllEmojis(true);
            Detector.setDetectGender(true);
            Detector.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = Detector.GetType().GetMethod(String.Format("setDetect{0}", canvas.NameMappings(metric)));
                setMethodInfo.Invoke(Detector, new object[] { true });
            }
        }
        public void TurnOnClassifiersOne()
        {
            DetectorOne.setDetectAllEmotions(false);
            DetectorOne.setDetectAllExpressions(false);
            DetectorOne.setDetectAllEmojis(true);
            DetectorOne.setDetectGender(true);
            DetectorOne.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = DetectorOne.GetType().GetMethod(String.Format("setDetect{0}", canvas1.NameMappings(metric)));
                setMethodInfo.Invoke(DetectorOne, new object[] { true });
            }
        }
        public void TurnOnClassifiersTwo()
        {
            DetectorTwo.setDetectAllEmotions(false);
            DetectorTwo.setDetectAllExpressions(false);
            DetectorTwo.setDetectAllEmojis(true);
            DetectorTwo.setDetectGender(true);
            DetectorTwo.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = DetectorTwo.GetType().GetMethod(String.Format("setDetect{0}", canvas2.NameMappings(metric)));
                setMethodInfo.Invoke(DetectorTwo, new object[] { true });
            }
        }
        public void TurnOnClassifiersThree()
        {
            DetectorThree.setDetectAllEmotions(false);
            DetectorThree.setDetectAllExpressions(false);
            DetectorThree.setDetectAllEmojis(true);
            DetectorThree.setDetectGender(true);
            DetectorThree.setDetectGlasses(true);
            foreach (String metric in EnabledClassifiers)
            {
                MethodInfo setMethodInfo = DetectorThree.GetType().GetMethod(String.Format("setDetect{0}", canvas3.NameMappings(metric)));
                setMethodInfo.Invoke(DetectorThree, new object[] { true });
            }
        }
        List<string> DeviceStr = new List<string>();//设备集合
        /// <summary>
        /// 获取电脑设备id和设备名称
        /// </summary>
        private void GetDeviceID()
        {
            Int16 VendorID = 0;
            Int16 ProductID = 0;
            ManagementObjectCollection USBControllerDeviceCollection = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice").Get();
            if (USBControllerDeviceCollection != null)
            {
                foreach (ManagementObject USBControllerDevice in USBControllerDeviceCollection)
                {   // 获取设备实体的DeviceID
                    String Dependent = (USBControllerDevice["Dependent"] as String).Split(new Char[] { '=' })[1];

                    // 过滤掉没有VID和PID的USB设备
                    Match match = Regex.Match(Dependent, "VID_[0-9|A-F]{4}&PID_[0-9|A-F]{4}");
                    if (match.Success)
                    {
                        UInt16 theVendorID = Convert.ToUInt16(match.Value.Substring(4, 4), 16);   // 供应商标识
                        if (VendorID != UInt16.MinValue && VendorID != theVendorID) continue;

                        UInt16 theProductID = Convert.ToUInt16(match.Value.Substring(13, 4), 16); // 产品编号
                        if (ProductID != UInt16.MinValue && ProductID != theProductID) continue;



                        ManagementObjectCollection PnPEntityCollection = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID=" + Dependent).Get();
                        if (PnPEntityCollection != null)
                        {
                            foreach (ManagementObject Entity in PnPEntityCollection)
                            {
                                string q = Entity["PNPDeviceID"] as String;
                                string w = Entity["Name"] as String;
                                DeviceStr.Add(q);
                            }
                        }
                    }
                }
                for (int i = 0; i < DeviceStr.Count; i++)
                {
                    //&& DeviceStr.Contains("USB//VID_046D&PID_0825//ECB09810") && DeviceStr.Contains("USB//VID_046D&PID_0825//ECB09810") && DeviceStr.Contains("USB//VID_046D&PID_0825//ECB09810")
                    if (DeviceStr.Contains("USB\\VID_046D&PID_0825\\ECB09810"))
                    {

                    }
                }
            }
        }
        /// <summary>
        /// Starts the camera processing.
        /// </summary>
        private void StartCameraProcessing()
        {
            try
            {
                btnStartCamera.IsEnabled = false;
                btnResetCamera.IsEnabled =
                Points.IsEnabled =
                Metrics.IsEnabled =
                Appearance.IsEnabled =
                Emojis.IsEnabled =
                btnStopCamera.IsEnabled =
                btnAppShot.IsEnabled =
                btnExit.IsEnabled = true;

                // Instantiate CameraDetector using default camera ID
                const int cameraId = 0;
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;
                Detector = new Affdex.CameraDetector(cameraId, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                Detector.setClassifierPath(FilePath.GetClassifierDataFolder());


                // Set the Classifiers that we are interested in tracking
                TurnOnClassifiers();

                Detector.setImageListener(this);
                Detector.setProcessStatusListener(this);

                Detector.start();


                // Hide the logo, show the camera feed and the data canvas
                logoBackground.Visibility = Visibility.Hidden;
                cornerLogo.Visibility = Visibility.Visible;
                canvas.Visibility = Visibility.Visible;

                cameraDisplay.Visibility = Visibility;
                cornerLogo.Visibility = Visibility.Visible;
            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    // If this is a camera failure, then reset the application to allow the user to turn on/enable camera
                    if (ex.Message.Equals("Unable to open webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message,
                                                                "SMGExpression Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                        StopCameraProcessing();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }


        /// <summary>
        /// Resets the camera processing.
        /// </summary>
        private void ResetCameraProcessing()
        {
            try
            {
                Detector.reset();
                DetectorOne.reset();
                DetectorTwo.reset();
                DetectorThree.reset();
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Stops the camera processing.
        /// </summary>
        public void StopCameraProcessing()
        {
            try
            {
                if ((Detector != null) && (Detector.isRunning()) && (DetectorOne != null) && (DetectorOne.isRunning()) && (DetectorTwo != null) && (DetectorTwo.isRunning()) && (DetectorThree != null) && (DetectorThree.isRunning()))
                {
                    Detector.stop();
                    Detector.Dispose();
                    Detector = null;
                    DetectorOne.stop();
                    DetectorOne.Dispose();
                    DetectorOne = null;
                    DetectorTwo.stop();
                    DetectorTwo.Dispose();
                    DetectorTwo = null;
                    DetectorThree.stop();
                    DetectorThree.Dispose();
                    DetectorThree = null;
                }

                // Enable/Disable buttons on start
                btnStartCamera.IsEnabled = true;
                btnResetCamera.IsEnabled =
                btnAppShot.IsEnabled =
                btnStopCamera.IsEnabled = false;

            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Changes the button style based on the specified flag.
        /// </summary>
        /// <param name="button">The button.</param>
        /// <param name="isOn">if set to <c>true</c> [is on].</param>
        private void changeButtonStyle(Button button, bool isOn)
        {
            Style style;
            String buttonText = String.Empty;

            if (isOn)
            {
                style = this.FindResource("PointsOnButtonStyle") as Style;
                buttonText = "Hide " + button.Name;
            }
            else
            {
                style = this.FindResource("CustomButtonStyle") as Style;
                buttonText = "Show " + button.Name;
            }
            button.Style = style;
            button.Content = buttonText;
        }

        /// <summary>
        /// Take a shot of the current canvas and save it to a png file on disk
        /// </summary>
        /// <param name="fileName">The file name of the png file to save it in</param>
        private void TakeScreenShot(String fileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            double dpi = 96d;
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height,
                                                                       dpi, dpi, PixelFormats.Default);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            renderBitmap.Render(dv);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(fileName))
            {
                encoder.Save(file);
            }

            appShotLocLabel.Content = String.Format("Screenshot saved to: {0}", fileName);
            ((System.Windows.Media.Animation.Storyboard)FindResource("autoFade")).Begin(appShotLocLabel);
        }


        /// <summary>
        /// Handles the Click eents of the Take Screenshot control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAppShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                String fileName = String.Format("SMGExpression ScreenShot {0:MMMM dd yyyy h mm ss}.png", DateTime.Now);
                fileName = System.IO.Path.Combine(picturesFolder, fileName);
                this.TakeScreenShot(fileName);
            }
            catch (Exception ex)
            {
                String message = String.Format("SMGExpression error encountered while trying to take a screenshot, details={0}", ex.Message);
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Handles the Click event of the Metrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Metrics_Click(object sender, RoutedEventArgs e)
        {
            ShowMetrics = !ShowMetrics;
            canvas.DrawMetrics = ShowMetrics;
            changeButtonStyle((Button)sender, ShowMetrics);
        }

        /// <summary>
        /// Handles the Click event of the Points control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Points_Click(object sender, RoutedEventArgs e)
        {
            ShowFacePoints = !ShowFacePoints;
            canvas.DrawPoints = ShowFacePoints;
            changeButtonStyle((Button)sender, ShowFacePoints);
        }

        /// <summary>
        /// Handles the Click event of the Appearance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Appearance_Click(object sender, RoutedEventArgs e)
        {
            ShowAppearance = !ShowAppearance;
            canvas.DrawAppearance = ShowAppearance;
            changeButtonStyle((Button)sender, ShowAppearance);
        }

        /// <summary>
        /// Handles the Click event of the Emojis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Emojis_Click(object sender, RoutedEventArgs e)
        {
            ShowEmojis = !ShowEmojis;
            canvas.DrawEmojis = ShowEmojis;
            changeButtonStyle((Button)sender, ShowEmojis);
        }

        /// <summary>
        /// Handles the Click event of the btnResetCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnResetCamera_Click(object sender, RoutedEventArgs e)
        {
            ResetCameraProcessing();
        }

        /// <summary>
        /// Handles the Click event of the btnStartCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStartCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DeviceStr.Contains("USB\\VID_046D&PID_0825\\ECB09810") && DeviceStr.Contains("USB\\VID_046D&PID_0825\\8DAD8810") && DeviceStr.Contains("USB\\VID_046D&PID_0826\\4851B350") && DeviceStr.Contains("USB\\VID_046D&PID_082D\\9CCE039F"))
                {
                    StartCameraProcessing();
                    tt.StartCameraProcessingOne();
                    ttt.StartCameraProcessingTwo();
                    Four.StartCameraProcessingThree();
                }
                else
                {
                    MessageBox.Show("摄像头不匹配");
                }
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                ShowExceptionAndShutDown(message);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnStopCamera control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStopCamera_Click(object sender, RoutedEventArgs e)
        {
            StopCameraProcessing();
            ResetDisplayArea();
            // MessageBox.Show(canvas.ee.Anger.ToString());          
        }

        /// <summary>
        /// Handles the Click event of the btnExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            StopCameraProcessing();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCameraProcessing();
            SaveSettings();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the btnChooseWin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnChooseWin_Click(object sender, RoutedEventArgs e)
        {
            Boolean wasRunning = false;
            if ((Detector != null) && (Detector.isRunning()))
            {
                StopCameraProcessing();
                ResetDisplayArea();
                wasRunning = true;
            }

            MetricSelectionUI w = new MetricSelectionUI(EnabledClassifiers);
            w.ShowDialog();
            EnabledClassifiers = new StringCollection();
            foreach (String classifier in w.Classifiers)
            {
                EnabledClassifiers.Add(classifier);
            }
            canvas.MetricNames = EnabledClassifiers;
            if (wasRunning)
            {
                StartCameraProcessing();
            }
        }

        /// <summary>
        /// Once a face's feature points get displayed, the number of successive captures that occur without
        /// the points getting redrawn in the OnResults callback.
        /// </summary>
        private int DrawSkipCount { get; set; }
        private int DrawSkipCount1 { get; set; }
        private int DrawSkipCount2 { get; set; }
        private int DrawSkipCount3 { get; set; }

        /// <summary>
        /// Affdex Detector
        /// </summary>

        private Affdex.Detector Detector { get; set; }
        public Affdex.Detector DetectorOne { get; set; }
        public Affdex.Detector DetectorTwo { get; set; }
        public Affdex.Detector DetectorThree { get; set; }

        /// <summary>
        /// Collection of strings represent the name of the active selected metrics;
        /// </summary>
        private StringCollection EnabledClassifiers { get; set; }

        private bool ShowFacePoints { get; set; }
        private bool ShowAppearance { get; set; }
        private bool ShowEmojis { get; set; }
        private bool ShowMetrics { get; set; }
        public DispatcherTimer InsertTime { get; private set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }
        private List<string> biaoqing = new List<string>()
        {
            "Anger",
            "Contempt",
            "Disgust",
            "Engagement",
            "Fear",
            "Joy",
            "Sadness",
            "Surprise",
            "Valence"
        };

        private void CreateChart()
        {

            Chart chartc = new Chart();
            chartc.Height = 500;
            chartc.Width = 600;
            chartc.Theme = "Theme3";
            chartc.ScrollingEnabled = false;
            chartc.ShadowEnabled = false;

            Title title = new Visifire.Charts.Title();
            title.Text = "SMG测试数据";
            chartc.Titles.Add(title);

            PlotArea plotarea = new PlotArea();
            plotarea.ShadowEnabled = false;
            plotarea.BorderBrush = new SolidColorBrush(Colors.Gray);
            plotarea.BorderThickness = new Thickness(0, 0, 0.5, 0);
            chartc.PlotArea = plotarea;

            //Axis xaxis = new Axis();
            //xaxis.IntervalType = IntervalTypes.Auto;
            //xaxis.StartFromZero = true;
            //chartc.AxesX.Add(xaxis);

            Axis yaxis = new Axis();
            yaxis.AxisMaximum = 100;
            chartc.AxesY.Add(yaxis);
            DataSeries ds = new DataSeries();
            for (int i = 0; i < biaoqing.Count; i++)
            {
                DataPoint dp = new DataPoint();
                dp.AxisXLabel = biaoqing[i];
                dp.YValue = 50;
                ds.DataPoints.Add(dp);
            }
            chartc.Series.Add(ds);
            Chartc.Children.Add(chartc);
        }
        int joyindex = 0;
        private void joy_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rd = sender as RadioButton;
            switch (rd.Name)
            {
                case "joy":
                    if (joyindex != 0)
                    {
                        cc.dsman.Opacity = 1;
                        cc.dsavg.Opacity = 0;
                        cc.dsedu.Opacity = 0;
                        cc.dsincome.Opacity = 0;
                        cc.dsjob.Opacity = 0;
                        cc.dswoman.Opacity = 0;
                    }
                    break;
                case "sadness":
                    cc.dsman.Opacity = 0;
                    cc.dsavg.Opacity = 1;
                    cc.dsedu.Opacity = 0;
                    cc.dsincome.Opacity = 0;
                    cc.dsjob.Opacity = 0;
                    cc.dswoman.Opacity = 0;
                    break;
                case "anger":
                    cc.dsman.Opacity = 0;
                    cc.dsavg.Opacity = 0;
                    cc.dsedu.Opacity = 0;
                    cc.dsincome.Opacity = 0;
                    cc.dsjob.Opacity = 0;
                    cc.dswoman.Opacity = 1;
                    break;
                case "digust":
                    cc.dsman.Opacity = 0;
                    cc.dsavg.Opacity = 0;
                    cc.dsedu.Opacity = 0;
                    cc.dsincome.Opacity = 0;
                    cc.dsjob.Opacity = 1;
                    cc.dswoman.Opacity = 0;
                    break;
                case "surprise":
                    cc.dsman.Opacity = 0;
                    cc.dsavg.Opacity = 0;
                    cc.dsedu.Opacity = 1;
                    cc.dsincome.Opacity = 0;
                    cc.dsjob.Opacity = 0;
                    cc.dswoman.Opacity = 0;
                    break;
                case "fear":
                    cc.dsman.Opacity = 0;
                    cc.dsavg.Opacity = 0;
                    cc.dsedu.Opacity = 0;
                    cc.dsincome.Opacity = 1;
                    cc.dsjob.Opacity = 0;
                    cc.dswoman.Opacity = 0;
                    break;
                default:
                    break;
            }
            joyindex++;
        }
    }
}
