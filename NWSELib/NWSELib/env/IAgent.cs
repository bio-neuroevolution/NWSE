using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.env
{
    public interface IAgent
    {
        int getId();
        List<double> getObserve();
        void doAction(double[] actions);

        Point2D Location { get; }
        Point2D OldLocation { get; }
    }
}
