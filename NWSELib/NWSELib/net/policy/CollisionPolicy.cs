using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;
namespace NWSELib.net.policy
{
    /// <summary>
    /// 行动策略
    /// </summary>
    public class CollisionPolicy : Policy
    {
        

        public override String Name { get { return "collision"; } }

        public CollisionPolicy(Network net):base(net)
        {
            
        }
        /// <summary>
        /// 一 处理上一个行动链
        /// 1.若当前行动链为空，则执行二
        /// 2.若当前行动链没有得到reward，且行动次数小于阈值，
        ///   将动作修订为维持，继续执行，否则执行3
        /// 3.将行动链条和奖励置入环境记忆空间，执行二制定新的行动计划
        /// 二 制订行动计划
        /// 1.获得所有与环境匹配的推理场景,若没有则执行2，否则5
        /// 2.当前环境下本能动作在场景记忆中没有，创建本能动作链，结束，否则3
        /// 3.当前环境下本能动作在场景记忆评估为正值，创建本能动作链，结束，否则4
        /// 4.如果当前奖励为负，则创建反向随机动作链，否则创建高斯高斯随机动作链，
        ///   反复创建直到创建的动作结合场景要么在记忆中没有，要么记忆评估为正，结束
        /// 5.创建测试动作集，其中第一个是本能动作，其次都是按照与当前动作相近排序
        /// 6.对动作集中的每一个动作，执行前向推理，得到预测场景
        /// 7.若预测场景+维持动作在场景记忆中有，则记录评估值，否则回到6
        /// 8.若探索优先，则在未评估动作中选择构造新的行动计划，优先选择本能
        /// 9.否则在正评估行动中选择构建新的行动计划
        /// </summary>
        /// <param name="time"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public override ActionPlan execute(int time, Session session)
        {
            //1.1 第一次规划，随机动作
            if (net.actionPlanChain.Length <= 0)
            {
                return net.actionPlanChain.Reset(ActionPlan.CreateInstinctPlan(net, time, "初始动作"));
            }

            //1.2 仍在随机动作阶段
            if (time < 100)
            {
                net.actionMemory.Merge(net, net.actionPlanChain.Last);
                return net.actionPlanChain.Reset(ActionPlan.CreateRandomPlan(net, time, "随机漫游"));
            }

            //1.3 随机漫游结束
            if (time == 100)
            {
                net.actionMemory.Merge(net, net.actionPlanChain.Last);
                return net.actionPlanChain.Reset(makeNewActionPlan(time, session));
            }

            //1.4 规划行动是否完成了(奖励负)
            if (policyConfig.PlanRewardRange.In(net.reward))
            {
                net.actionMemory.Merge(net, net.actionPlanChain);
                return net.actionPlanChain.Reset(makeNewActionPlan(time, session));
            }
            //1.5 规划行动是否完成了(完成计划步长)
            if (net.actionPlanChain.Length >= net.actionPlanChain.Root.planSteps && net.actionPlanChain.Root.planSteps > 0)
            {
                net.actionMemory.Merge(net, net.actionPlanChain.Last);
                return net.actionPlanChain.Reset(makeNewActionPlan(time, session));
            }

            //1.6 再探索若干步后，预测本次规划行动的结果是否会是负值
            /*if(net.actionPlanChain.Length >= net.actionPlanChain.Root.planSteps/4)
            {
                double expect = forcastActionPlan();
                if (expect < 0)
                {
                    net.actionMemory.Merge(net, net.actionPlanChain);
                    return net.actionPlanChain.Reset(makeNewActionPlan(time, session));
                }
            }*/
            

            //1.7 继续本次行动计划
            return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "", net.actionPlanChain.Last.expect, net.actionPlanChain.Last.planSteps - 1));

        }

        /// <summary>
        /// 对当前行为进行推理，预测其未来评估
        /// </summary>
        /// <returns></returns>
        private double forcastActionPlan()
        {
            
            if (this.net.actionPlanChain.Length <= 0) return double.NaN;
            //寻找当前场景，如果继续维持当前行为，是否会导致负评估
            //由于漫游空间大，碰找到的几率不大
            List<ObservationHistory.ActionRecord> actionRecords = this.net.actionMemory.FindMatchActionPlans();
            if(actionRecords != null && actionRecords.Count > 0)
            {
                ObservationHistory.ActionRecord record = actionRecords.FirstOrDefault(r => r.actions[0] == 0.5);
                if (record != null) return record.evaluation;
            }

            //执行推理操作
            List<Vector> observations = net.GetReceoptorValues();
            observations = net.ReplaceWithAction(observations, ActionPlan.MaintainAction);
            return forcastActionPlan(observations);
            

        }
        private double forcastActionPlan(List<Vector> envValues, List<double> action)
        {
            if (envValues == null || envValues.Count <= 0) return double.NaN;
            if (action == null || action.Count <= 0) return double.NaN;

            List<Vector> observation = net.GetMergeReceptorValues(envValues, action);
            return forcastActionPlan(observation);
        }
        private double forcastActionPlan(List<Vector> observation)
        {
            
            int maxinferencecount = Session.GetConfiguration().evaluation.policy.init_plan_depth;
            for (int n =1;n<=maxinferencecount;n++)
            {
                List<Vector> nextObservation = net.forward_inference(observation);
                if (nextObservation == null) break;

                List<ObservationHistory.ActionRecord>  actionRecords = this.net.actionMemory.FindMatchActionPlans(nextObservation);

                if (actionRecords != null && actionRecords.Count > 0)
                {
                    ObservationHistory.ActionRecord record = actionRecords.FirstOrDefault(r => r.actions[0] == 0.5);
                    if (record != null) return record.evaluation > 0 ? n + record.evaluation : -1 * n + record.evaluation;
                }
                observation = nextObservation;
                net.ReplaceMaintainAction(observation);
            }

            return double.NaN;

        }
        /// <summary>
        /// 制订新的行动计划
        /// </summary>
        /// <param name="time"></param>
        /// <param name="session"></param>
        /// <param name="reward"></param>
        /// <param name="policyConfig"></param>
        /// <returns></returns>
        private ActionPlan makeNewActionPlan(int time, Session session)
        {
            //取得与当前场景匹配的所有行动计划
            var s = net.actionMemory.FindMatchedScene();
            ObservationHistory.Scene scene = s.Item1;
            List<ObservationHistory.ActionRecord> actionRecords = scene == null ? new List<ObservationHistory.ActionRecord>() : scene.records;
            //如果行动计划不是全部，补齐全部可能的行动计划，且按照与本能行动一致的顺序排序
            List<ActionPlan> plans = checkActionPlansFull(actionRecords, time);

            //找到本能行动计划和维持行动计划
            List<double> instictAction = Session.instinctActionHandler(net, time);
            ActionPlan instinctPlan = plans.FirstOrDefault(p => p.actions[0] == instictAction[0]);
            ActionPlan maintainPlan = plans.FirstOrDefault(p => p.actions[0] == 0.5);

            //遍历所有的计划
            double[] forcast = new double[plans.Count];
            for(int i=0;i<plans.Count;i++)
            {
                //上次是负奖励，则维持行动没有必要
                if (net.reward < 0 && plans[i].IsMaintainAction()) continue;
                //如果第i个行动确定是正评估，就是它了
                if (plans[i].evaulation > 0)
                { 
                    plans[i].reason = "走向正评估";
                    plans[i].planSteps = (int)plans[i].evaulation + policyConfig.init_plan_depth;
                    //forcast[i] = forcastActionPlan(scene!=null?scene.scene:plans[i].inputObs, plans[i].actions);
                    //if (forcast[i] < 0) continue;
                    //plans[i].expect = forcast[i];
                    return plans[i];
                }
                //如果第i个行动是未知评估，就是它了
                else if (double.IsNaN(plans[i].evaulation))
                {
                    plans[i].reason = "探索未知";
                    plans[i].planSteps = policyConfig.init_plan_depth;
                    //forcast[i] = forcastActionPlan(scene != null ? scene.scene : plans[i].inputObs, plans[i].actions);
                    //if (forcast[i] < 0) continue;
                    //plans[i].expect = forcast[i];
                    return plans[i];
                }
                else
                    forcast[i] = plans[i].evaulation;
            }

            //执行到这里，说明所有的评估都是负评估了.
            //处理起来非常麻烦，因为奖励与目标点无关，使得根据迷宫不同，有时候应该向本能放下走，有时候不能。
            //1.要么修改奖励计算方法，在保留碰撞负奖励以外，让沿着路线走的方向获得的奖励大些，这样在这里就简单了，直接选择记忆场景中奖励最大的行为
            //2 麻烦的办法是用推理测试，沿着每个方向走的时候，其后面的每一步在本能附近方向未知或者碰撞距离较大,但是这种做法计算太复杂
            //3 计算每个行动记忆场景的奖励密度。按照优先级选择碰撞密度最小的前三个
            /*double[] evaluationDensity = computeEvaluationDensity(plans);
            for(int i=0;i<plans.Count;i++)
            {
                //负奖励表示已经碰撞，维持行动一定不可取
                if (net.reward < 0 && plans[i].actions[0] == 0.5) continue;
                //碰撞后维持行动附近的行动也不可取
                if (net.reward < 0 && Math.Abs(plans[i].actions[0] - 0.5) < 0.25)
                    continue;
                //计算是第几个碰撞密度
                int c = evaluationDensity.ToList().Count(d => d <= evaluationDensity[i]);
                if(c <=3)
                {
                    plans[i].reason = "最小评估密度("+ evaluationDensity[i].ToString("F3")+")";
                    plans[i].planSteps = -1 * (int)plans[i].evaulation / 2;
                    return plans[i];
                }
                
            }*/

            //下面应该不会发生
            List<ActionPlan> ps = plans;
            if(net.reward<0)ps = plans.FindAll(p => Math.Abs(p.actions[0] - 0.5) >= 0.25);
            int t = Network.rng.Next(0, ps.Count);
            ps[t].reason = "全部负面,随机选择";
            ps[t].planSteps = double.IsNaN(ps[t].evaulation)?16:-1 * (int)ps[t].evaulation / 2;
            return plans[t];


        }
        
        private double[] computeEvaluationDensity(List<ActionPlan> plans)
        {
            double[] density = new double[plans.Count];
            int inferencecount = Session.GetConfiguration().evaluation.policy.init_plan_depth;
            for(int i=0;i<plans.Count;i++)
            {
                List<double> values = new List<double>();
                
                List<Vector> obs = net.GetMergeReceptorValues(plans[i].observation, plans[i].actions);
                for (int j=0;j<inferencecount;j++)
                {
                    obs = net.forward_inference(obs);
                    if (obs == null) break;
                    ObservationHistory.Scene scene = net.actionMemory.FindMatchedScene(obs).Item1;
                    if (scene == null) continue;
                    if (double.IsNaN(scene.density)) continue;
                    values.Add(scene.density);
                    
                }
                density[i] = values.Count <= 0 ? -1* inferencecount : values.Average();
            }
            return density;
        }

        /// <summary>
        /// 生成可以测试的动作计划集：从动作记忆中找到的行动计划，加上新补充的一些
        /// </summary>
        /// <param name="plans">从动作记忆中找到的行动计划</param>
        /// <param name="time"></param>
        /// <returns></returns>
        private List<ActionPlan> checkActionPlansFull(List<ObservationHistory.ActionRecord> actionRecords,int time)
        {
            List<List<double>> actionSets = CreateTestActionSet(Session.instinctActionHandler(net,time));

            ActionPlan[] r = new ActionPlan[actionSets.Count];
            for (int i=0;i<actionSets.Count;i++)
            {
                ActionPlan plan = null;
                String judgeType = ActionPlan.JUDGE_INFERENCE;
                if (i == 0) judgeType = ActionPlan.JUDGE_INSTINCT;
                else if (actionSets[i][0] == 0.5) judgeType = ActionPlan.JUDGE_MAINTAIN;

                ObservationHistory.ActionRecord record = actionRecords.FirstOrDefault(p => p.actions[0] ==actionSets[i][0]);
                if(record == null)
                {
                    plan = ActionPlan.CreateActionPlan(net, actionSets[i], time, judgeType, "");
                }else
                {
                    plan = ActionPlan.CreateActionPlan(net, record.actions, time, judgeType, "");
                    plan.evaulation = record.evaluation;
                }
                r[i] = plan;
            }
            return r.ToList();

        }

        

        
    }

}
