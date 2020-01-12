using System;
using System.Collections.Generic;
using System.Text;

using NWSELib.net;

namespace NWSELib.env
{
    public interface IEnv
    {
        (List<double>, List<double>) reset(Network net);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="net"></param>
        /// <param name="actions"></param>
        /// <returns>环境数据、姿态数据、动作数据、奖励、</returns>
        (List<double>, List<double>, List<double>, double) action(Network net, List<double> actions);
        
    }
}
