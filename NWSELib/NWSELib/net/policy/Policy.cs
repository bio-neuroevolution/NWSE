using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NWSELib.common;

namespace NWSELib.net.policy
{
    public class PolicyState
    {

        public class EnviornmentEvaluation
        {
            public int num;
            public Vector env;
            public double evaulation;

            public override String ToString()
            {
                return num.ToString() + ";" + Utility.toString(env) + ";" + evaulation.ToString("F4");
            }
        }

        public List<EnviornmentEvaluation> envEvaluations = new List<EnviornmentEvaluation>();

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            if(envEvaluations.Count>0)
            {
                str.Append("evaluate enviorment:" + System.Environment.NewLine +
                   envEvaluations.ConvertAll(e => e.ToString() + System.Environment.NewLine).Aggregate((x, y) => x + y));
            }
            if (curGesture != null)
            {
                str.Append(
                       "evaluate action:" + System.Environment.NewLine +
                       "curGesture:" + Utility.toString(curGesture) + System.Environment.NewLine +
                        "optimaGesture:" + Utility.toString(optimaGesture) + System.Environment.NewLine +
                       "objectiveGesture:" + Utility.toString(objectiveGesture) + System.Environment.NewLine +
                       "action:" + action.ToString("F4") + System.Environment.NewLine +
                       "policy:" + policeText + System.Environment.NewLine);
            }

            /*if(actionToGesture!=null && this.actionToGesture.Count>0)
            {
                str.Append("action details:" + System.Environment.NewLine);
                foreach(KeyValuePair<Vector,Vector> keyValue in actionToGesture)
                {
                    str.Append("action:" + keyValue.Key[0].ToString("F3") + ",gesture:" + keyValue.Value[0].ToString("F4") + System.Environment.NewLine);
                }
            }*/

            return str.ToString();
        }


        public double evaulation;
        public Vector curGesture;
        public Vector optimaGesture;
        public Vector objectiveGesture;
        public Dictionary<Vector, Vector> actionToGesture;
        public double action;

        public String policeText;

        public PolicyState() {  }
        public void AddEnviormentEvaluation(int num,Vector env,double evaulation)
        {
            EnviornmentEvaluation enviormentEvaluation = new EnviornmentEvaluation()
            {
                num = num,
                env = env.clone(),
                evaulation = evaulation
            };
            envEvaluations.Add(enviormentEvaluation);

        }

        public void setGestureAction(double evaulation, Vector curGesture, Vector objectiveGesture, double action,Dictionary<Vector, Vector> actionToGesture)
        {
            this.evaulation = evaulation;
            this.curGesture = curGesture.clone();
            this.objectiveGesture = objectiveGesture.clone();
            this.action = action;
            this.actionToGesture = new Dictionary<Vector, Vector>(actionToGesture);
            
        }

    }

    public abstract class Policy
    {
        #region  基本属性和抽象方法
        public abstract String Name { get; }
        public abstract ActionPlan Execute(int time, Session session);

        public Network net;
        public Configuration.EvaluationPolicy policyConfig;

        public PolicyState policyState = new PolicyState();

        #endregion

        #region 初始化
        public Policy(Network net)
        {
            this.net = net;
            policyConfig = Session.GetConfiguration().evaluation.policy;
        }

        private static List<Policy> items;

        public static Policy GetPolicy(Network net,String name)
        {
            return new EmotionPolicy(net);
        }
        #endregion

        #region  公共方法


        protected Dictionary<double, List<List<double>>> _cached_ActionSet = new Dictionary<double, List<List<double>>>();
        /// <summary>
        /// 生成的测试集第一个是本能动作，第二个是方向不变动作，然后逐渐向两边增大
        /// </summary>
        /// <param name="instinctActions"></param>
        /// <returns></returns>
        protected List<List<double>> CreateTestActionSet(List<double> instinctActions)
        {
            List<List<double>> r = new List<List<double>>();
            Receptor receptor = (Receptor)this.net["_a2"];
            int count = receptor.getGene().SampleCount;
            double unit = receptor.getGene().LevelUnitDistance;

            double[] values = receptor.GetSampleValues();
            if (values != null)
            {
                int minIndex = values.ToList().ConvertAll(v => Math.Abs(v - instinctActions[0])).argmin();
                instinctActions[0] = values[minIndex];

                if (_cached_ActionSet.ContainsKey(instinctActions[0]))
                    return _cached_ActionSet[instinctActions[0]];

                r.Add(instinctActions);
                int index = 1;
                while (r.Count < count)
                {
                    int t = (minIndex + index) % (values.Length);
                    r.Add(new double[] { values[t] }.ToList());
                    if (r.Count >= count) break;

                    t = minIndex - index;
                    if (t < 0) t = values.Length - 1;
                    r.Add(new double[] { values[t] }.ToList());

                    index += 1;
                }

                _cached_ActionSet.Add(instinctActions[0], r);
                return r;
            }

            r.Add(instinctActions);


            int i = 1;
            while (r.Count < count)
            {
                double temp = instinctActions[0] + i * unit;
                if (temp < 0) temp = 1.0 + unit + temp;
                else if (temp > 1) temp = temp - 1.0 - unit;
                if (temp <= 0.0000001) temp = 0;
                r.Add((new double[] { temp }).ToList());
                if (r.Count >= count) break;

                temp = instinctActions[0] - i * unit;
                if (temp < 0) temp = 1.0 + unit + temp;
                else if (temp > 1) temp = temp - 1.0 - unit;
                if (temp <= 0.0000001) temp = 0;
                r.Add((new double[] { temp }).ToList());

                i++;
            }

            return r;
        }
        #endregion
    }
}
