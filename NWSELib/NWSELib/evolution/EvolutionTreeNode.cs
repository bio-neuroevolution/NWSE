using NWSELib.net;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

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
        [XmlIgnore]
        public EvolutionTreeNode parent = null;
        /// <summary>
        /// 是否被淘汰
        /// </summary>
        public bool extinct = false;
        /// <summary>
        /// 深度
        /// </summary>
        public readonly int depth = 0;
        /// <summary>
        /// 子节点
        /// </summary>
        public readonly List<EvolutionTreeNode> childs = new List<EvolutionTreeNode>();
        /// <summary>
        /// 网络
        /// </summary>
        public Network network;

        public override string ToString()
        {
            return this.network.ToString() + ",extinct=" + extinct + ",childs=" + this.childs.Count.ToString() + ",parent="+ (parent==null?"0": parent.network.Id.ToString())+",depth="+ depth.ToString()+",net="+this.network.Id.ToString();
        }
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
            this.depth = parent == null ? 0 : parent.depth + 1;
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
            if (this.parent != null)
            {
                foreach (EvolutionTreeNode c in this.parent.childs)
                {
                    if (c != this && !c.extinct) r.Add(c);
                }
            }
            if (this.childs != null && this.childs.Count > 0)
            {
                foreach (EvolutionTreeNode c in this.childs)
                {
                    if (c != this && !c.extinct) r.Add(c);
                }
            }
            return r;
        }
        public int getDepth()
        {
            return getDepth(this);
        }
        public static int getDepth(EvolutionTreeNode node)
        {
            if (node == null) return 0;
            if (node.childs == null || node.childs.Count <= 0)
                return node.depth;
            int d = node.depth;
            foreach(EvolutionTreeNode n in node.childs)
            {
                int t = getDepth(n);
                if (t > d) d = t;
            }
            return d;
        }
        public List<EvolutionTreeNode> getAliveNodes()
        {
            return getAliveNodes(this, null);
        }
        private static List<EvolutionTreeNode> getAliveNodes(EvolutionTreeNode node, List<EvolutionTreeNode> r)
        {
            if (r == null) r = new List<EvolutionTreeNode>();
            if (node == null) return r;
            if (node.extinct) return r;
            r.Add(node);
            if (node.childs == null || node.childs.Count <= 0) return r;
            foreach(EvolutionTreeNode child in node.childs)
            {
                r = getAliveNodes(child, r);
            }
            return r;
        }

        public void Save(String path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            FileInfo fileInfo = new FileInfo(path+@"\evolution.tree");
            if (fileInfo.Exists) fileInfo.Delete();

            StringBuilder str = BuildString();
            System.IO.File.WriteAllText(fileInfo.FullName, str.ToString());

            saveNetwork(directoryInfo,this);

        }
        private void saveNetwork(DirectoryInfo directoryInfo, EvolutionTreeNode node)
        {
            if (node == null) return;
            String filename = node.network.GetFileName(directoryInfo.FullName);
            if (!Directory.Exists(filename))
                node.network.Save(directoryInfo.FullName, node.network.Genome.generation);

            foreach (EvolutionTreeNode child in node.childs)
            {
                saveNetwork(directoryInfo, child);
            }

        }
        public StringBuilder BuildString()
        {
            StringBuilder str = new StringBuilder();
            str.Append(this+ System.Environment.NewLine);
            return buildString(this,str);
        }
        private StringBuilder buildString(EvolutionTreeNode node,StringBuilder str)
        {
            if (str == null) str = new StringBuilder();
            if (node == null) return str;

            foreach (EvolutionTreeNode child in node.childs)
            {
                str.Append(child.ToString() + System.Environment.NewLine);
            }

            foreach (EvolutionTreeNode child in node.childs)
            {
                str = buildString(child,str);
            }
            return str;
        }

    }
}
