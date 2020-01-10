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
                    this.covariance[i, i] += 0.001;
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
        /// <summary>
        /// 对推理记录的均值和协方差矩阵进行调整
        /// Adjust the mean and covariance matrix of the record of inference node
        /// </summary>
        /// <param name="net"></param>
        /// <param name="inf"></param>
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
        static ILog logger = LogManager.GetLogger(typeof(Inference));
        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        protected List<InferenceRecord> records = new List<InferenceRecord>();

        /// <summary>
        /// 新样本
        /// </summary>
        public List<List<Vector>> unclassified_samples = new List<List<Vector>>();
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
            //所有的输入节点都已经被激活
            //All input nodes have already been activated
            List<(int,int)> conds = ((InferenceGene)this.Gene).getConditions();
            List<(Node, int)> condNodes = conds.ConvertAll(c => (net.getNode(c.Item1), c.Item2));
            if (!condNodes.All(n => n.Item1.IsActivate(time - n.Item2)))
                return null;
           
                      
            (int varid, int vartime) = ((InferenceGene)this.Gene).getVariable();
            Node varNode = net.getNode(varid);
            if (!varNode.IsActivate(time - vartime))
                return null;
            List<Node> inputs = net.getInputNodes(this.Id);

            Vector activeValue = null;
            //确保推理基因的各维度的顺序正确（前置条件在前，后置变量在后，且前置条件id按从小到大排列）
            //Make sure that the dimensions of the inference gene are in the correct order
            ((InferenceGene)this.Gene).sort_dimension();

            //根据基因定义的顺序，将输入值组成List<Vector>
            //Put the input values into the List according to the order of the input dimensions
            List<Vector> values = new List<Vector>();
            int totaldimesion = 0;
            for(int i=0;i<((InferenceGene)this.gene).dimensions.Count;i++)
            {
                (int id,int t) = ((InferenceGene)this.gene).dimensions[i];
                Node input = inputs.FirstOrDefault(n => n.Id == id);
                Vector tValue = input.GetValue(time - t);
                if (tValue == null) { base.activate(net,time, activeValue); return null; }
                values.Add(tValue);
                totaldimesion += input.Dimension;
            }


            //如果没有任何节点记录，则生成第一个
            //Create a new record if there are no nodes in current inference node, 
            if (this.records.Count <= 0)
            {
                InferenceRecord record = new InferenceRecord();
                record.means = values;
                record.covariance = new double[totaldimesion, totaldimesion];
                for (int i = 0; i < totaldimesion; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = 1.0;
                record.weight = 1.0;
                this.records.Add(record);
                activeValue = values.flatten().Item1;
                base.activate(net, time, activeValue);
                return activeValue;
            }

            //计算输入值的归属
            //Calculate which record the input value belongs to
            List<double> probs = this.records.ConvertAll(r => r.prob(values)/r.prob(r.means));
            //double sumprobs = probs.Sum();
            //probs = probs.ConvertAll(p => p / sumprobs);
            int pindex = probs.argmax();
            if(probs.Max()>=Session.GetConfiguration().learning.inference.accept_prob)
            {
                this.records[pindex].acceptCount += 1;
                this.records[pindex].acceptRecords.Add(values);
                if(this.records[pindex].acceptCount >= Session.GetConfiguration().learning.inference.accept_max_count)
                {
                    this.records[pindex].do_adjust(net,this);
                }
                activeValue = this.records[pindex].means.flatten().Item1;
                base.activate(net, time, activeValue);
                return activeValue;

            }

            //计算每个记录的密度值，以及样本的密度值
            //If the new sample is not classified into any records, calculate the density values for each record and for all unclassified samples
            //1.Calculate the Euclidean distance from each record to the new sample and all unclassified samples to the new sample
            //2.Take the ratio of the above distances as the density increment of each record and unclassified sample
            //3.The density of the new sample is equal to that of the nearest record or unclassified sample
            List<List<Vector>> allValues = new List<List<Vector>>();
            this.records.ForEach(r => allValues.Add(r.means));
            allValues.AddRange(this.unclassified_samples);
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
            this.unclassified_samples.Add(values);
            int ti = distances.argmin();
            double td = ti < this.records.Count ? this.records[ti].density : this.density[ti - this.records.Count];
            this.density.Add(td);

            //如果新样本的最大密度接近原有高斯分量中心的最小密度，则启动新样本聚类过程
            //If the maximum density of the unclassified samples is close to the minimum density of the original Gaussian records, the clustering process of the the unclassified samples will be started
            double max_newsample_density = this.density.Max();
            double min_record_density = this.records.ConvertAll(r => r.density).Min();
            int newCount = 0;
            if(max_newsample_density >= min_record_density*2/3)
            {
                (List<List<List<Vector>>> clusters,List<List<double>> des) = do_unclassfied_cluster();
                for(int i=0;i<clusters.Count;i++)
                {
                    
                    create_newrecord_bysamples(clusters[i], des[i]);
                }
                newCount = clusters.Count;
            }
            //如果两个节点的距离太近，则合并节点
            //If two records are too close, merge nodes
            int mergeCount = try_merge_records();
            //重新调整权重
            adjust_weights();

            //输出
            logger.Debug(net.Id.ToString()+".inference" + this.Id.ToString() +"'records are adjusted:count of new="
                + newCount.ToString()
                + ",count of merge="
                + mergeCount.ToString()
                +",count of record="+this.records.Count
                +",accept counts =" + this.records.ConvertAll(r=>r.acceptCount.ToString()).Aggregate((x,y)=>x+","+y));

            activeValue = this.records[this.records.ConvertAll(r => r.prob(values) / r.prob(r.means)).argmax()].means.flatten().Item1;
            base.activate(net, time, activeValue);
            return activeValue;

        }
        /// <summary>
        /// 对未归类样本进行聚类操作
        /// Cluster unclassified samples
        /// </summary>
        /// <returns></returns>
        private (List<List<List<Vector>>>,List<List<double>>) do_unclassfied_cluster()
        {
            logger.Debug("do_unclassfied_cluster....");
            int count = this.unclassified_samples.Count;
            List<List<List<Vector>>> r = new List<List<List<Vector>>>();
            List<List<double>> dens = new List<List<double>>();
            while (this.unclassified_samples.Count > 0)
            {
                //取最大密度点，作为一个新分类
                //Take the maximum density point as a new classification
                int maxindex = this.density.argmax();
                List<Vector> sample = this.unclassified_samples[maxindex];
                List<List<Vector>> classes = new List<List<Vector>>();
                List<double> den = new List<double>();
                classes.Add(sample);
                den.Add(this.density[maxindex]);
                r.Add(classes);
                dens.Add(den);

                //从样本集中移除该点
                //Remove the sample from the unclassified samples
                this.unclassified_samples.RemoveAt(maxindex);
                this.density.RemoveAt(maxindex);
                if (this.unclassified_samples.Count <= 0) break;

                //计算未归类样本集中所有样本与该样本的距离
                //Calculate the distance between all samples in the unclassified sample set and the sample
                List<double> ds = this.unclassified_samples.ConvertAll(s => s.distance(sample));
                
                //对距离从小到大排序
                //sort the distances in ascending order
                List<int> sortedindex = ds.argsort();


                //对排列后的距离寻找最小方差分裂点
                //Finding the minimum variance split point
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
                    classes.Add(this.unclassified_samples[sortedindex[i]]);
                    den.Add(this.density[sortedindex[i]]);
                    this.unclassified_samples.RemoveAt(sortedindex[i]);
                    this.density.RemoveAt(sortedindex[i]);
                }
                

                if (this.unclassified_samples.Count <= 0) break;

            }

            logger.Debug("do_unclassfied_cluster:count = "+count.ToString()
                +",cluster="+r.Count.ToString()+",size of per cluster="+
                r.ConvertAll(e=>e.Count.ToString()).Aggregate((a,b)=>a+","+b));
            return (r,dens);
        }
        /// <summary>
        /// 将vs中的所有样本作为新的高斯分量记录
        /// All samples in vs are recorded as new Gaussian components
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        public InferenceRecord create_newrecord_bysamples(List<List<Vector>> vs,List<double> densitys=null)
        {
            InferenceRecord r = new InferenceRecord();
            r.acceptCount = vs.Count;

            List<int> dimensions = vs.ConvertAll(v => v.Count);
            int totaldimension = dimensions.Sum();
            List<Vector> flatten = vs.ConvertAll(v => v.flatten()).ConvertAll(v => v.Item1);
            r.means = flatten.average().split(dimensions);
            r.covariance = new double[totaldimension, totaldimension];
            r.covariance = Vector.covariance(flatten.ToArray());
            r.acceptCount = vs.Count;
            if(densitys != null || densitys.Count>0) r.density = densitys.Average();
            r.initGaussian();
            return r;

        }
        /// <summary>
        /// 合并靠的太近的记录
        /// Merge too close records
        /// </summary>
        protected int try_merge_records()
        {
            List<(InferenceRecord, InferenceRecord)> needMergeRecordPair = new List<(InferenceRecord, InferenceRecord)>();
            for(int i=0;i<this.records.Count;i++)
            {
                bool merged = false;
                for(int j=i+1;j<this.records.Count;j++)
                {
                    double d1 = this.records[i].prob(this.records[j].means) / this.records[i].prob(this.records[i].means);
                    double d2 = this.records[j].prob(this.records[i].means) / this.records[j].prob(this.records[j].means);
                    if (d1 >= Session.GetConfiguration().learning.inference.accept_prob || d2 >= Session.GetConfiguration().learning.inference.accept_prob)
                    {
                        needMergeRecordPair.Add((this.records[i], this.records[j]));
                        merged = true;
                        this.records.RemoveAt(j);
                        break;
                    }
                }
                if(merged)
                {
                    this.records.RemoveAt(i);i--;
                }
            }
            if (needMergeRecordPair.Count <= 0) return 0;
            for(int i=0;i< needMergeRecordPair.Count;i++)
            {
                InferenceRecord r1 = needMergeRecordPair[i].Item1;
                InferenceRecord r2 = needMergeRecordPair[i].Item2;
                List<List<Vector>> samples = new List<List<Vector>>();
                for (int j = 0; j < r1.acceptCount; j++)
                    samples.Add(r1.means);
                samples.AddRange(r1.acceptRecords);
                for (int j = 0; j < r2.acceptCount; j++)
                    samples.Add(r2.means);
                samples.AddRange(r2.acceptRecords);

                InferenceRecord newRecord = this.create_newrecord_bysamples(samples);
                newRecord.density = (r1.density + r2.density) / 2;
                this.records.Add(newRecord);
            }
            return needMergeRecordPair.Count;
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
