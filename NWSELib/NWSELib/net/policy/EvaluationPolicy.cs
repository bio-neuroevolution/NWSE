using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.net.policy
{
    public class EvaluationPolicy : Policy
    {
        public EvaluationPolicy(Network net) : base(net)
        {

        }
        public override String Name { get { return "evaluation"; } }

        /// <summary>
        /// 1.处理reward
        /// 1.1 将当前环境场景放入
        /// 2.生成测试行动集
        /// 3.对每一个行动，计算评估值
        /// 4.选择评估值最大行动
        /// </summary>
        /// <param name="time"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public override ActionPlan execute(int time, Session session)
        {
            //1.1 处理reward
            processReward(time);

            //1.2 仍在随机动作阶段
            if (time < 1)
            {
                return net.actionPlanChain.PutNext(ActionPlan.CreateRandomPlan(net, time, "随机漫游"));
            }

            if(net.reward > 0 && net.actionPlanChain.Length>0 && net.actionPlanChain.Root.planSteps>net.actionPlanChain.Length)
            {
                return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "maintain", 0, net.actionPlanChain.Last.planSteps - 1));

            }

            //1.3 创建测试动作集
            List<double> instinctAction = Session.instinctActionHandler(net, time);
            List<double> maintainAction = ActionPlan.MaintainAction;
            List<List<double>> testActionSet = this.CreateTestActionSet(instinctAction);

            //如果碰撞了，将维持动作删除去
            if(net.reward<=0)
            {
                int mindex = testActionSet.IndexOf(a => a[0] == 0.5);
                if (mindex >= 0) testActionSet.RemoveAt(mindex);
            }

            //1.4 寻找每个测试动作的匹配记录,计算所有匹配记录的均值
            double[] evaluations = new double[testActionSet.Count];
            List<InferenceRecord>[] rs = new List<InferenceRecord>[testActionSet.Count];
            for (int i = 0; i < evaluations.Length; i++) evaluations[i] = double.NaN;
            List<Vector> envValues = net.GetReceptorSceneValues();
            for(int i=0;i<testActionSet.Count;i++)
            {
                List<InferenceRecord> records = net.GetMatchInfRecords(envValues, testActionSet[i], time);
                rs[i] = new List<InferenceRecord>(records);
                List<InferenceRecord> evaluatedRecords = records.FindAll(r => !double.IsNaN(r.evulation));
                if (evaluatedRecords != null && evaluatedRecords.Count > 0)
                    evaluations[i] = evaluatedRecords.ConvertAll(r => r.evulation).Average();
            }

            for(int i=0;i< testActionSet.Count;i++)
            {
                if (this.net.reward < -1.0 && testActionSet[i][0] == 0.5)
                    evaluations[i] = double.MinValue;
                if(testActionSet[i][0] == 0.0 && net.actionPlanChain.Length>=2 && net.actionPlanChain.ToList()[net.actionPlanChain.Length-1].actions[0] == 0 &&
                   net.actionPlanChain.ToList()[net.actionPlanChain.Length - 2].actions[0] == 0)
                    evaluations[i] = double.MinValue;
            }


            ActionPlan plan = null;
            //寻找是否有为评估的，优先评估它
            int index = -1;
            for (int i = 0; i < evaluations.Length; i++)
            {
                if(double.IsNaN(evaluations[i]))
                {
                    index = i;break;
                }
            }
            if (index >= 0)
            {
                plan = net.actionPlanChain.PutNext(ActionPlan.CreateActionPlan(net, testActionSet[index], time, ActionPlan.JUDGE_INFERENCE, "未知评估"));
                if(rs[index]!=null && rs[index].Count > 0)
                    plan.inferenceRecords = rs[index].ConvertAll(r=>(r,0.0));
                plan.evaulation = evaluations[index];
                if (plan.actions[0] == instinctAction[0])
                    plan.planSteps = 0;
                else plan.planSteps = 0;
                return plan;
            }
                

            index = evaluations.ToList().IndexOf(evaluations.Max());
            plan = net.actionPlanChain.PutNext(ActionPlan.CreateActionPlan(net, testActionSet[index], time, ActionPlan.JUDGE_INFERENCE, "最大评估"));
            if (rs[index] != null && rs[index].Count > 0)
                plan.inferenceRecords = rs[index].ConvertAll(r => (r, 0.0));
            plan.evaulation = evaluations[index];
            if (plan.actions[0] == instinctAction[0])
                plan.planSteps = 0;
            else plan.planSteps = 0;
            return plan;

        }

        public void processReward(int time)
        {
            if (net.reward == 0) return;
            if (net.actionPlanChain.Length <= 0) return;

            net.actionPlanChain.Last.inferenceRecords.ForEach(r=>r.Item1.evulation = double.IsNaN(r.Item1.evulation)?net.reward: r.Item1.evulation+net.reward);

            if(net.actionPlanChain.Last.planSteps<=0)
                net.actionPlanChain.Clear();
            return;

            List<ActionPlan> plans = net.actionPlanChain.ToList();

            double reward = net.reward;
            bool doclear = reward <= -10;
            bool dijian = reward <= -10;
            reward = reward <= -10 ? reward + 10 -net.actionPlanChain.Length : reward;
            for(int i=plans.Count-1;i>=1;i--)
            {
                if (plans[i].inferenceRecords==null || plans[i].inferenceRecords.Count <= 0) continue;
                if (plans[i].reward > 0) break;
                for (int j=0;j<plans[i].inferenceRecords.Count;j++)
                {
                    if (double.IsNaN(plans[i].inferenceRecords[j].Item1.evulation))
                        plans[i].inferenceRecords[j].Item1.evulation = reward;
                    else
                        plans[i].inferenceRecords[j].Item1.evulation += reward;
                }

                

                
                if(dijian) 
                    reward += 1;
            }

            net.actionPlanChain.Clear();

            //if (doclear)
            //    net.actionPlanChain.Clear();
            //if (reward < 0) net.actionPlanChain.Clear();
        }
    }
}
