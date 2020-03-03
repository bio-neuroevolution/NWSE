using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace NWSELib.net.policy
{
    public class EmotionPolicy : Policy
    {
        public override string Name => "emotion";

        /// <summary>
        /// 记忆场景
        /// </summary>
        private Scene scene = new Scene();
        
        /// <summary>
        /// 期望姿态
        /// </summary>
        private Vector expectGesture;
        /// <summary>
        /// 期望方向
        /// </summary>
        private Vector expectDirection;
        

        public EmotionPolicy(Network net) : base(net) { }

        public override ActionPlan execute(int time, Session session)
        {
            //1.1 处理reward
            processReward(time);

            //1.2 仍在随机动作阶段
            if (time < 100)
            {
                return net.actionPlanChain.PutNext(ActionPlan.CreateRandomPlan(net, time, "随机漫游"));
            }
            //1.3 初始化值差异方向感
            List<double> actions = null;
            Vector curGesture = net.GetReceptorGesture(null);
            Vector optimaGesture = Session.handleGetOptimaGesture(net, time);
            if (expectGesture == null)
            {
                expectGesture = optimaGesture.clone();
                actions = net.doInference(expectGesture);
                return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net, actions, time, ActionPlan.JUDGE_INFERENCE, "最优姿态"));
            }

            //1.4计算当前环境状态下的评估
            double envEvaluation = doEvaluateEnviornment();

            //1.5当前姿态是最优姿态，且当前环境状态为正评估，执行维持动作，结束
            if (net.IsGestureInTolerateDistance(curGesture, optimaGesture) && envEvaluation >= 0)
            {
                return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "最优姿态维持", envEvaluation, 0));
            }

            //1.6 计算偏离方向（如果与最优姿态出现偏离，计算偏离的方向）
            //对应距离来说，方向是指偏大偏小，对于角度来说，方向是指顺时针逆时针
            if(expectDirection == null)
            {
                List<Receptor> gestureReceptors = net.GesturesReceptors;
                List<MeasureTools> measureTools = net.GestureMeasureTools;
                expectDirection = new Vector(true,curGesture.Size);
                for(int i=0;i<curGesture.Size;i++)
                {
                    if (double.IsNaN(optimaGesture[i]))
                        expectDirection[i] = 0;
                    else
                        expectDirection[i] = measureTools[i].getChangeDirection(curGesture[i],optimaGesture[i]);
                }
            }


            //1.6 当前环境是正评估或未知评估
            int K = 1;
            Vector objectiveGesture = null;
            int maintainSteps = net.actionPlanChain.Length;
            if (envEvaluation >= 0)
            {
                //1.6.1维持小于K步，执行维持动作
                if (maintainSteps <= K)
                {
                    return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "正评估姿态维持", envEvaluation, 0));
                }
                //1.6.2 当前环境是正评估,目标姿态为当前姿态向期望方向靠近
                else
                {
                    objectiveGesture = moveGesture(curGesture, optimaGesture, expectDirection, 1);
                    actions = net.doInference(objectiveGesture);
                    actions = checkMaxActions(actions);
                    actions = checkMove(actions, curGesture, objectiveGesture);
                    return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net, actions, time, ActionPlan.JUDGE_INFERENCE, "正评估后期望提升"));
                }
            }
            //1.7 当前奖励是负，切换期望姿态
            objectiveGesture = moveGesture(curGesture, optimaGesture, expectDirection, -1);
            actions = net.doInference(objectiveGesture);
            actions = checkMaxActions(actions);
            actions = checkMove(actions, curGesture, objectiveGesture);
            return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net, actions, time, ActionPlan.JUDGE_INFERENCE, "负奖励后期望下降"));
        }

        private Vector moveGesture(Vector curGesture, Vector optimaGesture, Vector expectDirection, int closeOrAway)
        {
            List<Receptor> gestureReceptors = net.GesturesReceptors;
            List<MeasureTools> measureTools = net.GestureMeasureTools;
            double MOVEDISTANCE = 0.1;

            Vector result = new Vector(true,curGesture.Size);
            for(int i=0;i<result.Size;i++)
            {
                if (expectDirection[i] == 0)
                {
                    result[i] = curGesture[i];
                    continue;
                }
                double d = measureTools[i].distance(curGesture[i],optimaGesture[i]);
                if (d > MOVEDISTANCE) d = MOVEDISTANCE;
                if(closeOrAway > 0)
                    result[i] = measureTools[i].move(curGesture[i], (int)expectDirection[i], d);
                else
                    result[i] = measureTools[i].move(curGesture[i], (int)expectDirection[i]*-1, d);
            }
            return result;
        }
        /// <summary>
        /// </summary>
        /// <param name="time"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        /*public override ActionPlan execute(int time, Session session)
        {
            //1.1 处理reward
            processReward(time);

            //1.2 仍在随机动作阶段
            if (time < 100)
            {
                return net.actionPlanChain.PutNext(ActionPlan.CreateRandomPlan(net, time, "随机漫游"));
            }
            //1.3 设置期望姿态
            List<double> actions = null;
            Vector optimaGesture = Session.handleGetOptimaGesture(net,time);
            if (expectGesture == null)
            {
                expectGesture = optimaGesture.clone();
                actions = net.doInference(expectGesture);
                return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net,actions,time,ActionPlan.JUDGE_INFERENCE,"最优姿态"));
            }

            //1.4计算当前环境状态下的评估
            double envEvaluation = doEvaluateEnviornment();

            //1.5当前姿态是最优姿态，且当前环境状态为正评估，执行维持动作，结束
            Vector curGesture = net.GetReceptorGesture(null);
            if (net.IsGestureInTolerateDistance(curGesture, optimaGesture) && envEvaluation >= 0)
            {
                return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net,time,"最优姿态维持",envEvaluation,0));
            }

            
            //1.6 当前环境是正评估或未知评估
            int K = 1;
            Vector objectiveGesture = null;
            Vector nextexpectGesture = null;
            int maintainSteps = net.actionPlanChain.Length;
            if (envEvaluation >= 0)
            {
                //1.6.1维持小于K步，执行维持动作
                if (maintainSteps <= K)
                {
                    return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "正评估姿态维持", envEvaluation, 0));
                }
                //1.6.2 当前环境是正评估，且维持大于等于K步，推理产生从当前状态到期望姿态的动作，修改期望姿态使之更靠近最优姿态（当前姿态和最优姿态之间）
                else
                {
                    (objectiveGesture, nextexpectGesture) = switchObjectiveGesture(optimaGesture, expectGesture, curGesture, 1);
                    expectGesture = nextexpectGesture;
                    actions = net.doInference(objectiveGesture);
                    actions = checkMaxActions(actions);
                    if (actions == null)
                    {
                        return net.actionPlanChain.PutNext(ActionPlan.createMaintainPlan(net, time, "正评估且期望姿态不可达", envEvaluation, 0));
                    }
                    else
                    {
                        return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net, actions, time, ActionPlan.JUDGE_INFERENCE, "正评估后期望提升"));
                    }
                }
            }
            //1.7 当前奖励是负，切换期望姿态
            (objectiveGesture, nextexpectGesture) = switchObjectiveGesture(optimaGesture, expectGesture, curGesture, -1);
            expectGesture = nextexpectGesture;
            actions = net.doInference(objectiveGesture);
            actions = checkMaxActions(actions);
            return net.actionPlanChain.Reset(ActionPlan.CreateActionPlan(net, actions, time, ActionPlan.JUDGE_INFERENCE, "负奖励后期望下降"));


        }*/
        private List<double> checkMaxActions(List<double> actions)
        {
            for(int i=0;i<actions.Count;i++)
            {
                if (Math.Abs(actions[i]-0.5) > 0.1) actions[i] = actions[i]>0.5?0.6:0.4;
            }
            return actions;
        }
        private List<double> checkMove(List<double> actions,Vector curGesture,Vector objecttiveGesture)
        {
            List<MeasureTools> gestureMesureTools = net.GestureMeasureTools;   
            if (Vector.equals(curGesture, objecttiveGesture))
            {
                for(int i=0;i<actions.Count;i++)
                {
                    if(actions[i] == 0.5)
                    {
                        int direction = gestureMesureTools[i].getChangeDirection(curGesture[i], objecttiveGesture[i]);
                        actions[i] = (direction > 0 ? actions[i] + 0.1 : actions[i] - 0.1);
                    }
                }
            }
            return actions;
        }
        /// <summary>
        /// 切换期望姿态和目标姿态
        /// 如果是正向的，让目标姿态为当前姿态向期望姿态靠近，期望姿态向最优姿态靠近
        /// 如果是负向的，让目标姿态为当前姿态远离期望姿态，期望姿态取当前姿态
        /// </summary>
        /// <param name="optimaGesture"></param>
        /// <param name="expectGesture"></param>
        /// <param name="curGesture"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private (Vector objectiveGesture,Vector nextExpectGesture) switchObjectiveGesture(Vector optimaGesture, Vector expectGesture, Vector curGesture,int direction)
        {
            Vector objectiveGesture = new Vector(true, curGesture.Size), nextExpectGesture = new Vector(true, expectGesture.Size);
            double MOVEDISTANCE = 0.1;
            List<Receptor> gestureReceptors = net.GesturesReceptors;    
            List<MeasureTools> measureTools = gestureReceptors.ConvertAll(g => MeasureTools.GetMeasure(g.getGene().Cataory));

            
            for (int i = 0; i < optimaGesture.Size; i++)
            {
                double d1 = measureTools[i].distance(optimaGesture[i],expectGesture[i]);
                double d2 = measureTools[i].distance(optimaGesture[i], curGesture[i]);
                double d3 = measureTools[i].distance(expectGesture[i], curGesture[i]);
                if (direction > 0)
                {
                    if (d1 < d2)//期望姿态比当前姿态距离最优姿态更近
                    {
                        objectiveGesture[i] = measureTools[i].moveTo(curGesture[i], expectGesture[i], 1, Math.Min(d2,MOVEDISTANCE));
                        nextExpectGesture[i] = measureTools[i].moveTo(expectGesture[i], optimaGesture[i], 1, Math.Min(d1, MOVEDISTANCE));
                    }
                    else//期望姿态比当前姿态距离最优姿态更远或者一样,说明反过来了
                    {
                        objectiveGesture[i] = measureTools[i].moveTo(curGesture[i], expectGesture[i], 1, Math.Min(d3, MOVEDISTANCE));
                        nextExpectGesture[i] = measureTools[i].moveTo(expectGesture[i], optimaGesture[i], 1, Math.Min(d1, MOVEDISTANCE));
                    }
                }else if(direction < 0)
                {
                    if (d1 < d2)//期望姿态比当前姿态距离最优姿态更近
                    {
                        objectiveGesture[i] = measureTools[i].moveTo(curGesture[i], expectGesture[i], -1, Math.Min(d2, MOVEDISTANCE));
                        nextExpectGesture[i] = curGesture[i];
                    }
                    else//期望姿态比当前姿态距离最优姿态更远或者一样，说明反过来了
                    {
                        objectiveGesture[i] = measureTools[i].moveTo(curGesture[i], optimaGesture[i], 1, Math.Min(d2, MOVEDISTANCE));
                        nextExpectGesture[i] = curGesture[i];
                    }
                }
                else
                {
                    objectiveGesture[i] = curGesture[i];
                    nextExpectGesture[i] = expectGesture[i];
                }
            }
            return (objectiveGesture,nextExpectGesture);
        }

        private void processReward(int time)
        {
            if (net.reward == 0) return;
            var s = net.GetReceoptorSplit();
            this.scene.Put(s.env, s.gesture, net.reward);

            List<ActionPlan> plans = net.actionPlanChain.ToList();
            if (plans == null || plans.Count <= 0) return;
            for (int i=plans.Count-1;i>=0;i--)
            {
                if (!plans[i].IsMaintainAction()) break;
                double r = Math.Exp(i - plans.Count + 1) * net.reward;
                this.scene.Put(plans[i].env, plans[i].gesture, r);
            }
            

        }
        /// <summary>
        /// 对环境做出评估
        /// Agent对环境有个好恶，该好恶与目标任务无关，只与它在环境中接收的奖励或惩罚有关
        /// 
        /// </summary>
        /// <returns></returns>
        private double doEvaluateEnviornment(Vector env=null)
        {
            //取得当前环境
            if (env == null)
            {
                var obs = net.GetReceoptorSplit();
                env = obs.env;
            }
            //在记忆库中查找当前环境
            SceneItem sceneItem = this.scene.Get(env);
            if (sceneItem != null) return sceneItem.evaluation;

            //记录库中不存在的话，尝试推理在当前环境下执行维持动作会不会在记忆库中找到
            List<Vector> observation = net.GetReceoptorValues();
            observation = net.ReplaceMaintainAction(observation);
            for(int i=0;i<3;i++)
            {
                List<Vector> newObs = net.forward_inference(observation);
                Vector newEnv = net.GetReceptorEnv(newObs.flatten().Item1);
                sceneItem = this.scene.Get(env);
                if (sceneItem != null) return sceneItem.evaluation;
                observation = newObs;
            }

            //如果找不到，则尝试在记忆库中找一个相似的作为替代
            List<double> dis = this.scene.items.ConvertAll(item => item.env.manhantan_distance(env));
            int minindex = dis.argmin();
            double mindis = dis[minindex];
            if (mindis < 0.1 * env.Size)
                return this.scene.items[minindex].evaluation;

            //如果还没有找到，说明这是一种新环境，以前没有出现过
            return 0;
        }

        
    }

    public class Scene
    {
        public readonly List<SceneItem> items = new List<SceneItem>();
        public SceneItem Get(Vector env)
        {
            return items.FirstOrDefault(item => Vector.equals(env, item.env));
        }
        public void Put(Vector env, Vector gesture, double evaluation) 
        {
            SceneItem item = Get(env);
            if (item == null)
            {
                item = new SceneItem(env, gesture, evaluation);
                items.Add(item);
            }
            else
                item.evaluation = evaluation;

        }

    }
    public class SceneItem
    {
        public Vector env;
        public Vector gesture;
        public double evaluation;
        public SceneItem(Vector env, Vector gesture, double evaluation)
        {
            this.env = env.clone();
            this.gesture = gesture.clone();
            this.evaluation = evaluation;
        }
    }
}
