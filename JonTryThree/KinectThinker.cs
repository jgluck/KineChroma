using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows;
using System.IO;



namespace JonTryThree
{
    class KinectThinker
    {
        KinectSensor sensor = null;
        public Boolean foundSensor = false;
        Model theModel = null;
        Image theImage = null;
        private Stream audioStream;

        private bool reading;

        private Thread readingThread;

        private const int AudioPollingInterval = 50;

        private const int SamplesPerMillisecond = 16;

        private const int BytesPerSample = 2;

        private readonly byte[] audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

        private readonly object energyLock = new object();
        private double accumulatedSquareSum;
        private double accumulatedSampleCount;
        private double SamplesPerColumn = 40;
        private const int EnergyBitmapWidth = 780;
        private readonly double[] energy = new double[(uint)(EnergyBitmapWidth * 1.25)];
        private int energyIndex;
        private int newEnergyAvailable;
        public int setupKinect(Model mod, Image img)
        {
            theModel = mod;
            theImage = img;
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    this.foundSensor = true;
                    break;
                }
            }

            if (this.foundSensor == false)
            {
                return -1;
            }

            this.setupSkeleton();
            this.setupColor();
            this.setupDepth();

            this.sensor.Start();
            this.setupAudio();
            this.sensor.AllFramesReady += sensor_AllFramesReady;


            return 0;
        }

        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            Skeleton[] skeletons = null;
            this.theModel.theSounds.aTimer.Enabled = true;

            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] bArray = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(bArray);
                    theImage.Source = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, bArray, frame.Width * 4);
                }
            }

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    skeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletons);
                }

                if (skeletons == null) return;

                foreach (Skeleton skeleton in skeletons)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        Joint leftHand = skeleton.Joints[JointType.HandLeft];
                        SkeletonPoint leftHandPosition = leftHand.Position;
                        //Debug.WriteLine("About to write left hand position");
                        theModel.addHandPoint(leftHandPosition);

                    }
                }
            }
        }

        int setupAudio()
        {
            this.audioStream = this.sensor.AudioSource.Start();
            this.sensor.AudioSource.BeamAngleChanged += AudioSource_BeamAngleChanged;
            this.sensor.AudioSource.SoundSourceAngleChanged += AudioSource_SoundSourceAngleChanged;




            this.reading = true;
            this.readingThread = new Thread(AudioReadingThread);
            this.readingThread.Start();

            return 0;
        }

        void AudioSource_SoundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            this.theModel.updateSource(e.Angle);
        }

        void AudioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            this.theModel.updateSource(e.Angle);
        }

        int setupColor()
        {
            this.sensor.ColorStream.Enable();
            return 0;
        }

        int setupDepth()
        {
            this.sensor.DepthStream.Enable();
            this.sensor.DepthStream.Range = DepthRange.Near;
            return 0;
        }



        int setupSkeleton()
        {
            this.sensor.SkeletonStream.EnableTrackingInNearRange = true;
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            this.sensor.SkeletonStream.Enable();
            return 0;
        }

        public void stopKinect()
        {
            this.sensor.Stop();
        }

        private void AudioReadingThread()
        {

            const double EnergyNoiseFloor = 0.2;

            while (this.reading)
            {
                int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);

                lock (energyLock)
                {
                    for (int i = 0; i < readCount; i += 2)
                    {
                        // compute the sum of squares of audio samples that will get accumulated
                        // into a single energy value.
                        short audioSample = BitConverter.ToInt16(audioBuffer, i);
                        this.accumulatedSquareSum += audioSample * audioSample;
                        ++this.accumulatedSampleCount;

                        if (this.accumulatedSampleCount < SamplesPerColumn)
                        {
                            continue;
                        }

                        // Each energy value will represent the logarithm of the mean of the
                        // sum of squares of a group of audio samples.
                        double meanSquare = this.accumulatedSquareSum / SamplesPerColumn;
                        double amplitude = Math.Log(meanSquare) / Math.Log(int.MaxValue);
                        
                        // Renormalize signal above noise floor to [0,1] range.
                        this.energy[this.energyIndex] = Math.Max(0, amplitude - EnergyNoiseFloor) / (1 - EnergyNoiseFloor);
                        theModel.theWindow.updateFromThreadAmplitude(this.energy[this.energyIndex]);
                      
                        //updateAmplitude(this.energy[this.energyIndex]);
                        this.energyIndex = (this.energyIndex + 1) % this.energy.Length;

                        this.accumulatedSquareSum = 0;
                        this.accumulatedSampleCount = 0;
                        ++this.newEnergyAvailable;
                    }
                }
            }
        }
    }
         
}
