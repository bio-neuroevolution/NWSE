using NWSELib.common;
using NWSELib.genome;
using System.Collections.Generic;

namespace NWSELib.net
{
    
    /// <summary>
    /// 推理节点中的记录
    /// </summary>
    public class InferenceRecord
    {
        /// <summary>
        /// 对应每个维的均值
        /// </summary>
        public List<Vector> means = new List<Vector>();
        /// <summary>
        /// 所有维构成的协方差矩阵
        /// </summary>
        public double[][] covariance;

        public List<List<Vector>> sample(int count)
        {

        }

    }
    /// <summary>
    /// 推理链
    /// </summary>
    public class InferenceChain
    {
        /// <summary>评判项</summary>
        public JudgeItem juegeItem;
        /// <summary>链头</summary>
        public Item head;
        /// <summary>链尾</summary>
        public Item tail;
        /// <summary>当前</summary>
        public Item current;
        public class Item
        {
            /// <summary>
            /// 推理节点Id
            /// </summary>
            public int referenceNode;
            /// <summary>
            /// 推理结果：对变量的取值
            /// </summary>
            public List<(int, Vector)> variables = new List<(int, Vector)>();
            /// <summary>
            /// 推理结果：对条件的取值
            /// </summary>
            public List<(int, Vector)> conditions = new List<(int, Vector)>();
            /// <summary>
            /// 前一个推理项
            /// </summary>
            public Item prev;
            /// <summary>
            /// 后一个推理项
            /// </summary>
            public Item next;
        }
    }
    public class Inference : Node
    {
        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        protected List<InferenceRecord> records = new List<InferenceRecord>();
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gene"></param>
        public Inference(NodeGene gene) : base(gene)
        {
        }

        public InferenceChain inference(InferenceChain chain)
        {
            
            
        }

        public InferenceRecord getRecord(int min_or_max, int id)
        {
            int index = ((InferenceGene)this.gene).getVariableIndex(id);
            if (index == -1) return null;

            InferenceRecord r = null;
            double min = double.MaxValue - 1, max = double.MinValue + 1;
            for (int i = 0; i < this.records.Count; i++)
            {
                double len = this.records[i].means[index].length();
                if (min_or_max == 1 && len < min)
                {
                    min = len; r = this.records[i];
                }
                else if (min_or_max == 2 && len > max)
                {
                    max = len; r = this.records[i];
                }
            }
            return r;
        }
    }
}
