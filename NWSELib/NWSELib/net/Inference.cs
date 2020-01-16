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
        /// 接收数量
        /// </summary>
        public int acceptCount;
        /// <summary>
        /// 评估值（表示按此记录做出行动的结果好坏）
        /// </summary>
        public double evulation;
        /// <summary>
        /// 使用次数
        /// </summary>
        public int usedCount;
        /// <summary>
        /// 表示该前置条件和后置变量映射关系的准确程度
        /// </summary>
        public double accuracyDistance;
        /// <summary>
        /// 抽象之前的夏季记录
        /// </summary>
        public List<InferenceRecord> childs = new List<InferenceRecord>();

        static ILog logger = LogManager.GetLogger(typeof(Inference));
        /// <summary>
        /// 字符串，不需要反向解析，因为记录为后天学习，不遗传
        /// </summary>
        /// <param name="xh"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public string toString(int xh,String prefix="   ")
        {
            StringBuilder str = new StringBuilder();

            str.Append("mean" + xh.ToString() + "=" + Utility.toString(means.flatten().Item1.ToList()) + System.Environment.NewLine +
                       prefix+"evulation=" + evulation.ToString("F3") + System.Environment.NewLine +
                       prefix+"accuracy=" + accuracyDistance.ToString("F3") + System.Environment.NewLine +
                       prefix+"usedCount=" + usedCount.ToString() + System.Environment.NewLine +
                       prefix+"acceptCount=" + acceptCount.ToString() + System.Environment.NewLine);
            return str.ToString();
        }

        

        #endregion

        #region  统计学习功能
        /// <summary>
        /// 高斯采样
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 根据均值和协方差数据生成高斯对象
        /// </summary>
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

        /// <summary>
        /// 高斯分布概率值
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal double prob(List<Vector> values)
        {
            this.initGaussian();
            Microsoft.ML.Probabilistic.Math.Vector v = values.toMathVector();
            return Math.Exp(gaussian.GetLogProb(v));
        }
        /// <summary>
        /// 给定值距均值的马氏距离
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public double mahalanobis_distance(List<Vector> values)
        {
            Microsoft.ML.Probabilistic.Math.Vector value = values.toMathVector();

            Microsoft.ML.Probabilistic.Math.Matrix m = new Microsoft.ML.Probabilistic.Math.Matrix(value.Count, 1);
            m.SetTo(value.ToArray());

            return Math.Sqrt((m.Transpose() * this.gaussian.GetVariance().Inverse() * m)[0,0]);
        }
        /// <summary>
        /// 前向推理
        /// 单个高斯的前向推理有两种方案：
        /// 1.只要条件满足，返回均值
        /// 2.只要条件满足，返回高斯采样中和条件最近的
        /// 目前实现的只是第1种
        /// </summary>
        /// <param name="inf"></param>
        /// <param name="allCondValues"></param>
        /// <returns></returns>
        public List<Vector> forward_inference(Inference inf, List<Vector> allCondValues)
        {
            int[] index = inf.getGene().getVariableIndexs();
            List<Vector> r = new List<Vector>();
            for (int i = 0; i < index.Length; i++)
            {
                r.Add(this.means[index[i]]);
            }
            return r;
        }
        #endregion

        #region 自适应学习
        /// <summary>
        /// 对推理记录的均值和协方差矩阵进行调整
        /// Adjust the mean and covariance matrix of the record of inference node
        /// </summary>
        /// <param name="net"></param>
        /// <param name="inf"></param>
        internal void do_adjust(Network net,Inference inf)
        {
            List<int> dimensions = inf.getDimensionLength(net);
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

        /// <summary>
        /// 给定的条件值是否与本记录均值条件部分匹配（足够近）
        /// </summary>
        /// <param name="net"></param>
        /// <param name="inf"></param>
        /// <param name="condValues"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool isConditionValueMatch(Network net, Inference inf, List<Vector> condValues,out double distance)
        {
            distance = getConditionValueDistance(net,inf,condValues);
            int size = condValues.size();
            return (distance < Session.GetConfiguration().learning.judge.tolerable_similarity);
        }
        /// <summary>
        /// 本记录的均值中条件部分与输入值的曼哈顿距离
        /// </summary>
        /// <param name="net"></param>
        /// <param name="inf"></param>
        /// <param name="condValues"></param>
        /// <returns></returns>
        public double getConditionValueDistance(Network net, Inference inf,List<Vector> condValues)
        {
            (List<Vector> meanCondValues, List<Vector> meanVarValues) = inf.splitRecordMeans2(net, this);
            
            return Vector.max_manhantan_distance(condValues, meanCondValues);
        }
        
        /// <summary>
        /// 根据前后观察验证该记录的准确度
        /// 以time为后置变量获得时间，若前置条件匹配，则检查后置变量是否与实际匹配
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public double adjustAccuracy(Network net,Inference inf,int time)
        {
            //取得inf的真实条件值和后置变量值
            (List<Vector> realCondValues, List<Vector> realVarValues) = inf.getValues2(net,time);
            //取得记录中心点的真实条件值和后置变量值
            (List<Vector> meanCondValues, List<Vector> meanVarValues) = inf.splitRecordMeans2(net, this);
            //条件值的总维度
            int size = realCondValues.size();
            //条件部分是否匹配
            double dis = Vector.max_manhantan_distance(realCondValues, meanCondValues);
            if (dis >= Session.GetConfiguration().learning.judge.tolerable_similarity)
                return this.accuracyDistance;

            this.accuracyDistance = Vector.manhantan_distance(realVarValues, meanVarValues);
            return this.accuracyDistance;
        }

        
        #endregion


    }
    /// <summary>
    /// 推理节点
    /// </summary>
    public class Inference : Node
    {
        #region 成员
        static ILog logger = LogManager.GetLogger(typeof(Inference));
        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        protected List<InferenceRecord> records = new List<InferenceRecord>();
        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        public List<InferenceRecord> Records { get => this.records; }
        /// <summary>
        /// 新样本
        /// </summary>
        public List<List<Vector>> unclassified_samples = new List<List<Vector>>();
        /// <summary>
        /// 新样本的密度值
        /// </summary>
        public List<double> density = new List<double>();

        /// <summary>
        /// 下一级别记录
        /// </summary>
        public Dictionary<int, List<InferenceRecord>> childs = new Dictionary<int, List<InferenceRecord>>();

        #endregion

        #region 初始化
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gene"></param>
        public Inference(NodeGene gene) : base(gene) { }
        
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("推理节点=" + Gene.Text + System.Environment.NewLine);
            str.Append("记录数=" + Records.Count.ToString() + System.Environment.NewLine);
            for (int j = 0; j < Records.Count; j++)
            {
                str.Append(Records[j].toString(j));
                
            }
            return str.ToString();
        }
        /// <summary>
        /// 分级显示
        /// </summary>
        /// <returns></returns>
        public string toCaption()
        {
            StringBuilder str = new StringBuilder();
            str.Append("推理节点=" + Gene.Text + System.Environment.NewLine);
            str.Append("记录数=" + Records.Count.ToString() + System.Environment.NewLine);
            for (int j = 0; j < Records.Count; j++)
            {
                str.Append(Records[j].toString(j));

            }
            return str.ToString();
        }
        #endregion

        #region 信息查询
        /// <summary>
        /// 推理基因
        /// </summary>
        /// <returns></returns>
        public InferenceGene getGene()
        {
            return (InferenceGene)gene;
        }
        /// <summary>
        /// 取得各个维度的长度
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public List<int> getDimensionLength(Network net)
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
        /// 取得所有维度的节点id
        /// </summary>
        /// <returns></returns>
        public List<int> getIdList()
        {
            return this.getGene().dimensions.ConvertAll(d => d.Item1);
        }

        public (List<int>,List<int>) splitCondAndPost()
        {
            List<int> e = new List<int>();
            List<int> v = new List<int>();

            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> ds = this.getGene().dimensions;
            for (int i = 0; i < ds.Count; i++)
            {
                if (ds[i].Item2 == t2) v.Add(ds[i].Item1);
                else e.Add(ds[i].Item1);
            }
            return (e, v);
        }

        /// <summary>
        /// 将各维度的id分解为环境类、动作类、后置变量类
        /// </summary>
        /// <returns></returns>
        public (List<int>, List<int>, List<int>) splitIds()
        {
            List<int> e = new List<int>();
            List<int> a = new List<int>();
            List<int> v = new List<int>();

            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> ds = this.getGene().dimensions;
            for (int i = 0; i < ds.Count; i++)
            {
                if (ds[i].Item2 == t2) v.Add(ds[i].Item1);
                else
                {
                    NodeGene g = this.getGene().owner[ds[i].Item1];
                    if (g.IsActionSensor())
                        a.Add(ds[i].Item1);
                    else
                        e.Add(ds[i].Item1);
                }
            }
            return (e, a, v);
        }

        public override List<Node> getInputNodes(Network net)
        {
            return this.getGene().dimensions.ConvertAll(d=>net.getNode(d.Item1));
        }
        #endregion

        #region 值管理
        /// <summary>
        /// 根据推理节点的维定义（依据时间要求）取得所有输入值
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public List<Vector> getInputValues(Network net,int time)
        {
            return this.getGene().dimensions.ConvertAll(d => net.getNode(d.Item1).GetValue(time - d.Item2));
        }
        /// <summary>
        /// 取得前置条件和后置变量值
        /// </summary>
        /// <param name="net"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public (List<Vector>,List<Vector>) getValues2(Network net, int time)
        {
            List<Vector> c = new List<Vector>();
            List<Vector> v = new List<Vector>();
            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> dimensions = this.getGene().dimensions;
            for(int i=0;i<dimensions.Count;i++)
            {
                if (dimensions[i].Item2 == t2)
                    v.Add(net.getNode(dimensions[i].Item1).GetValue(time- dimensions[i].Item2));
                else
                    c.Add(net.getNode(dimensions[i].Item1).GetValue(time- dimensions[i].Item2));

            }
            return (c, v);
        }

        public (List<Vector>, List<Vector>, List<Vector>) getValues3(Network net, int time)
        {
            List<Vector> ce = new List<Vector>();
            List<Vector> ca = new List<Vector>();
            List<Vector> v = new List<Vector>();
            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> dimensions = this.getGene().dimensions;
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item2 == t2)
                    v.Add(net.getNode(dimensions[i].Item1).GetValue(time - dimensions[i].Item2));
                else
                {
                    Node node = net.getNode(dimensions[i].Item1);
                    if (node.Gene.IsActionSensor())
                        ca.Add(node.GetValue(time- dimensions[i].Item2));
                    else 
                        ce.Add(node.GetValue(time - dimensions[i].Item2));
                }
            }
            return (ce,ca,v);
        }

        

        /// <summary>
        /// 将所有值中的环境部分（非动作部分）用envValues替换
        /// </summary>
        /// <param name="allValue"></param>
        /// <param name="envValues"></param>
        /// <returns></returns>
        internal List<Vector> replaceEnvValue(List<Vector> allValue, List<Vector> envValues)
        {
            (int t1, int t2) = this.getGene().getTimeDiff();
            List<Vector> r = new List<Vector>(allValue);
            for (int i = 0; i < this.getGene().dimensions.Count; i++)
            {
                if (this.getGene().dimensions[i].Item2 != t1) continue;
                NodeGene gene = this.getGene().owner[this.getGene().dimensions[i].Item1];
                if (gene.Group.Contains("action")) continue;
                r[i] = envValues[i];

            }
            return r;
        }


        /// <summary>
        /// 将记录值分解为环境、动作、后置变量三部分
        /// 这里的动作部分总是包含全部动作，即使inf中不需要动作，也会产生所有动作的缺省值
        /// </summary>
        /// <param name="net"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public (List<Vector>, List<Vector>, List<Vector>) splitRecordMeans3(Network net, InferenceRecord record)
        {
            //待返回结果
            List<Vector> env = new List<Vector>();
            List<Vector> actions = new List<Vector>();
            List<Vector> expects = new List<Vector>();

            //动作比较特殊，每个动作都要设置结构，尽管该节点可能不涉及
            int actioncount = net.Effectors.Count;
            for (int i = 0; i < actioncount; i++)
                actions.Add(new Vector(0.5));//0.5表示啥都不做

            //分解record的均值
            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> dimensions = this.getGene().dimensions;
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item2 == t2)
                {
                    expects.Add(record.means[i]);
                }
                else
                {
                    Node node = net.getNode(dimensions[i].Item1);
                    if (node.Gene.Group.Contains("action"))
                    {
                        Effector effector = (Effector)net.Effectors.FirstOrDefault(e => "_" + e.Name == node.Name);
                        int effectorIndex = net.Effectors.IndexOf(effector);
                        actions[effectorIndex] = record.means[i];
                    }
                    else
                    {
                        env.Add(record.means[i]);
                    }
                }
            }
            return (env, actions, expects);
        }

        // <summary>
        /// 将记录值分解为环境、后置变量部分
        /// </summary>
        /// <param name="net"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public (List<Vector>, List<Vector>) splitRecordMeans2(Network net, InferenceRecord record)
        {
            //待返回结果
            List<Vector> env = new List<Vector>();
            List<Vector> expects = new List<Vector>();

            
            //分解record的均值
            (int t1, int t2) = this.getGene().getTimeDiff();
            List<(int, int)> dimensions = this.getGene().dimensions;
            for (int i = 0; i < dimensions.Count; i++)
            {
                if (dimensions[i].Item2 == t2)
                {
                    expects.Add(record.means[i]);
                }
                else
                {
                    Node node = net.getNode(dimensions[i].Item1);
                    env.Add(record.means[i]);
                }
            }
            return (env, expects);
        }

        public List<InferenceRecord> getMatchRecordsInThink(Network net,int time)
        {
            List<(int, int)> conds = this.getGene().getConditions();
            List<Vector> values = conds.ConvertAll(cond =>
                net.getNode(cond.Item1).getThinkValues(time)
            );
            double distance = 0.0;
            return this.records.FindAll(r => r.isConditionValueMatch(net, this, values, out distance));
        }
        #endregion

        #region 激活和自适应调整
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object activate(Network net, int time, Object value = null)
        {
            //所有的输入节点都已经被激活
            //Wether all input nodes have already been activated
            List<(int,int)> conds = ((InferenceGene)this.Gene).getConditions();
            List<(Node, int)> condNodes = conds.ConvertAll(c => (net.getNode(c.Item1), c.Item2));
            if (!condNodes.All(n => n.Item1.IsActivate(time - n.Item2)))
                return null;

            List<(int, int)> vars = this.getGene().getVariables();
            List<(Node, int)> varNodes = vars.ConvertAll(v => (net.getNode(v.Item1), v.Item2));
            if (!varNodes.All(n => n.Item1.IsActivate(time - n.Item2)))
                return null;

            List<Node> inputs = net.getInputNodes(this.Id);

            Vector activeValue = null;
            //确保推理基因的各维度的顺序正确（前置条件在前，后置变量在后，且前置条件id按从小到大排列）
            //Make sure that the dimensions of the inference gene are in the correct order
            ((InferenceGene)this.Gene).sort_dimension();

            //根据基因定义的顺序，将输入值组成List<Vector>
            //Put the input values into the List according to the order of the input dimensions
            List<Vector> values = this.getInputValues(net,time);
            if(values == null)
            {
                base.activate(net, time, null);
                return null;
            }
            int totaldimesion = values.flatten().Item1.Size;


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
                record.acceptCount = 1;
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
            //判断是否需要加入到未归类样本中:如果节点中记录非常少，则尽量增加记录
            if(this.records.Count<=30)
            {
                InferenceRecord record = new InferenceRecord();
                record.means = values;
                record.covariance = new double[totaldimesion, totaldimesion];
                for (int i = 0; i < totaldimesion; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = 1.0;
                record.acceptCount = 1;
                this.records.Add(record);
                activeValue = values.flatten().Item1;
                base.activate(net, time, activeValue);

                adjust_weights();
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

            //未归类样本很少，暂不进行聚类
            if (unclassified_samples.Count <= 10)
            {
                base.activate(net, time, null);
                return values;
            }

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
                    
                    InferenceRecord newRecord = create_newrecord_bysamples(clusters[i], des[i]);
                    newRecord.density = des[i].Average();
                    this.records.Add(newRecord);
                }
                newCount = clusters.Count;
            }
            //如果两个节点的距离太近，则合并节点
            //If two records are too close, merge nodes
            int mergeCount = 0;// try_merge_records();
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

                //如果只剩下最后一个样本，将它独立作为一类
                if (this.unclassified_samples.Count == 1)
                {
                    List<List<Vector>> lastclasses = new List<List<Vector>>();
                    List<double> lastden = new List<double>();
                    lastclasses.Add(this.unclassified_samples[0]);
                    lastden.Add(this.density[0]);
                    r.Add(lastclasses);
                    dens.Add(lastden);
                    break;
                }

                //if (this.unclassified_samples.Count <= 0) break;
                

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
                List<List<Vector>> temps = new List<List<Vector>>();
                List<double> dtemps = new List<double>();
                for(int i=0;i<argminvar;i++)
                {
                    classes.Add(this.unclassified_samples[sortedindex[i]]);
                    den.Add(this.density[sortedindex[i]]);
                }
                for (int i = argminvar; i < sortedindex.Count; i++)
                {
                    temps.Add(this.unclassified_samples[sortedindex[i]]);
                    dtemps.Add(this.density[sortedindex[i]]);
                }
                unclassified_samples = temps;
                density = dtemps;
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

            List<int> dimensions = vs[0].ConvertAll(v => v.Size);
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
            double max = ws.Sum();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i]; 

            /*List<double> ws = this.records.ConvertAll(r => r.gaussian.GetLogProb(r.gaussian.GetMean()));
            double max = ws.Max();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i];*/
        }
        #endregion


        #region 推理

        /// <summary>
        /// 前向推理：给定条件，计算后置变量值
        /// 前向推理有三种方式
        /// 1.将混合高斯模型看作联合模型，进行前向推理
        /// 2.在混合高斯模型上选择距离中心最近的分量，单独对该高斯分量进行前向推理
        /// 3.在混合高斯模型上选择距离中心最近的分量，直接选择该分量的均值的变量部分作为结果
        /// 4.在混合高斯模型上进行采样，在采样中选择距离条件部分最近的，计算后置变量部分作为结果。
        /// 目前用的是第三种
        /// </summary>
        /// <param name="condvalues"></param>
        /// <returns></returns>
        public (InferenceRecord, List<Vector>) forward_inference(Network net,List<Vector> condvalues)
        {
            double distance = 0;
            InferenceRecord matchedRecord = this.records.FirstOrDefault(r => r.isConditionValueMatch(net, this, condvalues,out distance));
            if (matchedRecord == null) return (null, null);
            return (matchedRecord, matchedRecord.forward_inference(this, condvalues));
        }
        /// <summary>
        /// 前向推理：给定条件，计算结论
        /// 在多元高斯模型上进行前向推理
        /// </summary>
        /// <param name="condvalues"></param>
        /// <returns></returns>
        public Vector forward_inference_bysamples(List<Vector> condvalues)
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
        
        /// <summary>
        /// 求变量最大或者最小推理
        /// </summary>
        /// <param name="arg">argmin还是argmax</param>
        /// <returns>
        /// 记忆值（推理的依据）
        /// 变量索引位置
        /// 推理结果：得到的变量值
        /// 所使用的记录索引
        /// </returns>
        public (List<Vector>, int,double,int) arginference(string arg)
        {
            //当前记忆推理节点还没有任何记录
            if (this.records.Count <= 0)
            {
                return (null, -1, double.NaN, -1);
            }
            
            //根据arg选择varId最大或者最小的那个节点
            InferenceRecord argRecord = null;
            int argRecordIndex = -1;
            double value = arg == "argmin" ? double.MaxValue - 1 : double.MinValue + 1;
            int varindex = ((InferenceGene)this.gene).getVariableIndex();

            if(this.records.Count<=0)
            {
                return (null,-1,double.NaN,-1);
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
            argRecordIndex = this.records.IndexOf(argRecord);

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
            return (values, varindex, value, argRecordIndex);
            
        }

        /// <summary>
        /// 在混合高斯模型上采样
        /// </summary>
        /// <param name="inferencesamples"></param>
        /// <returns></returns>
        private List<List<Vector>> samples(int inferencesamples)
        {
            this.records.ForEach(r => r.initGaussian());

            double[] ws = this.records.ConvertAll(r => r.weight).ToArray();

            Discrete zt = new Discrete(ws);
            List<List<Vector>> result = new List<List<Vector>>();
            for (int i = 0; i < inferencesamples; i++)
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

        

        #endregion
    }
}
