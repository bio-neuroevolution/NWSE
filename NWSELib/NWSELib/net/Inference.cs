using System;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;
using System.Collections.Generic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML;
using log4net;

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
        /// 接收数量
        /// </summary>
        public int acceptCount;
        /// <summary>
        /// 接收记录
        /// </summary>
        public List<List<Vector>> acceptRecords = new List<List<Vector>>();
        /// <summary>
        /// 高斯分布
        /// </summary>
        public VectorGaussian gaussian;

        static ILog logger = LogManager.GetLogger(typeof(Inference));
        public List<List<Vector>> sample(int count)
        {
            this.initGaussian();
            try
            {
                List<int> dimension = means.ConvertAll(v => v.Size);
                List<List<Vector>> r = new List<List<Vector>>();
                for (int i = 0; i < count; i++)
                    r.Add(this.gaussian.Sample().fromMathVector(dimension));
                return r;
            }catch(Exception e)
            {
                //这里异常一般是协方差矩阵是奇异矩阵造成的，修改协方差矩阵
                logger.Error(e.Message);
                for (int i = 0; i < this.covariance.GetLength(0); i++)
                    this.covariance[i, i] += 0.01;
                this.gaussian = null;
                this.initGaussian();
                return sample(count);
            }
        }

        public void initGaussian()
        {
            if (gaussian != null) return;
            Microsoft.ML.Probabilistic.Math.Vector mean = null;
            Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix covar = null;
            try
            {
                mean = this.means.toMathVector();
                covar = new Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix(covariance);
                gaussian = VectorGaussian.FromMeanAndVariance(mean, covar);
            }catch(Exception e)
            {
                //这里异常的主要原因是得到的协方差矩阵不是正定矩阵。
                //This happens if the diagonal values of the covariance matrix are (very close to) zero. 
                //A simple fix is add a very small constant number to c.
                logger.Error(e.Message);
                for(int i=0;i<covariance.GetLength(0); i++)
                {
                    for(int j=0;j<covariance.GetLength(1);j++)
                    {
                        if (i == j) covariance[i, j] += 0.001;
                        //if (i == j && covariance[i, j] < 0) covariance[i, j] += 0.01;
                        //if (i != j) covariance[i,j] = 0;
                    }
                }
                covar = new Microsoft.ML.Probabilistic.Math.PositiveDefiniteMatrix(covariance);
                gaussian = VectorGaussian.FromMeanAndVariance(mean, covar);
            }


        }
        
        internal double prob(List<Vector> values)
        {
            this.initGaussian();
            Microsoft.ML.Probabilistic.Math.Vector v = values.toMathVector();
            return gaussian.GetLogProb(v);
        }
        public double mahalanobis_distance(List<Vector> values)
        {
            Microsoft.ML.Probabilistic.Math.Vector value = values.toMathVector();

            Microsoft.ML.Probabilistic.Math.Matrix m = new Microsoft.ML.Probabilistic.Math.Matrix(value.Count, 1);
            m.SetTo(value.ToArray());

            return Math.Sqrt((m.Transpose() * this.gaussian.GetVariance().Inverse() * m)[0,0]);
        }

        internal void do_adjust(Network net,Inference inf)
        {
            List<int> dimensions = inf.getDimensionList(net);
            int totaldimension = dimensions.Sum();
            List<List<Vector>> values = new List<List<Vector>>();
            for (int i = 0; i < Session.GetConfiguration().learning.inference.accept_max_count; i++)
                values.Add(this.means);
            for (int i = 0; i < this.acceptRecords.Count; i++)
                values.Add(this.acceptRecords[i]);
            List<Vector> flatten = values.ConvertAll(v => v.flatten()).ConvertAll(p=>p.Item1);
            Vector flatten_mean = flatten.average();
            this.covariance = Vector.covariance(flatten.ToArray());
            
            this.gaussian = null;
            this.initGaussian();

            this.acceptRecords.Clear();

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

        public List<int> getDimensionList(Network net)
        {
            List<int> dimension = new List<int>();
            for (int i = 0; i < ((InferenceGene)this.gene).dimensions.Count; i++)
            {
                (int id, int t) = ((InferenceGene)this.gene).dimensions[i];
                Node node = net.Nodes.FirstOrDefault(n => n.Id == id);
                dimension.Add(node.Dimension);
            }
            return dimension;
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
            int totaldimesion = 0;
            for(int i=0;i<((InferenceGene)this.gene).dimensions.Count;i++)
            {
                (int id,int t) = ((InferenceGene)this.gene).dimensions[i];
                Node input = inputs.FirstOrDefault(n => n.Id == id);
                values.Add(input.Value);
                totaldimesion += input.Dimension;
            }
           

            //如果没有任何节点记录，则生成第一个
            if (this.records.Count <= 0)
            {
                InferenceRecord record = new InferenceRecord();
                record.means = values;
                record.covariance = new double[totaldimesion, totaldimesion];
                for (int i = 0; i < totaldimesion; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = 1.0;
                record.weight = 1.0;
                this.records.Add(record);
                return null;
            }

            //计算输入值的归属
            List<double> probs = this.records.ConvertAll(r => r.prob(values)/r.prob(r.means));
            double sumprobs = probs.Sum();
            probs = probs.ConvertAll(p => p / sumprobs);
            int pindex = probs.argmax();
            if(probs.Max()>=Session.GetConfiguration().learning.inference.accept_prob)
            {
                this.records[pindex].acceptCount += 1;
                this.records[pindex].acceptRecords.Add(values);
                if(this.records[pindex].acceptCount >= Session.GetConfiguration().learning.inference.accept_max_count)
                {
                    this.records[pindex].do_adjust(net,this);
                }
                return null;

            }

            //计算每个节点的密度值，以及样本的密度值
            List<List<Vector>> allValues = new List<List<Vector>>();
            this.records.ForEach(r => allValues.Add(r.means));
            allValues.AddRange(this.newsamples);
            List<double> distances = allValues.ConvertAll(v => v.distance(values));
            double dissum = distances.Sum();
            List<double> delta_diensity = distances.ConvertAll(d => (dissum - d) / dissum);
            for(int i=0;i<this.records.Count;i++)
            {
                this.records[i].density += delta_diensity[i];
            }
            for(int i=this.records.Count;i<delta_diensity.Count;i++)
            {
                this.density[i - this.records.Count] += delta_diensity[i];
            }
            this.newsamples.Add(values);
            int ti = distances.argmin();
            double td = ti < this.records.Count ? this.records[ti].density : this.density[ti - this.records.Count];
            this.density.Add(td);

            //如果新样本的最大密度接近原有高斯分量中心的最小密度，则启动新样本聚类过程
            double max_newsample_density = this.density.Max();
            double min_record_density = this.records.ConvertAll(r => r.density).Min();
            if(max_newsample_density >= min_record_density*2/3)
            {
                List<List<List<Vector>>> clusters = do_newsamples_cluster();
                for(int i=0;i<clusters.Count;i++)
                {
                    create_newrecord_bysamples(net,clusters[i]);
                }
            }
            //重新调整权重
            adjust_weights();

            return null;

        }

        private List<List<List<Vector>>> do_newsamples_cluster()
        {
            List<List<List<Vector>>> r = new List<List<List<Vector>>>();
            while (this.newsamples.Count > 0)
            {
                //取最大密度点，作为一个新分类
                int maxindex = this.density.argmax();
                List<Vector> sample = this.newsamples[maxindex];
                List<List<Vector>> classes = new List<List<Vector>>();
                classes.Add(sample);
                //从样本集中移除该点
                this.newsamples.RemoveAt(maxindex);
                this.density.RemoveAt(maxindex);
                if (this.newsamples.Count <= 0) break;
                //计算样本集中所有点与该点的距离
                List<double> ds = this.newsamples.ConvertAll(s => s.distance(sample));
                //对距离从小到大排序
                List<int> sortedindex = ds.argsort();
                //对排列后的距离寻找最小方差分裂点
                List<double> disvar = new List<double>();
                for(int i=2;i<ds.Count;i++)
                {
                    List<double> temp = new List<double>();
                    for (int j = 0; j < i; j++)
                        temp.Add(ds[sortedindex[j]]);

                    (double t1, double t2) = new Vector(temp.ToArray()).avg_variance();
                    disvar.Add(t2);
                }
                int argminvar = disvar.argmin()+2;
                //将能够使方差最小的样本归属同一类
                for(int i=0;i<argminvar;i++)
                {
                    classes.Add(this.newsamples[sortedindex[i]]);
                }
                //移除已经归类的样本
                for(int i=0;i<classes.Count;i++)
                {
                    int t3 = this.newsamples.IndexOf(classes[i]);
                    this.newsamples.Remove(classes[i]);
                    this.density.RemoveAt(t3);
                }
                if (this.newsamples.Count <= 0) break;



            }

            return r;


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        public InferenceRecord create_newrecord_bysamples(Network net,List<List<Vector>> vs)
        {
            InferenceRecord r = new InferenceRecord();
            r.acceptCount = vs.Count;

            List<int> dimensions = this.getDimensionList(net);
            int totaldimension = dimensions.Sum();
            List<Vector> flatten = vs.ConvertAll(v => v.flatten()).ConvertAll(v => v.Item1);
            r.means = flatten.average().split(dimensions);
            r.covariance = new double[totaldimension, totaldimension];
            r.covariance = Vector.covariance(flatten.ToArray());
            
            r.initGaussian();
            return r;

        }

        public void adjust_weights()
        {
            List<double> ws = this.records.ConvertAll(r => (double)r.acceptCount);
            double max = ws.Max();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i]; 

            /*List<double> ws = this.records.ConvertAll(r => r.gaussian.GetLogProb(r.gaussian.GetMean()));
            double max = ws.Max();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i];*/
        }

        #region 推理
        /// <summary>
        /// 前向推理：给定条件，计算结论
        /// </summary>
        /// <param name="condvalues"></param>
        /// <returns></returns>
        public Vector forwardinference(List<Vector> condvalues)
        {
            //按照混合高斯分布进行采样
            List<List<Vector>> samples = this.samples(Session.GetConfiguration().agent.inferencesamples);
            //在采样中选择距离condValues最近的点
            int varindex = ((InferenceGene)this.gene).getVariableIndex();
            List<double> dis = new List<double>();
            for(int i=0;i<samples.Count;i++)
            {
                List<Vector>  t = new List<Vector>(samples[i]);
                t.RemoveAt(varindex);
                Vector d1 = new Vector(t.toDoubleArray());
                List<Vector> temp = new List<Vector>(condvalues);
                Vector d2 = new Vector(temp.toDoubleArray());
                dis.Add(d1.distance(d2));
            }
            int argminindex = dis.argmin();
            return samples[argminindex][varindex];
        }
        /// <summary>
        /// 给定后置变量的值，推理条件部分的值
        /// </summary>
        /// <param name="varValue"></param>
        /// <returns></returns>
        public (List<Vector>, int) backinference(Vector varValue)
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
            
            double[] ws = this.records.ConvertAll(r => r.weight).ToArray();
            
            Discrete zt = new Discrete(ws);
            List<List<Vector>> result = new List<List<Vector>>();
            for (int i=0;i< inferencesamples;i++)
            {
                int index = zt.Sample();
                result.Add(this.records[index].sample(1)[0]);
            }
            return result;

            /*
            using (Variable.ForEach(n))
            {
                z[n] = Variable.Discrete(ws);
                //z.SetValueRange(n);
                int index = Variable.Switch(z[n]).ConditionValue;

                using (Variable.Switch(z[n]))
                {
                    data[n] = Variable.VectorGaussianFromMeanAndVariance(
                      means[z[n]], variances[z[n]]);
                }
            }
            List<int> dimension = this.records[0].means.ConvertAll(v => v.Size);
            return data.ObservedValue.ToList().ConvertAll(v => v.fromMathVector(dimension));
            */
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

            if(this.records.Count<=0)
            {
                return (null,-1,double.NaN);
            }
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
