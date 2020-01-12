using NWSELib.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.net
{
    /// <summary>
    /// 行动规划
    /// </summary>
    public class ActionPlanChain
    {
        /// <summary>
        /// 当前已经实施的行动计划
        /// </summary>
        public ActionPlan.Item curPlanItem;
        /// <summary>
        /// 行动选择标记，指示选择roots中的哪一个
        /// </summary>
        public int selected = -1;
        /// <summary>
        /// 行动计划集
        /// </summary>
        public readonly List<ActionPlan> roots = new List<ActionPlan>();
        

    }
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
        /// 行动条件
        /// </summary>
        public List<Vector> conditions;

        /// <summary>
        /// 与行动条件最匹配的记录
        /// </summary>
        public InferenceRecord record;
        /// <summary>
        /// 相似度
        /// </summary>
        public double similarity;
        /// <summary>
        /// 行动
        /// </summary>
        public List<Item> items = new List<Item>();
        /// <summary>
        /// 前一个行动计划
        /// </summary>
        public ActionPlan prev;
        /// <summary>
        /// 行动选择标记，指示选择items中的哪一个
        /// </summary>
        public int selected = -1;

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
            if (plan.prev == null) return false;
            return exist(plan.prev, inf);
        }

        /// <summary>
        /// Item
        /// </summary>
        public class Item
        {
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

            /// <summary>
            /// 对这个行动计划的预期评价
            /// </summary>
            public double evaulation;

            /// <summary>
            /// 上一个行动计划
            /// </summary>
            public ActionPlan owner;
            /// <summary>
            /// 下面的行动
            /// </summary>
            public List<ActionPlan> childs = new List<ActionPlan>();
            /// <summary>
            /// 行动选择标记，指示选择childs中的哪一个
            /// </summary>
            public int selected = 0;
        }
        

       

        

        
             
    }
}
