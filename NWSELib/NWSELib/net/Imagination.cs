using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.net
{
    /// <summary>
    /// 推理机能
    /// </summary>
    public class Imagination
    {
        /// <summary>
        /// 所属网络
        /// </summary>
        protected Network net;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="net"></param>
        public Imagination(Network net)
        {
            this.net = net;
        }

        #region 生成融合数据的抽象
        /// <summary>
        /// 这个是抽象处理后的整合节点，将用于推理任务
        /// </summary>
        public List<Inference> inferences = new List<Inference>();
        /// <summary>
        /// 对整合层的记录进行抽象操作
        /// </summary>
        public void doAbstract()
        {
            inferences.Clear();

            if (net.Inferences.Count <= 0) return;
            //对每一个融合节点
            foreach(Inference integration in net.Inferences)
            {
                //创建一个对应
                Inference absIntegration = new Inference(integration.Gene)
                {
                    Reability = integration.Reability
                };
                this.inferences.Add(absIntegration);
                List<List<double>> accuracies = new List<List<double>>();
                List<List<double>> evaulations = new List<List<double>>();
                List<List<int>> usedCounts = new List<List<int>>();

                //取得各个维，以及各个维的分级方式
                List<(int, int)> dimensions = integration.getGene().dimensions;
                List<int> ids = dimensions.ConvertAll(d => d.Item1);
                
                //对每一个记录做分级处理
                for (int i=0;i<integration.Records.Count;i++)
                {
                    InferenceRecord orginRecord = integration.Records[i];
                    List<Vector> values = net.getRankedValues(ids, orginRecord.means);
                    //检查下新值应归属到哪里
                    int index = putValue(absIntegration, orginRecord, values, accuracies, evaulations, usedCounts);
                }

                //处理准确度等数据
                for(int i=0;i< absIntegration.Records.Count;i++)
                {
                    absIntegration.Records[i].accuracyDistance = accuracies[i].Average();
                    absIntegration.Records[i].evulation = evaulations[i].Average();
                    absIntegration.Records[i].usedCount = usedCounts[i].Sum();
                }
            }
        }
        /// <summary>
        /// 为原记录和中心点的新值选择放到新记录中去
        /// </summary>
        /// <param name="orginRecord"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private int putValue(Inference absIntegration,InferenceRecord orginRecord, List<Vector> newValue,List<List<double>> accuracies, List<List<double>> evaulations, List<List<int>> usedCounts)
        {
            InferenceRecord newRecord = null;
            for (int i=0;i<absIntegration.Records.Count;i++)
            {
                newRecord = absIntegration.Records[i];
                if (!Vector.equals(newRecord.means,newValue))
                    continue;
                newRecord.acceptCount += orginRecord.acceptCount;
                accuracies[i].Add(orginRecord.accuracyDistance);
                evaulations[i].Add(orginRecord.evulation);
                usedCounts[i].Add(orginRecord.usedCount);
                newRecord.childs.Add(orginRecord);
                return i;
            }
            newRecord = new InferenceRecord();
            absIntegration.Records.Add(newRecord);
            accuracies.Add(new List<double>());
            evaulations.Add(new List<double>());
            usedCounts.Add(new List<int>());

            newRecord.acceptCount += orginRecord.acceptCount;
            accuracies[accuracies.Count-1].Add(orginRecord.accuracyDistance);
            evaulations[evaulations.Count-1].Add(orginRecord.evulation);
            usedCounts[usedCounts.Count-1].Add(orginRecord.usedCount);
            newRecord.covariance = orginRecord.covariance;
            newRecord.means = newValue;
            newRecord.childs.Add(orginRecord);

            return absIntegration.Records.Count - 1;
        }
        #endregion

        #region 推理
        /// <summary>
        /// 进行推理，返回推理后得到的动作
        /// </summary>
        /// <returns></returns>
        public ActionPlan doImagination(int time,Session session)
        {
            //获取本能行动方案
            List<double> instinctActions = Session.instinctActionHandler(net, time);
            //以本能方案为模版，测试生成一系列测试方案
            List<List<double>> testActions = createTestActionSet(instinctActions);
            List<double> evulations = new List<double>();

            //对每一个动作
            List<List<Vector>> obs = new List<List<Vector>>();
            for (int a=0;a<testActions.Count;a++)
            {
                List<double> actions = testActions[a];
                //执行直到无法执行，得到评估值
                (double evulation,List<Vector> initObs) = doActionImagination(actions,time,session);
                evulations.Add(evulation);
                obs.Add(initObs);
            }
            if (evulations.All(ed => double.IsNaN(ed)))
                return null;
            ActionPlan plan = new ActionPlan();
            //行动方案第一个是本能行动方案，如果它有效且评估值大于0，优先选择它
            if (double.IsNaN(evulations[0]) && evulations[0] > 0)
            {
                plan.actions = testActions[0];
                plan.judgeTime = time;
                plan.judgeType = ActionPlan.JUDGE_INFERENCE;
                plan.evluation = evulations[0];
                plan.inputObs = obs[0];
            }
            else
            {
                //返回评估值最大的
                double m = evulations.Max();
                int inx = evulations.IndexOf(m);
                plan.actions = testActions[inx];
                plan.judgeTime = time;
                plan.judgeType = ActionPlan.JUDGE_INFERENCE;
                plan.evluation = m;
                plan.inputObs = obs[inx];
            }
            

            return plan;
        }

        
        /// <summary>
        /// 对当前场景想象执行一个动作，返回一个评估
        /// </summary>
        /// <param name="inte"></param>
        /// <param name="record"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        private (double, List<Vector>) doActionImagination(List<double> actions,int time,Session session)
        {
            //取得动作对应的效应器
            List<Effector> effetors = net.Effectors;
            //计算当前环境观察值(动作感知替换中测试推理动作值)
            List<Vector> obsValues = net.Receptors.ConvertAll(r => r.GetValue(time));
            int index = 0;
            net.ActionReceptors.ForEach(s =>
            {
                obsValues[net.Receptors.IndexOf(s)] = actions[index++];
            });
            Dictionary<Receptor, Vector> observations = new Dictionary<Receptor, Vector>();
            for (int i = 0; i < obsValues.Count; i++)
                observations.Add(net.Receptors[i], obsValues[i]);
            List<Vector> initObs = obsValues;

            //虚拟初始化
            net.thinkReset();
            double evaulation = double.NaN;

            //对网络进行虚拟激活，得到新的观察值
            int maxsteps = 20,step = 0;
            while (true)
            {
                //初始化感知节点
                foreach(KeyValuePair<Receptor,Vector> obs in observations)
                {
                    obs.Key.think(net, time, obs.Value);
                }

                //激活处理节点
                List<Handler> handlers = this.getCanThinkHandlers(net,time);
                while (handlers!=null && !handlers.All(n => n.IsThinkCompleted(time)))
                {
                    net.Handlers.ForEach(n => n.think(net, time, null));
                }

                
                List<Inference> infs = getCanThinkInferences(net,time);
                if (infs == null || infs.Count <= 0)
                    return (evaulation, initObs);

                List<double> evulations = new List<double>();
                //准备生成新的观察
                Dictionary<Receptor, Vector> newObservations = new Dictionary<Receptor, Vector>();
                index = 0;
                net.ActionReceptors.ForEach(s =>
                {
                    newObservations.Add(s, actions[index++]);
                });
                //对每一个整合项进行前向推理
                foreach (Inference inte in infs)
                {
                    List<Vector> condValues = net.getInputNodes(inte.Id).ConvertAll(n=>n.getThinkValues(time));
                    (InferenceRecord matchedRecord,List<Vector> varValues) = inte.forward_inference(net,condValues);
                    if (matchedRecord == null) continue;
                    List<Receptor> varNodes = inte.getGene().getVariableIds().ConvertAll(id => (Receptor)net.getNode(id));
                    for(int i=0;i<varValues.Count;i++)
                    {
                        int inx = net.Receptors.IndexOf(varNodes[i]);
                        if (inx < 0) continue;
                        newObservations.Add(net.Receptors[inx], varValues[i]);
                    }
                    evulations.Add(matchedRecord.evulation);
                }
                //取所有历史评估的最小值
                if (evulations.Count > 0)
                {
                    if (double.IsNaN(evaulation))
                        evaulation = evulations.Min();
                    else if (evaulation > evulations.Min())
                        evaulation = evulations.Min();
                }
                    

                //检查所否所有的观察值都生成了
                if (newObservations.Count <= 0)
                    return (evaulation, initObs);

                //继续循环
                observations = newObservations;
                if (++step > maxsteps) { return (evaulation, initObs); }

                time += 1;
            }


        }

        private List<Inference> getCanThinkInferences(Network net, int time)
        {
            List<Node> nodes = new List<Node>();
            foreach (Inference h in this.inferences)
            {
                nodes = nodeThink(net, h, time, nodes);
            }
            return nodes.FindAll(n => n is Inference).ConvertAll(n => (Inference)n);
        }

        private List<Handler> getCanThinkHandlers(Network net, int time)
        {
            List<Node> nodes = new List<Node>();
            foreach(Handler h in net.Handlers)
            {
                nodes = nodeThink(net, h, time, nodes);
            }
            return nodes.FindAll(n => n is Handler).ConvertAll(n => (Handler)n);
            
        }

        private List<Node> nodeThink(Network net, Node h, int time, List<Node> nodes)
        {
            if (h == null) return nodes;
            if (nodes.Contains(h)) return nodes;

            List<Node> childs = net.getInputNodes(h.Id);
            if (childs == null || childs.Count <= 0)
            {
                nodes.Add(h);return nodes;
            }
            foreach(Node c in childs)
            {
                nodes = nodeThink(net, c, time, nodes);
            }
            if (childs.All(c => nodes.Contains(c)))
                nodes.Add(h);
            return nodes;
        }

        private List<List<double>> createTestActionSet(List<double> instinctActions)
        {
            List<List<double>> r = new List<List<double>>();
            r.Add(instinctActions);

            //这块应该实现组合，目前没有实现;这里应该生成一个高斯均值的随机动作，目前是直接将均值返回了
            Configuration.Sensor s = Session.config.agent.receptors.GetSensor("_a2");
            int count = 16;
            double unit = s.Range.Distance / count;
            for (int i = 0; i < count; i++)
            {
                List<double> action = new List<double>();
                action.Add(i * unit);
                r.Add(action);
            }
            return r;
        }

        #endregion

        
    }
}
