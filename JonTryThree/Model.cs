using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Diagnostics;



namespace JonTryThree
{
    class Model
    {
        public enum direction { movingLeft, movingRight, stationary };
        public String labelString = "Unknown";
        public direction currDir = direction.stationary;

        public List<PointDiff> handPoints = new List<PointDiff>();
        int historySize = 20;
        int windowSize = 5;
        float threshold = .1F;
        int frameCount = 0;

        public double beamAngle = 0;
        public double sourceAngle = 0;

        public TextBlock theText;

        public ArduinoFun myArduino;
        public int toSend = 0;

        public MainWindow theWindow = null;

        public SoundWindow theSounds;

        public Model()
        {
            myArduino = new ArduinoFun();
        }

        public Model(MainWindow window)
        {
            myArduino = new ArduinoFun();
            theWindow = window;
            theSounds = new SoundWindow(myArduino);
        }


        public void updateBeam(double beam)
        {
            beamAngle = beam;
            this.theWindow.updateBeam(beam);
        }

        public void updateSource(double source)
        {
            sourceAngle = source;
            this.theWindow.updateSource(source);
        }



        public float remap(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
        {
            float OldRange = (OldMax - OldMin); 
            float NewRange = (NewMax - NewMin);
            float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
            return NewValue;
        }





        public String dirToString()
        {
            if (currDir == direction.stationary)
            {
                return "stationary";
            }
            else if (currDir == direction.movingLeft)
            {
                return "moving left";
            }
            else
            {
                return "moving right";
            }
        }

        public void updateDir(PointDiffList pdList)
        {
            float dirX = 0;
            float dirY = 0;
            float dirZ = 0;
            Tuple<float,float,float> dirs;
            if (pdList.windowFull())
            {
                //full
                for (int i = pdList.getCount() - pdList.getWindowSize(); i < pdList.getCount(); i++)
                {
                    dirs = pdList.getDifAt(i);
                    dirX += dirs.Item1;
                    dirY += dirs.Item2;
                    dirZ += dirs.Item3;
                }
            }
            else
            {
                //not full
                for (int i = 0; i < pdList.getCount(); i++)
                {
                    dirs = pdList.getDifAt(i);
                    dirX += dirs.Item1;
                    dirY += dirs.Item2;
                    dirZ += dirs.Item3;
                }
            }
        }

        public void updateDir()
        {
            float dirX = 0;

            if (handPoints.Count < this.windowSize)
            {
                for (int i = 0; i < handPoints.Count; i++)
                {
                    dirX += handPoints[i].getDiff().Item1;
                }
            }
            else
            {
                for (int i = handPoints.Count - this.windowSize; i < handPoints.Count; i++)
                {
                    dirX += handPoints[i].getDiff().Item1;
                }
            }

            if (dirX <= this.threshold && dirX >= -this.threshold)
            {
                currDir = direction.stationary;
            }
            else if (dirX > this.threshold)
            {
                currDir = direction.movingRight;
            }
            else
            {
                currDir = direction.movingLeft;
            }

            theText.Text = this.dirToString();
            if (shouldSend((int)remap(dirX, -.5F, .5F, -20F, 20F)))
            {
                //myArduino.servoGo(1, toSend);
                
                //myArduino.servoGo(1, Convert.ToInt32(remap(theSounds.getAverageAngle(), -50, 50, 0, 180)));
                //myArduino.servoGo(0, Convert.ToInt32(remap(theSounds.getAverageAmp(),-1,1,0,180)));
                toSend = 0;
            }
          
            //Debug.WriteLine("remapped: " + dirX + " to: " + (int) remap(dirX, -1F, 1F, -100F, 100F));
            //Debug.WriteLine("Direction calculated: " + this.handPoints[0].getPoint().X.ToString());
            
        }

        public int addHandPoint(SkeletonPoint p)
        {
            if (handPoints.Count == 0)
            {
                //first point
                handPoints.Add(new PointDiff(p));
            }
            else if (handPoints.Count < historySize)
            {
                //just add
                handPoints.Add(new PointDiff(p, handPoints[handPoints.Count - 1].getPoint()));
            }
            else
            {
                //remove then add
                handPoints.RemoveAt(0);
                handPoints.Add(new PointDiff(p, handPoints[handPoints.Count - 1].getPoint()));
            }

            updateDir();
            return 0;
        }

        public bool shouldSend(int x){
            this.frameCount += 1;
            toSend += x;
            if (this.frameCount == 10)
            {
                this.frameCount = 0;
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }

}


