using System;
using System.Linq;
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
            public (int, Vector) variable;
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

        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net, int time, Object value = null)
        {
            List<Node> inputs = net.getInputNodes(this.Id);
            if (!inputs.All(n => n.IsActivate(time)))
                return null;
            
            //根据基因定义的顺序，将输入值组成List<Vector>


        }

        #region 推理
        /// <summary>
        /// 给定后置变量的值，推理条件部分的值
        /// </summary>
        /// <param name="varValue"></param>
        /// <returns></returns>
        public (List<Vector>, int) postinference(Vector varValue)
        {
            //按照混合高斯分布进行采样
            List<List<Vector>> samples = this.samples(Session.GetConfiguration().agent.inferencesamples);
            int varindex = ((InferenceGene)this.gene).getVariableIndex();
            //在采样中寻找与varValue最接近的值
            double dis = double.MaxValue - 1;
            List<Vector> vs = null;
            for(int i=0;i<samples.Count;i++)
            {
                Vector v = samples[i][varindex];
                double d = v.distance(varValue);
                if(d < dis)
                {
                    d = dis;
                    vs = samples[i];
                }
            }
            //返回该采样
            return (vs, varindex);
        }
        /// <summary>
        /// 求变量最大或者最小推理
        /// </summary>
        /// <param name="arg">argmin还是argmax</param>
        /// <returns></returns>
        public (List<Vector>, int,double) arginference(string arg)
        {
            //根据arg选择varId最大或者最小的那个节点
            InferenceRecord argRecord = null;
            double value = arg == "argmin" ? double.MaxValue - 1 : double.MinValue + 1;
            int varindex = ((InferenceGene)this.gene).getVariableIndex();
            for(int i=0;i<this.records.Count;i++)
            {
                double len = this.records[i].means[varindex].length();
                if(arg == "argmin" && len < value)
                {
                    argRecord = this.records[i];
                    value = len;
                }else if(arg == "argmax" && len > value)
                {
                    argRecord = this.records[i];
                    value = len;
                }
            }
            //对这个节点根据分布进行采样
            List<List<Vector>> samples = argRecord.sample(Session.GetConfiguration().agent.inferencesamples);
            //选择采样中使得varId最大或者最小的那个节点
            List<Vector> values = null;
            value = arg == "argmin" ? double.MaxValue - 1 : double.MinValue + 1;
            for (int i=0;i<samples.Count;i++)
            {
                double len = samples[i][varindex].length();
                if (arg == "argmin" && len < value)
                {
                    values = samples[i];
                    value = len;
                }
                else if (arg == "argmax" && len > value)
                {
                    values = samples[i];
                    value = len;
                }
            }
            //返回该节点的条件和变量的值
            return (values, varindex, value);
            
        }

        #endregion
    }
}
