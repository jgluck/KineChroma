using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JonTryThree
{
    class SoundWindow
    {
        static int ampWindowSize = 100;
        static int angleWindowSize = 15;
        double latestWindow = 0;
        List<double> ampWindow = new List<double>();
        List<double> angleWindow = new List<double>();
        ArduinoFun theArduino = null;
        public System.Timers.Timer aTimer;
        private static int delay = 250;

        public SoundWindow(ArduinoFun Theduino)
        {
            this.theArduino = Theduino;
            aTimer = new System.Timers.Timer(delay);
            aTimer.Elapsed += aTimer_Elapsed;
        }

        void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.writeCoords();
        }


        public double remap(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            double OldRange = (OldMax - OldMin);
            double NewRange = (NewMax - NewMin);
            double NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }

        public double ampThreshold(double OldValue, double OldMin, double OldMax, double NewMin, double NewMax)
        {
            if (OldValue < .3)
            {
                return remap(OldValue, OldMin, .3, NewMin, NewMin + ((NewMax - NewMin) / 5));
            }
            else
            {
                return remap(OldValue, .3, OldMax, NewMin + ((NewMax - NewMin) / 5), NewMax);
            }
        }

        public double getAverageAmp()
        {
            if (ampWindow.Count > 0)
            {
                return ampWindow.Average();
            }
            else
            {
                return 90;
            }
        }

        public double getAverageAngle()
        {
            if (angleWindow.Count > 0)
            {
                return angleWindow.Average();
            }
            else
            {
                return 90;
            }
            
        }

        public void addAmplitude(double amp)
        {
            if (ampWindow.Count == ampWindowSize)
            {
                ampWindow.RemoveAt(0);
                ampWindow.Add(amp);
            }
            else
            {
                ampWindow.Add(amp);
            }


        }

        public void addAngle(double angle)
        {
            if (angleWindow.Count == angleWindowSize)
            {
                angleWindow.RemoveAt(0);
                angleWindow.Add(angle);
            }
            else
            {
                angleWindow.Add(angle);
            }
  
        }

        public void writeCoords()
        {
            if (angleWindow.Count > 1 && ampWindow.Count > 1 && (getAverageAmp() > .18))
            {
                //this.theArduino.sendData(Convert.ToInt32(remap(-1 * getAverageAngle(), -50, 50, 93, 135)),
                //    Convert.ToInt32(remap(getAverageAmp(), .1, .8, 103, 120)));

                

                this.theArduino.sendData(Convert.ToInt32(remap(-1 * getAverageAngle(), -50, 50, 93, 135)),
                    Convert.ToInt32(this.ampThreshold(getAverageAmp(), .1, .8, 103, 120)));
            }
            else
            {
                return;
            }
        }

    }
}
