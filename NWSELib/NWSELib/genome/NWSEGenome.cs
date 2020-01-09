using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using NWSELib.common;

namespace NWSELib.genome
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class NWSEGenome
    {
        #region 染色体组成
        /// <summary>
        /// 染色体Id
        /// </summary>
        public int id;
        /// <summary>
        /// 感受器基因
        /// </summary>
        public readonly List<ReceptorGene> receptorGenes = new List<ReceptorGene>();

        /// <summary>
        /// 不同处理器的选择概率
        /// </summary>
        public readonly List<double> handlerSelectionProb = new List<double>();
        /// <summary>
        /// 处理器基因
        /// </summary>
        public readonly List<HandlerGene> handlerGenes = new List<HandlerGene>();

        
        /// <summary>
        /// 推理器基因
        /// </summary>
        public readonly List<InferenceGene> infrernceGenes = new List<InferenceGene>();

        
        /// <summary>
        /// 评判基因
        /// </summary>
        public readonly List<JudgeGene> judgeGenes = new List<JudgeGene>();


        /// <summary>
        /// 无效推理基因
        /// </summary>
        public List<NodeGene> invaildInferenceNodes = new List<NodeGene>();

        /// <summary>
        /// 有效推理基因
        /// </summary>
        public List<NodeGene> vaildInferenceNodes = new List<NodeGene>();

        static ILog log = LogManager.GetLogger(typeof(NWSEGenome));
        /// <summary>
        /// 构造函数
        /// </summary>
        public NWSEGenome()
        {
          
        }
        #endregion

        #region 克隆、编码和等价性
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public NWSEGenome clone()
        {
            NWSEGenome genome = new NWSEGenome();
            receptorGenes.ForEach(r => genome.receptorGenes.Add(r.clone<ReceptorGene>()));
            genome.handlerSelectionProb.AddRange(handlerSelectionProb);
            handlerGenes.ForEach(h => genome.handlerGenes.Add(h.clone<HandlerGene>()));
            infrernceGenes.ForEach(i => genome.infrernceGenes.Add(i.clone<InferenceGene>()));
            judgeGenes.ForEach(j => genome.judgeGenes.Add(j.clone()));
            invaildInferenceNodes.ForEach(inf => genome.invaildInferenceNodes.Add(inf.clone<NodeGene>()));
            vaildInferenceNodes.ForEach(vf => genome.vaildInferenceNodes.Add(vf.clone<NodeGene>()));

            return genome;
        }

        /// <summary>
        /// 判断两个染色体是否等价：如果处理器基因和推理基因完全一样的话（不包括可变异参数在内）
        /// </summary>
        /// <param name="genome"></param>
        /// <returns></returns>
        public bool equiv(NWSEGenome genome)
        {
            if (handlerGenes.Count != genome.handlerGenes.Count)
                return false;

            for (int i = 0; i < handlerGenes.Count; i++)
            {
                for (int j = 0; j < genome.handlerGenes.Count; j++)
                {
                    if (!handlerGenes[i].equiv(genome.handlerGenes[j])) return false;
                }
            }

            if (genome.handlerGenes.Count != handlerGenes.Count)
                return false;
            for (int i = 0; i < genome.handlerGenes.Count; i++)
            {
                if (handlerGenes.Exists(g => g.Id == genome.handlerGenes[i].Id))
                    continue;
                for (int j = 0; j < handlerGenes.Count; j++)
                {
                    if (!genome.handlerGenes[i].equiv(handlerGenes[j])) return false;
                }
            }

            if (infrernceGenes.Count != genome.infrernceGenes.Count)
                return false;
            for (int i = 0; i < infrernceGenes.Count; i++)
            {
                for (int j = 0; j < genome.infrernceGenes.Count; j++)
                {
                    if (!infrernceGenes[i].equiv(genome.infrernceGenes[j])) return false;
                }
            }
            
            return true;
        }

        public bool exist(NodeGene g)
        {
            if(g.GetType() == typeof(HandlerGene))
            {
                return this.handlerGenes.Exists(h => this.equiv(h, g));
            }else if(g.GetType() == typeof(InferenceGene))
            {
                return this.infrernceGenes.Exists(i => this.equiv(i, g));
            }
            return false;
        }

        
        /// <summary>
        /// 是否等价
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public bool equiv(NodeGene g1, NodeGene g2)
        {
            return g1.Text == g2.Text;
        }
        
        

        #endregion

        #region 查询
        /// <summary>
        /// 根据Id查找
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NodeGene this[int id]
        {
            get
            {
                NodeGene gene = receptorGenes.FirstOrDefault(g => g.Id == id);
                if (gene != null) return gene;
                gene = handlerGenes.FirstOrDefault(g => g.Id == id);
                if (gene != null) return gene;
                gene = infrernceGenes.FirstOrDefault(g => g.Id == id);
                return gene;
            }
        }
        public NodeGene this[String name]
        {
            get
            {
                NodeGene gene = receptorGenes.FirstOrDefault(g => g.Name == name);
                if (gene != null) return gene;
                gene = handlerGenes.FirstOrDefault(g => g.Name == name);
                if (gene != null) return gene;
                gene = infrernceGenes.FirstOrDefault(g => g.Name == name);
                return gene;
            }
        }
        /// <summary>
        /// 查找某节点的传入节点
        /// </summary>
        /// <param name="gene"></param>
        /// <returns></returns>
        public List<NodeGene> getInputs(NodeGene gene)
        {
            if (gene is ReceptorGene) return new List<NodeGene>();
            if (gene is HandlerGene) return ((HandlerGene)gene).inputs.ConvertAll(g => this[g]);
            if (gene is InferenceGene) return ((InferenceGene)gene).dimensions.ConvertAll(x => this[x.Item1]);
            if(gene is JudgeGene)return new List<NodeGene>();
            return new List<NodeGene>();
        }

        public List<ReceptorGene> getEnvSensorGenes()
        {
            return this.receptorGenes.FindAll(r => r.Group.StartsWith("env"));
        }
        public List<ReceptorGene> getNonActionSensorGenes()
        {
            return this.receptorGenes.FindAll(r => r.Group.StartsWith("env") || r.Group.StartsWith("body"));
        }

        public int getHandlerMaxDepth()
        {
            if(this.handlerGenes.Count<=0)
            {
                return this.receptorGenes.ConvertAll(r => r.Depth).Max();
            }
            return this.handlerGenes.ConvertAll(h => h.Depth).Max();
        }
        public int getInferenceMaxDepth()
        {
            int d1 = this.infrernceGenes.ConvertAll(i => i.Depth).Max();
            int d2 = this.getHandlerMaxDepth();
            return System.Math.Max(d1,d2);

        }
        public int computeNodeDepth(NodeGene gene)
        {
            List<NodeGene> inputs = this.getInputs(gene);
            if(inputs == null || inputs.Count <= 0)
            {
                gene.Depth = 0;return 0;
            }
            gene.Depth = inputs.ConvertAll(i => i.Depth).Max() + 1;
            return gene.Depth;
        }
        public void computeNodeDepth()
        {
            this.receptorGenes.ForEach(r => r.Depth = 0);
            this.handlerGenes.ForEach(h => this.computeNodeDepth(h));
            int d = this.getHandlerMaxDepth();
            this.infrernceGenes.ForEach(i =>
            {
                int t = this.computeNodeDepth(i);
                if (t < d + 1) i.Depth = d + 1;
            });
            d = this.infrernceGenes.ConvertAll(i => i.Depth).Max();
            this.judgeGenes.ForEach(j=>j.Depth = d + 1);
        }
        
        #endregion


        #region 漂移和变异
        /// <summary>
        /// 基因漂移处理
        /// </summary>
        /// <param name="invaildInferenceNodes"></param>
        /// <param name="vaildInferenceNodes"></param>
        public void gene_drift(List<NodeGene> invaildInferenceNodes, List<NodeGene> vaildInferenceNodes)
        {
            if (invaildInferenceNodes != null)
            {
                for (int i = 0; i < invaildInferenceNodes.Count; i++)
                {
                    NodeGene g1 = invaildInferenceNodes[i];
                    bool exist = false;
                    for (int j = 0; j < this.invaildInferenceNodes.Count; j++)
                    {
                        NodeGene g2 = this.invaildInferenceNodes[j];
                        if (this.equiv(g1, g2)) { exist = true; break; }
                    }
                    if (!exist) this.invaildInferenceNodes.Add(g1);
                }
            }
            if(vaildInferenceNodes != null)
            {
                for(int i=0;i< vaildInferenceNodes.Count;i++)
                {
                    for (int j = 0; j < this.vaildInferenceNodes.Count; j++)
                    {
                        if (this.equiv(vaildInferenceNodes[i], this.vaildInferenceNodes[j]))
                            continue;
                        this.vaildInferenceNodes.Add(vaildInferenceNodes[i]);
                    }
                }
            }
        }

        public bool isVaildGene(NodeGene gene)
        {
            return this.vaildInferenceNodes.Exists(n => isVaildGene(n, gene) == 1);
        }
        private int isVaildGene(NodeGene p,NodeGene gene)
        {
            if (p.Id == gene.Id) return 1;
            List<NodeGene> inputs = this.getInputs(p);
            if (inputs == null || inputs.Count <= 0) return 0;
            for(int i=0;i<inputs.Count;i++)
            {
                int r = isVaildGene(inputs[i],gene);
                if (r == 1) break;
            }
            return 0;
        }

        public static NWSEGenome create(Session session)
        {
            NWSEGenome genome = new NWSEGenome();
            //生成感受器
            List<Configuration.Sensor> sensors = Session.GetConfiguration().agent.receptors.GetAllSensor();
            for(int i=0;i< sensors.Count;i++)
            {
                ReceptorGene receptorGene = new ReceptorGene(genome);
                receptorGene.Cataory = sensors[i].cataory;
                receptorGene.Generation = session.Generation;
                receptorGene.Group = sensors[i].group;
                receptorGene.Name = sensors[i].name;
                receptorGene.Id = session.GetIdGenerator().getGeneId(receptorGene);
                genome.receptorGenes.Add(receptorGene);
            }
            //生成感受器的分段数
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                if (genome.isVaildGene(genome.receptorGenes[i])) continue;
                Configuration.Sensor sensor = Session.GetConfiguration().agent.receptors.GetSensor(genome.receptorGenes[i].Name);
                int min1 = (int)sensor.Level.Min;
                int max1 = (int)sensor.Level.Max;
                if (new Random().NextDouble() <= 0.5)
                    genome.receptorGenes[i].SectionCount = new Random().Next(min1, max1 + 1);
            }

            //生成缺省推理节点
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                if (genome.receptorGenes[i].Group.StartsWith("action")) continue;

                InferenceGene inferenceGene = new InferenceGene(genome);
                inferenceGene.Generation = session.Generation;
                inferenceGene.dimensions = new List<(int, int)>();
                for (int j = 0; j < genome.receptorGenes.Count; j++)
                {
                    inferenceGene.dimensions.Add((genome.receptorGenes[j].Id, 1));
                    
                }
                inferenceGene.dimensions.Add((genome.receptorGenes[i].Id, 0));
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                genome.infrernceGenes.Add(inferenceGene);


            }

            

            //生成判定基因
            
            JudgeGene judgeItem = new JudgeGene(genome);
            judgeItem.conditions.Add(genome["_a2"].Id);
            judgeItem.variable = genome["d3"].Id;
            judgeItem.expression = JudgeGene.ARGMAX;
            judgeItem.weight = 1.0;
            judgeItem.Generation = session.Generation;
            judgeItem.Id = Session.idGenerator.getGeneId(judgeItem);
            genome.judgeGenes.Add(judgeItem);

            genome.id = Session.idGenerator.getGenomeId();
            genome.computeNodeDepth();
            return genome;
        }
        /// <summary>
        /// 变异
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public NWSEGenome mutate(Session session)
        {
            NWSEGenome genome = this.clone();
            Random rng = new Random();
            //对感受器的分段数进行变异
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                if (this.isVaildGene(genome.receptorGenes[i])) continue;
                Configuration.Sensor sensor = Session.GetConfiguration().agent.receptors.GetSensor(genome.receptorGenes[i].Name);
                int min1 = (int)sensor.Level.Min;
                int max1 = (int)sensor.Level.Max;
                if(rng.NextDouble() <= 0.5)
                    genome.receptorGenes[i].SectionCount = new Random().Next(min1,max1+1);
            }

            //选择一个处理器对参数进行变异
            for(int i=0;i< genome.handlerGenes.Count;i++)
            {
                if (this.isVaildGene(genome.handlerGenes[i])) continue;
                genome.handlerGenes[i].mutate();
            }

            //对处理器的选择概率变异
            double[] handler_selection_prob = Session.GetConfiguration().evolution.mutate.Handlerprob.ToArray();
            /*int handler_index = new Random().Next(0,handler_selection_prob.Length);
            double min = Session.GetConfiguration().handlers[handler_index].Selection_prob_range.Min;
            double max = Session.GetConfiguration().handlers[handler_index].Selection_prob_range.Max;
            handler_selection_prob[handler_index] = rng.NextDouble()*(max-min)+min;
            int handler_index2 = handler_index;
            while(handler_index2 == handler_index)
                handler_index2 = rng.Next(0, handler_selection_prob.Length);
            handler_selection_prob[handler_index2] = 0.0;
            handler_selection_prob[handler_index2] = 1.0 - handler_selection_prob.ToList().Sum();
            genome.handlerSelectionProb.Clear();
            genome.handlerSelectionProb.AddRange(handler_selection_prob);
            */

            //添加一个处理器
            Configuration.Handler cHandler = Session.GetConfiguration().random_handler(handler_selection_prob);
            HandlerGene newGene = null;
            List<NodeGene> inputs = new List<NodeGene>();
            inputs.AddRange(receptorGenes);
            inputs.AddRange(handlerGenes);
            bool newhandler_created = false;
            for (int num = 1; num < inputs.Count; num++)
            {
                int[] index = new int[num];
                for (int t = 0; t < num; t++) index[t] = -1;
                index[0] = rng.Next(0, inputs.Count);
                int cur = 1;
                int whilecount = 0;
                while (cur < num)
                {
                    int t = rng.Next(0, inputs.Count);
                    if (index.Contains(t) || inputs[t].Cataory != inputs[index[0]].Cataory)
                        continue;
                    index[cur++] = t;
                    whilecount += 1;
                    if (whilecount >= 10)
                        log.Warn("too many iteration in handler creating");
                }

                double[] ps = cHandler.randomParam();
                newGene = new HandlerGene(this, cHandler.name, index.ToList().ConvertAll(xh=>inputs[xh].Id), ps);
                newGene.Generation = session.Generation;
                newGene.Cataory = inputs[index[0]].Cataory;
                newGene.Id = session.GetIdGenerator().getGeneId(newGene);
                newGene.sortInput();
                if (!genome.exist(newGene))
                {
                    genome.handlerGenes.Add(newGene);
                    session.triggerEvent(Session.EVT_NAME_MESSAGE, "A new handler gene is produced in "+ genome.id.ToString() +":" + newGene.Text);
                    newhandler_created = true;
                    break;
                }
            }
            if (!newhandler_created)
                log.Warn("new Handler gene failed!");

            //对推理节点进行变异
            if(this.infrernceGenes.Count>0 && rng.NextDouble()<=0.5)
            {
                int index = rng.Next(0, this.infrernceGenes.Count);
                double operation = rng.NextDouble();
                if(operation <= 0.3)//添加一个维度
                {
                    (int t1, int t2) = infrernceGenes[index].getTimeDiff();
                    int i = rng.Next(0, inputs.Count);
                    genome.infrernceGenes[index].dimensions.Add((inputs[i].Id, t2));
                    genome.infrernceGenes[index].sort_dimension();
                    session.triggerEvent(Session.EVT_NAME_MESSAGE, "A inference gene is modified in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text);
                }
                else if(operation <= 0.6 && this.infrernceGenes[index].dimensions.Count > 2) //删除一个维度
                {
                    List<(int, int)> conds = this.infrernceGenes[index].getConditions();
                    int i = rng.Next(0, conds.Count);
                    genome.infrernceGenes[index].dimensions.Remove(conds[i]);
                    session.triggerEvent(Session.EVT_NAME_MESSAGE, "A inference gene is modifiedd in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text);
                }
                else //修改一个维度
                {
                    int i = rng.Next(0, inputs.Count);
                    int j = rng.Next(0, this.infrernceGenes[index].dimensions.Count);
                    genome.infrernceGenes[index].dimensions[j] = (inputs[i].Id, this.infrernceGenes[index].dimensions[j].Item2);
                    session.triggerEvent(Session.EVT_NAME_MESSAGE, "A inference gene is modifiedx in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text);
                }
                genome.infrernceGenes[index].sort_dimension();
                genome.infrernceGenes[index].Id = Session.idGenerator.getGeneId(genome.infrernceGenes[index]);
                
            }
            else //添加一个推理节点
            {
                //维度
                int count = rng.Next(2, inputs.Count);
                List<(int, int)> diemesnion = new List<(int, int)>();
                //选择一个作为变量
                int varindex = -1;
                while(varindex == -1 || inputs[varindex].Group.StartsWith("action"))
                {
                    varindex = rng.Next(0, inputs.Count);
                }
                diemesnion.Add((inputs[varindex].Id,0));
                //选择前置条件
                List<int> conds = new List<int>();
                while(conds.Count<count-1)
                {
                    int condindex = rng.Next(0, inputs.Count);
                    if (conds.Contains(condindex)) continue;
                    conds.Add(condindex);
                    diemesnion.Add((inputs[condindex].Id, 1));
                }

                InferenceGene inferenceGene = new InferenceGene(this);
                inferenceGene.dimensions = diemesnion;
                inferenceGene.Cataory = inputs[varindex].Cataory;
                inferenceGene.Generation = session.Generation;
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                inferenceGene.sort_dimension();
                genome.infrernceGenes.Add(inferenceGene);
                session.triggerEvent(Session.EVT_NAME_MESSAGE, "A inference gene is added in " + genome.id.ToString() + ":" + inferenceGene.Text);
            }
            //对判定节点权重进行变异
            genome.computeNodeDepth();
            genome.id = Session.idGenerator.getGenomeId();
            return genome;
        }

        #endregion


    }
}
