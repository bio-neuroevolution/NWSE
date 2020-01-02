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

        public InferenceRecord getRecord(int min_or_max, int id)
        {
            int index = ((InferenceGene)this.gene).getVariableIndex(id);
            if (index == -1) return null;

            InferenceRecord r = null;
            double min = double.MaxValue - 1, max = double.MinValue + 1;
            for (int i = 0; i < this.records.Count; i++)
            {
                double len = this.records[i].means[index].length;
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
