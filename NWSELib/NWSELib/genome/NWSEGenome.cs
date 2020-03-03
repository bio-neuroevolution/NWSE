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
        /// 有效推理基因
        /// </summary>
        public List<NodeGene> vaildInferenceNodes = new List<NodeGene>();

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
            foreach(ReceptorGene receptorGene in receptorGenes)
            {
                str.Append(receptorGene.ToString()+ System.Environment.NewLine);
            }
            foreach(HandlerGene handlerGene in handlerGenes)
            {
                str.Append(handlerGene.ToString() + System.Environment.NewLine);
            }

            foreach(InferenceGene inferenceGene in this.infrernceGenes)
            {
                str.Append(inferenceGene.ToString() + System.Environment.NewLine);
            }

            str.Append("handlerSelectionProb=" + Utility.toString(handlerSelectionProb) + System.Environment.NewLine);

            
            foreach(NodeGene vaildGene in this.vaildInferenceNodes)
            {
                str.Append("vaild=" + vaildGene.ToString() + System.Environment.NewLine);
            }
            return str.ToString();

        }

        public static NWSEGenome parse(String str)
        {
            if (str == null || str.Trim() == "") return null;
            List<String> s1 = str.Split(new String[] { System.Environment.NewLine },StringSplitOptions.RemoveEmptyEntries).ToList();
            if (s1 == null || s1.Count <= 0) return null;

            NWSEGenome genome = new NWSEGenome();
            genome.id = int.Parse(s1[0]);
            s1.RemoveAt(0);

            foreach (String s in s1)
            {
                if (s == null || s.Trim() == "") continue;
                if (s.StartsWith("ReceptorGene"))
                    genome.receptorGenes.Add(ReceptorGene.parse(genome,s));
                else if (s.StartsWith("HandlerGene"))
                    genome.handlerGenes.Add(HandlerGene.parse(genome,s));
                else if (s.StartsWith("InferenceGene"))
                    genome.infrernceGenes.Add(InferenceGene.parse(genome,s));
                else if (s.StartsWith("handlerSelectionProb"))
                {
                    int s2 = s.IndexOf("handlerSelectionProb");
                    s2 = s.IndexOf("=", s2 + 1);
                    String s3 = s.Substring(s2+1).Trim();
                    genome.handlerSelectionProb.Clear();
                    genome.handlerSelectionProb.AddRange(Utility.parse<double>(s3));
                }
                else if (s.StartsWith("vaild"))
                {
                    String s2 = s.Substring(s.IndexOf("="));
                    genome.vaildInferenceNodes.Add(NodeGene.parseGene(genome,s2));
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
            infrernceGenes.ForEach(i => genome.infrernceGenes.Add(i.clone<InferenceGene>()));
           
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
        public bool exist(int id)
        {
            return this.receptorGenes.Exists(r => r.Id == id) ||
                   this.handlerGenes.Exists(h => h.Id == id) ||
                   this.infrernceGenes.Exists(inf => inf.Id == id);
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
                this.infrernceGenes.Add((InferenceGene)gene);
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
            if (gene is InferenceGene) return ((InferenceGene)gene).getDimensions().ConvertAll(x => this[x.Item1]);
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
        }

        
        public List<String> GetAllCataory()
        {
            return this.GetAllInputGenes("").ConvertAll(g => g.Cataory).Distinct().ToList();
        }

        public List<NodeGene> GetAllInputGenes(String cataory)
        {
            List<NodeGene> genes = new List<NodeGene>(this.receptorGenes);
            genes.AddRange(this.handlerGenes);

            if (cataory == null || cataory.Trim() == "") return genes;
            return genes.FindAll(g => g.Cataory == cataory);
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
                for (int j = 0; j < this.vaildInferenceNodes.Count; j++)
                {
                    if (this.equiv(vaildInferenceNodes[i], this.vaildInferenceNodes[j]))
                        continue;
                    this.vaildInferenceNodes.Add(vaildInferenceNodes[i].clone<NodeGene>());
                }
            }
        }

        public bool isVaildGene(NodeGene gene)
        {
            return this.vaildInferenceNodes.Exists(n => n.Id == gene.Id);
        }
        
        
        
        /// <summary>
        /// 变异
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public NWSEGenome mutate(Session session)
        {
            NWSEGenome genome = this.clone();

            //若有效基因库不空，生成有效基因
            List<NodeGene> vaildGenes = genome.vaildInferenceNodes;
            if(vaildGenes != null && vaildGenes.Count>0)
            {
                foreach(NodeGene g in vaildGenes)
                {
                    if (genome.exist(g.Id)) continue;
                    genome.put(g.clone<NodeGene>());
                }
            }



            //修改一个处理器
            /*int num = 0;
            HandlerGene modifiedGene = null;
            while(++num<=5 && handlerGenes.Count>0)
            {
                int hIndex = rng.Next(0, handlerGenes.Count);
                if (isVaildGene(handlerGenes[hIndex])) continue;
                HandlerGene g = handlerGenes[hIndex].mutate();
                if (g == null) continue;
                if (exist(g)) continue;
                modifiedGene = g;
                genome.handlerGenes.Add(g);
                genome.handlerGenes.Remove(handlerGenes[hIndex]);
                break;
            }*/

            //添加一个处理器
            HandlerGene newHandleGene = null;
            int maxcount = 8;
            if (rng.NextDouble()<=0.5)
            {
                int num = 0;
                while (++num <= maxcount)
                {
                    //随机选择处理器
                    double[] handler_selection_prob = Session.GetConfiguration().evolution.mutate.Handlerprob.ToArray();
                    Configuration.Handler cHandler = Session.GetConfiguration().random_handler(handler_selection_prob);
                    //选择感知组
                    List<String> cataories = genome.GetAllCataory();
                    String cataory = cataories[rng.Next(0, cataories.Count)];
                    List<NodeGene> inputGenes = genome.GetAllInputGenes(cataory);
                    inputGenes.shuffle();
                    //确定输入数
                    int inputcount = rng.Next(cHandler.mininputcount,cHandler.maxinputcount + 1);
                    if (inputGenes.Count < inputcount) continue;

                    //确定输入
                    List<int> inputids = new List<int>();
                    for (int i = 0; i < inputcount; i++) inputids.Add(inputGenes[i].Id);

                    //生成
                    double[] ps = cHandler.randomParam();
                    HandlerGene g = new HandlerGene(this, cHandler.name, inputids, ps);
                    g.Generation = session.Generation;
                    g.Cataory = inputGenes[0].Cataory;
                    g.Group = inputGenes[0].Group;
                    g.sortInput();
                    g.Id = session.GetIdGenerator().getGeneId(g);
                    if (!genome.exist(g.Id)) continue;
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
                inf = new InferenceGene(this);
                inf.Cataory = newHandleGene.Cataory;
                inf.Generation = session.Generation;
                inf.conditions = new List<(int, int)>();
                inf.variables = new List<(int, int)>();
                inf.variables.Add((newHandleGene.Id,0));
                List<ReceptorGene> actionSensorGenes = genome.getActionSensorGenes();
                for (int i = 0; i < actionSensorGenes.Count; i++)
                    inf.conditions.Add((actionSensorGenes[i].Id,1));
                inf.sort_dimension();
                inf.Id = Session.idGenerator.getGeneId(inf);
                genome.infrernceGenes.Add(inf);
                session.triggerEvent(Session.EVT_LOG, "A inference gene is added in " + genome.id.ToString() + ":" + inf.Text);
            }

            //对推理节点进行变异
            List<InferenceGene> infGenes = genome.infrernceGenes.FindAll(g => g.validity == -1);
            if (infGenes == null || infGenes.Count <= 0)
                infGenes = genome.infrernceGenes.FindAll(g => g.validity != 1);

            for(int i=0;i< genome.infrernceGenes.Count;i++)
            {
                if (this.infrernceGenes[i].validity == 1) continue;
                if (rng.NextDouble() >= 0.5) continue;

                inf = genome.infrernceGenes[i];

                List<int> condIds = inf.getConditionIds();
                List<NodeGene> genes = this.GetAllInputGenes("");
                double modifyProb = 0.5, addProb = 0.8;
                int num = 0;
                maxcount = 5;
                while (++num <= maxcount)
                {
                    double operation = rng.NextDouble();
                    if (operation <= modifyProb) //更换条件
                    {
                        
                        int index = condIds.
                        genes.shuffle();
                        NodeGene gene = genes.FirstOrDefault(g => !inf.getConditionIds().Contains(g.Id));
                        if (gene == null) { modifyProb = 0.0;continue; }
                    }
                    else if(operation <= 0.8)//添加条件
                    {

                    }
                    else//删除条件
                    {

                    }
                }

            }
            if (this.infrernceGenes.Count>0 && rng.NextDouble()<=0.5)
            {
                //选择一个非有效推理基因
                num = 0;
                while (++num <= maxcount)
                {
                    int index = rng.Next(0, this.infrernceGenes.Count);
                    if (isVaildGene(this.infrernceGenes[index]))
                        continue;

                    InferenceGene inf = genome.infrernceGenes[index].clone<InferenceGene>();
                    double operation = rng.NextDouble();
                    if (operation <= 0.3)//添加一个维度,目前只添加条件
                    {
                        int i = rng.Next(0, inputs.Count);
                        if (inf.getConditionIds().Contains(inputs[i].Id))
                            continue;

                        inf.conditions.Add((inputs[i].Id, 1));
                        inf.sort_dimension();
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        if (genome.exist(inf.Id)) continue;
                        if (Session.IsInvaildGene(inf)) continue;
                        modifiedInferenceGene = inf;
                        genome.infrernceGenes.Remove(genome.infrernceGenes[index]);
                        genome.infrernceGenes.Add(modifiedInferenceGene);
                        session.triggerEvent(Session.EVT_LOG, "A inference gene is modified in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text+":"+inf.Text);
                        break;
                    }
                    else if (operation <= 0.6 && inf.conditions.Count > 2) //删除一个维度
                    {
                        List<(int, int)> conds = inf.getConditions();
                        int i = rng.Next(0, conds.Count);
                        inf.conditions.Remove(conds[i]);
                        inf.sort_dimension();
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        if (genome.exist(inf.Id)) continue;
                        if (Session.IsInvaildGene(inf)) continue;
                        modifiedInferenceGene = inf;
                        genome.infrernceGenes.Remove(genome.infrernceGenes[index]);
                        genome.infrernceGenes.Add(modifiedInferenceGene);
                        session.triggerEvent(Session.EVT_LOG, "A inference gene is modified in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text + ";" + inf.Text);
                        break;
                    }
                    else //修改一个维度
                    {
                        int i = rng.Next(0, inputs.Count);
                        int j = rng.Next(0, inf.conditions.Count);
                        if (inf.getConditionIds().Contains(inputs[i].Id))
                            continue;
                        inf.conditions[j] = (inputs[i].Id, inf.conditions[j].Item2);
                        inf.sort_dimension();
                        inf.Id = Session.idGenerator.getGeneId(inf);
                        if (genome.exist(inf.Id)) continue;
                        if (Session.IsInvaildGene(inf)) continue;
                        modifiedInferenceGene = inf;
                        genome.infrernceGenes.Remove(genome.infrernceGenes[index]);
                        genome.infrernceGenes.Add(modifiedInferenceGene);
                        session.triggerEvent(Session.EVT_LOG, "A inference gene is modifiedx in " + genome.id.ToString() + ":" + genome.infrernceGenes[index].Text+";"+inf.Text);
                        break;
                    }                   
                }
            }

            num = 0;
            while(modifiedInferenceGene==null && ++num<=maxcount)
            {
                //维度
                int count = rng.Next(2, inputs.Count);
                List<(int, int)> variables = new List<(int, int)>();
                //选择一个作为变量
                int varindex = -1;
                while (varindex == -1 || inputs[varindex].IsActionSensor())
                {
                    varindex = rng.Next(0, inputs.Count);
                }
                variables.Add((inputs[varindex].Id, 0));
                //选择前置条件
                List<(int, int)> conditions = new List<(int, int)>();
                while (conditions.Count < count - 1)
                {
                    int condindex = rng.Next(0, inputs.Count);
                    if (conditions.Contains((inputs[condindex].Id, 1))) continue;
                    conditions.Add((inputs[condindex].Id, 1));
                }

                InferenceGene inferenceGene = new InferenceGene(this);
                inferenceGene.conditions = conditions;
                inferenceGene.variables = variables;
                inferenceGene.Cataory = "";
                inferenceGene.Generation = session.Generation;
                inferenceGene.sort_dimension();
                inferenceGene.Id = Session.idGenerator.getGeneId(inferenceGene);
                if (genome.exist(inferenceGene.Id))
                    continue;
                if (Session.IsInvaildGene(inferenceGene))
                    continue;
                genome.infrernceGenes.Add(inferenceGene);
                session.triggerEvent(Session.EVT_LOG, "A inference gene is added in " + genome.id.ToString() + ":" + inferenceGene.Text);
                break;
            }
            

            genome.computeNodeDepth();
            genome.id = Session.idGenerator.getGenomeId();
            return genome;
        }

        #endregion

        
    }
}
