using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.env
{
    public interface IEnv
    {
        List<double> reset();
        (List<double>, double) action(List<double> actions);
    }
}
