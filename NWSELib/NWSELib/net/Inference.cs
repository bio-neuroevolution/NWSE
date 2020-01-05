using System;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;
using System.Collections.Generic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;

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
        /// 权重
        /// </summary>
        public double weight;
        /// <summary>
        /// 所有维构成的协方差矩阵
        /// </summary>
        public double[,] covariance;
        /// <summary>
        /// 密度值
        /// </summary>
        public double density;
        /// <summary>
        /// 高斯分布
        /// </summary>
        public VectorGaussian gaussian;


        public List<List<Vector>> sample(int count)
        {
            throw new NotImplementedException();
        }

        public void initGaussian()
        {
            if (gaussian != null) return;
            Microsoft.ML.Probabilistic.Math.Vector mean = this.means.toMathVector();
            Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix covar = new Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix(covariance);
            gaussian = VectorGaussian.FromMeanAndVariance(mean, covar);
        }
        
        internal double prob(List<Vector> values)
        {
            this.initGaussian();
            Microsoft.ML.Probabilistic.Math.Vector v = values.toMathVector();
            return gaussian.GetLogProb(v);
        }
    }
    
    public class Inference : Node
    {
        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        protected List<InferenceRecord> records = new List<InferenceRecord>();

        /// <summary>
        /// 新样本
        /// </summary>
        public List<List<Vector>> newsamples = new List<List<Vector>>();
        /// <summary>
        /// 新样本的密度值
        /// </summary>
        public List<double> density = new List<double>();

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
            List<Vector> values = new List<Vector>();
            for(int i=0;i<((InferenceGene)this.gene).dimensions.Count;i++)
            {
                (int id,int t) = ((InferenceGene)this.gene).dimensions[i];
                Node input = inputs.FirstOrDefault(n => n.Id == id);
                values.Add(input.Value);
            }
            //计算输入值的归属
            List<double> probs = this.records.ConvertAll(r => r.prob(values));
            double sumprobs = probs.Sum();
            probs = probs.ConvertAll(p => p / sumprobs);
            int pindex = probs.argmax();

            //计算每个节点的密度值，以及样本的密度值
            List<List<Vector>> allValues = new List<List<Vector>>();
            this.records.ForEach(r => allValues.Add(r.means));
            allValues.AddRange(this.newsamples);
            List<double> distances = allValues.ConvertAll(v => v.distance(values));
            double dissum = distances.Sum();
            List<double> delta_diensity = distances.ConvertAll(d => (dissum - d) / dissum);

            //如果最大密度点不大于阈值，且原有高斯分量密度大于给定阈值，则调整该高斯分量的均值和协方差，结束
            //否则对新样本根据密度进行聚类：将密度值由高到底排列，对密度值高于阈值的搜索某半径内的其他点作为一类
            //根据聚类结果计算新高斯分量参数
            //降低原有高斯分量的密度，如果密度低于阈值，则该执行分量撤销操作
            //调整原有高斯分量的均值和协方差
            throw new NotImplementedException();

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

        private List<List<Vector>> samples(int inferencesamples)
        {
            this.records.ForEach(r => r.initGaussian());
            Range k = new Range(this.records.Count);
            VariableArray<Microsoft.ML.Probabilistic.Math.Vector> means = Variable.Array<Microsoft.ML.Probabilistic.Math.Vector>(k);
            means.ObservedValue = this.records.ConvertAll(r => r.gaussian.GetMean()).ToArray();

            
            VariableArray<Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix> variances = Variable.Array<Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix>(k);
            variances.ObservedValue = this.records.ConvertAll(r => r.gaussian.GetVariance()).ToArray();

            Range n = new Range(inferencesamples);
            VariableArray<Microsoft.ML.Probabilistic.Math.Vector> data = Variable.Array<Microsoft.ML.Probabilistic.Math.Vector>(n);
            VariableArray<int> z = Variable.Array<int>(n);
            double[] weights = this.records.ConvertAll(r => r.weight).ToArray();
            using (Variable.ForEach(n))
            {
                z[n] = Variable.Discrete(weights);
                using (Variable.Switch(z[n]))
                {
                    data[n] = Variable.VectorGaussianFromMeanAndVariance(
                      means[z[n]], variances[z[n]]);
                }
            }
            List<int> dimension = this.records[0].means.ConvertAll(v => v.Size);
            return data.ObservedValue.ToList().ConvertAll(v => v.fromMathVector(dimension));
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
