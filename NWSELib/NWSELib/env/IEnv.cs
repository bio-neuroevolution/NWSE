using System;
using System.Collections.Generic;
using System.Text;

using NWSELib.net;

namespace NWSELib.env
{
    public interface IEnv
    {
        (List<double>, List<double>) reset(Network net,bool clearAgent = true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="net"></param>
        /// <param name="actions"></param>
        /// <returns>环境数据、姿态数据、动作数据、奖励、结束</returns>
        (List<double>, List<double>, List<double>, double,bool) action(Network net, List<double> actions);

        /// <summary>
        /// 查询网络对应的Agent
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        Agent GetAgent(int id=0);
    }
}
