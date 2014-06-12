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

namespace JonTryThree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Model myModel;
        
       
        KinectThinker kinectThinker = new KinectThinker();
        
        public delegate void amplitudeUpdated(double amp);

        public static amplitudeUpdated myDelegate;
        

        public MainWindow()
        {
            myModel = new Model(this);
            InitializeComponent();
            myModel.theText = DirText;
            myDelegate = new amplitudeUpdated(updateAmplitude);
           
        }

        public void updateDirectionText()
        {
            DirText.Text = myModel.dirToString();

        }

        public void updateSource(double source)
        {
            myModel.theSounds.addAngle(source);
            SourceAngleText.Text = "sourceAngle: " + myModel.theSounds.getAverageAngle();
        }

        public void updateAmplitude(double amplitude)
        {
            myModel.theSounds.addAmplitude(amplitude);
            AmplitudeText.Text = "Amplitude: " + myModel.theSounds.getAverageAmp();

        }

        public void updateFromThreadAmplitude(double amplitude)
        {
            Dispatcher.BeginInvoke(new Action<double>((amp) =>
                {
                    updateAmplitude(amp);
                }
                ), amplitude);
        }



        public void updateBeam(double beam)
        {
            beamAngleText.Text = "beamAngle: " + beam.ToString();
        }
        
     

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("About to close");
            DirText.Text = "Closing";
            MessageBox.Show("Changed Text");
            kinectThinker.stopKinect();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectThinker.setupKinect(myModel,kinectImage);
            DirText.Text = kinectThinker.foundSensor.ToString();
        }

    }
}
