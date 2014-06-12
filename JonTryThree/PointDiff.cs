using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.IO.Ports;

namespace JonTryThree
{
    class PointDiff
    {
        SkeletonPoint p1;
        SkeletonPoint p2;
        Boolean singlePoint = false;

        public PointDiff(SkeletonPoint p)
        {
            this.p1 = p;
            singlePoint = true;
        }

        public PointDiff(SkeletonPoint p, SkeletonPoint op)
        {
            this.p1 = p;
            this.p2 = op;
        }

        public SkeletonPoint getPoint()
        {
            return p1;
        }

        public Tuple<float, float, float> getDiff()
        {
            if (singlePoint)
            {
                return new Tuple<float, float, float>(0, 0, 0);
            }
            return new Tuple<float,float,float>(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        
    }
}
