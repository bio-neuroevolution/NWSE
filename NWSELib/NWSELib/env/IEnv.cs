using System;
using System.Collections.Generic;
using System.Text;

using NWSELib.net;

namespace NWSELib.env
{
    public interface IEnv
    {
        List<double> reset(Network net);
        (List<double>, double) action(Network net, List<double> actions);
        
    }
}
