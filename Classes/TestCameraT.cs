using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Affdex;
using System.Windows;

namespace SMGExpression
{
    public class TestCameraT : Affdex.ImageListener
    {
        private MainWindow mm;
        public TestCameraT(MainWindow mw)
        {
            mm = mw;         
        }
        public void onImageCapture(Affdex.Frame image)
        {
            //MainWindow mm = new MainWindow();
            mm.DrawCapturedImageOne(image);
        }

        public void onImageResults(Dictionary<int, Affdex.Face> faces, Affdex.Frame image)
        {
            mm.DrawDataOne(image, faces);
        }
        public void StartCameraProcessingOne()
        {
            try
            {
                // Instantiate CameraDetector u\sing default camera ID
                const int numberOfFaces = 10;
                const int cameraFPS = 15;
                const int processFPS = 15;

                mm.DetectorOne = new Affdex.CameraDetector(1, cameraFPS, processFPS, numberOfFaces, Affdex.FaceDetectorMode.LARGE_FACES);

                //Set location of the classifier data files, needed by the SDK
                mm.DetectorOne.setClassifierPath(FilePath.GetClassifierDataFolder());
                mm.TurnOnClassifiersOne();
                mm.DetectorOne.setImageListener(this);
                //mm.DetectorOne.setProcessStatusListener(this);

                mm.DetectorOne.start();

                // Hide the logo, show the camera feed and the data canvas

                mm.logoBackground1.Visibility = Visibility.Hidden;
                mm.canvas1.Visibility = Visibility.Visible;

                mm.cameraDisplay1.Visibility = Visibility.Visible;
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
