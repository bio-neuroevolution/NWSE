using System;
using System.Collections.Generic;
using System.Text;

using NWSELib.genome;

namespace NWSELib.net
{
    /// <summary>
    /// 网络
    /// </summary>
    public class Network
    {
        #region
        /// <summary>
        /// 染色体
        /// </summary>
        private NWSEGenome genome;
        /// <summary>
        /// 所有节点
        /// </summary>
        private List<Node> nodes = new List<Node>();
        /// <summary>
        /// 邻接矩阵
        /// </summary>
        private List<List<int>> adjMatrix = new List<List<int>>();

        /// <summary>
        /// 记忆信息，每个记忆是三个值，分别是分段前值、分段索引、分段后值
        /// 记忆信息有T行，T为最大短时记忆容量（参见Configuration）
        /// 记忆信息有N列，N为所有感知节点和处理节点的数量（输出和特征节点除外）
        /// </summary>
        private ValueTuple<double, int, double>[][] memories;
        /// <summary>
        /// 记忆最早的时间点
        /// </summary>
        private int memoryBeginTime;

        /// <summary>
        /// 所有感知节点
        /// </summary>
        public List<Node> Receptors
        {
            get => nodes.FindAll(n => n is Receptor);
        }

        /// <summary>
        /// 所有处理节点
        /// </summary>
        public List<Node> Handlers
        {
            get => nodes.FindAll(n => n is HandlerNode);
        }

        /// <summary>
        /// 效应器
        /// </summary>
        public List<Node> Effectors
        {
            get => nodes.FindAll(n => n is Effector);
        }
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.nodes.ForEach(a => a.Reset());
        }


        

        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="obs"></param>
        /// <returns></returns>
        public List<double> activate(List<double> obs,int time)
        {
            //初始化
            Reset();
            //初始化所有感知节点
            for(int i=0;i<this.Receptors.Count;i++)
            {
                this.Receptors[i].activate(0, obs[i]);
            }

            //反复执行直到都激活
            while(!this.Handlers.TrueForAll(n=>n.IsActivate()))
            {
                this.Handlers.ForEach(n => n.activate(time++));
            }
            //写入短时记忆
            int col = time;
            

            //取出输出节点
            return this.Effectors.ConvertAll<double>(n => (double)n.Value);

        }
    }
}
