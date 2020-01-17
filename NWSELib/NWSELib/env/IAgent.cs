using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.env
{
    public abstract class Agent
    {
        public const double Max_Rotate_Action = Math.PI;
        public const double Max_Speed_Action = 5;
        

        public abstract int getId();
        public abstract List<double> getObserve();
        public abstract bool doAction(double[] actions);

        
    }
}
