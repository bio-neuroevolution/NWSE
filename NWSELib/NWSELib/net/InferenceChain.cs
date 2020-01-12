
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NWSELib.common;
using NWSELib.genome;

namespace NWSELib.net
{
    /// <summary>
    /// 推理链
    /// </summary>
    public class InferenceChain
    {
        #region 基本信息
        /// <summary>评判项</summary>
        public JudgeGene juegeItem;

        /// <summary>链头</summary>
        public Item head;
        
        /// <summary>当前</summary>
        public Item current;
        /// <summary>变量值</summary>
        public double varValue;
        /// <summary>
        /// 动作轨迹：key是动作感知Id，Value是动作值和推理轨迹
        /// </summary>
        public Dictionary<int, (Vector, int[])> actionTraces = new Dictionary<int, (Vector, int[])>();
        #endregion

        #region 推理项
        public class Item
        {
            /// <summary>
            /// 推理节点Id
            /// </summary>
            public int referenceNode;
            /// <summary>
            /// 推理过程选择的记忆记录索引
            /// </summary>
            public int referenceRecordIndex;
            /// <summary>
            /// 推理结果
            /// </summary>
            public List<Vector> values;
            /// <summary>
            /// referenceNode的推理变量索引
            /// </summary>
            public int varIndex;
            /// <summary>
            /// 变量时间
            /// </summary>
            public int varTime;
            
            /// <summary>
            /// 前一个推理项
            /// </summary>
            public Item prev;
            /// <summary>
            /// 后一个推理项
            /// </summary>
            public List<Item> next = new List<Item>();
        }

        #endregion
        /// <summary>
        /// 寻找包含特定动感知Id的所有路径
        /// </summary>
        /// <param name="actionSensorId"></param>
        /// <returns></returns>
        public List<List<int>> findActionTrace(Network net, int actionSensorId)
        {
            List<List<int>> traces = new List<List<int>>();

            return findActionTrace(net,head, actionSensorId, traces, null);
        }
        /// <summary>
        /// 递归寻找包含特定动感知Id的所有路径
        /// </summary>
        /// <param name="item"></param>
        /// <param name="actionSensorId"></param>
        /// <param name="traces"></param>
        /// <param name="curTrace"></param>
        /// <returns></returns>
        private List<List<int>> findActionTrace(Network net,Item item,int actionSensorId, List<List<int>> traces,List<int> curTrace)
        {
            if (curTrace == null) curTrace = new List<int>();
            if (traces == null) traces = new List<List<int>>();

            Inference inf = (Inference)net.getNode(item.referenceNode);

            int p = inf.getGene().getConditions().ConvertAll(i => i.Item1).IndexOf(actionSensorId);
            if (p >= 0)
            {
                traces.Add(curTrace);
                curTrace = new List<int>();
            }
            
            for(int i=0;i<item.next.Count;i++)
            {
                curTrace.Add(i);
                traces = findActionTrace(net,item.next[i], actionSensorId, traces, curTrace);
            }
            return traces;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeid"></param>
        /// <param name="index"></param>
        /// <param name="trace"></param>
        /// <returns></returns>
        public double getValueFromTrace(Network net,int nodeid,int index,List<int> trace)
        {

            Item item = do_trace(trace, 0, head);
            Inference inf = (Inference)net.getNode(item.referenceNode);
            (int id,Vector value) = inf.getGene().getConditions().FirstOrDefault(x => x.Item1 == nodeid);
            return value[index];
        }

        private Item do_trace(List<int> trace,int xh,Item cur)
        {
            if (cur == null) cur = head;
            if (trace.Count <= 0) return cur;
            cur = cur.next[trace[xh++]];
            if (xh >= trace.Count) return cur;
            return do_trace(trace, xh, cur);

        }

        public List<Item> getItemsFromTrace(int[] trace)
        {
            List<Item> items = new List<Item>();
            Item item = head;
            items.Add(item);
            if (trace == null || trace.Length <= 0) return items;
            
            for(int i=0;i<trace.Length; i++)
            {
                item = item.next[trace[i]];
                items.Add(item);
            }
            return items;
        }

        internal bool contains(Inference inf, Item cur=null)
        {
            if (cur == null) cur = this.current;
            if (cur.referenceNode == inf.Id) return true;
            if (cur.prev == null) return false;
            return contains(inf, cur.prev);
        }
        
    }
}
