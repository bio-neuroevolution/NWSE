
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NWSELib.common;
using NWSELib.env;
using NWSELib.genome;
namespace NWSELib.net
{
    /// <summary>
    /// 网络
    /// </summary>
    public class Network
    {
        #region 基本信息
        private static Random rng = new Random();
        /// <summary>
        /// 染色体
        /// </summary>
        private NWSEGenome genome;
        /// <summary>
        /// 所有节点
        /// </summary>
        private List<Node> nodes = new List<Node>();
        /// <summary>
        /// 邻接矩阵
        /// </summary>
        private double[,] adjMatrix;
        /// <summary>
        /// 染色体
        /// </summary>
        public NWSEGenome Genome { get => genome; }
        /// <summary>
        /// 节点集
        /// </summary>
        public List<Node> Nodes { get => nodes; }
        /// <summary>
        /// Id
        /// </summary>
        /// <returns></returns>
        public int getId() { return this.genome.id; } 
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get => this.genome.id; }
        /// <summary>
        /// 抽象思维
        /// </summary>
        public Imagination imagination;
        #endregion





        #region 节点查询
        /// <summary>
        /// 所有感知节点
        /// </summary>
        public List<Receptor> Receptors
        {
            get => nodes.FindAll(n => n is Receptor).ConvertAll(n=>(Receptor)n);
        }
        /// <summary>
        /// 所有环境感知节点
        ///</summary>
        public List<Receptor> EnvReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("env")).ConvertAll(n => (Receptor)n);
        }
        /// <summary>
        /// 所有姿态感知节点
        ///</summary>
        public List<Receptor> GesturesReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("gestures")).ConvertAll(n => (Receptor)n);
        }
        /// <summary>
        /// 所有动作感知节点
        ///</summary>
        public List<Receptor> ActionReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("action")).ConvertAll(n=>(Receptor)n);
        }


        /// <summary>
        /// 所有处理节点
        /// </summary>
        public List<Handler> Handlers
        {
            get => nodes.FindAll(n => n is Handler).ConvertAll(n=>(Handler)n);
        }

        public List<Inference> Inferences
        {
            get => nodes.FindAll(n => n is Inference).ConvertAll(n=>(Inference)n);
        }

        /// <summary>
        /// 效应器
        /// </summary>
        public List<Effector> Effectors
        {
            get => nodes.FindAll(n => n is Effector).ConvertAll(n=>(Effector)n);
        }

        

        /// <summary>
        /// 根据节点Id查找节点索引下标
        /// </summary>
        public int idToIndex(int id)
        {
            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.nodes[i].Id == id)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 根据节点Id的上游连接节点
        /// </summary>
        public List<Node> getInputNodes(int id)
        {
            int index = this.idToIndex(id);
            List<Node> r = new List<Node>();
            for (int i = 0; i < this.nodes.Count; i++)
            {
                if (this.adjMatrix[i, index] != 0)
                    r.Add(this.nodes[i]);
            }
            return r;
        }

        public Node getNode(int id)
        {
            return this.nodes.FirstOrDefault(n => n.Id == id);
        }
        public Node getNode(String name)
        {
            return this.nodes.FirstOrDefault(n => n.Name == name);
        }
        public int getActionIdByName(String name)
        {
            Node n = this.Effectors.FirstOrDefault(e => e.Name == name || e.Name == name.Substring(1));
            return n == null ? 0 : n.Id;
        }

        public Effector getEffectorByActionSensorId(int id)
        {
            String name = this.getNode(id).Name;
            return (Effector)this.Effectors.FirstOrDefault(e => e.Name == name || e.Name == name.Substring(1));

        }
        public List<Node> getAcceptNodes()
        {
            List<Node> nodes = new List<Node>();
            nodes.AddRange(this.Receptors);
            nodes.AddRange(this.Handlers);
            return nodes;
        }
        #endregion

        #region 初始化
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.nodes.ForEach(a => a.Reset());
        }
        public void thinkReset()
        {
            this.nodes.ForEach(n => n.think_reset());
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public Network(NWSEGenome genome)
        {
            this.genome = genome;
            //初始化节点
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                Receptor receptor = new Receptor(genome.receptorGenes[i]);
                this.nodes.Add(receptor);
            }
            for (int i = 0; i < genome.handlerGenes.Count; i++)
            {
                Handler handler = Handler.create(genome.handlerGenes[i]);
                this.nodes.Add(handler);
            }
            for (int i = 0; i < genome.infrernceGenes.Count; i++)
            {
                Inference inference = new Inference(genome.infrernceGenes[i]);
                this.nodes.Add(inference);
            }
            for (int i = 0; i < this.ActionReceptors.Count; i++)
            {
                Effector effector = new Effector(((ReceptorGene)this.ActionReceptors[i].Gene).toActionGene());
                this.nodes.Add(effector);
            }

            //构造连接矩阵
            this.adjMatrix = new double[this.nodes.Count, this.nodes.Count];
            for (int i = 0; i < genome.handlerGenes.Count; i++)
            {
                int k1 = idToIndex(genome.handlerGenes[i].Id);
                for(int j=0;j< genome.handlerGenes[i].inputs.Count;j++)
                {
                    int k2 = idToIndex(genome.handlerGenes[i].inputs[j]);
                    this.adjMatrix[k2, k1] = 1;
                }
            }
            for (int i = 0; i < genome.infrernceGenes.Count; i++)
            {
                int k1 = idToIndex(genome.infrernceGenes[i].Id);
                for (int j = 0; j < genome.infrernceGenes[i].dimensions.Count; j++)
                {
                    int k2 = idToIndex(genome.infrernceGenes[i].dimensions[j].Item1);
                    this.adjMatrix[k2, k1] = 1;
                }
            }

            imagination = new Imagination(this);
        }



        #endregion

        #region 状态
        

        /// <summary>
        /// 行动轨迹
        /// </summary>
        public List<ActionPlan> actionPlanTraces = new List<ActionPlan>();

        /// <summary>
        /// 最后行动计划
        /// </summary>
        public ActionPlan lastActionPlan
        {
            get
            {
                return actionPlanTraces.Count <= 0 ? null : actionPlanTraces[actionPlanTraces.Count - 1];
            }
        }
        /// <summary>
        /// 取得某个时间的行动计划
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public ActionPlan getActionPlan(int time)
        {
            return actionPlanTraces.FirstOrDefault(ap => ap.judgeTime == time);
        }

        
        #endregion

        #region 评价信息
        /// <summary>
        /// 网络可靠度，是指节点平均可靠度占所有个体的节点平均可靠度之和的比例
        /// </summary>
        protected double reability;
        /// <summary>
        /// 所有节点平均可靠度
        /// </summary>
        /// <returns></returns>
        public double GetNodeAverageReability()
        {
            double r = 0.0;
            this.Handlers.ForEach(h => r += h.Reability);
            this.Inferences.ForEach(i => r += i.Reability);
            return r / (this.Handlers.Count + this.Inferences.Count);
        }
        /// <summary>
        /// 网络可靠度
        /// </summary>
        public double Reability
        {
            get => reability;
            set => reability = value;
        }

        public List<NodeGene> getVaildInferenceGene()
        {
            double highlimit = Session.GetConfiguration().evaluation.gene_reability_range.Max;
            return this.Inferences.FindAll(n => n.Reability > highlimit).ConvertAll(n => n.Gene);
        }

        public List<NodeGene> getInvaildInferenceGene()
        {

            double lowlimit = Session.GetConfiguration().evaluation.gene_reability_range.Min;
            return this.Inferences.FindAll(n => n.Reability < lowlimit).ConvertAll(n=>n.Gene);
        }
        #endregion



        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="obs"></param>
        /// <returns></returns>
        public List<double> activate(List<double> obs, int time,Session session)
        {
            
            //0.初始化
            Reset();
            //1.接收输入
            for (int i = 0; i < obs.Count; i++)
            {
                this.Receptors[i].activate(this, time, obs[i]);
            }

            //2.处理感知
            while (!this.Handlers.All(n => n.IsActivate(time)))
            {
                this.Handlers.ForEach(n => n.activate(this, time, null));
            }

            //3. 对现有推理记录的准确性进行评估
            for (int i = 0; i < this.Inferences.Count; i++)
            {
                Inference inf = this.Inferences[i];
                inf.Records.ForEach(r => r.adjustAccuracy(this, inf, time));
                
            }
            //4. 记忆整理
            for (int i=0;i<this.Inferences.Count;i++)
            {
                Inference inf = this.Inferences[i];
                inf.Records.ForEach(r => r.adjustAccuracy(this, inf, time));
                inf.activate(this, time);
            }
            //5. 信息抽象
            //imagination.doAbstract();
            imagination.inferences = this.Inferences;

            //6. 推理想象、行为决策
            ActionPlan plan = imagination.doImagination(time, session);
            if (plan == null) plan = createDefaultPlan(time);
            this.actionPlanTraces.Add(plan);
            setEffectValue(time);

            //7.记录行为
            List<double> actions = this.Effectors.ConvertAll<double>(n => (double)n.Value);
            for(int i=0;i< this.ActionReceptors.Count;i++)
            {
                this.ActionReceptors[i].activate(this, time, actions[i]);
            }
            //行为评估不在这里，而是在setReward里
            return actions;
        }

        #region 回忆和推理
        public ActionPlan createDefaultPlan(int time)
        {
            ActionPlan plan = new ActionPlan();
            if(rng.NextDouble()<=0.5)
            {
                plan.actions = Session.instinctActionHandler(this, time);
                plan.judgeTime = time;
                plan.judgeType = ActionPlan.JUDGE_INSTINCT;
            }else
            {
                plan.actions = Effectors.ConvertAll(e=>e.randomValue(this,time));
                plan.judgeTime = time;
                plan.judgeType = ActionPlan.JUDGE_RANDOM;
            }
            int index = 0;
            plan.inputObs = this.Receptors.ConvertAll(r => r.Gene.IsActionSensor()? new Vector(plan.actions[index++]):r.Value);

            return plan;
        }
        public String showActionPlan()
        {
            if (lastActionPlan == null) return "";
            StringBuilder str = new StringBuilder();
            str.Append("行动方式="+ lastActionPlan.judgeType + System.Environment.NewLine);
            str.Append("   行为=");
            str.Append(showActionText() + System.Environment.NewLine);

            str.Append(System.Environment.NewLine);
            return str.ToString();
        }
        public String showActionText()
        {
            List<double> values = this.Effectors.ConvertAll(e => e.Value[0]);
            //double delta_speed = (values[0] - 0.5) * Agent.Max_Speed_Action;
            double delta_degree = (((values[0] - 0.5) * Agent.Max_Rotate_Action * 2) * Agent.DRScale) % 360;
            return delta_degree.ToString("F3");
        }
        /// <summary>
        /// 根据行动计划设定输出
        /// //没有行动计划,两种原因导致：1）没有找到相似场景；2）行动计划的最大评估值太小,相当于处于困境
            //对于前一种，可以执行本能行为，后一种则执行随机行为
        /// </summary>
        private void setEffectValue(int time,bool random=true)
        {
            for (int i = 0; i < this.Effectors.Count; i++)
            {
                this.Effectors[i].activate(this, time, this.lastActionPlan.actions[i]);
            }
        }

        
        
        /// <summary>
        /// 取得特定id的最新值
        /// </summary>
        /// <param name="inf"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public List<Vector> getValues(List<int> ids)
        {
            return ids.ConvertAll(id => this.getNode(id).Value);
        }

        public List<Vector> getRankedValues(List<int> ids,List<Vector> orginValues)
        {
            
            List<Vector> values = new List<Vector>();
            for (int j = 0; j < ids.Count; j++)
            {
                //取得每维的原始值
                Vector orginValue = orginValues[j];
                //取得该维度的分级数和范围
                NodeGene gene = Genome[ids[j]];
                List<(int, double)> level = Session.GetConfiguration().getLevel(gene.Id, gene.Name, gene.Cataory);
                //分级处理
                Vector newValue = new Vector(true, orginValue.Size);
                for (int k = 0; k < orginValue.Size; k++)
                {
                    double unit = level[k].Item2 / level[k].Item1;
                    int levelValue = (int)(orginValue[k] / unit);
                    if (levelValue >= level[k].Item1)
                        levelValue = level[k].Item1 - 1;
                    newValue[k] = levelValue;
                }
            }
            return values;
        }
        
        
        /// <summary>
        /// 取得inference接续的推理节点
        /// 要求是inference的后置变量部分完全包含了其他推理的前置条件部分（动作感知除外）
        /// </summary>
        /// <param name="inference"></param>
        /// <returns></returns>
        public List<Inference> getNextInferences(Inference inference)
        {
            List<int> postVarIds = inference.getGene().getVariableIds();
            List<Inference> r = new List<Inference>();

            for (int i=0;i<this.Inferences.Count;i++)
            {
                if (this.Inferences[i] == inference) continue;
                List<int> varCondIds = ((Inference)this.Inferences[i]).getGene().getConditionsExcludeActionSensor();
                if (Utility.ContainsAll(postVarIds, varCondIds))
                    r.Add((Inference)this.Inferences[i]);
            }
            return r;
        }

        /// <summary>
        /// 查找与envValues输入相似的节点
        /// </summary>
        /// <param name="inference">推理</param>
        /// <param name="values"></param>
        /// <returns></returns>
        public (InferenceRecord record, double similarity) recall(Inference inference, List<Vector> envValues)
        {
            if (inference == null || inference.Records.Count<=0) return (null,0);
            List<double> similarities = new List<double>();
            int envcondCount = inference.splitIds().Item1.Count;
            //计算相似度
            for (int i=0;i< inference.Records.Count;i++)
            {
                List<Vector> center = inference.Records[i].means;
                List<Vector> clone = new List<Vector>(center);
                clone = inference.replaceEnvValue(clone, envValues);
                double sim = Vector.manhantan_distance(clone, center);
                //double sim = inference.Records[i].prob(clone) / inference.Records[i].prob(center);
                similarities.Add(sim);
            }
            //相似度从大到小排序
            List<int> index = similarities.argsort();
            
            //相似度上界
            double tolerable_similarity = Session.GetConfiguration().learning.judge.tolerable_similarity;
            //寻找满足相似度上界，且评价最高的
            InferenceRecord record = null;
            double similarity = 0;
            double evulation = double.MinValue;
            for(int i=0;i<index.Count;i++)
            {
                if (similarities[index[i]] >= envcondCount*tolerable_similarity) break;
                if(inference.Records[index[i]].evulation > evulation)
                {
                    record = inference.Records[index[i]];
                    similarity = similarities[index[i]];
                    evulation = inference.Records[index[i]].evulation;
                }
            }
            return (record, similarity);
        }

        /// <summary>
        /// 因为inference的结果results包含了nextinf的所有输入（动作除外）
        /// 将这些输入提取出来
        /// </summary>
        /// <param name="inference">推理</param>
        /// <param name="results">结果</param>
        /// <param name="nextinf"></param>
        /// <returns>只是环境输入部分</returns>
        public List<Vector> computeInput(Inference inference, List<Vector> results, Inference nextinf)
        {
            List<int> infVarIds = inference.getGene().getVariableIds();
            if (infVarIds.Count != results.Count) return null;
            List<int> nextinfcondIds = nextinf.getGene().getConditionsExcludeActionSensor();
            List<Vector> r = new List<Vector>();
            for(int i=0;i< nextinfcondIds.Count;i++)
            {
                int index = infVarIds.IndexOf(nextinfcondIds[i]);
                if (index < 0) return null;
                r.Add(results[index]);
            }
            return r;
        }

        /// <summary>
        /// 取得推理节点输出部分(后置变量)的值
        /// 只要有一个输出在time处没有值，都将返回null
        /// </summary>
        /// <param name="inference"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public List<Vector> getOutputValues(Inference inference, int time)
        {
            List<int> varIds = inference.getGene().getVariableIds();
            List<Vector> vs = varIds.ConvertAll(id => this.getNode(id).GetValue(time));
            return vs.Contains(null) ? null : vs;
        }

        public const int REWARD_EXP = 1;
        public const int REWARD_MEAN = 2;
        public const int REWARD_ONCE = 3;

        public void setReward(double reward,int time,int mode = 1,bool clear=true)
        {
            if (actionPlanTraces.Count <= 0) return;
            for (int i = actionPlanTraces.Count - 1; i >= 0; i--)
            {
                ActionPlan plan = actionPlanTraces[i];
                thinkReset();
                for(int j=0;j<this.Receptors.Count;j++)
                {
                    this.Receptors[j].think(this, time, plan.inputObs[j]);
                }
                this.Handlers.ForEach(h => h.think(this, time, null));

                for(int j=0;j<this.imagination.inferences.Count;j++)
                {
                    Inference inf = this.imagination.inferences[j];
                    if (!inf.getGene().hasEnvDenpend()) continue; //根外界环境无关的不做评估
                    List<InferenceRecord> matchedRecords = inf.getMatchRecordsInThink(this,time);
                    double r = reward;
                    if(mode == 1)r = Math.Exp(i - this.actionPlanTraces.Count + 1) * reward;
                    matchedRecords.ForEach(mr =>
                    {
                        mr.evulation += r;
                        mr.childs.ForEach(mrc => mrc.evulation += r);
                    });
                }
                if (mode == REWARD_ONCE) return;
            }
        }
        #endregion

        #region 反向推理
        /*
        /// <summary>
        /// 评判
        /// </summary>
        private void judge(int time)
        {
            //这是该函数将得到的结果，第一项是推理链，第二项是对每一个动作Id，其选择的值和推理路径
            List<(InferenceChain chain, Dictionary<int, (double, int[])>)> judgeResultList = new List<(InferenceChain chain, Dictionary<int, (double, int[])>)>();

            
            //拷贝评判项和权重，做好一项就删除一项
            List<JudgeGene> judges = new List<JudgeGene>(this.genome.judgeGenes);
            List<double> ws = judges.ConvertAll(j=>j.weight);
            //对每项做评判
            while (judges.Count > 0)
            {
                int judgeIndex = ws.argmax();
                JudgeGene judgeItem = judges[judgeIndex];
                double juegeItemWeight = ws[judgeIndex];

                (var var1, var var2) = doJudge(judgeItem);
                if (var1 != null)
                {
                    judgeResultList.Add((var1, var2));  
                }
                ws.RemoveAt(judgeIndex);
                judges.RemoveAt(judgeIndex);
            }
            //没有得到有效评判结果（初始的时候所有节点都没有值）
            if(judgeResultList.Count<=0)
            {
                this.Effectors.ForEach(e => e.randomValue(this,time));
                this.currentActionTraces = null;
                this.currentInferenceChain = null;
                this.judgeTime = time;
                return;
            }
            //对每一个评判的结果一个评分：进行正向推断，选择距离容忍界限最近的
            List<double> errors = new List<double>();
            for(int i=0;i<judgeResultList.Count;i++)
            {
                double error = 0;
                if(judgeResultList[i].chain == null)
                {
                    errors.Add(10000);
                    continue;
                }
                for(int j=0;j< judgeResultList.Count;j++)
                {
                    Inference inf = (Inference)this.getNode(judgeResultList[j].chain.head.referenceNode);
                    List<int> condids = ((InferenceGene)inf.Gene).getConditions().ConvertAll(x => x.Item1);
                    List<Vector> condValues = new List<Vector>();
                    for (int k = 0; k < condids.Count; k++)
                    {
                        Node node = this.getNode(condids[k]);
                        if (!node.Group.StartsWith("action"))
                            condValues.Add(node.Value);
                        else
                        {
                            int actionId = this.getActionIdByName(node.Name.Substring(1));
                            double val = judgeResultList[i].Item2[actionId].Item1;
                            condValues.Add(new Vector(new double[] { val }));
                        }
                    }
                    Vector varValue = inf.forwardinference(condValues);
                    double varlen = varValue.length();
                    double expectlen = judgeResultList[i].chain.varValue;
                    if (judgeResultList[i].chain.juegeItem.expression == "argmax")
                    {
                        if (varlen >= expectlen) error += 0;
                        else error += System.Math.Abs(varlen - expectlen);
                    }
                    else
                    {
                        if (varlen <= expectlen) error += 0;
                        else error += System.Math.Abs(varlen - expectlen);

                    }
                    int judgeIndex = this.genome.judgeGenes.IndexOf(judgeResultList[i].chain.juegeItem);
                    error *= this.genome.judgeGenes[judgeIndex].weight;
                    errors.Add(error);
                }
            }

            //选择误差最小的推理
            int minerrorIndex = errors.argmin();
            this.currentInferenceChain = judgeResultList[minerrorIndex].chain;
            this.currentActionTraces = judgeResultList[minerrorIndex].Item2;

            if (currentActionTraces == null || currentActionTraces.Count <= 0)
            {
                this.Effectors.ForEach(e => e.activate(this, time, 0));
            }
            else
            {
                //计算效应器输出
                foreach (int key in this.currentActionTraces.Keys)
                {
                    double value = this.currentActionTraces[key].Item1;
                    Effector effector = getEffectorByActionSensorId(key);
                    effector.activate(this, time, new Vector(value));

                }
            }
            
            this.judgeTime = time;
        }
        private (InferenceChain chain, Dictionary<int, (double, int[])> actionValues) doJudge(JudgeGene judgeItem)
        { 
            List<int> judge_conditions = judgeItem.conditions;
            double judge_variableValue = judgeItem.expression == "argmax" ? double.MinValue : double.MaxValue;

            //找到所有包含推理变量（后置）的推理项
            List<Node> varInferences = this.Inferences.FindAll(inf => ((InferenceGene)inf.Gene).getVariable().Item1 == judgeItem.variable);
            if (varInferences == null || varInferences.Count <= 0) return (null,null);

            //选择一个最合适的根推理
            Inference rootInference = null;
            List<Vector> rootInferenceValues = null;
            int rootInferenceVarId = 0, rootInferenceVarIndex = -1;
            int rootInferenceRecordIndex = -1;
            for (int j = 0; j < varInferences.Count; j++)
            {
                (List<Vector> values, int varId, double value,int recordindex) = ((Inference)varInferences[j]).arginference(judgeItem.expression);
                if (values == null) continue;
                if (judgeItem.expression == "argmax" && value > judge_variableValue)
                {
                    judge_variableValue = value;
                    rootInference = (Inference)varInferences[j];
                    rootInferenceValues = values;
                    rootInferenceRecordIndex = recordindex;
                }
                else if (judgeItem.expression == "argmin" && value < judge_variableValue)
                {
                    judge_variableValue = value;
                    rootInference = (Inference)varInferences[j];
                    rootInferenceValues = values;
                    rootInferenceRecordIndex = recordindex;
                }
            }
            if (rootInference == null)
                return (null, null);
            rootInferenceVarId = ((InferenceGene)rootInference.Gene).getVariable().Item1;
            rootInferenceVarIndex = ((InferenceGene)rootInference.Gene).getVariableIndex();

            //在选择的根推理上逐级回溯构造推理链
            InferenceChain chain = new InferenceChain()
            {
                juegeItem = judgeItem,
                varValue = judge_variableValue,
                head = new InferenceChain.Item()
                {
                    referenceNode = rootInference.Id,
                    referenceRecordIndex = rootInferenceRecordIndex,
                    values = rootInferenceValues,
                    varIndex = rootInferenceVarIndex,
                    varTime = 0
                }
            };

            
            chain = do_reverse_inference(chain,chain.head);
            this.currentInferenceChain = chain;



            //在推理链上选择要执行的动作
            List<int> actionSensorIds = this.ActionReceptors.ConvertAll(r => r.Id);
            Dictionary<int, (double,int[])> actionValues = new Dictionary<int, (double, int[])>();

            for(int k=0;k<actionSensorIds.Count;k++)
            {
                List<List<int>> traces = chain.findActionTrace(this,actionSensorIds[k]);
                if(traces == null || traces.Count<=0) //没有推理到该动作，给一个随机值
                {
                    double min = Session.GetConfiguration().agent.receptors.actions[k].Range.Min;
                    double max = Session.GetConfiguration().agent.receptors.actions[k].Range.Max;
                    double value = 0.5;//new Random().NextDouble() * (max - min) + min;
                    actionValues.Add(actionSensorIds[k], (value, null));
                }
                else
                {
                    //随机选择一个推理迹
                    int traceIndex = new Random().Next(0,traces.Count);
                    double value = chain.getValueFromTrace(this,actionSensorIds[k], 0, traces[traceIndex]);
                    double min = Session.GetConfiguration().agent.receptors.actions[k].Range.Min;
                    double max = Session.GetConfiguration().agent.receptors.actions[k].Range.Max;
                    value = Math.Min(Math.Max(value, min), max);
                    actionValues.Add(actionSensorIds[k], (value, traces[traceIndex].ToArray()));
                }
            }
            return (chain, actionValues);
        }

        

        /// <summary>
        /// 反向推理
        /// </summary>
        /// <param name="chain">当前推理链</param>
        /// <param name="item">当前推理项</param>
        /// <returns></returns>
        private InferenceChain do_reverse_inference(InferenceChain chain, InferenceChain.Item item)
        {
            //1.取得记忆项
            Inference inf = (Inference)this.getNode(item.referenceNode);

            //2取得与该记忆项的条件匹配的其他记忆项（即以inf的前置条件作为后置变量的所有记忆节点）
            ////2.1取得inf的条件部分
            List<int> condids = ((InferenceGene)inf.Gene).getConditions().ConvertAll(c => c.Item1);
            List<int> condTimes = ((InferenceGene)inf.Gene).getConditions().ConvertAll(c => c.Item2);
            ////2.2遍历所有记忆节点，查找满足条件的
            List<Node> childInfs = this.Inferences.FindAll(f => ((InferenceGene)f.Gene).matchVariable(condids.ToArray()));
            if (childInfs == null || childInfs.Count <= 0)
                return chain;

            //3去掉在推理轨迹上已经出现的记忆节点
            for (int i=0;i< childInfs.Count;i++)
            {
                if(inferenceInChainTrace(chain,item,(Inference)childInfs[i]))
                {
                    childInfs.RemoveAt(i--);
                }
            }
            if (childInfs == null || childInfs.Count <= 0)
                return chain;

            //4 深度遍历
            for(int i=0;i< childInfs.Count;i++)
            {
                //记忆节点基本信息
                Inference cinf = (Inference)childInfs[i];
                int varIndex = cinf.getGene().getVariableIndex();
                (int varid, int vartime) = cinf.getGene().getVariable();
                Vector varValue = item.values[item.varIndex];
                (int t1, int t2) = cinf.getGene().getTimeDiff();
                //记忆节点中的记忆记录的后置变量维的值与上一个推理获得的值最接近的
                IntegrationRecord cinfRecord =  cinf.getNearestRecord(varIndex,varValue);
                if (cinfRecord == null) continue;
                
                //在记忆记录附近采样
                int sampleCount = Session.GetConfiguration().agent.inferencesamples;
                List<List<Vector>> cinf_samples = cinfRecord.sample(sampleCount);
                List<double> distances = cinf_samples.ConvertAll(s => s[varIndex].distance(varValue));
                List<Vector> samplesSelected = cinf_samples[distances.argmin()];

                //创建新的推理项
                InferenceChain.Item newItem = new InferenceChain.Item()
                {
                    referenceNode = cinf.Id,
                    referenceRecordIndex = cinf.Records.IndexOf(cinfRecord),
                    values = samplesSelected,
                    varIndex = varIndex,
                    varTime = (t1==t2)?item.varTime:item.varTime+1,
                    prev = item
                };
                item.next.Add(newItem);

                //深度递归
                chain = do_reverse_inference(chain, newItem);
            }

            
            return chain;

        }

        /// <summary>
        /// 在从head到item的推理路径上是否有inf出现
        /// </summary>
        /// <param name="chain"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool inferenceInChainTrace(InferenceChain chain,InferenceChain.Item item,Inference inf)
        {
            if (item == null) return false;
            if (item.referenceNode == inf.Id) return true;
            while(item != null)
            {
                if (item.referenceNode == inf.Id) return true;
                item = item.prev;
            }
            return false;
        }
        /// <summary>
        /// 处理接受到的奖励，相当于适应度（-1到1之间）
        /// 根据适应度，设定推理路径上的各个推断节点和处理节点的可靠度
        /// </summary>
        /// <param name="reward"></param>
        public void setRewardInInferenceChain(double reward)
        {
            if (currentInferenceChain == null) return;
            foreach(int key in currentActionTraces.Keys)
            {
                if (this.currentActionTraces[key].Item2 == null) continue;
                int count = this.currentActionTraces[key].Item2.Length;
                double avg = reward / (count + 1);
                List<InferenceChain.Item> items = currentInferenceChain.getItemsFromTrace(this.currentActionTraces[key].Item2);
                if (items == null || items.Count <= 0) continue;
                for (int j = 0; j < items.Count; j++)
                {
                    Node node = this.nodes.FirstOrDefault(n => n.Id == items[j].referenceNode);
                    node.Reability = node.Reability + avg;
                }
            }
            
        }
        */
        #endregion



    }
}
