using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Affdex;
using System.Windows;

namespace SMGExpression
{
   public class TestCameraTR : Affdex.ImageListener
    {
        private MainWindow mm;
        public TestCameraTR(MainWindow mw)
        {
            mm = mw;
        }

        public void onImageCapture(Affdex.Frame image)
        {
            mm.DrawCapturedImageTwo(image);
        }

        public void onImageResults(Dictionary<int, Affdex.Face> faces, Affdex.Frame image)
        {
            mm.DrawDataTwo(image, faces);
        }
        public void StartCameraProcessingTwo()
        {
            try
            {
                // Instantiate CameraDetector u\sing default camera ID
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;

                mm.DetectorTwo = new Affdex.CameraDetector(2, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                mm.DetectorTwo.setClassifierPath(FilePath.GetClassifierDataFolder());
                mm.TurnOnClassifiersTwo();
                mm.DetectorTwo.setImageListener(this);

                mm.DetectorTwo.start();

                mm.logoBackground2.Visibility = Visibility.Hidden;
                mm.canvas2.Visibility = Visibility.Visible;

                mm.cameraDisplay2.Visibility = Visibility.Visible;
            }
            catch (Affdex.AffdexException ex)
            {
                if (!String.IsNullOrEmpty(ex.Message))
                {
                    if (ex.Message.Equals("Unable to open webcam."))
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message,
                                                                "SMGExpression Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                        mm.StopCameraProcessing();
                        return;
                    }
                }

                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                mm.ShowExceptionAndShutDown(message);
            }
            catch (Exception ex)
            {
                String message = String.IsNullOrEmpty(ex.Message) ? "SMGExpression error encountered." : ex.Message;
                mm.ShowExceptionAndShutDown(message);
            }
        }
    }
}
