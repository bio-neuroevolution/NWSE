
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// 
        /// 寻找满足条件的推理项
        /// </summary>
        /// <param name="condition_or_variable">根据条件查找，还是根据后置变量查找</param>
        /// <param name="allmatch">是否要求全部匹配</param>
        /// <param name="ids">待匹配ID</param>
        /// <returns></returns>
        public List<Inference> GetInferences(int condition_or_variable, bool allmatch, params int[] ids)
        {
            List<Node> inferences = this.Inferences;
            List<Inference> results = new List<Inference>();
            const int CONDITION = 1;
            const int VARIABLE = 2;

            for (int i = 0; i < inferences.Count; i++)
            {
                if (condition_or_variable == VARIABLE)
                {
                    if (((InferenceGene)inferences[i].Gene).matchVariable(ids[0]))
                        results.Add((Inference)inferences[i]);
                }
                else if (condition_or_variable == CONDITION)
                {
                    if (((InferenceGene)inferences[i].Gene).matchCondition(allmatch, ids))
                        results.Add((Inference)inferences[i]);
                }



            }
            return results;



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
            adjMatrix = new double[this.nodes.Count, this.nodes.Count];
            for (int i = 0; i < this.genome.connectionGene.Count; i++)
            {
                (int, int) connection = this.genome.connectionGene[i];
                int srcIndex = this.idToIndex(connection.Item1);
                int destIndex = this.idToIndex(connection.Item2);
                this.adjMatrix[srcIndex, destIndex] = 1;
            }

            //初始化短时记忆
            int memoryCapacity = Session.GetConfiguration().agent.shorttermcapacity;
            memories = new MemoryItem[this.nodes.Count];
            for(int i = 0; i < this.nodes.Count; i++)
            {
                memories[i] = new MemoryItem();
                memories[i].records = new Queue<Vector>(memoryCapacity);
            }


        }

        #endregion

        #region 推理状态
        /// <summary>
        /// 当前推理链
        /// </summary>
        private InferenceChain currentInferenceChain;
        /// <summary>
        /// 动作输出和推理迹
        /// </summary>
        private Dictionary<int, (double, int[])> currentActionTraces = new Dictionary<int, (double, int[])>();
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
            for (int i = 0; i < this.Receptors.Count; i++)
            {
                this.Receptors[i].activate(this, 0, obs[i]);
            }

            //反复执行直到都激活
            while (!this.Handlers.TrueForAll(n => n.IsActivate(time)))
            {
                this.Handlers.ForEach(n => n.activate(this, time, null));
            }
            //进行评判
            int col = time;


            //取出输出节点
            return this.Effectors.ConvertAll<double>(n => (double)n.Value);

        }
        /// <summary>
        /// 评判
        /// </summary>
        private void judge()
        {
            //对每一个评判项，选择合适的动作
            for (int i = 0; i < this.genome.judgeGene.items.Count; i++)
            {
                //取出评判项和其条件部分
                JudgeItem judgeItem = this.genome.judgeGene.items[i];
                List<int> conditions = judgeItem.conditions;
                double variableValue = judgeItem.expression == "argmax" ? double.MinValue : double.MaxValue;

                //找到所有包含推理变量（后置）的推理项
                List<Inference> varInferences = this.GetInferences(2, false, judgeItem.variable);
                if (varInferences == null || varInferences.Count <= 0) continue;

                //选择一个最合适的根推理
                Inference selectedInference = null;
                List<Vector> conditionValues = null;
                for (int j = 0; j < varInferences.Count; j++)
                {
                    (List<Vector> condition, int varId, double value) = varInferences[i].arginference(judgeItem.expression);
                    if (judgeItem.expression == "argmax" && value > variableValue)
                    {
                        variableValue = value;
                        selectedInference = varInferences[j];
                        conditionValues = condition;

                    }
                    else if (judgeItem.expression == "argmin" && value < variableValue)
                    {
                        variableValue = value;
                        selectedInference = varInferences[j];
                        conditionValues = condition;
                    }
                }

                //在选择的根推理上逐级回溯构造推理链
                InferenceChain chain = new InferenceChain()
                {
                    juegeItem = judgeItem,
                    varValue = variableValue
                };
                List<(int, int)> conds = ((InferenceGene)selectedInference.Gene).getConditions();
                List<(int, Vector)> condValues = new List<(int, Vector)>();
                for (int k = 0; k < conditionValues.Count; k++) condValues.Add((conds[k].Item1, conditionValues[k]));
                chain.addItem(selectedInference.Id, (((InferenceGene)selectedInference.Gene).getVariable().Item1, null), condValues);
                chain = do_inference(selectedInference, conditionValues, variableValue, chain);
                this.currentInferenceChain = chain;
                //在推理链上选择要执行的动作
                List<int> actionSensorIds = this.ActionReceptors.ConvertAll(r => r.Id);
                Dictionary<int, (double,int[])> actionValues = new Dictionary<int, (double, int[])>();

                for(int k=0;k<actionSensorIds.Count;k++)
                {
                    List<List<int>> traces = chain.findActionTrace(actionSensorIds[k]);
                    if(traces == null) //没有推理到该动作，给一个随机值
                    {
                        double min = Session.GetConfiguration().agent.receptors.actions[k].Range.Min;
                        double max = Session.GetConfiguration().agent.receptors.actions[k].Range.Max;
                        double value = new Random().NextDouble() * (max - min) + min;
                        actionValues.Add(actionSensorIds[k],(value,null));
                    }
                    else
                    {
                        //随机选择一个推理迹
                        int traceIndex = new Random().Next(0,traces.Count);
                        double value = chain.getValueFromTrace(actionSensorIds[i], 0, traces[traceIndex]);
                        actionValues.Add(actionSensorIds[k], (value, traces[traceIndex].ToArray()));
                    }
                }
            }
        }

        private InferenceChain do_inference(Inference inference, List<Vector> conditionValues,double varargValue, InferenceChain chain)
        {
            List<int> condids = ((InferenceGene)inference.Gene).getConditions().ConvertAll(c => c.Item1);
            List<Node> infs = this.Inferences.FindAll(i => ((InferenceGene)i.Gene).matchCondition(false, condids.ToArray()));
            if (infs == null || infs.Count <= 0) return chain;

            List<InferenceChain.Item> items = new List<InferenceChain.Item>();
            for(int i=0;i<infs.Count;i++)
            {
                Inference inf = (Inference)infs[i];
                List<(int,int)> conds = ((InferenceGene)inf.Gene).getConditions();
                int varindex = condids.IndexOf(((InferenceGene)inf.Gene).getVariable().Item1);
                Vector varvalue = conditionValues[varindex];
                (List<Vector> condValue,int vindex) = inf.postinference(varvalue);
                List<(int, Vector)> condValues = new List<(int, Vector)>();
                for (int k = 0; k < condValue.Count; k++) condValues.Add((conds[k].Item1, condValue[k]));
                InferenceChain.Item item = chain.addItem(inf.Id, (((InferenceGene)inf.Gene).getVariable().Item1, varvalue), condValues);
                items.Add(item);

                chain.current = item;
                chain = do_inference(inf, condValue, varargValue, chain);
            }

            return chain;

        }

        /// <summary>
        /// 处理接受到的奖励，相当于适应度（-1到1之间）
        /// 根据适应度，设定推理路径上的各个推断节点和处理节点的可靠度
        /// </summary>
        /// <param name="reward"></param>
        public void setReward(double reward)
        {
            if (currentInferenceChain == null) return;
            for(int i=0;i<this.currentActionTraces.Count;i++)
            {
                if (this.currentActionTraces[i].Item2 == null) continue;
                int count = this.currentActionTraces[i].Item2.Length;
                double avg = reward / (count+1);
                List<InferenceChain.Item> items = currentInferenceChain.getItemsFromTrace(this.currentActionTraces[i].Item2);
                if (items == null || items.Count <= 0) continue;
                for(int j=0;j<items.Count;j++)
                {
                    Node node = this.nodes.FirstOrDefault(n => n.Id == items[j].referenceNode);
                    node.Reability = node.Reability + avg;
                }
            }
        }
    }
}
