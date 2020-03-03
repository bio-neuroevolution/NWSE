using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NWSELib.common;

namespace NWSELib.net.policy
{
    public abstract class Policy
    {
        #region  基本属性和抽象方法
        public abstract String Name { get; }
        public abstract ActionPlan execute(int time, Session session);

        public Network net;
        public Configuration.EvaluationPolicy policyConfig;

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
            if(items == null)
            {
                items = new List<Policy>(new Policy[] { new CollisionPolicy(net),new EvaluationPolicy(net),new EmotionPolicy(net)});
            }
            return items.FirstOrDefault(item => item.Name == name);
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
