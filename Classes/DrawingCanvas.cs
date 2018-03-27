
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SMGExpression
{
    public class DrawingCanvas : System.Windows.Controls.Canvas
    {
        public Expression ee = new Expression();
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingCanvas"/> class.
        /// </summary>
        public DrawingCanvas()
        {
            DrawMetrics = true;
            DrawPoints = true;
            DrawAppearance = true;
            DrawEmojis = true;
            boundingBrush = new SolidColorBrush(Colors.LightGray);
            pointBrush = new SolidColorBrush(Colors.Cornsilk);
            emojiBrush = new SolidColorBrush(Colors.Black);
            pozMetricBrush = new SolidColorBrush(Colors.LimeGreen);
            negMetricBrush = new SolidColorBrush(Colors.Red);
            boundingPen = new Pen(boundingBrush, 1);

            NameToResourceConverter conv = new NameToResourceConverter();
            metricTypeFace = Fonts.GetTypefaces((Uri)conv.Convert("Square", null, "ttf", null)).First();

            Faces = new Dictionary<int, Affdex.Face>();
            emojiImages = new Dictionary<Affdex.Emoji, BitmapImage>();
            appImgs = new Dictionary<string, BitmapImage>();
            MetricNames = new StringCollection();
            upperConverter = new UpperCaseConverter();
            maxTxtWidth = 0;
            maxTxtHeight = 0;

            var emojis = Enum.GetValues(typeof(Affdex.Emoji));
            foreach (int emojiVal in emojis)
            {
                BitmapImage img = loadImage(emojiVal.ToString());
                emojiImages.Add((Affdex.Emoji)emojiVal, img);
            }

            var gender = Enum.GetValues(typeof(Affdex.Gender));
            foreach (int genderVal in gender)
            {
                for (int g = 0; g <= 1; g++)
                {
                    string name = ConcatInt(genderVal, g);
                    BitmapImage img = loadImage(name);
                    appImgs.Add(name, img);
                }

            }
        }
        public bool OnlineOneCanvas { get; set; }
        public int onchanged { get; set; }
        /// <summary>
        /// Draws the content of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a <see cref="T:System.Windows.Controls.Panel" /> element.
        /// </summary>
        /// <param name="dc">The <see cref="T:System.Windows.Media.DrawingContext" /> object to draw.</param>
        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {           
            //For each face        
            foreach (KeyValuePair<int, Affdex.Face> pair in Faces)
            {
                Affdex.Face face = pair.Value;

                var featurePoints = face.FeaturePoints;

                //Calculate bounding box corners coordinates.
                System.Windows.Point tl = new System.Windows.Point(featurePoints.Min(r => r.X) * XScale,
                                                   featurePoints.Min(r => r.Y) * YScale);
                System.Windows.Point br = new System.Windows.Point(featurePoints.Max(r => r.X) * XScale,
                                                                   featurePoints.Max(r => r.Y) * YScale);

                System.Windows.Point bl = new System.Windows.Point(tl.X, br.Y);

                //Draw Points
                if (DrawPoints)
                {
                    foreach (var point in featurePoints)
                    {
                        dc.DrawEllipse(pointBrush, null, new System.Windows.Point(point.X * XScale, point.Y * YScale), fpRadius, fpRadius);
                    }

                    //Draw BoundingBox
                    dc.DrawRectangle(null, boundingPen, new System.Windows.Rect(tl, br));
                }

                //Draw Metrics  
                if (DrawMetrics)
                {
                    double padding = (bl.Y - tl.Y) / MetricNames.Count;
                    double startY = tl.Y - padding;
                    foreach (string metric in MetricNames)
                    {
                        double width = maxTxtWidth;
                        double height = maxTxtHeight;
                        float value = -1;
                        PropertyInfo info;
                        if ((info = face.Expressions.GetType().GetProperty(NameMappings(metric))) != null)
                        {
                            value = (float)info.GetValue(face.Expressions, null);
                        }
                        else if ((info = face.Emotions.GetType().GetProperty(NameMappings(metric))) != null)
                        {
                            Console.WriteLine(face.Emotions.Joy);
                            value = (float)info.GetValue(face.Emotions, null);                           
                            ee.Anger = Judge(Math.Round(face.Emotions.Anger * 100));
                            ee.Contempt = Judge(Math.Round(face.Emotions.Contempt * 100));
                            ee.Disgust = Judge(Math.Round(face.Emotions.Disgust * 10));
                            ee.Fear = Judge(Math.Round(face.Emotions.Fear * 100));
                            ee.Engagement = Judge(Math.Round(face.Emotions.Engagement * 100));
                            ee.Joy = Judge(Math.Round(face.Emotions.Joy * 1000));
                            ee.Sadness = Judge(Math.Round(face.Emotions.Sadness * 1000));
                            ee.Surprise = Judge(Math.Round(face.Emotions.Surprise * 10));
                            ee.Valence = Judge(Math.Round(face.Emotions.Valence * 100));
                            //Console.WriteLine(ee.Anger + "   " + ee.Disgust + "    " + ee.Fear + "   " + ee.Sadness + "    " + ee.Surprise + "   " + ee.Joy);
                        }

                        SolidColorBrush metricBrush = value > 0 ? pozMetricBrush : negMetricBrush;
                        value = Math.Abs(value);
                        SolidColorBrush txtBrush = value > 1 ? emojiBrush : boundingBrush;

                        double x = tl.X - width - margin;
                        double y = startY += padding;
                        double valBarWidth = width * (value / 100);

                        if (value > 1) dc.DrawRectangle(null, boundingPen, new System.Windows.Rect(x, y, width, height));
                        dc.DrawRectangle(metricBrush, null, new System.Windows.Rect(x, y, valBarWidth, height));

                        FormattedText metricFTScaled = new FormattedText((String)upperConverter.Convert(metric, null, null, null),
                                                                System.Globalization.CultureInfo.CurrentCulture,
                                                                System.Windows.FlowDirection.LeftToRight,
                                                                metricTypeFace, metricFontSize * width / maxTxtWidth, txtBrush);

                        dc.DrawText(metricFTScaled, new System.Windows.Point(x, y));
                    }
                   // MySqlHelper.ExecuteDataTable("insert into AffdexTable(id,Anger,Contempt,Disgust,Fear,Engagement,Joy,Sadness,Surprise,Valence,test_Date) VALUES(100," + ee.Anger + "," + ee.Contempt + "," + ee.Disgust + "," + ee.Fear + "," + ee.Engagement + "," + ee.Joy + "," + ee.Sadness + "," + ee.Surprise + "," + ee.Valence + ","+DateTime.Now+");", new MySqlParameter[] { });
                }


                //Draw Emoji
                if (DrawEmojis)
                {
                    if (face.Emojis.dominantEmoji != Affdex.Emoji.Unknown)
                    {
                        BitmapImage img = emojiImages[face.Emojis.dominantEmoji];
                        double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
                        System.Windows.Point tr = new System.Windows.Point(br.X + margin, tl.Y);
                        dc.DrawImage(img, new System.Windows.Rect(tr.X, tr.Y, img.Width * imgRatio, img.Height * imgRatio));
                    }
                }
                //Draw Appearance metrics
                if (DrawAppearance)
                {                    
                    BitmapImage img = appImgs[ConcatInt((int)face.Appearance.Gender, (int)face.Appearance.Glasses)];
                    double imgRatio = ((br.Y - tl.Y) * 0.3) / img.Width;
                    double imgH = img.Height * imgRatio;
                    dc.DrawImage(img, new System.Windows.Rect(br.X + margin, br.Y - imgH, img.Width * imgRatio, imgH));
                }
            }




            //base.OnRender(dc);
        }
        private double Judge(double SomeInt)
        {
            if (SomeInt > 100)
                return 100;
            else
                return SomeInt;
        }
        public String NameMappings(String classifierName)
        {
            if (classifierName == "Frown")
            {
                return "LipCornerDepressor";
            }
            return classifierName;
        }

        public Dictionary<int, Affdex.Face> Faces { get; set; }
        public StringCollection MetricNames
        {
            get
            {
                return metricNames;
            }
            set
            {
                metricNames = value;
                Dictionary<string, FormattedText> txtArray = new Dictionary<string, FormattedText>();

                foreach (string metric in metricNames)
                {
                    FormattedText metricFT = new FormattedText((String)upperConverter.Convert(metric, null, null, null),
                                                            System.Globalization.CultureInfo.CurrentCulture,
                                                            System.Windows.FlowDirection.LeftToRight,
                                                            metricTypeFace, metricFontSize, emojiBrush);
                    txtArray.Add(metric, metricFT);

                }

                if (txtArray.Count > 0)
                {
                    maxTxtWidth = txtArray.Max(r => r.Value.Width);
                    maxTxtHeight = txtArray.Max(r => r.Value.Height);
                }
            }
        }
        public double XScale { get; set; }
        public double YScale { get; set; }

        public bool DrawMetrics { get; set; }
        public bool DrawPoints { get; set; }
        public bool DrawEmojis { get; set; }
        public bool DrawAppearance { get; set; }

        private BitmapImage loadImage(string name, string extension="png")
        {
            NameToResourceConverter conv = new NameToResourceConverter();
            var pngURI = conv.Convert(name, null, extension, null);
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = (Uri)pngURI;
            img.EndInit();
            return img;
        }

        private string ConcatInt(int x, int y)
        {
            return String.Format("{0}{1}", x, y); 
        }

        private const int metricFontSize = 14;
        private const int fpRadius = 2;
        private const int margin = 5;
        private double maxTxtWidth;
        private double maxTxtHeight;
        private Typeface metricTypeFace;
        private SolidColorBrush boundingBrush;
        private SolidColorBrush pozMetricBrush;
        private SolidColorBrush negMetricBrush;
        private SolidColorBrush pointBrush;
        private SolidColorBrush emojiBrush;
        private Pen boundingPen;
        private UpperCaseConverter upperConverter;
        private StringCollection metricNames;
        private Dictionary<Affdex.Emoji, BitmapImage> emojiImages;
        private Dictionary<string, BitmapImage> appImgs;
    }
}
