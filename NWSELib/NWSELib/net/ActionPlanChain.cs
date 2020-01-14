using NWSELib.common;
using NWSELib.genome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;

namespace NWSELib.net
{
    
    /// <summary>
    /// 行动规划项
    /// </summary>
    public class ActionPlan
    {
        /// <summary>
        /// 预期结果依据的推理项
        /// </summary>
        public Inference inference;

        /// <summary>
        /// 前一个行动计划
        /// </summary>
        public ActionPlan parent;
        /// <summary>
        /// 后序行动
        /// </summary>
        public List<ActionPlan> childs = new List<ActionPlan>();

        public int Depth
        {
            get
            {
                return getDepth(this);
                
            }
        }
        private static int getDepth(ActionPlan plan, int d=0)
        {
            if (plan == null) return d;
            d += 1;
            if (plan.childs.Count <= 0) return d;
            return getDepth(plan.childs[0], d);
        }
        /// <summary>
        /// 行动选择标记，指示选择childs中的哪一个
        /// </summary>
        public int selected = -1;

        /// <summary>
        /// 与行动条件最匹配的记录
        /// </summary>
        public InferenceRecord record;
        /// <summary>
        /// 相似度
        /// </summary>
        public double similarity;
        /// <summary>
        /// 相似度匹配的环境数据，可以用来再计算一次相似度
        /// </summary>
        public List<Vector> scene = new List<Vector>();

        /// <summary>
        /// 行动条件
        /// </summary>
        public List<Vector> conditions;

        /// <summary>
        /// 行动
        /// </summary>
        public List<Vector> actions;
        /// <summary>
        /// 本次动作是否是本能动作
        /// </summary>
        public bool instinct;
        /// <summary>
        /// 预期结果
        /// </summary>
        public List<Vector> expects;

        /// <summary>
        /// 真实结果
        /// </summary>
        public List<Vector> reals;
        /// <summary>
        /// 真实结果和预期结果的距离
        /// </summary>
        public double distance;


        public ActionPlan() { }
        public ActionPlan(Network net,Inference inf,InferenceRecord record,double similarity,List<Vector> inputValues)
        {
            this.inference = inf;
            this.record = record;
            this.similarity = similarity;
            this.scene = inputValues;

            (this.conditions,this.actions,this.expects) = inf.splitRecordMeans(net, record);
        }

        /// <summary>
        /// 在当前行动计划向上回溯的计划链条中，是否包含inf
        /// </summary>
        /// <param name="inf"></param>
        /// <returns></returns>
        public bool exist(Inference inf)
        {
            return exist(this,inf);
        }
        /// <summary>
        /// 在plan，以及plan以上的行动链条上，是否包含inf
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="inf"></param>
        /// <returns></returns>
        private bool exist(ActionPlan plan, Inference inf)
        {
            if (plan.inference == inf) return true;
            if (plan.parent == null) return false;
            return exist(plan.parent, inf);
        }
        public List<ActionPlan> toList()
        {
            List<ActionPlan> r = new List<ActionPlan>();
            ActionPlan temp = this;
            while(temp != null)
            {
                r.Add(temp);
                if (temp.childs.Count <= 0) return r;
                if (temp.selected < 0) return r;
                temp = temp.childs[temp.selected];
            }
            return r;
        }
        public string print()
        {
            StringBuilder str = new StringBuilder();
            str.Append("    inference=" + this.inference.Gene.Text + System.Environment.NewLine);
            str.Append("    scene=" + this.scene.toString() + System.Environment.NewLine);
            str.Append("    similarity=" + this.similarity.ToString("F3") + System.Environment.NewLine);
            str.Append("    record=" + this.record.means.toString() + System.Environment.NewLine);
            str.Append("    actions=" + this.actions.toString() + System.Environment.NewLine);
            str.Append("    expect=" + this.expects.toString() + System.Environment.NewLine);
            str.Append("    evulation=" + this.record.evulation.ToString("F3") + System.Environment.NewLine);
            str.Append("    usedCount=" + this.record.usedCount.ToString() + System.Environment.NewLine);
            str.Append("    accuracy=" + this.record.accuracy.ToString("F3") + System.Environment.NewLine);
            
            return str.ToString();
        }

        public string ToString(ActionPlan curActionPlan)
        {
            StringBuilder str = new StringBuilder();
            
            List<ActionPlan> plans = toList();
            for (int i = 0; i < plans.Count; i++)
            {
                if(plans.Count>0)
                    str.Append("    第" + (i + 1).ToString() + "步");
                if (plans[i] == curActionPlan)
                    str.Append("    正在执行");
                str.Append(System.Environment.NewLine);
                str.Append(plans[i].print());
                str.Append(System.Environment.NewLine);
            }
            str.Append(System.Environment.NewLine);
            return str.ToString();

        }
    }
}
