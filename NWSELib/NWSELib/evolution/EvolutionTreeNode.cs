using NWSELib.net;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWSELib.evolution
{
    /// <summary>
    /// 进化树
    /// </summary>
    public class EvolutionTreeNode
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public EvolutionTreeNode parent = null;
        /// <summary>
        /// 是否被淘汰
        /// </summary>
        public bool extinct = false;
        /// <summary>
        /// 子节点
        /// </summary>
        public readonly List<EvolutionTreeNode> childs = new List<EvolutionTreeNode>();
        /// <summary>
        /// 网络
        /// </summary>
        public Network network;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="network"></param>
        /// <param name="parent"></param>
        /// <param name="childs"></param>
        public EvolutionTreeNode(Network network, EvolutionTreeNode parent=null, List<EvolutionTreeNode> childs=null)
        {
            this.network = network;
            this.parent = parent;
            if (childs != null) this.childs.AddRange(childs);
        }
        /// <summary>
        /// 查找特定网络所在节点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        public static EvolutionTreeNode search(EvolutionTreeNode node, Network network)
        {
            if (node == null) return null;
            if (node.network == network) return node;
            for(int i = 0; i < node.childs.Count; i++)
            {
                EvolutionTreeNode n = search(node.childs[i], network);
                if (n != null) return n;
            }
            return null;
        }
        /// <summary>
        /// 取得当前节点的周围的节点，周围是指父节点、子节点和兄弟节点
        /// </summary>
        /// <returns></returns>
        public List<EvolutionTreeNode> getNearestNode()
        {
            List<EvolutionTreeNode> r = new List<EvolutionTreeNode>();
            if(this.parent != null && !this.parent.extinct)r.Add(this.parent);
            foreach(EvolutionTreeNode c in this.parent.childs)
            {
                if (c != this && !c.extinct) r.Add(c);
            }
            foreach (EvolutionTreeNode c in this.childs)
            {
                if (c != this && !c.extinct) r.Add(c);
            }
            return r;
        }

    }
}
