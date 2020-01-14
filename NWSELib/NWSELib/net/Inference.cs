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
    public class Inference
    {
        /// <summary>
        /// 所属网络
        /// </summary>
        protected Network net;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="net"></param>
        public Inference(Network net)
        {
            this.net = net;
        }

        #region 生成融合数据的抽象
        /// <summary>
        /// 这个是抽象处理后的整合节点，将用于推理任务
        /// </summary>
        protected List<Integration> Integrations = new List<Integration>();
        /// <summary>
        /// 对整合层的记录进行抽象操作
        /// </summary>
        public void doAbstract()
        {
            Integrations.Clear();

            if (net.Integrations.Count <= 0) return;
            //对每一个融合节点
            foreach(Integration integration in net.Integrations)
            {
                //创建一个对应
                Integration absIntegration = new Integration(integration.Gene)
                {
                    Reability = integration.Reability
                };
                this.Integrations.Add(absIntegration);
                List<List<double>> accuracies = new List<List<double>>();
                List<List<double>> evaulations = new List<List<double>>();
                List<List<int>> usedCounts = new List<List<int>>();

                //取得各个维，以及各个维的分级方式
                List<(int, int)> dimensions = integration.getGene().dimensions;
                List<int> ids = dimensions.ConvertAll(d => d.Item1);
                
                //对每一个记录做分级处理
                for (int i=0;i<integration.Records.Count;i++)
                {
                    IntegrationRecord orginRecord = integration.Records[i];
                    List<Vector> values = net.getRankedValues(ids, orginRecord.means);
                    //检查下新值应归属到哪里
                    int index = putValue(absIntegration, orginRecord, values, accuracies, evaulations, usedCounts);
                }

                //处理准确度等数据
                for(int i=0;i< absIntegration.Records.Count;i++)
                {
                    absIntegration.Records[i].accuracy = accuracies[i].Average();
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
        private int putValue(Integration absIntegration,IntegrationRecord orginRecord, List<Vector> newValue,List<List<double>> accuracies, List<List<double>> evaulations, List<List<int>> usedCounts)
        {
            IntegrationRecord newRecord = null;
            for (int i=0;i<absIntegration.Records.Count;i++)
            {
                newRecord = absIntegration.Records[i];
                if (!Vector.equals(newRecord.means,newValue))
                    continue;
                newRecord.acceptCount += orginRecord.acceptCount;
                accuracies[i].Add(orginRecord.accuracy);
                evaulations[i].Add(orginRecord.evulation);
                usedCounts[i].Add(orginRecord.usedCount);
                return i;
            }
            newRecord = new IntegrationRecord();
            absIntegration.Records.Add(newRecord);
            accuracies.Add(new List<double>());
            evaulations.Add(new List<double>());
            usedCounts.Add(new List<int>());

            newRecord.acceptCount += orginRecord.acceptCount;
            accuracies[accuracies.Count-1].Add(orginRecord.accuracy);
            evaulations[evaulations.Count-1].Add(orginRecord.evulation);
            usedCounts[usedCounts.Count-1].Add(orginRecord.usedCount);
            newRecord.covariance = orginRecord.covariance;
            newRecord.means = newValue;

            return absIntegration.Records.Count - 1;
        }
        #endregion

        #region 推理
        /// <summary>
        /// 进行推理，返回推理后得到的动作
        /// </summary>
        /// <returns></returns>
        public (List<double>,double) doInference(int time,Session session)
        {
            //获取本能行动方案
            List<double> instinctActions = Session.instinctActionHandler(net, time);
            //以本能方案为模版，测试生成一系列测试方案
            List<List<double>> testActions = createTestActionSet(instinctActions);
            List<double> evulations = new List<double>();

            //对每一个动作
            for (int a=0;a<testActions.Count;a++)
            {
                List<double> actions = testActions[a];
                //执行直到无法执行，得到评估值
                double evulation = doActionInference(actions);
                evulations.Add(evulation);
            }

            //行动方案第一个是本能行动方案，如果它有效且评估值大于0，优先选择它
            if (evulations[0]  > 0)
                return (testActions[0], evulations[0]);
            //返回评估值最大的
            double m= evulations.Max();
            return (testActions[evulations.IndexOf(m)],m);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inte"></param>
        /// <param name="record"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        private double doActionInference(List<double> actions)
        {
            //取得当前所有感知值，动作值替换成测试动作
            List<Receptor> receptors = net.Receptors.ConvertAll(r=>(Receptor)r);
            List<Vector> receptorValues = new List<Vector>();
            int index = 0;
            for(int i=0;i< receptors.Count;i++)
            {
                if(!receptors[i].Gene.Group.Contains("action"))
                {
                    if (receptors[i].Value == null) return double.MinValue;//无效
                    receptorValues.Add(receptors[i].Value);
                }
                else
                {
                    receptorValues.Add(new Vector(actions[index++]));
                }
            }
            //对所有节点进行推理
            int maxsteps = 20,step = 0;
            double result = double.MinValue;
            while (true)
            {
                foreach(Integration inte in this.Integrations)
                {
                    List<Vector> varValues = inte.forwardinference(receptors,receptorValues);
                    if (varValues == null || varValues.Count <= 0) continue;

                }
                if (++step > maxsteps) break;
            }


        }


        private List<(Integration, List<IntegrationRecord>)> findMatchedIntegrationRecords(List<double> actions)
        {
            List<(Integration, List<IntegrationRecord>)> r = new List<(Integration, List<IntegrationRecord>)>();
            List<int> actionIds = net.Genome.receptorGenes.FindAll(g=>g.Group.Contains("action")).ConvertAll(g => g.Id);
            List<Vector> rankedActions = net.getRankedValues(actionIds, actions.ConvertAll(a => new Vector(a)));
            
            foreach (Integration inte in this.Integrations)
            {
                (List<int> envids,List<int> gestureIds,List<int> actids,List<int> varids) = inte.getGene().getIds();
                List<Vector> envValues = net.getValues(envids);
                List<Vector> gestureValues = net.getValues(gestureIds);
                List<Vector> conditionValues = new List<Vector>();
                if (envValues != null && envValues.Count > 0) conditionValues.AddRange(envValues);
                if (gestureValues != null && gestureValues.Count > 0) conditionValues.AddRange(gestureValues);
                if(actids != null && actids.Count>0)
                {
                    actids.ForEach(a => conditionValues.Add(rankedActions[actionIds.IndexOf(a)]));
                }

                List<int> condIds = inte.getGene().getConditions().ConvertAll(c => c.Item1);
                List<IntegrationRecord> matchRecord = new List<IntegrationRecord>();
                foreach(IntegrationRecord record in inte.Records)
                {
                    List<Vector> means = record.getConditionInMeans();
                    if (!Vector.equals(conditionValues, means))
                        matchRecord.Add(record);
                }
                if(matchRecord.Count>0)
                {
                    r.Add((inte, matchRecord));
                }
            }
        }

        private List<List<double>> createTestActionSet(List<double> instinctActions)
        {
            List<List<double>> r = new List<List<double>>();
            r.Add(instinctActions);

            //这块应该实现组合，目前没有实现
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
