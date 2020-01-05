
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
        /// <summary>评判项</summary>
        public JudgeItem juegeItem;
        /// <summary>链头</summary>
        public Item head;
        
        /// <summary>当前</summary>
        public Item current;
        /// <summary>变量值</summary>
        public double varValue;

        public Item addItem(int referenceNode, (int, Vector) variable, List<(int, Vector)> conditionValues)
        {
            Item item = new Item()
            {
                referenceNode = referenceNode,
                variable = variable,
                conditions = conditionValues,
                prev = current
            };
            if (current == null) current = item;
            else
                current.next.Add(item);
            if (head == null) head = item;
            return item;
        }
        public class Item
        {
            /// <summary>
            /// 推理节点Id
            /// </summary>
            public int referenceNode;
            /// <summary>
            /// 推理结果：对变量的取值
            /// </summary>
            public (int, Vector) variable;
            /// <summary>
            /// 推理结果：对条件的取值
            /// </summary>
            public List<(int, Vector)> conditions = new List<(int, Vector)>();
            /// <summary>
            /// 前一个推理项
            /// </summary>
            public Item prev;
            /// <summary>
            /// 后一个推理项
            /// </summary>
            public List<Item> next = new List<Item>();
        }
        /// <summary>
        /// 寻找包含特定动感知Id的所有路径
        /// </summary>
        /// <param name="actionSensorId"></param>
        /// <returns></returns>
        public List<List<int>> findActionTrace(int actionSensorId)
        {
            List<List<int>> traces = new List<List<int>>();

            return findActionTrace(head, actionSensorId, traces, null);
        }
        /// <summary>
        /// 递归寻找包含特定动感知Id的所有路径
        /// </summary>
        /// <param name="item"></param>
        /// <param name="actionSensorId"></param>
        /// <param name="traces"></param>
        /// <param name="curTrace"></param>
        /// <returns></returns>
        private List<List<int>> findActionTrace(Item item,int actionSensorId, List<List<int>> traces,List<int> curTrace)
        {
            if (curTrace == null) curTrace = new List<int>();
            if (traces == null) traces = new List<List<int>>();
            int p = item.conditions.ConvertAll(i => i.Item1).IndexOf(actionSensorId);
            if (p >= 0)
            {
                traces.Add(curTrace);
                curTrace = new List<int>(curTrace);

            }
            
            for(int i=0;i<item.next.Count;i++)
            {
                curTrace.Add(i);
                traces = findActionTrace(item.next[i], actionSensorId, traces, curTrace);
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
        public double getValueFromTrace(int nodeid,int index,List<int> trace)
        {
            Item item = do_trace(trace, 0, head);
            (int id,Vector value) = item.conditions.FirstOrDefault(x => x.Item1 == nodeid);
            return value[index];
        }

        private Item do_trace(List<int> trace,int xh,Item cur)
        {
            if (cur == null) cur = head;
            cur = cur.next[trace[xh++]];
            if (xh >= trace.Count) return cur;
            return do_trace(trace, xh, cur);

        }

        public List<Item> getItemsFromTrace(int[] trace)
        {
            List<Item> items = new List<Item>();
            if (trace == null || trace.Length <= 0) return items;
            Item item = head;
            items.Add(item);
            for(int i=0;i<trace.Length; i++)
            {
                item = item.next[trace[i]];
                items.Add(item);
            }
            return items;
        }
    }
}
