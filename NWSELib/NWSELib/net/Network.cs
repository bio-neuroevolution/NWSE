
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.ML.Probabilistic.Distributions;
using NWSELib.common;
using NWSELib.env;
using NWSELib.genome;
using NWSELib.net.policy;
using NWSELib.net.handler;

namespace NWSELib.net
{
    /// <summary>
    /// 网络
    /// 
    /// </summary>
    public class Network
    {
        #region 基本信息
        public static Random rng = new Random();
        /// <summary>
        /// 染色体
        /// </summary>
        private NWSEGenome genome;
        /// <summary>
        /// 所有节点
        /// </summary>
        private List<Node> nodes = new List<Node>();

        /// <summary>
        /// 策略单元
        /// </summary>
        public Policy policy;
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
        public int Id { get => this.genome.id; }

        /// <summary>
        /// 策略单元
        /// </summary>
        public Policy Policy { get => this.policy; }
        

        /// <summary>
        /// 显示字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "net id="+Id.ToString()+",fitness="+
                this.Fitness.ToString("F4")+
                ",reability="+this.Reability.ToString("F4");
        }
        #endregion

        #region 节点查询
        private List<Receptor> _receptors;
        /// <summary>
        /// 所有感知节点
        /// </summary>
        public List<Receptor> Receptors
        {
            get => _receptors==null? _receptors=nodes.FindAll(n => n is Receptor).ConvertAll(n=>(Receptor)n): _receptors;
        }
        private List<Receptor> _envReceptors;
        /// <summary>
        /// 所有环境感知节点
        ///</summary>
        public List<Receptor> EnvReceptors
        {
            get => _envReceptors==null? _envReceptors= Receptors.FindAll(n => n.Gene.IsEnvSensor()).ConvertAll(n => (Receptor)n): _envReceptors;
        }

        

        private List<Receptor> _gesturesReceptors;
        /// <summary>
        /// 所有姿态感知节点
        ///</summary>
        public List<Receptor> GesturesReceptors
        {
            get => _gesturesReceptors==null? _gesturesReceptors= Receptors.FindAll(n => n.Gene.IsGestureSensor()).ConvertAll(n => (Receptor)n): _gesturesReceptors;
        }

        private List<Receptor> _actionReceptors;
        /// <summary>
        /// 所有动作感知节点
        ///</summary>
        public List<Receptor> ActionReceptors
        {
            get => _actionReceptors == null? _actionReceptors = Receptors.FindAll(n => n.Gene.IsActionSensor()).ConvertAll(n=>(Receptor)n): _actionReceptors;
        }

        public Node RewardReceptor
        {
            get => Receptors.FirstOrDefault(x => x.Gene.Group.Contains("reward"));
        }

        private List<MeasureTools> _gestureMeasureTools;

        public List<MeasureTools> GestureMeasureTools
        {
            get
            {
                if (_gestureMeasureTools == null)
                    _gestureMeasureTools = GesturesReceptors.ConvertAll(g => MeasureTools.GetMeasure(g.Cataory));
                return _gestureMeasureTools;
            }
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

        /// <summary>
        /// 根据Id或者名称访问节点
        /// </summary>
        /// <param name="idorname"></param>
        /// <returns></returns>
        public Node this[Object idorname]
        {
            get 
            {
                Node r = this.nodes.FirstOrDefault(n => n.Id.Equals(idorname));
                if (r != null) return r;
                return this.nodes.FirstOrDefault(n => n.Name.Equals(idorname));
            }
        }

        #endregion

        #region 初始化
       
        


        /// <summary>
        /// 构造函数
        /// </summary>
        public Network(NWSEGenome genome)
        {
            this.genome = genome;
            //初始化节点
            for (int i = 0; i < genome.receptorGenes.Count; i++)
            {
                Receptor receptor = new Receptor(genome.receptorGenes[i],this);
                this.nodes.Add(receptor);
            }
            var handlerDepths = genome.GetHanlerDepth();
            for(int d = handlerDepths.Item1;d<=handlerDepths.Item2;d++)
            {
                List<HandlerGene> handlerGenes = genome.handlerGenes.FindAll(g => g.Depth == d);
                foreach(HandlerGene handlerGene in handlerGenes)
                {
                    Handler handler = Handler.create(handlerGene, this);
                    this.nodes.Add(handler);
                }
            }
            
            for (int i = 0; i < genome.inferenceGenes.Count; i++)
            {
                Inference inference = new Inference(genome.inferenceGenes[i],this);
                this.nodes.Add(inference);
            }
            for (int i = 0; i < this.ActionReceptors.Count; i++)
            {
                Effector effector = new Effector(((ReceptorGene)this.ActionReceptors[i].Gene).toActionGene(),this);
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
            for (int i = 0; i < genome.inferenceGenes.Count; i++)
            {
                int k1 = idToIndex(genome.inferenceGenes[i].Id);
                for (int j = 0; j < genome.inferenceGenes[i].Dimensions.Count; j++)
                {
                    int k2 = idToIndex(genome.inferenceGenes[i].Dimensions[j]);
                    this.adjMatrix[k2, k1] = 1;
                }
            }

            actionPlanChain = new ActionPlanChain(this);
            actionMemory = new ObservationHistory(this);
            policy = new EmotionPolicy(this);
        }

        public List<Vector> CreateDefaultActions()
        {
            return new double[] { 0.5 }.ToList().ConvertAll(d=>new Vector(d));
        }

        #endregion

        #region 状态
        /** 最后一次获得的奖励 */
        public double reward;
        /// <summary>
        /// 行动记忆空间
        /// </summary>
        public ObservationHistory actionMemory;
        /// <summary>
        /// 行动计划链
        /// </summary>
        public ActionPlanChain actionPlanChain;

        /// <summary>
        /// 重置计算
        /// </summary>
        public void Reset()
        {
            this.nodes.ForEach(a => a.Reset());
            this.nodes.ForEach(n => n.think_reset());
        }

        public void ThinkReset()
        {
            this.nodes.ForEach(n => n.think_reset());
        }

        public void thinkHandles(int time)
        {
            List<Handler> handlers = this.Handlers;
            if (handlers == null || handlers.Count <= 0) return;
            while (!handlers.All(h => h.IsThinkCompleted(time)))
            {
                handlers.ForEach(h => h.think(this, time, null));
            }

        }

        #endregion

        #region 评价信息
        protected double fitness = double.NaN;
        /// <summary>
        /// 适应度
        /// </summary>
        public double Fitness { get => fitness; set => fitness = value; }
        /// <summary>
        /// 是否完成了任务
        /// </summary>
        public bool TaskCompleted;
        
        /// <summary>
        /// 网络可靠度
        /// </summary>
        public double Reability
        {
            get 
            {
                List<double> r = this.Inferences.ConvertAll(inf=>inf.Reability).FindAll(reability => !double.IsNaN(reability));
                if (r == null || r.Count <= 0) return double.NaN;
                return r.Average();
            }
        }

        
        public List<NodeGene> FindVaildInferences(bool newOnly=true,int validUpperLimit=2)
        {
            List<Inference> infs = this.Inferences.FindAll(inf => inf.ValidRecordCount>= validUpperLimit);
            if (infs == null || infs.Count <= 0) return new List<NodeGene>();
            List<NodeGene> genes = infs.ConvertAll(inf => inf.Gene);
            if (newOnly)genes = genes.FindAll(g=>!this.genome.isVaildGene(g));
            if (genes == null || genes.Count <= 0) return genes;

            for (int i=0;i<genes.Count;i++)
            {
                NodeGene gene = genes[i];
                List<NodeGene> temp = gene.getUpstreamGenes();
                if (temp == null || temp.Count<=0) continue;
                foreach(NodeGene t in temp)
                {
                    if (t is ReceptorGene) continue;
                    else if (genes.Contains(t)) continue;
                    genes.Add(t);
                }
            }

            return genes;
        }

       
        public List<Inference> FindInvaildInference(bool newOnly=true, int validUpperLimit = 2)
        {
            List<Inference> infs = this.Inferences.FindAll(inf => inf.ValidRecordCount < validUpperLimit);
            if (newOnly)
                infs = infs.FindAll(inf => !Session.IsInvaildGene(inf.GetGene()));
            return infs;
        }
        #endregion


        #region 网络活动
        public void PrepareTask(Session session)
        {
            Session.taskBeginHandler(this, session);
            this.Receptors.ForEach(r => r.Reset());
            this.Handlers.ForEach(h => h.Reset());
        }
        /// <summary>
        /// 激活
        /// </summary>
        /// <param name="obs"></param>
        /// <returns></returns>
        public List<double> Activate(List<double> obs, int time,Session session,double reward)
        {
            //0. 缓存奖励  
            this.reward = reward;
            if (this.actionPlanChain.Last != null)
                this.actionPlanChain.Last.reward = reward;

            //1.接收输入
            for (int i = 0; i < obs.Count; i++)
            {
                this.Receptors[i].Activate(this, time, obs[i]);
            }

            //2.处理感知
            var handlerDepth = this.genome.GetHanlerDepth();
            for(int d = handlerDepth.Item1;d<=handlerDepth.Item2;d++)
            {
                List<Handler> handlers = this.Handlers.FindAll(h => h.Gene.Depth == d);
                handlers.ForEach(n => n.Activate(this, time, null));
            }
            
            
            //3. 记忆整理
            for (int i=0;i<this.Inferences.Count;i++)
            {
                Inference inf = this.Inferences[i];
                inf.Activate(this, time);
            }

            //4. 对现有推理记录的准确性进行评估
            /*for (int i = 0; i < this.Inferences.Count; i++)
            {
                Inference inf = this.Inferences[i];
                inf.Records.ForEach(r => r.adjustAccuracy(time));
                inf.removeWrongRecords();
            }*/

            //5. 行为决策   
            this.reward = reward;
            if (this.actionPlanChain.Last != null)
                this.actionPlanChain.Last.reward = reward;

            String policyName = Session.GetConfiguration().evaluation.policy.name;
            policy.Execute(time,session);
                  
            setEffectValue(time);

            //6.记录行为
            List<double> actions = this.Effectors.ConvertAll<double>(n => (double)n.Value);
            for(int i=0;i< this.ActionReceptors.Count;i++)
            {
                this.ActionReceptors[i].Activate(this, time, actions[i]);
            }
           
            return actions;
        }
        /// <summary>
        /// 根据行动计划设定输出

        /// </summary>
        private void setEffectValue(int time, bool random = true)
        {
            for (int i = 0; i < this.Effectors.Count; i++)
            {
                this.Effectors[i].Activate(this, time, this.actionPlanChain.Last.actions[i]);
            }
        }

        public const int TESTACTION_SORT_UNIFORM = 0;
        public const int TESTACTION_SORT_INSTINCT = 1;
        public const int TESTACTION_SORT_MAINTAIN = 2;

        public List<List<double>> CreateTestActionSet(int[] counts,int sortMode= TESTACTION_SORT_UNIFORM, List<double> instinctActions=null)
        {
            List<List<double>> actionSet = new List<List<double>>();
            double unit = 1.0 / counts[0];
            double value = 0;
            for(int i=0;i<counts[0];i++)
            {
                actionSet.Add(new double[] { value }.ToList());
                value += unit;
            }
            
            return actionSet;
        
        }
        /// <summary>
        /// 推测姿态不变的情况下，从当前环境所导致的奖励
        /// </summary>
        /// <param name="gesture">初始姿态</param>
        /// <param name="envirment">初始环境</param>
        /// <param name="count">预测次数</param>
        /// <returns></returns>
        public double DoForcastReward(int time,Vector gesture,Vector env,int count=3,int rewardLowlimit=0)
        {
            List<double> actions = ActionPlan.MaintainAction;
            List<Vector> observations = this.GetReceoptorValues();
            /*observations = this.ReplaceObservationValue(observations, this.EnvReceptors.ConvertAll(r=>(Node)r), env.ToList().ConvertAll(e=>new Vector(e)));
            observations = this.ReplaceObservationValue(observations, this.GesturesReceptors.ConvertAll(r => (Node)r), gesture.ToList().ConvertAll(e => new Vector(e)));
            observations = this.ReplaceObservationValue(observations, this.ActionReceptors.ConvertAll(r => (Node)r),
                actions.ConvertAll(a => new Vector(a)));*/
            ActionReceptors.ForEach(r => r.Activate(this, time, 0.5));

            Node rewardNode = this.RewardReceptor;
            this.ThinkReset();

            //循环count次，每次预测是否会得到负奖励
            int vtime = time;
            for (int i = 0; i < count; i++)
            {
                List<Node> nonactionReceptors = this.Receptors.FindAll(r=>!r.Gene.IsActionSensor()).ConvertAll(r=>(Node)r);
                Dictionary<Node, Vector> handlerValueDict = new Dictionary<Node, Vector>();
                foreach (Inference inf in this.Inferences)
                {
                    List<Vector> condValues = inf.ConditionNodes.ConvertAll(n => n.getThinkValues(vtime));
                    List<Vector> varValues = inf.forward_inference(condValues, "record").Item2;
                    if (varValues == null || varValues.Count<=0) continue;

                    int rewardIndex = inf.VariableNodes.IndexOf(rewardNode);
                    if(rewardIndex >= 0)
                    {
                        double rewardValue = varValues[rewardIndex][0];
                        if (rewardValue < rewardLowlimit)
                        {
                            this.ThinkReset();
                            return -1;
                        }
                    }
                    for(int j=0;j<inf.VariableNodes.Count;j++)
                    {
                        if (inf.VariableNodes[j] is Handler)
                            handlerValueDict.Add(inf.VariableNodes[j], varValues[j]);
                        else
                            nonactionReceptors.Remove(inf.VariableNodes[j]);
                    }
                    
                    observations = ReplaceObservationValue(observations, inf.VariableNodes, varValues);
                }
                //如果有感受器的值没有推测出来，则尝试通过Handler来推测
                if(nonactionReceptors.Count>0)
                {
                    //倒着遍历所有handler
                    int maxDepth = this.genome.getHandlerMaxDepth();
                    for (int depth = maxDepth; depth>=1;depth--)
                    {
                        //取得第depth层的handler
                        List<Node> handlers = this.genome.handlerGenes.FindAll(h => h.Depth == depth).ConvertAll(g => this[g.Id]);
                        if (handlers == null || handlers.Count <= 0) continue;
                        for(int k=0;k<handlers.Count;k++)
                        {
                            //这个handler没有计算出来值
                            if (!handlerValueDict.ContainsKey(Handlers[k])) continue;
                            if(handlers[k] is DiffHandler)
                            {
                                List<Node> inputs = handlers[k].getInputNodes(this);
                                Node input1 = inputs[0];
                                Node input2 = inputs[1];
                                //输入1和2都有值
                                if ((input1 is Handler && handlerValueDict.ContainsKey(input1)) || (input1 is Receptor && !nonactionReceptors.Contains(input1)) && (input2 is Handler && handlerValueDict.ContainsKey(input2)) || (input2 is Receptor && !nonactionReceptors.Contains(input2)))
                                    continue;
                                //输入1有值，输入2没有
                                else if((input1 is Handler && handlerValueDict.ContainsKey(input1)) || (input1 is Receptor && !nonactionReceptors.Contains(input1)))
                                {
                                    double newValue = input1.getThinkValues(vtime)[0] - handlerValueDict[handlers[k]][0];
                                    //如果是handler，将新值加入到handler值字典；否则替换观察值
                                    if (input2 is Handler) handlerValueDict.Add(input2, new Vector(newValue));
                                    else if (input2 is Receptor)
                                    {
                                        if (input2 == rewardNode && newValue < rewardLowlimit) return newValue;
                                        observations = this.ReplaceObservationValue(observations, new Node[] { input2 }.ToList(), new Vector[] { new Vector(newValue) }.ToList());
                                        nonactionReceptors.Remove(input2);
                                    }
                                }
                                //输入2有值，输入1没有
                                else if ((input2 is Handler && handlerValueDict.ContainsKey(input2)) || (input2 is Receptor && !nonactionReceptors.Contains(input2)))
                                {
                                    double newValue = input2.getThinkValues(vtime)[0] + handlerValueDict[handlers[k]][0];
                                    //如果是handler，将新值加入到handler值字典；否则替换观察值
                                    if (input1 is Handler) handlerValueDict.Add(input1, new Vector(newValue));
                                    else if (input1 is Receptor)
                                    {
                                        if (input1 == rewardNode && newValue < rewardLowlimit) return newValue;
                                        observations = this.ReplaceObservationValue(observations, new Node[] { input1 }.ToList(), new Vector[] { new Vector(newValue) }.ToList());
                                        nonactionReceptors.Remove(input1);
                                    }
                                }
                            }
                            else if(handlers[k] is VariationHandler)
                            {
                                Node input = handlers[k].getInputNodes(this)[0];
                                //handler的输入也是Handler，且已经有值，则不需要计算
                                if (input is Handler && handlerValueDict.ContainsKey(input)) continue;
                                //handler的输入是Receptor，且已经有值，则不需要计算
                                if (input is Receptor && !nonactionReceptors.Contains(input)) continue;
                                //计算新值=原有值+变动值
                                double newValue = input.getThinkValues(vtime)[0] + handlerValueDict[handlers[k]][0];

                                //如果是handler，将新值加入到handler值字典；否则替换观察值
                                if (input is Handler) handlerValueDict.Add(input, new Vector(newValue));
                                else if (input is Receptor)
                                {
                                    if (input == rewardNode && newValue < rewardLowlimit) return newValue;
                                    observations = this.ReplaceObservationValue(observations, new Node[] { input }.ToList(), new Vector[] { new Vector(newValue) }.ToList());
                                    nonactionReceptors.Remove(input);
                                }

                            }
                        }
                    }
                }
                observations = this.ReplaceMaintainAction(observations);
                vtime += 1;
                this.ThinkActivation(vtime, observations);
            }

            this.ThinkReset();
            return 1;

        }
        /// <summary>
        /// 推理找到达到期望姿态的动作
        /// 
        /// </summary>
        /// <param name="expectGesture"></param>
        /// <returns></returns>
        public List<double> doInference(int time,Vector expectGesture,out Dictionary<Vector, Vector> actionToGesture)
        {
            actionToGesture = new Dictionary<Vector, Vector>();
            List<List<double>> sampleActionSet = this.CreateTestActionSet(new int[]{ 16});
            List<Vector> gestures = new List<Vector>();
            for(int i=0;i<sampleActionSet.Count;i++)
            {
                List<Vector> newObs = this.forward_inference(time,sampleActionSet[i]);
                if (newObs == null) return null;
                Vector gesture = this.GetReceptorGesture(newObs.flatten().Item1);
                gestures.Add(gesture);
                actionToGesture.Add(new Vector(sampleActionSet[i]), gesture.clone());
            }

            List<double> dis = gestures.ConvertAll(g => g.manhantan_distance(expectGesture));
            return sampleActionSet[dis.argmin()];

        }
        /// <summary>
        /// 给定动作，执行一步推理
        /// </summary>
        /// <param name="time"></param>
        /// <param name="actions"></param>
        /// <param name="reset"></param>
        /// <param name="inferenceMethod"></param>
        /// <returns></returns>
        public List<Vector> forward_inference(int time,List<double> actions,bool reset=true,String inferenceMethod= "recordsample")
        {
            //取得当前动作，用于修改后恢复
            List<double> curActionValues = this.GetReceoptorSplit().action;
            //如果调用者连续调用，则reset应为false
            if(reset) this.ThinkReset();
            //将动作感知替换成参数动作，结束后需要替换回来
            this.ReplaceReceptorActionValues(time,actions);

            List<Receptor> receptors = this.Receptors;
            List<Vector> observations = this.GetReceoptorValues();
            
            //对每一个推理节点执行前向推理
            foreach (Inference inf in this.Inferences)
            {
                List<Vector> condValues = inf.ConditionNodes.ConvertAll(n => n.getThinkValues(time));
                List<Vector> varValues = inf.forward_inference(condValues, inferenceMethod).Item2;
                if (varValues == null) continue;

                observations = ReplaceObservationValue(observations, inf.VariableNodes, varValues);
            }
            observations = this.ReplaceMaintainAction(observations);

            this.ReplaceReceptorActionValues(time, curActionValues);
            return observations;

        }

        public void ThinkActivation(int vtime, List<Vector>  observations)
        {
            List<Receptor> receptors = this.Receptors;
            for (int i = 0; i < receptors.Count; i++)
                receptors[i].think(this, vtime, observations[i]);

            var handlerDepth = this.genome.GetHanlerDepth();
            for(int d = handlerDepth.Item1;d <= handlerDepth.Item2;d++)
            {
                List<Handler> handlers = this.Handlers.FindAll(h => h.Gene.Depth == d);
                handlers.ForEach(h => h.think(this, vtime, null));
            }

            foreach(Inference inf in this.Inferences)
            {
                inf.think(this, vtime, null);
            }
        }
        public void ReplaceReceptorActionValues(int time, List<double> actions)
        {
            List<Receptor> receptors = this.ActionReceptors;
            for (int i = 0; i < receptors.Count; i++)
                receptors[i].Activate(this,time,new Vector(actions[i]));
        }

        public List<Vector> ReplaceObservationValue(List<Vector> observations, List<Node> nodes, List<Vector> values)
        {
            List<Receptor> receptors = this.Receptors;
            for(int i=0;i<nodes.Count;i++)
            {
                if (!(nodes[i] is Receptor)) continue;
                int index = receptors.IndexOf((Receptor)nodes[i]);
                observations[index] = values[i].clone();
            }
            return observations;
        }

        
        
        #endregion

        #region 观察空间管理
        public String GetObservationText(Vector observation,String sep = null)
        {
            if (sep == null) sep = System.Environment.NewLine;
            StringBuilder str = new StringBuilder();
            List<Receptor> receptors = this.Receptors;
            for(int i=0;i<receptors.Count;i++)
            {
                if (i >= observation.Size) continue;
                str.Append(receptors[i].Gene.Name + "=" + observation[i].ToString("F4") + sep);
            }
            return str.ToString();
        }
        /// <summary>
        /// 取得当前外部观察
        /// </summary>
        /// <returns></returns>
        public List<Vector> GetReceoptorValues()
        {
            return this.Receptors.ConvertAll(r => r.Value.clone());
        }

        public (Vector env,Vector gesture,Vector action) GetReceoptorSplit()
        {
            return 
            (this.Receptors.FindAll(r => r.Gene.IsEnvSensor()).ConvertAll(r => r.Value.clone()).flatten().Item1,
            this.Receptors.FindAll(r => r.Gene.IsGestureSensor()).ConvertAll(r => r.Value.clone()).flatten().Item1,
            this.Receptors.FindAll(r => r.Gene.IsActionSensor()).ConvertAll(r => r.Value==null?0:r.Value.clone()).flatten().Item1);
        }
        public Vector GetReceptorEnv(Vector observation)
        {
            if (observation == null)
                return this.Receptors.FindAll(r => r.Gene.IsEnvSensor()).ConvertAll(r => r.Value.clone()).flatten().Item1;
            List<double> env = new List<double>();
            List<Receptor> receptors = this.Receptors;
            for (int i = 0; i < receptors.Count; i++)
            {
                if (receptors[i].getGene().IsEnvSensor())
                    env.Add(observation[i]);
            }
            return env;
        }
        public Vector GetReceptorGesture(Vector observation)
        {
            if(observation == null)
                return this.Receptors.FindAll(r => r.Gene.IsGestureSensor()).ConvertAll(r => r.Value.clone()).flatten().Item1;
            List<double> gesture = new List<double>();
            List<Receptor> receptors = this.Receptors;
            for(int i=0;i<receptors.Count;i++)
            {
                if (receptors[i].getGene().IsGestureSensor())
                    gesture.Add(observation[i]);
            }
            return gesture;
        }

        public List<Vector> GetReceptorSceneValues()
        {
            return 
                this.Receptors.FindAll(r => !r.Gene.IsActionSensor()).ConvertAll(r => r.Value.clone());
        }
        /// <summary>
        /// 分非动作和动作取得外部观察值
        /// </summary>
        /// <returns></returns>
        public (List<Vector> scene,List<double> actions) GetSplitReceptorValues()
        {
            return (
                this.Receptors.FindAll(r => !r.Gene.IsActionSensor()).ConvertAll(r => r.Value.clone()),
                this.Receptors.FindAll(r => r.Gene.IsActionSensor()).ConvertAll(r => r.Value==null?double.NaN:r.Value[0])
                );
        }

        public List<Vector> GetMergeReceptorValues(List<Vector> envValues,List<double> actions)
        {
            int aIndex = 0, eIndex = 0;
            List<Vector> observations = new List<Vector>();
            List<Receptor> receptors = this.Receptors;
            for (int i = 0; i < receptors.Count; i++)
            {
                if (receptors[i].Gene.IsActionSensor())
                    observations.Add(new Vector(actions[aIndex++]));
                else
                    observations.Add(envValues[eIndex++]);
            }
            return observations;
        }
        /// <summary>
        /// 计算两个外部观察的距离
        /// </summary>
        /// <param name="obs1"></param>
        /// <param name="obs2"></param>
        /// <returns></returns>
        public List<double> GetReceptorDistance(List<Vector> obs1, List<Vector> obs2)
        {
            List<double> r = new List<double>();
            for (int i = 0; i < Receptors.Count; i++)
            {
                r.Add(Receptors[i].distance(obs1[i][0],obs2[i][0]));
            }
            return r;
        }

        public List<double> GetReceptorDistance(List<Receptor> receptors,Vector obs1, Vector obs2)
        {
            List<double> r = new List<double>();
            for (int i = 0; i < obs1.Size; i++)
            {
                r.Add(receptors[i].distance(obs1[i], obs2[i]));
            }
            return r;
        }

        /// <summary>
        /// 创建一组新的观察，将其中动作部分移除
        /// </summary>
        /// <param name="obs"></param>
        /// <returns></returns>
        public List<Vector> RemoveActionFromReceptor(List<Vector> obs)
        {
            List<Receptor> receptors = this.Receptors;
            int[] actionIndex = this.ActionReceptors.ConvertAll(ar => receptors.IndexOf(ar)).ToArray();

            List<Vector> r = new List<Vector>();
            for(int i=0;i<obs.Count;i++)
            {
                if (actionIndex.Contains(i)) continue;
                r.Add(obs[i]);
            }
            return r;
        }
        /// <summary>
        /// 将观察中的行为部分用计划部分替换
        /// </summary>
        /// <param name="curObs"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public List<Vector> ReplaceWithAction(List<Vector> curObs, List<double> actions)
        {
            int index = 0;
            for (int i = 0; i < Receptors.Count; i++)
            {
                if (Receptors[i].Gene.IsActionSensor())
                {
                    curObs[i] = new Vector(actions[index++]);
                }
            }
            return curObs;
        }
        /// <summary>
        /// 将当前观察中的行为部分替换为维持行动
        /// </summary>
        /// <param name="curObs"></param>
        /// <returns></returns>
        public List<Vector> ReplaceMaintainAction(List<Vector> curObs)
        {
            for (int i = 0; i < Receptors.Count; i++)
            {
                if (Receptors[i].Gene.IsActionSensor())
                {
                    curObs[i] = new Vector(0.5);
                }
            }
            return curObs;
        }

        /// <summary>
        /// 创建随机动作
        /// </summary>
        /// <param name="randomType"></param>
        /// <returns></returns>
        public List<double> CreateRandomActions(String randomType="uniform")
        {
            return new double[] { rng.NextDouble() * 0.5 + 0.25 }.ToList();
            //return this.Receptors.FindAll(r => r.getGene().IsActionSensor())
            //    .ConvertAll(r => r.randomValue(randomType));
        }

        #endregion

        #region 感知数据的相似性判断
        public bool IsGestureInTolerateDistance(Vector v1, Vector v2)
        {
            List<MeasureTools> measureTools = GesturesReceptors.ConvertAll(g => MeasureTools.GetMeasure(g.getGene().Cataory));
            return IsTolerateDistance(measureTools, v1, v2);
        }
        public bool IsTolerateDistance(List<MeasureTools> measureTools, Vector v1, Vector v2)
        {
            for (int i = 0; i < measureTools.Count; i++)
            {
                if (double.IsNaN(v1[i]) || double.IsNaN(v2[i]))
                    continue;
                if (Math.Abs(v1[i] - v2[i]) > measureTools[i].tolerate)
                    return false;
            }
            return true;
        }
        #endregion
        #region 保存和读取

        public String GetFileName(String path)
        {
            return path + "\\" + this.Id.ToString() + "_" +
            this.genome.generation.ToString() + "_" + this.Fitness.ToString("F5") + ".ind";
        }
        public void Save(String path,int generation)
        {
            String filename = path + "\\" + this.Id.ToString() + "_" +
            generation.ToString()+"_"+this.Fitness.ToString("F5") + ".ind";

            File.WriteAllText(filename, this.genome.ToString());
        }
        public static Network Load(String filename)
        {
            String text = File.ReadAllText(filename);
            if (text == null || text.Trim() == "") return null;
            NWSEGenome genome = NWSEGenome.Parse(text);
            return new Network(genome);
        }
        #endregion
    }
}
