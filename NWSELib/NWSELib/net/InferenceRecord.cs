using System;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;
using System.Collections.Generic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML;
using log4net;
using System.Text;

namespace NWSELib.net
{
    public class InferenceRecord
    {
        
        #region 基本信息   
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
        /// 高斯分布
        /// </summary>
        public VectorGaussian gaussian;

        /// <summary>
        /// 归属节点
        /// </summary>
        public Inference inf;

        /// <summary>
        /// 创建缺省协方差矩阵
        /// </summary>
        /// <returns></returns>
        public double[,] createDefaultCovariance(double coeff = 0.1)
        {
            if (this.means == null) return null;
            this.covariance = new double[this.means.size(), this.means.size()];
            for (int i = 0; i < this.means.size(); i++)
                this.covariance[i, i] = coeff;
            return this.covariance;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inf"></param>
        public InferenceRecord(Inference inf)
        {
            this.inf = inf;
        }
        #endregion

        #region 临时数据，用于记录未融合采样
        /// <summary>
        /// 密度值
        /// </summary>
        public double density;

        /// <summary>
        /// 接收记录
        /// </summary>
        public List<List<Vector>> acceptRecords = new List<List<Vector>>();

        #endregion

        #region 统计数据
        /// <summary>
        /// 接收数量:多少个样本归属该记录
        /// </summary>
        public int acceptCount;
        /// <summary>
        /// 评估值（表示按此记录做出行动的结果好坏）
        /// </summary>
        public double evulation = double.NaN;
        /// <summary>
        /// 使用次数，推理过程使用的次数
        /// </summary>
        public int usedCount;
        /// <summary>
        /// 表示该前置条件和后置变量映射关系的准确程度
        /// </summary>
        public double accuracy = double.NaN;

        /// <summary>
        /// 上层抽象记录
        /// </summary>
        public InferenceRecord parent;
        /// <summary>
        /// 抽象之前的记录
        /// </summary>
        public List<InferenceRecord> childs = new List<InferenceRecord>();
        #endregion

        #region 字符串

        static ILog logger = LogManager.GetLogger(typeof(Inference));
        /// <summary>
        /// 字符串，不需要反向解析，因为记录为后天学习，不遗传
        /// </summary>
        /// <param name="xh"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string ToString(Inference inf, int xh, String prefix = "   ")
        {
            StringBuilder str = new StringBuilder();

            str.Append("mean" + xh.ToString() + "=" + meanToString(inf) + System.Environment.NewLine +
                       prefix + "evulation=" + evulation.ToString("F3") + System.Environment.NewLine +
                       prefix + "accuracy=" + accuracy.ToString("F3") + System.Environment.NewLine +
                       prefix + "usedCount=" + usedCount.ToString() + System.Environment.NewLine +
                       prefix + "acceptCount=" + acceptCount.ToString() + System.Environment.NewLine);
            return str.ToString();
        }
        private String meanToString(Inference inf)
        {
            (Vector flattenedValue, List<int> sizes) = this.means.flatten();
            return inf.GetValueText(flattenedValue);
        }

        public override string ToString()
        {
            return this.means.toString();
        }

        public String Summary()
        {
            return this.means.toString() + ",e=" + evulation.ToString("F4")+",accept="+acceptCount.ToString();
        }

        #endregion

        #region 均值处理
        
        /// <summary>
        /// 分解均值为条件和结论部分
        /// </summary>
        /// <returns></returns>
        public (List<Vector> condValues, List<Vector> varValues) GetMeanValues()
        {
            List<Vector> condValues = new List<Vector>();
            List<Vector> varValues = new List<Vector>();

            int condcount = this.inf.GetGene().ConditionCount;
            int varcount = this.inf.GetGene().VariableCount;
            condValues.AddRange(this.means.GetRange(0, condcount));
            varValues.AddRange(this.means.GetRange(condcount,varcount));
            return (condValues, varValues);
        }

        /// <summary>
        /// 将记录值分解为环境、动作、后置变量三部分
        /// 这里的动作部分总是包含全部动作，即使inf中不需要动作，也会产生所有动作的缺省值
        /// </summary>
        /// <param name="net"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public (List<Vector>, List<Vector>, List<Vector>) splitMeans3()
        {
            //待返回结果
            List<Vector> env = new List<Vector>();
            List<Vector> actions = new List<Vector>();
            List<Vector> expects = new List<Vector>();

            //动作比较特殊，每个动作都要设置结构，尽管该节点可能不涉及
            actions = inf.net.CreateDefaultActions();

            //分解条件部分
            List<int> conditions = inf.GetGene().conditions;
            for (int i = 0; i < conditions.Count; i++)
            {
                Node node = inf.net[conditions[i]];
                if (node.Gene.IsActionSensor())
                {
                    Effector effector = (Effector)inf.net.Effectors.FirstOrDefault(e => "_" + e.Name == node.Name);
                    int effectorIndex = inf.net.Effectors.IndexOf(effector);
                    actions[effectorIndex] = means[i];
                }
                else
                {
                    env.Add(means[i]);
                }
            }

            int condCount = inf.GetGene().ConditionCount;
            int varCount = inf.GetGene().VariableCount;
            return (env, actions, this.means.GetRange(condCount, varCount));
        }

        
        
        #endregion

        #region  统计学习功能
        /// <summary>
        /// 高斯采样
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<List<Vector>> DoSamples(int count)
        {
            this.InitGaussian();
            try
            {
                List<int> dimension = means.ConvertAll(v => v.Size);
                List<List<Vector>> r = new List<List<Vector>>();
                for (int i = 0; i < count; i++)
                    r.Add(this.gaussian.Sample().fromMathVector(dimension));
                return r;
            }
            catch (Exception e)
            {
                //这里异常一般是协方差矩阵是奇异矩阵造成的，修改协方差矩阵
                logger.Error(e.Message);
                for (int i = 0; i < this.covariance.GetLength(0); i++)
                    this.covariance[i, i] += 0.001;
                this.gaussian = null;
                this.InitGaussian();
                return DoSamples(count);
            }
        }

        protected void checkGaussian()
        {
            if (gaussian == null) return;

            for (int i = 0; i < covariance.GetLength(0); i++)
            {
                for (int j = 0; j < covariance.GetLength(1); j++)
                {
                    if (double.IsNaN(covariance[i, j]))
                    {
                        if (i == j) covariance[i, j] = 1.0;
                        else covariance[i, j] = 0.0;
                    }
                }
            }

            if (gaussian.GetMean().ToList().Exists(v => double.IsNaN(v)))
                return;

            

        }
        /// <summary>
        /// 根据均值和协方差数据生成高斯对象
        /// </summary>
        public void InitGaussian()
        {
            if (gaussian != null)
                return;

            Microsoft.ML.Probabilistic.Math.Vector mean = null;
            Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix covar = null;
            try
            {
                mean = this.means.toMathVector();
                covar = new Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix(covariance);
                gaussian = VectorGaussian.FromMeanAndVariance(mean, covar);

                checkGaussian();
            }
            catch (Exception e)
            {
                //这里异常的主要原因是得到的协方差矩阵不是正定矩阵。
                //This happens if the diagonal values of the covariance matrix are (very close to) zero. 
                //A simple fix is add a very small constant number to c.
                //logger.Error(e.Message);
                for (int i = 0; i < covariance.GetLength(0); i++)
                {
                    for (int j = 0; j < covariance.GetLength(1); j++)
                    {
                        if (i == j) covariance[i, j] += 0.001;
                        //if (i == j && covariance[i, j] < 0) covariance[i, j] += 0.01;
                        //if (i != j) covariance[i,j] = 0;
                    }
                }
                covar = new Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix(covariance);
                gaussian = VectorGaussian.FromMeanAndVariance(mean, covar);
                checkGaussian();
            }

            
        }

        /// <summary>
        /// 高斯分布概率值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public double Prob(List<Vector> values)
        {
            this.InitGaussian();
            Microsoft.ML.Probabilistic.Math.Vector v = values.toMathVector();
            return Math.Exp(gaussian.GetLogProb(v));
        }
        
        /// <summary>
        /// 前向推理
        
        #endregion

        #region 自适应学习
        public const double variance = 0.1;
        /// <summary>
        /// 对推理记录的均值和协方差矩阵进行调整
        /// Adjust the mean and covariance matrix of the record of inference node
        /// </summary>
        /// <param name="net"></param>
        /// <param name="inf"></param>
        public void DoAdjust(Network net)
        {
            List<List<Vector>> values = new List<List<Vector>>();
            for (int i = 0; i < this.acceptCount; i++)
                values.Add(this.means);
            for (int i = 0; i < this.acceptRecords.Count; i++)
                values.Add(this.acceptRecords[i]);
            List<Vector> flatten = values.ConvertAll(v => v.flatten()).ConvertAll(p => p.Item1);
            Vector flatten_mean = flatten.average();
            this.covariance = Vector.covariance(flatten.ToArray());

            this.gaussian = null;
            this.InitGaussian();
            this.acceptCount += this.acceptRecords.Count;
            this.acceptRecords.Clear();

        }

        
        #endregion

        #region 环境与推理的匹配性检查
        public bool IsConditionMatch(List<Vector> condValues, out List<double> distances)
        {
            distances = DistanceFromCondition(condValues);
            for (int i=0;i<inf.ConditionNodes.Count;i++)
            {
                if (!inf.ConditionNodes[i].IsTolerateDistance(distances[i]))
                    return false;
            }
            return true;
        }

        public bool IsVariableMatch(List<Vector> varValues,out List<double> distances)
        {
            distances = DistanceFromVariable(varValues);
            for (int i = 0; i < inf.VariableNodes.Count; i++)
            {
                if (!inf.VariableNodes[i].IsTolerateDistance(distances[i]))
                    return false;
            }
            return true;
        }

        

        public List<double> DistanceFromCondition(List<Vector> value)
        {
            Vector v1 = value.flatten().Item1;
            Vector v2 = this.means.flatten().Item1;

            List<double> distances = new List<double>();
            for (int i = 0; i < this.inf._conditionNodes.Count; i++)
            {
                double d = this.inf.ConditionNodes[i].distance(v1[i], v2[i]);
                distances.Add(d);
            }

            return distances;
        }

        public List<double> DistanceFromConditions(List<int> ids, List<Vector> values)
        {
            Vector v1 = values.flatten().Item1;
            Vector v2 = this.splitMeans3().Item1.flatten().Item1;

            List<Receptor> receptors = inf.net.Genome.getReceptorGenes(ids.ToArray()).ConvertAll(g=>(Receptor)inf.net[g.Id]);
            List<double> dis = new List<double>();
            for(int i=0;i< receptors.Count;i++)
            {
                dis.Add(receptors[i].distance(v1[i],v2[i]));
            }

            return dis;
        }

        public List<double> DistanceFromVariable(List<Vector> value)
        {
            Vector v1 = value.flatten().Item1;
            Vector v2 = this.GetMeanValues().varValues.flatten().Item1;

            List<double> distances = new List<double>();
            for (int i = 0; i < this.inf.VariableNodes.Count; i++)
            {
                double d = this.inf.VariableNodes[i].distance(v1[i], v2[i]);
                distances.Add(d);
            }

            return distances;
        }

        
        public List<double> Distance(List<Vector> value)
        {
            Vector v1 = value.flatten().Item1;
            Vector v2 = this.means.flatten().Item1;

            List<double> distances = new List<double>();
            for (int i = 0; i < this.inf.LeafReceptors.Count; i++)
            {
                double d = this.inf.LeafReceptors[i].distance(v1[i], v2[i]);
                distances.Add(d);
            }
            return distances;
        }


        

        


        #endregion
    }
}
