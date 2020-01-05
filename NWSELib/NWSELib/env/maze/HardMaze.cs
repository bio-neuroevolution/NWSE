using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.env.maze
{
    public class HardMaze : IEnv
    {
        (List<double>, double) IEnv.action(List<double> actions)
        {
            throw new NotImplementedException();
        }

        List<double> IEnv.reset()
        {
            throw new NotImplementedException();
        }
    }
}
