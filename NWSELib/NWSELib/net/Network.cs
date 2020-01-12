
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

        #region 记忆信息
        /*
        private class MemoryItem
        {
            public int timeScale = 1;
            public int beginTime = -1;
            public Queue<Vector> records = new Queue<Vector>();
            public MemoryItem() { }
            public MemoryItem(int timeScale) { this.timeScale = timeScale; }
        }

        

        private MemoryItem[] memories;

        public void putMemoryItem(int nodeIndex, int time, Vector value)
        {

            if (memories[nodeIndex].beginTime < 0)
                memories[nodeIndex].beginTime = time;
            memories[nodeIndex].records.Enqueue(value);
        }
        public Vector getMemoryItem(int nodeIndex, int time)
        {
            if (memories[nodeIndex].beginTime < 0)
                return null;
            int index = (time - memories[nodeIndex].beginTime) / memories[nodeIndex].timeScale;
            Vector[] vs = memories[nodeIndex].records.ToArray();
            return index < vs.Length ? vs[index] : null;
        }

        

        public List<Vector> getNodeMemory(int nodeIndex)
        {
            return new List<Vector>(memories[nodeIndex].records.ToArray());
        }
        */
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
        /// 当前推理链
        /// </summary>
        public InferenceChain currentInferenceChain;
        /// <summary>
        /// 动作输出和推理迹
        /// </summary>
        public Dictionary<int, (double, int[])> currentActionTraces = new Dictionary<int, (double, int[])>();

        /// <summary>
        /// 推理发生时间
        /// </summary>
        private int judgeTime;

        /// <summary>
        /// 行动计划链
        /// </summary>
        public ActionPlanChain actionPlanChain;

        /// <summary>
        /// 显示推理过程
        /// </summary>
        /// <returns></returns>
        public String getInferenceChainText()
        {
            if (currentInferenceChain == null) return "";
            StringBuilder str = new StringBuilder();
            str.Append("...1.JudgeMent:" + currentInferenceChain.juegeItem.Text+System.Environment.NewLine);
            Inference inf = (Inference)this.getNode(currentInferenceChain.head.referenceNode);
            str.Append("...2.For achieving the judgment goal,the inference " + inf.Id + " was uesed and the expection value is "+ currentInferenceChain.varValue.ToString()+":" + inf.Gene.Text+System.Environment.NewLine);
            int xh = 3;
            for(int i=0;i<this.ActionReceptors.Count;i++)
            {
                Receptor receptor = (Receptor)this.ActionReceptors[i];
                (double, int[]) trace = currentActionTraces[receptor.Id];
                if(trace.Item2 == null || trace.Item2.Length<=0)
                {
                    str.Append("..." + (xh + i).ToString() + ". action " + receptor.Gene.Text + "'s fitness value is " + trace.Item1.ToString()+System.Environment.NewLine);
                    continue;
                        
                }
                str.Append("..." + (xh + i).ToString() + ". action " + receptor.Gene.Text + "'s inference trace is:"+ System.Environment.NewLine);
                List<InferenceChain.Item> items = currentInferenceChain.getItemsFromTrace(trace.Item2);
                foreach(InferenceChain.Item item in items)
                {
                    str.Append("        " + this.getNode(item.referenceNode).Gene.Text);
                }
                str.Append("        action " + receptor.Gene.Text + "'s fitness value is " + trace.Item1.ToString() + System.Environment.NewLine);

            }
            return str.ToString();
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
        public List<double> activate(List<double> obs, int time)
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
            judge(time);


            //取出输出节点
            List<double> actions = this.Effectors.ConvertAll<double>(n => (double)n.Value);
            for(int i=0;i< this.ActionReceptors.Count;i++)
            {
                this.ActionReceptors[i].activate(this, time, actions[i]);
            }
            return actions;
        }

        #region 回忆和推理
        private void judge2(int time)
        {
            //如果当前行动计划不空
            if(actionPlanChain != null && actionPlanChain.curPlanItem != null)
            {
                //检查行动实际结果与预期的匹配程度
                actionPlanChain.curPlanItem.reals = this.getOutputValues(actionPlanChain.curPlanItem.owner.inference,time);
                actionPlanChain.curPlanItem.distance = Vector.manhantan_distance(actionPlanChain.curPlanItem.reals, actionPlanChain.curPlanItem.expects);
                if(actionPlanChain.curPlanItem.distance <= Session.GetConfiguration().learning.inference.env_distance)
                {
                    //两者接近，本次行动成功，设置奖励
                    actionPlanChain.curPlanItem.owner.inference.Reability += 0.1;
                    //进行下一次行动
                    if (actionPlanChain.curPlanItem.selected >=0)
                    {
                        ActionPlan nextPlan = actionPlanChain.curPlanItem.childs[actionPlanChain.curPlanItem.selected];
                        actionPlanChain.curPlanItem = nextPlan.items[nextPlan.selected];
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
                }
            }
            //计算行动计划链
            this.actionPlanChain = doActionPlan(time);
            //选择行动记录评估值最高的
            doSelectActionPlan(this.actionPlanChain);
            //根据行动计划设置输出
            setEffectValue(time);

        }

        private void doSelectActionPlan(ActionPlanChain chain)
        {
            if (chain == null) return;
            double max = double.MinValue;
            ActionPlan.Item item = null;
            for (int i=0;i<chain.roots.Count;i++)
            {
               doSelectActionPlan(chain, chain.roots[i],ref max,ref item);
            }

            
            
        }
        private void setSelectedIndex(ActionPlanChain chain,ActionPlan.Item item)
        {
            if (item == null) return;
            item.selected = -1;
            item.owner.selected = item.owner.items.IndexOf(item);

            if(item.owner.prev == null)
            {
                chain.selected = chain.roots.IndexOf(item.owner);
                return;
            }
            for(int i=0;i< item.owner.prev.items.Count;i++)
            {
                int index = item.owner.prev.items[i].childs.IndexOf(item.owner);
                if(index>=0)
                {
                    setSelectedIndex(chain, item.owner.prev.items[i]);
                    break;
                }
            }
        }
        private void doSelectActionPlan(ActionPlanChain chain, ActionPlan plan, ref double max,ref  ActionPlan.Item maxitem)
        {
            
            if (plan == null) return;
            if (plan.items.Count <= 0) return;
            
            for(int i=0;i<plan.items.Count;i++)
            {
                if(plan.items[i].evaulation > max)
                {
                    max = plan.items[i].evaulation;
                    maxitem = plan.items[i];
                }
            }
            for(int i=0;i<plan.items.Count;i++)
            {
                if (plan.items[i].childs.Count <= 0) continue;
                for(int j=0;j< plan.items[i].childs.Count;j++)
                {
                    doSelectActionPlan(chain, plan.items[i].childs[j], ref max, ref maxitem);
                }
            }

        }
        /// <summary>
        /// 根据行动计划设定输出
        /// </summary>
        private void setEffectValue(int time)
        {
            //没有行动计划，设置随机动作
            if(this.actionPlanChain == null)
            {
                for (int i = 0; i < this.Effectors.Count; i++)
                    this.Effectors[i].randomValue(this, time);
            }else if(this.actionPlanChain.curPlanItem == null)
            {
                ActionPlan plan = this.actionPlanChain.roots[this.actionPlanChain.selected];
                this.actionPlanChain.curPlanItem = plan.items[plan.selected];
                
            }
            for (int i = 0; i < this.Effectors.Count; i++)
            {
                this.Effectors[i].activate(this, time, this.actionPlanChain.curPlanItem.actions[i]);
            }
        }
        private ActionPlanChain doActionPlan(int time)
        {
            ActionPlanChain chain = null;
            for (int i=0;i<this.Inferences.Count;i++)
            {
                //取得推理节点的真实环境输入
                List<Vector> inputValues = this.getInputValues((Inference)this.Inferences[i],time);
                if (inputValues == null || inputValues.Count <= 0) continue;

                //根据真实输入找到最相似的记录（记录，相似度）
                (InferenceRecord record,double similarity) = recall((Inference)this.Inferences[i], inputValues);
                if (record == null) continue;
                if (similarity < Session.GetConfiguration().learning.inference.inference_distance)
                    continue;

                if (chain == null) chain = new ActionPlanChain();

                ActionPlan plan = new ActionPlan();
                plan.inference = (Inference)Inferences[i];
                plan.conditions = inputValues;
                plan.record = record;
                plan.similarity = similarity;
                chain.roots.Add(plan);

                doForcast(time, chain, plan);
                
            }
            return chain;
        }
        /// <summary>
        /// 取得推理节点的输入
        /// </summary>
        /// <param name="inf"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private List<Vector> getInputValues(Inference inf, int time)
        {
            List<Node> inputs = this.getInputNodes(inf.Id);
            List<Vector> r = inputs.ConvertAll(i => i.GetValue(time));
            return r.Contains(null) ? null : r;
        }
        private ActionPlanChain doForcast(int time,ActionPlanChain chain,ActionPlan plan)
        {
            List<Inference> nextinfs = this.getNextInferences(plan.inference);
            //这个推理的前提上有多少个动作
            List<Node> actionReceptors = getActionSensors(plan.inference);
            if (actionReceptors == null || actionReceptors.Count <= 0)
                return chain;
            //这个动作上所有可能值的组合
            List<List<Vector>> actionComposites = this.createActionComposites(actionReceptors);

            //对每一个动作组合做预测
            for(int i=0;i< actionComposites.Count;i++)
            {
                List<Vector> actions = actionComposites[i];
                List<Vector> condValues = plan.inference.createConditions(plan.conditions, actions);
                List<Vector> results = plan.record.forward_inference(plan.inference,condValues);
                ActionPlan.Item actionItem = new ActionPlan.Item();
                actionItem.actions = actions;
                actionItem.expects = results;
                actionItem.owner = plan;
                actionItem.evaulation = doEvaulation(plan,actionItem);
                plan.items.Add(actionItem);

                if (nextinfs == null || nextinfs.Count <= 0) continue;
                for(int j=0;j<nextinfs.Count;j++)
                {
                    if (!plan.exist(nextinfs[j])) continue;
                    List<Vector> inputValues = computeInput(plan.inference, results, nextinfs[j]);
                    if (inputValues == null || inputValues.Count <= 0) continue;
                    (InferenceRecord record, double similarity) = recall(nextinfs[j], inputValues);
                    if (record == null) continue;
                    if (similarity < Session.GetConfiguration().learning.inference.inference_distance)
                        continue;

                    ActionPlan plan2 = new ActionPlan();
                    plan2.inference = nextinfs[j];
                    plan2.conditions = inputValues;
                    plan2.record = record;
                    plan2.similarity = similarity;
                    actionItem.childs.Add(plan2);

                    doForcast(time, chain, plan2);
                }
            }
            return chain;
        }
        /// <summary>
        /// 判断执行这组动作得到的预期评判
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="actionItem"></param>
        /// <returns></returns>
        private double doEvaulation(ActionPlan plan, ActionPlan.Item actionItem)
        {
            List<int> varIds = plan.inference.getGene().getVariables();
            List<double> evualtions = new List<double>();
            for(int i=0;i<this.Genome.judgeGenes.Count;i++)
            {
                int index = varIds.IndexOf(this.Genome.judgeGenes[i].variable);
                //预期结果中不包含评判项，
                if (index<0)
                {
                    continue;
                }
                Vector v = actionItem.expects[index];
                double value = v.length();
                double expect = 0;
                if (this.Genome.judgeGenes[i].expression == "argmax")
                    expect = 1.0;
                evualtions.Add((1 - Math.Abs(expect - value))* this.Genome.judgeGenes[i].weight);
                actionItem.judgeItems.Add(this.Genome.judgeGenes[i]);
            }
            return evualtions.Sum();
        }
        /// <summary>
        /// 计算所有的动作组合
        /// </summary>
        /// <param name="actionReceptors"></param>
        /// <returns></returns>
        public List<List<Vector>> createActionComposites(List<Node> actionReceptors)
        {
            List<int> counts = actionReceptors.ConvertAll(a => (int)Session.GetConfiguration().agent.receptors.GetSensor(a.Name).Level.Min);
            List<double> units = actionReceptors.ConvertAll(a => (Session.GetConfiguration().agent.receptors.GetSensor(a.Name).Range.Distance)/(Session.GetConfiguration().agent.receptors.GetSensor(a.Name).Level.Min));
            List<List<Vector>> r = new List<List<Vector>>();


            if(actionReceptors.Count == 1)
            {
                for(int i=0;i<counts[0];i++)
                {
                    List<Vector> t1 = new List<Vector>();
                    t1.Add(new Vector((i* units[0]+(i+1)*units[0])/2));
                    r.Add(t1);
                }
            }else
            {
                for (int i = 0; i < counts[0]; i++)
                {
                    for(int j=0;j<counts[1];j++)
                    {
                        List<Vector> t1 = new List<Vector>();
                        t1.Add(new Vector((i * units[0] + (i + 1) * units[0]) / 2));
                        t1.Add(new Vector((j * units[1] + (j + 1) * units[1]) / 2));
                        r.Add(t1);
                    }
                }
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
            List<int> postVarIds = inference.getGene().getVariables();
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
            InferenceRecord r = null;
            double maxsimilarity = double.MinValue;
            for (int i=0;i< inference.Records.Count;i++)
            {
                List<Vector> center = inference.Records[i].means;
                List<Vector> clone = new List<Vector>(center);
                clone = inference.replaceEnvValue(clone, envValues);
                double similarity = inference.Records[i].prob(clone) / inference.Records[i].prob(center);
                if(similarity > maxsimilarity)
                {
                    maxsimilarity = similarity;
                    r = inference.Records[i];
                }
            }
            return (r, maxsimilarity);
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
            List<int> infVarIds = inference.getGene().getVariables();
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
            List<int> varIds = inference.getGene().getVariables();
            List<Vector> vs = varIds.ConvertAll(id => this.getNode(id).GetValue(time));
            return vs.Contains(null) ? null : vs;
        }
        #endregion

        #region 反向推理
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
        #endregion


        /// <summary>
        /// 处理接受到的奖励，相当于适应度（-1到1之间）
        /// 根据适应度，设定推理路径上的各个推断节点和处理节点的可靠度
        /// </summary>
        /// <param name="reward"></param>
        public void setReward(double reward)
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
    }
}
