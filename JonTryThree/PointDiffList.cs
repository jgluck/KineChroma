using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace JonTryThree
{
    class PointDiffList
    {

        List<PointDiff> theList = new List<PointDiff>();
        int historySize = 20;
        int windowSize = 5;
        float threshold = .1F;
        Action<PointDiffList> listUpdated = null;


        public PointDiffList(int hSize, int wSize, float th)
        {
            this.historySize = hSize;
            this.windowSize = wSize;
            this.threshold = th;
        }

        public PointDiffList(int hSize, int wSize)
        {
            this.historySize = hSize;
            this.windowSize = wSize;
        }

        public Boolean windowFull()
        {
            if (this.theList.Count < windowSize)
            {
                return false;

            }
            else
            {
                return true;
            }
        }

        public int getCount()
        {
            return theList.Count;
        }

        public int getWindowSize()
        {
            return this.windowSize;
        }


        public Tuple<float,float,float> getDifAt(int position)
        {
            return this.theList[position].getDiff();
        }

        public void addPoint(SkeletonPoint p)
        {
            if (this.theList.Count == 0)
            {
                //first point
                theList.Add(new PointDiff(p));
            }
            else if (theList.Count < historySize)
            {
                //just add
                theList.Add(new PointDiff(p, theList[this.theList.Count - 1].getPoint()));
            }
            else
            {
                //remove then add
                theList.RemoveAt(0);
                theList.Add(new PointDiff(p, theList[this.theList.Count - 1].getPoint()));
            }
            if (listUpdated != null)
            {
                listUpdated(this);
            }
        }


    }
}
