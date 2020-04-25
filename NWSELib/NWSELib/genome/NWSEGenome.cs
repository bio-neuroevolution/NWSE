using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;
using NWSELib.common;

namespace NWSELib.genome
{
    /// <summary>
    /// 染色体
    /// </summary>
    public class NWSEGenome
    {
        public static Random rng = new Random();
        #region 染色体组成
        /// <summary>
        /// 染色体Id
        /// </summary>
        public int id;
        /// <summary>
        /// 生成年代
        /// </summary>
        public int generation;
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
        public readonly List<InferenceGene> inferenceGenes = new List<InferenceGene>();


        /// <summary>
        /// 有效推理基因
        /// </summary>
        public List<NodeGene> validInferenceGenes = new List<NodeGene>();

        static ILog log = LogManager.GetLogger(typeof(NWSEGenome));
        /// <summary>
        /// 构造函数
        /// </summary>
        public NWSEGenome()
        {
            this.handlerSelectionProb.Clear();
            this.handlerSelectionProb.AddRange(Session.GetConfiguration().evolution.mutate.Handlerprob);
        }
        #endregion

        #region 读写

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(this.id.ToString() + System.Environment.NewLine);
            str.Append(this.generation.ToString() + System.Environment.NewLine);
            foreach(ReceptorGene receptorGene in receptorGenes)
            {
                str.Append(receptorGene.ToString()+ System.Environment.NewLine);
            }
            foreach(HandlerGene handlerGene in handlerGenes)
            {
                str.Append(handlerGene.ToString() + System.Environment.NewLine);
            }

            foreach(InferenceGene inferenceGene in this.inferenceGenes)
            {
                str.Append(inferenceGene.ToString() + System.Environment.NewLine);
            }

            str.Append("handlerSelectionProb=" + Utility.toString(handlerSelectionProb) + System.Environment.NewLine);

            
            foreach(NodeGene vaildGene in this.validInferenceGenes)
            {
                str.Append("vaild=" + vaildGene.ToString() + System.Environment.NewLine);
            }
            return str.ToString();

        }

        public static NWSEGenome Parse(String str)
        {
            if (str == null || str.Trim() == "") return null;
            List<String> s1 = str.Split(new String[] { System.Environment.NewLine },StringSplitOptions.RemoveEmptyEntries).ToList();
            if (s1 == null || s1.Count <= 0) return null;

            NWSEGenome genome = new NWSEGenome();
            genome.id = int.Parse(s1[0]);
            s1.RemoveAt(0);

            if(int.TryParse(s1[0],out genome.generation))
                s1.RemoveAt(0);

            foreach (String s in s1)
            {
                if (s == null || s.Trim() == "") continue;
                if (s.StartsWith("ReceptorGene"))
                    genome.receptorGenes.Add(ReceptorGene.parse(genome,s));
                else if (s.StartsWith("HandlerGene"))
                    genome.handlerGenes.Add(HandlerGene.parse(genome,s));
                else if (s.StartsWith("InferenceGene"))
                    genome.inferenceGenes.Add(InferenceGene.parse(genome,s));
                else if (s.StartsWith("handlerSelectionProb"))
                {
                    int s2 = s.IndexOf("handlerSelectionProb");
                    s2 = s.IndexOf("=", s2 + 1);
                    String s3 = s.Substring(s2+1).Trim();
                    genome.handlerSelectionProb.Clear();
                    genome.handlerSelectionProb.AddRange(Utility.parse(s3));
                }
                else if (s.StartsWith("vaild"))
                {
                    String s2 = s.Substring(s.IndexOf("="));
                    genome.validInferenceGenes.Add(NodeGene.parseGene(genome,s2));
                }
            }

            genome.computeNodeDepth();
            return genome;
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
            genome.handlerSelectionProb.Clear();
            genome.handlerSelectionProb.AddRange(handlerSelectionProb);
            handlerGenes.ForEach(h => genome.handlerGenes.Add(h.clone<HandlerGene>()));
            inferenceGenes.ForEach(i => genome.inferenceGenes.Add(i.clone<InferenceGene>()));
           
            validInferenceGenes.ForEach(vf => genome.validInferenceGenes.Add(vf.clone<NodeGene>()));

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

            if (inferenceGenes.Count != genome.inferenceGenes.Count)
                return false;
            for (int i = 0; i < inferenceGenes.Count; i++)
            {
                for (int j = 0; j < genome.inferenceGenes.Count; j++)
                {
                    if (!inferenceGenes[i].equiv(genome.inferenceGenes[j])) return false;
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
                return this.inferenceGenes.Exists(i => this.equiv(i, g));
            }
            return false;
        }
        public bool exist(int id)
        {
            return this.receptorGenes.Exists(r => r.Id == id) ||
                   this.handlerGenes.Exists(h => h.Id == id) ||
                   this.inferenceGenes.Exists(inf => inf.Id == id);
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

        public bool put(NodeGene gene)
        {
            if (gene == null) return false;
            if (this.exist(gene.Id)) return false;
            if (this.exist(gene)) return false;
            if (gene is ReceptorGene)
            {
                this.receptorGenes.Add((ReceptorGene)gene);
                return true;
            }
            else if (gene is HandlerGene)
            {
                this.handlerGenes.Add((HandlerGene)gene);
                return true;
            }
            else if (gene is InferenceGene)
            {
                this.inferenceGenes.Add((InferenceGene)gene);
                return true;
            }
            
            return false;
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
                gene = inferenceGenes.FirstOrDefault(g => g.Id == id);
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
                gene = inferenceGenes.FirstOrDefault(g => g.Name == name);
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
            if (gene is InferenceGene) return ((InferenceGene)gene).Dimensions.ConvertAll(x => this[x]);
            return new List<NodeGene>();
        }

        public List<ReceptorGene> getEnvSensorGenes()
        {
            return this.receptorGenes.FindAll(r => r.IsEnvSensor());
        }
        public List<ReceptorGene> getActionSensorGenes()
        {
            return this.receptorGenes.FindAll(r => r.IsActionSensor());
        }
        public List<ReceptorGene> getNonActionSensorGenes()
        {
            return this.receptorGenes.FindAll(r => !r.IsActionSensor());
        }
        /// <summary>
        /// 取得ids对应的所有感受器基因
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<ReceptorGene> getReceptorGenes(params int[] ids)
        {
            List<ReceptorGene> r = new List<ReceptorGene>();
            if (ids == null || ids.Length <= 0) return r;
            foreach(int id in ids)
            {
                r.AddRange(this[id].getLeafGenes());
            }
            return r;
        }

        public (int,int) GetHanlerDepth()
        {
            if (this.handlerGenes == null || this.handlerGenes.Count <= 0)
                return (1,1);
            List<int> depths = this.handlerGenes.ConvertAll(h => h.Depth);
            return (depths.Min(), depths.Max());
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
            int d1 = this.inferenceGenes.ConvertAll(i => i.Depth).Max();
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
            this.inferenceGenes.ForEach(i =>
            {
                int t = this.computeNodeDepth(i);
                if (t < d + 1) i.Depth = d + 1;
            });
            d = this.inferenceGenes.ConvertAll(i => i.Depth).Max();
        }

        
        public List<String> GetAllCataory(params String[] exclude)
        {
            return this.GetAllInputGenes("").ConvertAll(g => g.Cataory).Distinct().ToList().FindAll(s=>exclude!=null&&!exclude.ToList().Contains(s));
        }

        public List<NodeGene> GetAllInputGenes(String cataory)
        {
            List<NodeGene> genes = new List<NodeGene>(this.receptorGenes);
            genes.AddRange(this.handlerGenes);

            if (cataory == null || cataory.Trim() == "") return genes;
            return genes.FindAll(g => g.Cataory == cataory);
        }


        #endregion

        #region 添加修改基因
        public NodeGene remove(int id)
        {
            ReceptorGene g1 = this.receptorGenes.FirstOrDefault(g => g.Id == id);
            if(g1 != null)
            {
                this.receptorGenes.Remove(g1);
                return g1;
            }

            HandlerGene g2 = this.handlerGenes.FirstOrDefault(g => g.Id == id);
            if (g2 != null)
            {
                this.handlerGenes.Remove(g2);
                return g2;
            }

            InferenceGene g3 = this.inferenceGenes.FirstOrDefault(g => g.Id == id);
            if (g3 != null)
            {
                this.inferenceGenes.Remove(g3);
                return g3;
            }
            return null;
        }
        public void replaceGene(int oldid, NodeGene gene)
        {
            remove(oldid);
            if (gene == null) return;
            if(gene is ReceptorGene)
            {
                this.receptorGenes.Add((ReceptorGene)gene);
            }else if(gene is HandlerGene)
            {
                this.handlerGenes.Add((HandlerGene)gene);
            }else if(gene is InferenceGene)
            {
                this.inferenceGenes.Add((InferenceGene)gene);
            }
        }

        #endregion

        #region 漂移和变异
        /// <summary>
        /// 基因漂移处理
        /// </summary>
        /// <param name="vaildInferenceNodes"></param>
        public void gene_drift(List<NodeGene> vaildInferenceNodes)
        {
            if (vaildInferenceNodes == null || vaildInferenceNodes.Count <= 0) return;

            for (int i = 0; i < vaildInferenceNodes.Count; i++)
            {
                for (int j = 0; j < this.validInferenceGenes.Count; j++)
                {
                    if (this.equiv(vaildInferenceNodes[i], this.validInferenceGenes[j]))
                        continue;
                    this.validInferenceGenes.Add(vaildInferenceNodes[i].clone<NodeGene>());
                }
            }
        }

        public bool isVaildGene(NodeGene gene)
        {
            return this.validInferenceGenes.Exists(n => n.Id == gene.Id);
        }
        /// <summary>
        /// 交叉
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public static NWSEGenome crossover(NWSEGenome g1,NWSEGenome g2, Session session)
        {
            NWSEGenome genome = g1.clone();
            genome.generation = session.Generation;

            List<InferenceGene> infGenes = new List<InferenceGene>(g2.inferenceGenes);
            int count = 0;
            for(int i=0;i<8;i++)
            {
                int index = rng.Next(0, infGenes.Count);
                InferenceGene g2InfGene = infGenes[index].clone<InferenceGene>();
                InferenceGene g1InfGene = (InferenceGene)genome[g2InfGene.Id];
                if (g1InfGene == null) continue;
                int d = genome.inferenceGenes.IndexOf(g1InfGene);
                genome.inferenceGenes[d] = g2InfGene;
                count += 1;
                if (count >= 3) break;
            }

            genome.computeNodeDepth();
            genome.id = Session.idGenerator.getGenomeId();
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
            genome.generation = session.Generation;

            //若有效基因库不空，生成有效基因
            List<NodeGene> vaildGenes = genome.validInferenceGenes;
            if(vaildGenes != null && vaildGenes.Count>0)
            {
                foreach(NodeGene g in vaildGenes)
                {
                    if (genome.exist(g.Id)) continue;
                    genome.put(g.clone<NodeGene>());
                }
            }

            //添加一个处理器
            List<NodeGene> inputGenes = null;
            HandlerGene newHandleGene = null;
            int maxcount = 8;
            if (rng.NextDouble()< Session.config.evolution.mutate.newHandleGeneProb)
            {
                int num = 0;
                while (++num <= maxcount)
                {
                    //随机选择处理器
                    double[] handler_selection_prob = Session.GetConfiguration().evolution.mutate.Handlerprob.ToArray();
                    Configuration.Handler cHandler = Session.GetConfiguration().random_handler(handler_selection_prob);
                    //选择感知组
                    List<String> cataories = genome.GetAllCataory("rotate");
                    String cataory = cataories[rng.Next(0, cataories.Count)];
                    inputGenes = genome.GetAllInputGenes(cataory);
                    inputGenes.shuffle();
                    //确定输入数
                    int inputcount = rng.Next(cHandler.mininputcount,cHandler.maxinputcount + 1);
                    if (inputGenes.Count < inputcount) continue;

                    //确定输入
                    List<int> inputids = new List<int>();
                    for (int i = 0; i < inputcount; i++) inputids.Add(inputGenes[i].Id);

                    //生成
                    double[] ps = cHandler.randomParam();
                    HandlerGene g = new HandlerGene(genome, cHandler.name, inputids, ps);
                    g.Generation = session.Generation;
                    g.Cataory = inputGenes[0].Cataory;
                    g.Group = inputGenes[0].Group;
                    g.sortInput();

                    g.Id = session.GetIdGenerator().getGeneId(g);
                    if (genome.exist(g.Id)) continue;
                    newHandleGene = g;
                    genome.handlerGenes.Add(newHandleGene);
                    session.triggerEvent(Session.EVT_LOG, "A new handler gene is produced in " + genome.id.ToString() + ":" + newHandleGene.Text);
                    break;
                }     
            }

            //如果生成了新hanler，生成后置变量
            InferenceGene inf = null;
            if (newHandleGene != null)
            {
                List<int> conditions = new List<int>();
                List<int> variables = new List<int>();
                int timediff = 1;
                variables.Add(newHandleGene.Id);
                List<ReceptorGene> actionSensorGenes = genome.getActionSensorGenes();
                for (int i = 0; i < actionSensorGenes.Count; i++)
                    conditions.Add(actionSensorGenes[i].Id);

                inf = new InferenceGene(genome,timediff,conditions,variables);
                inf.Cataory = newHandleGene.Cataory;
                inf.Generation = session.Generation;
                inf.Id = Session.idGenerator.getGeneId(inf);
                genome.inferenceGenes.Add(inf);
                session.triggerEvent(Session.EVT_LOG, "A inference gene is added in " + genome.id.ToString() + ":" + inf.Text);
            }

            //选择需要变异的推理节点(按照可靠度排序，可靠度越小变异概率越大)
            List<InferenceGene> infGenes = genome.inferenceGenes.FindAll(g => g.reability <= 0.5);
            if (infGenes == null || infGenes.Count <= 0)
                infGenes = genome.inferenceGenes.FindAll(g => g.reability <= 0.98);
            //没有需要变异的推理节点
            inputGenes = genome.GetAllInputGenes(null);
            int mutateinfcount = 0;
            double infMutateProb = Session.config.evolution.mutate.infMutateProb;
            double infMutateAddProb = Session.config.evolution.mutate.infMutateAddProb;
            double infMutateRemoveProb = Session.config.evolution.mutate.infMutateRemoveProb;
            for (int i = 0; i < infGenes.Count; i++)
            {
                if (rng.NextDouble() >= infMutateProb) continue;
                inf = infGenes[i].clone<InferenceGene>();
                inf.owner = genome;
                int oldinfid = inf.Id;
                String prevText = inf.Text;
                List<int> condIds = inf.conditions;
                
                
                int num = 0;
                maxcount = 5;
                while (++num <= maxcount)
                {
                
                    //删除
                    if(condIds.Count>2 && (condIds.Count == inputGenes.Count || rng.NextDouble()<= infMutateRemoveProb))
                    {
                        condIds = inf.conditions.FindAll(c => !genome[c].IsActionSensor());
                        int index = rng.Next(0, condIds.Count);
                        inf.conditions.RemoveAt(index);
                        if (Session.IsInvaildGene(inf))
                        {
                            inf = infGenes[i].clone<InferenceGene>(); inf.owner = genome; continue;
                        }
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        genome.replaceGene(oldinfid, inf);
                        mutateinfcount += 1;
                        session.triggerEvent(Session.EVT_LOG, "conditions of inference gene is removed(" + genome.id.ToString() + "):" + prevText+"--->"+inf.Text);
                        break;
                    }
                    //添加
                    else if(condIds.Count < inputGenes.Count && rng.NextDouble() <= infMutateAddProb)
                    {
                        List<NodeGene> temp = inputGenes.FindAll(g => !condIds.Contains(g.Id));
                        int index = rng.Next(0, temp.Count);
                        inf.conditions.Add(temp[index].Id);
                        inf.Sortdimension();
                        if (Session.IsInvaildGene(inf))
                        {
                            inf = infGenes[i].clone<InferenceGene>(); inf.owner = genome; continue;
                        }
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        genome.replaceGene(oldinfid, inf);
                        mutateinfcount += 1;
                        session.triggerEvent(Session.EVT_LOG, "conditions of inference gene is added(" + genome.id.ToString() + "):" + prevText + "--->" + inf.Text);
                        break;
                    }
                    else  //修改
                    {
                        condIds = inf.conditions.FindAll(c => !genome[c].IsActionSensor());
                        if (condIds.Count <=0) continue;
                        int index1 = rng.Next(0, condIds.Count);
                        List<NodeGene> temp = inputGenes.FindAll(g => !condIds.Contains(g.Id) || g.Id != condIds[index1]);
                        int index2 = rng.Next(0, temp.Count);
                        inf.conditions[index1] = temp[index2].Id;
                        inf.Sortdimension();
                        if (Session.IsInvaildGene(inf))
                        {
                            inf = infGenes[i].clone<InferenceGene>(); inf.owner = genome; continue;
                        }
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        genome.replaceGene(oldinfid, inf);
                        mutateinfcount += 1;
                        session.triggerEvent(Session.EVT_LOG, "conditions of inference gene is modified(" + genome.id.ToString() + "):" + prevText + "--->" + inf.Text);
                        break;
                    }
                }
            }

            genome.computeNodeDepth();
            genome.id = Session.idGenerator.getGenomeId();
            return genome;
        }

        

        #endregion


    }
}
