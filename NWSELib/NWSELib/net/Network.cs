
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
        #endregion

        

        

        #region 节点查询
        /// <summary>
        /// 所有感知节点
        /// </summary>
        public List<Node> Receptors
        {
            get => nodes.FindAll(n => n is Receptor);
        }
        /// <summary>
        /// 所有环境感知节点
        ///</summary>
        public List<Node> EnvReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("env"));
        }
        /// <summary>
        /// 所有姿态感知节点
        ///</summary>
        public List<Node> GesturesReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("gestures"));
        }
        /// <summary>
        /// 所有动作感知节点
        ///</summary>
        public List<Node> ActionReceptors
        {
            get => nodes.FindAll(n => n is Receptor && n.Group.StartsWith("action"));
        }


        /// <summary>
        /// 所有处理节点
        /// </summary>
        public List<Node> Handlers
        {
            get => nodes.FindAll(n => n is Handler);
        }

        public List<Node> Inferences
        {
            get => nodes.FindAll(n => n is Inference);
        }

        /// <summary>
        /// 效应器
        /// </summary>
        public List<Node> Effectors
        {
            get => nodes.FindAll(n => n is Effector);
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
        #endregion

        #region 初始化
        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.nodes.ForEach(a => a.Reset());
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

            
        }

        #endregion

        #region 推理状态
        
        /// <summary>
        /// 推理发生时间
        /// </summary>
        private int judgeTime;

        /// <summary>
        /// 行动计划轨迹
        /// </summary>
        public List<ActionPlan> actionPlanTraces = new List<ActionPlan>();

        /// <summary>
        /// 行动计划链
        /// </summary>
        public ActionPlan rootActionPlan;
        /// <summary>
        /// 当前正在执行的行动计划
        /// </summary>
        public ActionPlan curActionPlan;

        
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
            //初始化
            Reset();
            //初始化所有感知节点
            for (int i = 0; i < obs.Count; i++)
            {
                this.Receptors[i].activate(this, time, obs[i]);
            }

            //反复执行直到都激活
            while (!this.Handlers.All(n => n.IsActivate(time)))
            {
                this.Handlers.ForEach(n => n.activate(this, time, null));
            }

            //激活推理节点
            for(int i=0;i<this.Inferences.Count;i++)
            {
                this.Inferences[i].activate(this, time);
            }
            //进行评判
            judge2(session,time);


            //取出输出节点
            List<double> actions = this.Effectors.ConvertAll<double>(n => (double)n.Value);
            for(int i=0;i< this.ActionReceptors.Count;i++)
            {
                this.ActionReceptors[i].activate(this, time, actions[i]);
            }
            return actions;
        }

        #region 回忆和推理
        private void judge2(Session session,int time)
        {
            //如果当前行动计划不空
            if(this.curActionPlan != null)
            {
                //检查行动实际结果与预期的匹配程度
                curActionPlan.reals = this.getOutputValues(curActionPlan.inference, time);
                curActionPlan.distance = Vector.manhantan_distance(curActionPlan.reals, curActionPlan.expects);
                if(curActionPlan.distance <= Session.GetConfiguration().learning.judge.tolerable_similarity * curActionPlan.reals.size())
                {
                    //两者接近，本次行动成功，设置奖励
                    curActionPlan.record.accuracy += 0.1;
                    //进行下一次行动
                    if (curActionPlan.selected >=0)
                    {
                        ActionPlan nextPlan = curActionPlan.childs[curActionPlan.selected];
                        curActionPlan = nextPlan;
                        setEffectValue(time);
                        return;
                    }
                    else
                    {
                        //本次行动执行完毕
                    }
                }
                else
                {
                    //执行与预期出入较大
                    curActionPlan.record.accuracy -= 0.1;
                }
            }
            //以一定的概率探索
            if(rng.NextDouble()>=Session.GetConfiguration().learning.eplison)
            {
                this.rootActionPlan = null;
                this.curActionPlan = null;
                setEffectValue(time);
                return;
            }

            //计算行动计划树
            this.curActionPlan = null;
            this.rootActionPlan = doActionPlan(time);
            if (this.rootActionPlan == null)
            {
                setEffectValue(time);
                return;
            }
            //选择行动记录评估值最高的
            double maxEvaluation = doSelectActionPlan(this.rootActionPlan);
            
            //根据行动计划设置输出
            setEffectValue(time, maxEvaluation<-1.0);

        }

        private double doSelectActionPlan(ActionPlan plan)
        {
            //查找行为链中的最大评估值
            if (plan == null) return 0;
            double max = plan.record.evulation;
            ActionPlan maxplan = plan;
            for (int i=0;i< plan.childs.Count; i++)
            {
               doSelectActionPlan(plan.childs[i],ref max,ref maxplan);
            }
            if (maxplan == null) return max;
            
            ActionPlan temp = maxplan;
            while(temp.parent != null)
            {
                temp.parent.selected = temp.parent.childs.IndexOf(temp);
                temp = temp.parent;
            }
            return max;
        }
        
        private void doSelectActionPlan(ActionPlan plan, ref double max,ref  ActionPlan maxPlan)
        {
            
            if (plan == null) return;
            if(plan.record.evulation > max)
            {
                max = plan.record.evulation;
                maxPlan = plan;
            }
            if (plan.childs.Count <= 0) return;
            
            for(int i=0;i<plan.childs.Count;i++)
            {
                if(plan.childs[i].record.evulation > max)
                {
                    max = plan.childs[i].record.evulation;
                    maxPlan = plan.childs[i];
                }

                doSelectActionPlan(plan.childs[i], ref max, ref maxPlan);
            }
        }
        public String showActionPlan()
        {
            StringBuilder str = new StringBuilder();
            if (this.rootActionPlan == null)
            {
                if (this.actionPlanTraces.Count > 0 &&
                    this.actionPlanTraces.Last().record == null &&
                    this.actionPlanTraces.Last().instinct)
                    str.Append("行动方式=本能行为" + System.Environment.NewLine);
                else str.Append("行动方式=随机探索" + System.Environment.NewLine);
                str.Append("   行为=");
                str.Append(showActionText() + System.Environment.NewLine);
            }
            else
            {
                str.Append("行动方式=" + this.rootActionPlan.Depth.ToString() + "步规划" + System.Environment.NewLine);
                str.Append("   行为=");
                str.Append(showActionText() + System.Environment.NewLine);

                str.Append(this.rootActionPlan.ToString(this.curActionPlan) + System.Environment.NewLine);
            }


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
            
            if(this.rootActionPlan == null)
            {
                List<double> actions = null;
                bool instinct = false;
                if (!random)
                {
                    actions = Session.instinctActionHandler(this, time);
                    instinct = true;
                    for (int i = 0; i < this.Effectors.Count; i++)
                        this.Effectors[i].activate(this, time, actions[i]);
                }
                if (actions == null)
                {
                    for (int i = 0; i < this.Effectors.Count; i++)
                        this.Effectors[i].randomValue(this, time);
                }
                this.actionPlanTraces.Add(new ActionPlan()
                {
                    actions = this.Effectors.ConvertAll(e=>e.Value),
                    instinct = instinct
                });
                return;
            }else if(this.curActionPlan == null)
            {
                this.curActionPlan = this.rootActionPlan;
                this.actionPlanTraces.Add(this.curActionPlan);
            }

            curActionPlan.record.usedCount += 1;
            for (int i = 0; i < this.Effectors.Count; i++)
            {
                this.Effectors[i].activate(this, time, this.curActionPlan.actions[i]);
            }
        }

        
        private ActionPlan doActionPlan(int time, ActionPlan root = null,ActionPlan plan =null)
        {
            Inference select_inf = null;
            InferenceRecord select_record = null;
            double select_record_similarity = double.MinValue;
            double select_record_evulation = double.MinValue;
            List<Vector> select_record_inputValues = null;

            for (int i=0;i<this.Inferences.Count;i++)
            {
                //取得推理节点的真实环境输入
                List<Vector> inputValues = null;
                if (plan == null)
                    inputValues = this.getValues(((Inference)this.Inferences[i]).getIdList());
                else
                    inputValues = this.getValues(plan.inference.getGene().getVariableIds(),plan.expects, (Inference)this.Inferences[i]);
                if (inputValues == null || inputValues.Count <= 0) continue;

                //根据真实输入找到最相似的记录（记录，相似度）
                (InferenceRecord record,double similarity) = recall((Inference)this.Inferences[i], inputValues);
                if (record == null) continue;
                
                
                if(plan != null)
                {
                    //如果找到的记录，评估是负的，则跳过
                    if (record.evulation < 0 && record.accuracy > 0) continue;
                    ActionPlan cplan = new ActionPlan(this,(Inference)this.Inferences[i], record, similarity,inputValues);
                    plan.childs.Add(cplan);
                    cplan.parent = plan;
                    continue;
                }

                if(select_record_evulation < record.evulation)
                {
                    select_inf = (Inference)this.Inferences[i];
                    select_record = record;
                    select_record_similarity = similarity;
                    select_record_evulation = record.evulation;
                    select_record_inputValues = inputValues;
                }
            }

            if(root == null)
            {
                if (select_inf == null) return root;
                root = new ActionPlan(this,select_inf, select_record, select_record_similarity, select_record_inputValues);
                
                return doActionPlan(time, root, root);
            }

            if (root.Depth >= 3) return root;
            if (plan.childs.Count <= 0) return root;
            for(int i=0;i<plan.childs.Count;i++)
            {
                root = doActionPlan(time, root, plan.childs[i]);
            }
            return root;
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
        /// <summary>
        /// 给定一组值，从中选择与inf的条件部分匹配的
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="values"></param>
        /// <param name="inf"></param>
        /// <returns></returns>
        public List<Vector> getValues(List<int> ids,List<Vector> values, Inference inf)
        {
            List<int> infcondids = inf.getGene().getConditions().ConvertAll(x => x.Item1);
            if (!Utility.ContainsAll(ids, infcondids))
                return null;

            List<Vector> r = new List<Vector>();
            for(int i=0;i< infcondids.Count;i++)
            {
                int index = ids.IndexOf(infcondids[i]);
                r.Add(values[index]);
            }
            return r;
        }

        
        /// <summary>
        /// 取得inf中动作感知部分的节点
        /// </summary>
        /// <param name="inf"></param>
        /// <returns></returns>
        public List<Node> getActionSensors(Inference inf)
        {
            List<int> ids = inf.getGene().getActionSensorsConditions();
            if (ids == null) return new List<Node>();
            return ids.ConvertAll(id => this.getNode(id));
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

        public void setReward(double reward,int mode = 1,bool clear=true)
        {
            if (this.actionPlanTraces.Count <= 0) return;
            if(mode == 1)//指数下降方式分配
            {
                for (int i = this.actionPlanTraces.Count - 1; i >= 0; i--)
                {
                    ActionPlan plan = this.actionPlanTraces[i];
                    if (plan == null || plan.record == null) continue;
                    plan.record.evulation += Math.Exp(i - this.actionPlanTraces.Count + 1) * reward;

                }
                ActionPlan p = actionPlanTraces.Last();
                actionPlanTraces.Clear();
                actionPlanTraces.Add(p);
            }else if(mode == 2)//平均分配
            {
                for (int i = this.actionPlanTraces.Count - 1; i >= 0; i--)
                {
                    ActionPlan plan = this.actionPlanTraces[i];
                    if (plan == null || plan.record == null) continue;
                    plan.record.evulation += reward;

                }
                ActionPlan p = actionPlanTraces.Last();
                actionPlanTraces.Clear();
                actionPlanTraces.Add(p);
            }
            else//只分配给最后一个
            {
                ActionPlan p = actionPlanTraces.Last();
                p.record.evulation += reward;
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
                InferenceRecord cinfRecord =  cinf.getNearestRecord(varIndex,varValue);
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
