using System.Collections.Generic;
using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net
{
    /// <summary>
    /// 网络
    /// </summary>
    public class Network
    {
        #region 基本信息
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
        private double[,] adjMatrix;

        /// <summary>
        /// 记忆信息，每个记忆是三个值，分别是分段前值、分段索引、分段后值
        /// 记忆信息有T行，T为最大短时记忆容量（参见Configuration）
        /// 记忆信息有N列，N为所有感知节点和处理节点的数量（输出和特征节点除外）
        /// </summary>
        private (Vector, int, Vector)[,] memories;
        /// <summary>
        /// 记忆最早的时间点
        /// </summary>
        private int memoryBeginTime;
        #endregion

        #region 节点查询
        /// <summary>
        /// 所有感知节点
        /// </summary>
        public List<Node> Receptors
        {
            get => nodes.FindAll(n => n is Receptor);
        }
        /// <summary>
        /// 所有环境感知节点
        ///</summary>
        public List<Node> EnvReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("env"));
        }
        /// <summary>
        /// 所有姿态感知节点
        ///</summary>
        public List<Node> GesturesReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("gestures"));
        }
        /// <summary>
        /// 所有动作感知节点
        ///</summary>
        public List<Node> ActionReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("action"));
        }


        /// <summary>
        /// 所有处理节点
        /// </summary>
        public List<Node> Handlers
        {
            get => nodes.FindAll(n => n is Handler);
        }

        /// <summary>
        /// 效应器
        /// </summary>
        public List<Node> Effectors
        {
            get => nodes.FindAll(n => n is Effector);
        }

        /// <summary>
        /// 根据节点Id查找节点索引下标
        /// </summary>
        public int idToIndex(int id)
        {
            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.nodes[i].Id == id)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 根据节点Id的上游连接节点
        /// </summary>
        public List<Node> getInputNodes(int id)
        {
            int index = this.idToIndex(id);
            List<Node> r = new List<Node>();
            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.adjMatrix[i, index] != 0)
                    r.Add(this.nodes[i]);
            }
            return r;
        }

        #endregion

        #region 初始化
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.nodes.ForEach(a => a.Reset());
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public Network(NWSEGenome genome)
        {
            this.genome = genome;
            //初始化节点
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                Receptor receptor = new Receptor(genome.receptorGenes[i]);
                this.nodes.Add(receptor);
            }
            for (int i = 0; i < genome.handlerGenes.Count; i++)
            {
                Handler handler = new Handler(genome.handlerGenes[i]);
                this.nodes.Add(handler);
            }
            for (int i = 0; i < genome.infrernceGenes.Count; i++)
            {
                Inference inference = new Inference(genome.infrernceGenes[i]);
                this.nodes.Add(inference);
            }
            for (int i = 0; i < this.ActionReceptors.Count; i++)
            {
                Effector effector = new Effector(((ReceptorGene)this.ActionReceptors[i].Gene).toActionGene());
                this.nodes.Add(effector);
            }

            //构造连接矩阵
            adjMatrix = new double[this.nodes.Count, this.nodes.Count];
            for (int i = 0; i < this.genome.connectionGene.Count; i++)
            {
                (int, int) connection = this.genome.connectionGene[i];
                int srcIndex = this.idToIndex(connection.Item1);
                int destIndex = this.idToIndex(connection.Item2);
                this.adjMatrix[srcIndex, destIndex] = 1;
            }

            //初始化记忆部分
            int capacity = Session.GetConfiguration().agent.shorttermcapacity;
            this.memories = new (Vector, int, Vector)[this.nodes.Count, capacity];
            this.memoryBeginTime = -1;
        }

        #endregion


        #region 记忆管理
        /// <summary>
        /// 短时记忆容量
        /// </summary>
        public int MemoryCapcaity
        {
            get => this.memories.GetLength(1);

        }

        /// <summary>
        /// 取得短时记忆数据
        /// </summary>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public (Vector, int, Vector) getMemoryItem(int time, int index)
        {
            time -= this.memoryBeginTime;
            return this.memories[index, time];
        }
        /// <summary>
        /// 记录数据到短时记忆
        /// </summary>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void setMemoryItem(int time, int index, (Vector, int, Vector) value)
        {
            time -= this.memoryBeginTime;
            if (time >= this.MemoryCapcaity)
            {
                for (int i = 1; i < this.MemoryCapcaity; i++)
                {
                    for (int j = 0; j < this.nodes.Count; j++)
                    {
                        this.memories[j, i - 1] = this.memories[j, i];
                    }
                }
                this.memoryBeginTime += 1;
            }
            this.memories[index, this.MemoryCapcaity - 1] = value;
        }


        #endregion

        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="obs"></param>
        /// <returns></returns>
        public List<double> activate(List<double> obs, int time)
        {
            //初始化
            Reset();
            //初始化所有感知节点
            for (int i = 0; i < this.Receptors.Count; i++)
            {
                this.Receptors[i].activate(this, 0, obs[i]);
            }

            //反复执行直到都激活
            while (!this.Handlers.TrueForAll(n => n.IsActivate()))
            {
                this.Handlers.ForEach(n => n.activate(this, time, null));
            }
            //写入短时记忆
            int col = time;


            //取出输出节点
            return this.Effectors.ConvertAll<double>(n => (double)n.Value);

        }
    }
}
