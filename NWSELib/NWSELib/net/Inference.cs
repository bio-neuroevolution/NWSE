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
    /// 推理节点
    /// </summary>
    public class Inference : Node
    {
        #region 推断记录
        static ILog logger = LogManager.GetLogger(typeof(Inference));

        

        /// <summary>
        /// 所有推断记录
        /// </summary>
        protected List<InferenceRecord> records = new List<InferenceRecord>();


        /// <summary>
        /// 推理节点存储的记录
        /// </summary>
        public List<InferenceRecord> Records { get => records; }

        
        
        #endregion

        #region 中间过程数据
        /// <summary>
        /// 新样本,尚未归属到任何记录中，因为要积累一些才进行记录融合
        /// </summary>
        public List<List<Vector>> unclassified_samples = new List<List<Vector>>();
        /// <summary>
        /// 新样本的密度值，作为密度聚类的依据，其数量应与unclassified_samples始终一致
        /// </summary>
        public List<double> density = new List<double>();

        #endregion

        #region 初始化
        public readonly List<Node> _conditionNodes;
        public readonly List<Node> _variablesNodes;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="gene"></param>
        public Inference(NodeGene gene,Network net) : base(gene,net) 
        {
            _conditionNodes = this.GetGene().conditions.ConvertAll(id => (Node)net[id]);
            _variablesNodes = this.GetGene().variables.ConvertAll(id => (Node)net[id]);
        }
        public List<Node> ConditionNodes { get => this._conditionNodes; }
        public List<Node> VariableNodes { get => this._variablesNodes; }
        
        public String Summary()
        {
            return this.GetGene().Text + ",r=" + this.Reability.ToString("F4");
        }
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append("推理节点=" + Gene.Text + System.Environment.NewLine);
            str.Append("记录数=" + Records.Count.ToString() + System.Environment.NewLine);
            for (int j = 0; j < Records.Count; j++)
            {
                str.Append(Records[j].ToString(this,j));
                
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
                str.Append(Records[j].ToString(this,j));

            }
            return str.ToString();
        }
        #endregion

        #region 信息查询
        /// <summary>
        /// 推理基因
        /// </summary>
        /// <returns></returns>
        public InferenceGene GetGene()
        {
            return (InferenceGene)gene;
        }

        #endregion

        #region 值管理
        /// <summary>
        /// 根据推理节点的维定义（依据时间要求）取得所有输入值
        /// </summary>
        /// <param name="net"></param>
        /// <returns></returns>
        public List<Vector> GetInputValues(int time)
        {
            int timediff = this.GetGene().timediff;
            List<Vector> r1 = this.GetGene().conditions.ConvertAll(d => net[d].GetValue(time - timediff));
            List<Vector> r2 = this.GetGene().variables.ConvertAll(d => net[d].GetValue(time));
            r1.AddRange(r2);
            return r1;
            
        }

        /// <summary>
        /// 取得给定值的文本显示信息
        /// </summary>
        /// <param name="value">为空，则取当前最新值</param>
        /// <returns></returns>
        public override String GetValueText(Vector value = null)
        {
            if (value == null) value = Value;
            if (value == null) return "";
            List<Receptor> receptors = this.GetGene().getLeafGenes().ConvertAll(g=>(Receptor)net[g.Id]);
            int condCount = this.GetGene().ConditionCount;
            int varCount = this.GetGene().VariableCount;

            StringBuilder str = new StringBuilder();
            for (int i = 0; i <this.ConditionNodes.Count;i++)
            {
                if (i > 0) str.Append(",");
                if (ConditionNodes[i] is Receptor)
                    str.Append(ConditionNodes[i].GetValueText(new Vector(value[i])));
                else
                    str.Append(value[i].ToString("F4"));

            }
            str.Append("=>");

            for(int i=0;i<VariableNodes.Count;i++)
            {
                if (i > 0) str.Append(",");
                if (VariableNodes[i] is Receptor)
                    str.Append(VariableNodes[i].GetValueText(new Vector(value[i+ ConditionNodes.Count])));
                else
                    str.Append(value[i + ConditionNodes.Count].ToString("F4"));
            }

            
            return str.ToString();
        }

        

        

        public InferenceRecord GetEqualsRecord(List<Vector> values)
        {
            Vector v1 = values.flatten().Item1;
            List<InferenceRecord> records = this.records;
            foreach (InferenceRecord r in records)
            {
                Vector v2 = r.means.flatten().Item1;
                if (Vector.equals(v1, v2,Session.config.realerror)) return r;
            }
            return null;
        }

        
        public (InferenceRecord, List<double>) GetMatchRecord(List<Vector> condvalues)
        {
            List<(InferenceRecord, List<double>)> result = this.GetMatchRecords(condvalues);
            if (result == null || result.Count <= 0) return (null, null);
            double d = double.MaxValue;
            (InferenceRecord, List<double>) r = (null,null);
            foreach(var temp in result)
            {
                if(temp.Item2.Average()<d)
                {
                    d = temp.Item2.Average();
                    r = temp;
                }
            }
            r.Item1.usedCount += 1;
            return r;
        }
        public List<(InferenceRecord,List<double>)> GetMatchRecords(List<Vector> condvalues)
        {
            List<(InferenceRecord, List<double>)> result = new List<(InferenceRecord, List<double>)>();

            List<InferenceRecord> records = UsedRecords;
            foreach (InferenceRecord r in records)
            {
                List<double> dis = null;
                if(r.IsConditionMatch(condvalues, out dis))
                {
                    result.Add((r,dis));
                }
            }
            return result;
        }

        public (InferenceRecord, double) getNearestRecord(List<Vector> values)
        {
            InferenceRecord record = null;
            double mindis = double.MaxValue;

            List<InferenceRecord> records = UsedRecords;
            foreach (InferenceRecord r in records)
            {
                double dis = r.DistanceFromCondition(values).Average();
                if(dis < mindis)
                {
                    mindis = dis;
                    record = r;
                }
            }
            record.usedCount += 1;
            return (record, mindis);
        }

        public List<double> DistanceFromCondition(List<Vector> condValue1, List<Vector> condValue2)
        {
            return  net.GetReceptorDistance(this.LeafReceptors, condValue1.flatten().Item1, condValue2.flatten().Item1);
        }

        public (List<Vector> CondValue, List<Vector> VarValue) SplitValues2(List<Vector> values)
        {
            List<Vector> condValues = new List<Vector>();
            List<Vector> varValues = new List<Vector>();

            int condcount = this.GetGene().ConditionCount;
            int varcount = this.GetGene().VariableCount;
            condValues.AddRange(values.GetRange(0, condcount));
            varValues.AddRange(values.GetRange(condcount, varcount));
            return (condValues, varValues);
        }
        #endregion

        #region 激活和自适应调整
        /*
        public override Object Activate(Network net, int time, Object value = null)
        {
            //所有的输入节点都已经被激活
            //Wether all input nodes have already been activated
            List<int> conds = ((InferenceGene)this.Gene).conditions;
            List<Node> condNodes = conds.ConvertAll(c => net[c]);
            if (!condNodes.All(n => n.IsActivate(time - this.GetGene().timediff)))
                return null;

            List<int> vars = this.GetGene().variables;
            List<Node> varNodes = vars.ConvertAll(v => (net[v]));
            if (!varNodes.All(n => n.IsActivate(time)))
                return null;




            //取得值
            List<Node> inputs = net.getInputNodes(this.Id);
            List<Vector> values = this.GetInputValues(time);
            if (values == null)
            {
                base.Activate(net, time, null);
                return null;
            }

            int d = values.flatten().Item1.Size;
            InferenceRecord record = this.GetEqualsRecord(values);
            if (record == null)
            {
                record = new InferenceRecord(this);
                record.means = values.ConvertAll(v=>v.clone());
                record.covariance = new double[d, d];
                for (int i = 0; i < d; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = InferenceRecord.variance;
                this.records.Add(record);
            }
            record.acceptCount += 1;
            this.checkRecord(record);
            base.Activate(net, time, values.flatten().Item1);
            return values;
        }*/

        
        /// <summary>
        /// 设置当前值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public override Object Activate(Network net, int time, Object value = null)
        {
            //所有的输入节点都已经被激活
            //Wether all input nodes have already been activated
            if (!ConditionNodes.All(n => n.IsActivate(time - this.GetGene().timediff)))
                return null;
            if (!VariableNodes.All(n => n.IsActivate(time)))
                return null;

            //根据基因定义的顺序，将输入值组成List<Vector>
            //Put the input values into the List according to the order of the input dimensions
            Vector activeValue = null;
            List<Node> inputs = net.getInputNodes(this.Id); 
            List<Vector> values = this.GetInputValues(time);
            if(values == null)
            {
                base.Activate(net, time, null);
                return null;
            }
            int totaldimesion = values.flatten().Item1.Size;


            //如果没有任何节点记录，则生成第一个
            //Create a new record if there are no nodes in current inference node, 
            if (this.records.Count <= 0)
            {
                InferenceRecord record = new InferenceRecord(this);
                record.means = values;
                record.covariance = new double[totaldimesion, totaldimesion];
                for (int i = 0; i < totaldimesion; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = InferenceRecord.variance;
                record.weight = 1.0;
                record.acceptCount = 1;
                //var nearestRecord = this.getMatchRecord(record.getMeanValues().condValues);
                //if (nearestRecord.Item1 != null) record.evulation = nearestRecord.Item1.evulation;
                this.records.Add(record);
                this.checkRecord(record);
                
                activeValue = values.flatten().Item1;
                base.Activate(net, time, activeValue);
               
                return activeValue;
            }

            //计算输入值的归属
            //Calculate which record the input value belongs to
            List<double> probs = this.records.ConvertAll(r => r.Prob(values)/r.Prob(r.means));
            int pindex = probs.argmax();
            if(pindex!=-1 && probs.Max()>=Session.GetConfiguration().learning.inference.accept_prob)
            {
                this.records[pindex].acceptCount += 1;
                this.records[pindex].acceptRecords.Add(values);
                if(this.records[pindex].acceptCount >= Session.GetConfiguration().learning.inference.accept_max_count)
                {
                    this.records[pindex].DoAdjust(net);
                    this.checkRecord(this.records[pindex],true);
                }
                activeValue = this.records[pindex].means.flatten().Item1;
                base.Activate(net, time, activeValue);
                return activeValue;
            }
            //判断是否需要加入到未归类样本中:如果节点中记录非常少，则尽量增加记录
            if(this.records.Count<=50)
            {
                InferenceRecord record = new InferenceRecord(this);
                record.means = values;
                record.covariance = new double[totaldimesion, totaldimesion];
                for (int i = 0; i < totaldimesion; i++) //缺省协方差矩阵为单位阵
                    record.covariance[i, i] = InferenceRecord.variance;
                record.acceptCount = 1;
                //var nearestRecord = this.getMatchRecord(record.getMeanValues().condValues);
                //if (nearestRecord.Item1 != null) record.evulation = nearestRecord.Item1.evulation;
                this.records.Add(record);
                this.checkRecord(record);
                activeValue = values.flatten().Item1;
                base.Activate(net, time, activeValue);

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
                base.Activate(net, time, null);
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
                    //var nearestRecord = this.getMatchRecord(newRecord.getMeanValues().condValues);
                    //if (nearestRecord.Item1 != null) newRecord.evulation = nearestRecord.Item1.evulation;
                    this.records.Add(newRecord);
                    this.checkRecord(newRecord);
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

            activeValue = this.records[this.records.ConvertAll(r => r.Prob(values) / r.Prob(r.means)).argmax()].means.flatten().Item1;
            base.Activate(net, time, activeValue);
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
        private InferenceRecord create_newrecord_bysamples(List<List<Vector>> vs,List<double> densitys=null)
        {
            InferenceRecord r = new InferenceRecord(this);
            r.acceptCount = vs.Count;

            List<int> dimensions = vs[0].ConvertAll(v => v.Size);
            int totaldimension = dimensions.Sum();
            List<Vector> flatten = vs.ConvertAll(v => v.flatten()).ConvertAll(v => v.Item1);
            r.means = flatten.average().split(dimensions);
            r.covariance = new double[totaldimension, totaldimension];
            r.covariance = Vector.covariance(flatten.ToArray());
            r.acceptCount = vs.Count;
            if(densitys != null || densitys.Count>0) r.density = densitys.Average();
            r.InitGaussian();
            return r;

        }
        
        
        
        /// <summary>
        /// 调整权重
        /// </summary>
        private void adjust_weights()
        {
           //根据接收样本数量设定权重
            List<double> ws = this.records.ConvertAll(r => (double)r.acceptCount);
            double max = ws.Sum();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i]; 

            /*
             * 根据最大高斯值峰度确定权重
             * List<double> ws = this.records.ConvertAll(r => r.gaussian.GetLogProb(r.gaussian.GetMean()));
            double max = ws.Max();
            ws = ws.ConvertAll(w => w / max);
            for (int i = 0; i < this.records.Count; i++)
                this.records[i].weight = ws[i];*/
        }


        #endregion

        #region 有效性(可靠性)判定
        
        /// <summary>
        /// 可靠度
        /// </summary>
        public override double Reability
        {
            get
            {
                return this.Gene.reability;
               // return this.Gene.reability = 1.0 * this.validItems.Count / this.records.Count;
                //return this.Gene.reability = 1.0 * this.validItems.Count / (this.validItems.Count + this.invalidItems.Count);

            }
        }

        public const String Unconfirmed = "unconfirmed";
        public const String Valid = "valid";
        public const String Invalid = "invalid";

        protected List<InferenceRecord> unconfirmedItems = new List<InferenceRecord>();
        protected List<InferenceRecord> validItems = new List<InferenceRecord>();
        protected List<InferenceRecord> invalidItems = new List<InferenceRecord>();

        public List<InferenceRecord> ValidRecords
        {
            get { return this.records; }
        }
        public int ValidRecordCount
        {
            get => this.validItems.Count;
        }


        public List<InferenceRecord> UsedRecords
        {
            get
            {
                return this.records;
            }
        }

        public double CheckReability()
        {
            
            List<MeasureTools> condtools = this.ConditionNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
            List<MeasureTools> vartools = this.VariableNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));

            List<InferenceRecord> validRecords = new List<InferenceRecord>();
            for (int i = 0; i < records.Count; i++)
            {
                InferenceRecord r1 = this.records[i];
               // if (r1.usedCount <= 0) continue;
                var r1MeanValues = r1.GetMeanValues();


                bool valid = true;
                for (int j = i + 1; j < records.Count; j++)
                {
                    InferenceRecord r2 = this.records[j];
                   // if (r2.usedCount <= 0) continue;
                    var r2MeanValues = r2.GetMeanValues();

                    if (!net.IsTolerateDistance(condtools, r1MeanValues.condValues.flatten().Item1, r2MeanValues.condValues.flatten().Item1))
                        continue;
                    if (!net.IsTolerateDistance(vartools, r1MeanValues.varValues.flatten().Item1, r2MeanValues.varValues.flatten().Item1))
                        valid = false;
                }
                if (!valid) continue;
                validRecords.Add(records[i]);
            }
            if (validRecords.Count <= 0) return this.Gene.reability = 0;


            List<double> distances = new List<double>();
            for (int i=0;i< validRecords.Count;i++)
            {
                InferenceRecord r1 = validRecords[i];
                var r1MeanValues = r1.GetMeanValues();
                for (int j=i+1;j< validRecords.Count;j++)
                {
                   
                    InferenceRecord r2 = validRecords[j];
                    var r2MeanValues = r2.GetMeanValues();
                    //if(!Vector.equals(r1MeanValues.condValues.flatten().Item1, r2MeanValues.condValues.flatten().Item1,0.01))
                    if (!net.IsTolerateDistance(condtools, r1MeanValues.condValues.flatten().Item1, r2MeanValues.condValues.flatten().Item1))
                        continue;
                    double dis = 0.0;
                    for(int k=0;k<r1MeanValues.varValues.Count;k++)
                    {
                        dis += vartools[k].distance(r1MeanValues.varValues[k][0], r2MeanValues.varValues[k][0]);
                    }
                    distances.Add(dis);

                }
            }
            if(distances.Count<=0) return this.Gene.reability = 0;
            double min = distances.Min();
            double max = distances.Max();
            double u = max - min;
            distances = distances.ConvertAll(d => (max-d) / u);
            return this.Gene.reability = distances.Average();
        }
        
        private String checkRecord(InferenceRecord record,bool removed=false,int validOccurCount=2)
        {
            return "";
            if (record == null) return "";

            if(record.acceptCount >= validOccurCount)
            {
                validItems.Add(record);
                return Valid;
            }
            
            double error = Session.GetConfiguration().realerror;
            (List<Vector> condValues, List<Vector> varValues) = record.GetMeanValues();

            for(int i=0;i<validItems.Count;i++)
            {
                if (validItems[i] == record) continue;
                var meanValues = validItems[i].GetMeanValues();
                List<MeasureTools> tools = this.ConditionNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
                if (!net.IsTolerateDistance(tools, condValues.flatten().Item1, meanValues.condValues.flatten().Item1))
                    continue;

                tools = this.VariableNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
                if (net.IsTolerateDistance(tools, varValues.flatten().Item1, meanValues.varValues.flatten().Item1))
                {
                    if (!this.validItems.Contains(record))
                        this.validItems.Add(record);
                    return Valid;
                }
                else
                {
                    invalidItems.Add(validItems[i]);
                    invalidItems.Add(record);
                    validItems.RemoveAt(i);
                    return Invalid;
                }
            }

            for (int i = 0; i < this.invalidItems.Count; i++)
            {
                if (invalidItems[i] == record) continue;
                List<MeasureTools> tools = this.ConditionNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
                var meanValues = invalidItems[i].GetMeanValues();
                if (!net.IsTolerateDistance(tools, condValues.flatten().Item1, meanValues.condValues.flatten().Item1))
                    continue;
                invalidItems.Add(record);
                return Invalid;
            }



            for (int i=0;i< unconfirmedItems.Count;i++)
            {
                if (unconfirmedItems[i] == record) continue;

                var meanValues = unconfirmedItems[i].GetMeanValues();

                List<MeasureTools> tools = this.ConditionNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
                if (!net.IsTolerateDistance(tools, condValues.flatten().Item1, meanValues.condValues.flatten().Item1))
                    continue;

                //if (!Vector.equals(condValues, meanValues.condValues, error))
                //    continue;

                tools = this.VariableNodes.ConvertAll(node => MeasureTools.GetMeasure(node.Gene.Cataory));
                if(net.IsTolerateDistance(tools, varValues.flatten().Item1, meanValues.varValues.flatten().Item1))
                //if (Vector.equals(varValues, meanValues.varValues, error))
                {
                    validItems.Add(unconfirmedItems[i]);
                    validItems.Add(record);
                    this.unconfirmedItems.RemoveAt(i);
                    return Valid;
                }
                else
                {
                    invalidItems.Add(unconfirmedItems[i]);
                    invalidItems.Add(record);
                    this.unconfirmedItems.RemoveAt(i);
                    return Invalid;
                }

            }

            

            this.unconfirmedItems.Add(record);
            return Unconfirmed;
        }
        
        #endregion


        #region 推理

        /// <summary>
        /// 前向推理
        /// </summary>
        /// <param name="condvalues">推理条件值</param>
        /// <param name="inferenceMethod">推理方法：samples,record</param>
        /// <returns></returns>
        public (InferenceRecord record, List<Vector> postValues) forward_inference(List<Vector> condvalues,String inferenceMethod= "recordsample")
        {
            if (inferenceMethod == "recordsample")
                return this.forward_inference_ByRecordSample(condvalues);
            else if (inferenceMethod == "sample")
                return this.forward_inference_BySample(condvalues);
            else
                return this.forward_inference_ByRecord(condvalues);
        }

        public (InferenceRecord, List<Vector>) forward_inference_ByRecord(List<Vector> condvalues)
        {
            //(InferenceRecord record,double distance)= this.getNearestRecord(condvalues);
            (InferenceRecord record, List<double> distances) = this.GetMatchRecord(condvalues);
            if (record == null) return (null, null);
            return (record, record.GetMeanValues().varValues);
        }

        public (InferenceRecord, List<Vector>) forward_inference_ByRecordSample(List<Vector> condvalues)
        {
            List<InferenceRecord> records = UsedRecords;
            if (records.Count <= 0) return (null, null);

            (InferenceRecord record, double distance) = this.getNearestRecord(condvalues);

            if (record == null) return (null, null);

            List<List<Vector>> s = record.DoSamples(5);

            double dis = double.MaxValue;
            List<Vector> varValue = null;

            for(int i =0;i<s.Count;i++)
            {
                List<Vector> sample = s[i];
                (List<Vector> sampleCondValue,List<Vector> sampleVarValue) = SplitValues2(sample);
                double d = DistanceFromCondition(sampleCondValue, condvalues).Average();
                if(d < dis)
                {
                    dis = d;
                    varValue = sampleVarValue;
                }
            }

            if (dis < distance) return (record, varValue);
            else return (record, record.GetMeanValues().varValues);

        }

        public (InferenceRecord, List<Vector>) forward_inference_BySample(List<Vector> condvalues)
        {
            List<InferenceRecord> records = UsedRecords;
            if (records.Count <= 0) return (null, null);

            List<List<Vector>> s = this.samples(5);

            double dis = double.MaxValue;
            List<Vector> varValue = null;

            foreach (List<Vector> sample in s)
            {
                (List<Vector> sampleCondValue, List<Vector> sampleVarValue) = this.SplitValues2(sample);
                double d = this.DistanceFromCondition(sampleCondValue, condvalues).Average();
                if (d < dis)
                {
                    dis = d;
                    varValue = sampleVarValue;
                }
            }

            (InferenceRecord record, double distance) = this.getNearestRecord(condvalues);

            if (dis < distance) return (record, varValue);
            else return (record, record.GetMeanValues().varValues);

        }

        /// <summary>
        /// 在混合高斯模型上采样
        /// </summary>
        /// <param name="inferencesamples"></param>
        /// <returns></returns>
        private List<List<Vector>> samples(int inferencesamples)
        {
            List<InferenceRecord> records = UsedRecords;
            records.ForEach(r => r.InitGaussian());

            double[] ws = this.records.ConvertAll(r => r.weight).ToArray();

            Discrete zt = new Discrete(ws);
            List<List<Vector>> result = new List<List<Vector>>();
            for (int i = 0; i < inferencesamples; i++)
            {
                int index = zt.Sample();
                result.Add(this.records[index].DoSamples(1)[0]);
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

        public String doPostVariableStat()
        {
            Dictionary<String, List<double>> r1 = new Dictionary<String, List<double>>();
            int total = 0;

            List<InferenceRecord> records = UsedRecords;
            foreach (InferenceRecord record in records)
            {
                Vector v = record.GetMeanValues().varValues[0];
                String strv = v[0].ToString("F4");
                if (!r1.ContainsKey(v[0].ToString("F4")))
                {
                    r1.Add(strv, new List<double>());
                    
                }
                r1[strv].Add(record.evulation );

            }

            StringBuilder str = new StringBuilder();
            foreach(KeyValuePair<String,List<double>> t in r1)
            {
                if(str.ToString() != "")
                {
                    str.Append(System.Environment.NewLine);
                }
                str.Append(t.Key + "=" + t.Value.Average().ToString("F4"));
            }
            return str.ToString();

        }

        #endregion
    }
}
