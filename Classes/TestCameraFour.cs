using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SMGExpression
{
    class TestCameraFour: Affdex.ImageListener
    {
        private MainWindow mm;
        public TestCameraFour(MainWindow mw)
        {
            mm = mw;
        }

        public void onImageCapture(Affdex.Frame image)
        {
            mm.DrawCapturedImageThree(image);
        }

        public void onImageResults(Dictionary<int, Affdex.Face> faces, Affdex.Frame image)
        {
            mm.DrawDataThree(image, faces);
        }
        public void StartCameraProcessingThree()
        {
            try
            {
                // Instantiate CameraDetector u\sing default camera ID
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;

                mm.DetectorThree = new Affdex.CameraDetector(3, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                mm.DetectorThree.setClassifierPath(FilePath.GetClassifierDataFolder());
                mm.TurnOnClassifiersThree();
                mm.DetectorThree.setImageListener(this);
                //mm.Detector1.setProcessStatusListener(this);

                mm.DetectorThree.start();

                // Hide the logo, show the camera feed and the data canvas
                mm.logoBackground3.Visibility = Visibility.Hidden;
                mm.canvas3.Visibility = Visibility.Visible;

                mm.cameraDisplay3.Visibility = Visibility.Visible;
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
