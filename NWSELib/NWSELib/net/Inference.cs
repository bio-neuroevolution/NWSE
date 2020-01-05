using System;
using System.Linq;
using NWSELib.common;
using NWSELib.genome;
using System.Collections.Generic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;
using Microsoft.ML;
using Microsoft.ML.Trainers;

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


        public List<List<Vector>> sample(int count)
        {
            this.initGaussian();
            List<int> dimension = means.ConvertAll(v => v.Size);
            List<List<Vector>> r = new List<List<Vector>>();
            for (int i = 0; i < count; i++)
                r.Add(this.gaussian.Sample().fromMathVector(dimension));
            return r;
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
            Vector variance = Vector.variance(flatten.ToArray());
            this.means = flatten_mean.split(dimensions);
            this.covariance = new double[totaldimension, totaldimension];
            for(int i=0;i<totaldimension;i++)
            {
                this.covariance[i, i] = variance[i];
            }

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
                for (int i = 0; i < totaldimesion; i++)
                    record.covariance[i, i] = 1.0;
                record.weight = 1.0;
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

        }

        private List<List<List<Vector>>> do_newsamples_cluster()
        {
            int i = 0;
            //    int clusters_count = 0;
            //    List<point> center = new List<point>();

            //    for (i = 0; i < setOfPoints.Count;i++ )
            //    {
            //       if (setOfPoints[i].is_core == true && setOfPoints[i].is_clusterID == false)//
            //        {//选择一个尚未分类的核心点作为种子开始聚类
            //            clusters_count++;
            //            if (clusters_count % 10 == 0)
            //            {
            //                //Console.WriteLine(clusters_count + " clusters generated!!");
            //                int clustered = checkClustered();
            //                Console.WriteLine(clustered+" points has been clustered!");
            //            }
            //            expand_Cluster((point)setOfPoints[i],clusters_count);//给所以与此点density-connect的点标记聚类ID
            //        }
            //        Console.WriteLine(i+"  points finished!!");

            //    }
            List<List<List<Vector>>> r = new List<List<List<Vector>>>();
            List<Vector> centers = new List<Vector>();
            
            while(this.newsamples.Count>0)
            {
                int maxindex = this.density.argmax();
                List<List<Vector>> iis = new List<List<Vector>>();
                iis.Add(this.newsamples[maxindex]);
                Vector center = this.newsamples[maxindex].flatten().Item1;
                centers.Add(center);
                r.Add(iis);

                this.newsamples.RemoveAt(maxindex);
                this.density.RemoveAt(maxindex);

                for(int i=0;i<centers.Count;i++)
                {
                    List<double> dis = this.newsamples.ConvertAll(s => s.flatten().Item1.distance(centers[i]));
                    dis = dis.ConvertAll(d => d / dis.Max());
                    int t = dis.argmin();
                    if()
                }
                
            }

                
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
            Vector variance = Vector.variance(flatten.ToArray());
            for (int i = 0; i < totaldimension; i++)
                r.covariance[i, i] = variance[i];
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
