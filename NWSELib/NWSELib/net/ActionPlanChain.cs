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
        /// 行动条件
        /// </summary>
        public List<Vector> conditions;

        /// <summary>
        /// 行动
        /// </summary>
        public List<Vector> actions;
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
        public ActionPlan(Inference inf,InferenceRecord record,double similarity)
        {
            this.inference = inf;
            this.record = record;
            this.similarity = similarity;
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

        public string print()
        {
            StringBuilder str = new StringBuilder();
            str.Append("    inference=" + this.inference.Gene.Text + System.Environment.NewLine);
            str.Append("    recall=" + this.conditions.toString() + System.Environment.NewLine);
            str.Append("    record=" + this.record.means.toString() + System.Environment.NewLine);
            str.Append("    expect=" + this.expects.toString() + System.Environment.NewLine);
            str.Append("    accuracy=" + this.record.accuracy.ToString("F3") + System.Environment.NewLine);
            
            return str.ToString();
        }








    }
}
