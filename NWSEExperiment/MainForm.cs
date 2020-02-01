using NWSEExperiment.maze;
using NWSELib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

using NWSELib.net;
using NWSELib.genome;
using NWSELib.evolution;
using NWSELib.env;
using NWSELib.common;

namespace NWSEExperiment
{
    

    public partial class MainForm : Form
    {
        #region 基本信息
        public delegate void BeginInvokeDelegate();
        private ILog logger = LogManager.GetLogger(typeof(MainForm));
        public MainForm()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure();
            this.Width = Session.GetConfiguration().view.width;
            this.Height = Session.GetConfiguration().view.height;

            Session.GetConfiguration();
            MeasureTools.init();

            resetEvolution();
            resetDemo();
        }

        private void btnshowTrail_Click(object sender, EventArgs e)
        {
            if (demoMaze == null) return;
            this.demoMaze.ShowTrail = btnoShowTrail.Checked;
        }
        #endregion

        #region 进化过程

        public HardMaze evolutionMaze;
        private CoordinateFrame evolutionFrame;
        public Session evolutionSession;

        private RobotAgent optimaAgent;
        private Network optima_net;
        private int optima_generation;
        public void resetEvolution()
        {
            evolutionMaze = HardMaze.loadEnvironment("QDMaze.xml");
            evolutionFrame = new CoordinateFrame(0.0f, evolutionMaze.AOIRectangle.Y, 1.1f, 0.0f);
            optimaAgent = null;
            optima_net = null;
            optima_generation = -1;

            evolutionSession = new Session(this.evolutionMaze, new FitnessHandler(evolutionMaze.compute_fitness), EventHandler, new InstinctActionHandler(HardMaze.createInstinctAction));  
        }
        private void btnERun_Clicked(object sender, EventArgs e)
        {
            if (evolutionSession.Running) return;
            try
            {
                resetEvolution();
                evolutionSession.run();
            }
            catch(Exception ex)
            {
                logger.Error(ex.StackTrace);
            }
            finally
            {

            }
        }

        public void EventHandler(String eventName, int generation,params Object[] states)
        {
            if(eventName == Session.EVT_OPTIMA_IND)
            {
                this.optima_generation = generation;
                this.optima_net = (Network)states[0];
                ToolStripItem tItem = this.btnOpenOptima.DropDownItems.Add("Generation:" + generation.ToString() + ",ind=" + optima_net.Id.ToString());
                tItem.Tag = optima_net;
                this.BeginInvoke(new BeginInvokeDelegate(refreshEvaulation));

            }
            else if(eventName == Session.EVT_MESSAGE)
            {
                txtLog.Text += states[0].ToString() + System.Environment.NewLine;
            }else if(eventName == Session.EVT_INVAILD_GENE)
            {
                Network net = (Network)states[0];
                NodeGene gene = (NodeGene)states[1];
                txtMilestone.Text += "########" + generation.ToString() + "########" + System.Environment.NewLine;
                txtMilestone.Text += "gene was eliminated in generation" + generation.ToString() + ":" + System.Environment.NewLine;
                txtMilestone.Text += gene.ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_VAILD_GENE)
            {
                Network net = (Network)states[0];
                NodeGene gene = (NodeGene)states[1];
                txtMilestone.Text += "########" + generation.ToString() + "########" + System.Environment.NewLine;
                txtMilestone.Text += "gene was considered valid in generation" + generation.ToString() + ":" + System.Environment.NewLine;
                txtMilestone.Text += gene.ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_REABILITY)
            {
                txtLog.Text += "reablity avg=" + states[0].ToString() + ",variance=" + states[1].ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_IND_COUNT)
            {
                this.lblindcount.Text = "size of population:"+states[1].ToString() + "(Preceding " + states[0].ToString()+")";
                txtLog.Text += "Elimination stat:" + states[1].ToString() + "(Preceding " + states[0].ToString() + ")" + ",reability limit=" + states[2].ToString();
            }
            else if(eventName == Session.EVT_GENERATION_END)
            {
                this.BeginInvoke(new BeginInvokeDelegate(refreshEvolutionTree));
               
                
            }
        }

        private void beginGenerationInvoke(Action p)
        {
            refreshEvolutionTree();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (this.evolutionSession == null) return;
            this.evolutionSession.paused = !this.evolutionSession.paused;
        }

        public void refreshEvaulation()
        {
            if (evolutionSession == null) return;
            txtDepth.Text = evolutionSession.root.getDepth().ToString();
            txtGeneration.Text = evolutionSession.Generation.ToString();
            txtIndCount.Text = evolutionSession.inds.Count.ToString();
            List<double> fts = evolutionSession.inds.ConvertAll(ind => ind.Fitness);
            txtMaxFitness.Text = fts.Max().ToString("F4");
            txtOptimaNetId.Text = evolutionSession.inds[fts.argmax()].Id.ToString();

            refreshNetwork(evolutionSession.inds[fts.argmax()],treeViewOptimaNet);


        }
        public void refreshEvolutionTree()
        {
            if (evolutionSession == null) return;
            treeViewEvolution.Nodes.Clear();
            TreeNode node = treeViewEvolution.Nodes.Add(evolutionSession.root.ToString());
            node.Tag = evolutionSession.root.ToString();

            refreshEvolutionTreeNode(node,evolutionSession.root);
        }
        private void refreshEvolutionTreeNode(TreeNode tn,EvolutionTreeNode node)
        {
            if (node == null) return;
            if (node.childs == null || node.childs.Count <= 0) return;

            foreach(EvolutionTreeNode cn in node.childs)
            {
                TreeNode ctn = tn.Nodes.Add(cn.ToString());
                ctn.Tag = cn;
                refreshEvolutionTreeNode(ctn,cn);
            }
            
        }

        public void refreshNetwork(Network net,TreeView tv)
        {
            tv.Nodes.Clear();
            if (net == null) return;
            TreeNode handlersNode = tv.Nodes.Add("handlers");
            foreach(Handler h in net.Handlers)
            {
                TreeNode t = handlersNode.Nodes.Add(h.Gene.Text);
                t.Tag = h;
            }

            TreeNode infsNodes = tv.Nodes.Add("inferences");
            foreach(Inference inf in net.Inferences)
            {
                TreeNode t = infsNodes.Nodes.Add(inf.Gene.Text);
                t.Tag = inf;
                TreeNode t1 = t.Nodes.Add("reability="+inf.Reability.ToString("F4"));
                TreeNode t2 = t.Nodes.Add("records");
                for(int i=0;i<inf.Records.Count;i++)
                {
                    TreeNode t3 = t2.Nodes.Add(inf.Records[i].toString(inf,i));
                    t3.Tag = inf.Records[i];
                }
            }
        }

        #endregion

        #region 演示过程

        public HardMaze demoMaze;
        private CoordinateFrame demoFrame;
        private RobotAgent demoAgent;
        private Network demoNet;
        public void resetDemo()
        {
            demoMaze = HardMaze.loadEnvironment("QDMaze.xml");
            demoFrame = new CoordinateFrame(0.0f, demoMaze.AOIRectangle.Y, 1.1f, 0.0f);
        }

        public void panel_Paint(object sender, PaintEventArgs e)
        {

            if (demoMaze == null) return;

            demoMaze.draw(e.Graphics, demoFrame);

            if (btnPolicyShow.Checked && inferencing)
            {
                if (this.demoAgent == null) return;
                this.demoAgent.drawEvaulation(e.Graphics, demoFrame);
            }
        }


        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (MeasureTools.Position == null) return;
            if (demoMaze == null) return;
            float mazeX, mazeY;
            demoFrame.convertFromDisplay(e.X, e.Y, out mazeX, out mazeY);

            (double poscode, (int gridx, int gridy)) = MeasureTools.Position.poscodeCompute(demoMaze.AOIRectangle, mazeX, mazeY);
            if (poscode <= 0)
                this.statusXY.Text = String.Format("X={0:000.00},Y={1:000.00}", mazeX, mazeY);
            else
                this.statusXY.Text = String.Format("X={0:000.00},Y={1:000.00},pos={2:0.0000},grid=[{3},{4}]", mazeX, mazeY, poscode, gridx, gridy);
        }
        #endregion


        #region 交互式执行
        private NWSEGenomeFactory genomeFactory = new NWSEGenomeFactory();
        private int interactive_time = 0;
        private List<double> obs;
        private List<double> gesture;
        private List<double> actions;
        private double reward;
        private bool end;

        private bool inferencing;
        /// <summary>
        /// 交互式环境重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if(demoNet == null)
            {
                demoNet = new Network(genomeFactory.createDemoGenome(evolutionSession)); 
            }
            interactive_time = 0;
            (obs, gesture) = demoMaze.reset(optima_net);
            demoAgent = demoMaze.Agents[0];

            (int ptx, int pty) = MeasureTools.Position.poscodeSplit(obs[11]);
            this.txtMsg.Text = "第" + interactive_time.ToString() + "次交互" + System.Environment.NewLine;
            this.txtMsg.Text += "障碍=" + Utility.toString(obs.GetRange(0, 6)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "位置=" + obs[11].ToString("F4") + "(" + ptx.ToString() + "," + pty.ToString() + ")" + System.Environment.NewLine;
            this.txtMsg.Text += "目标=" + Utility.toString(obs.GetRange(6, 4)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "朝向=" + MeasureTools.Direction.headingToDegree(gesture[0]).ToString("F2") +"("+ gesture[0].ToString("F2")+")"+ System.Environment.NewLine;
            this.txtMsg.Text += "到达=" + end.ToString() + System.Environment.NewLine;
            this.txtMsg.Text += System.Environment.NewLine;
            inferencing = false;
            this.Refresh();
            
        }
        

        /// <summary>
        /// 推理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //网络执行
            List<double> inputs = new List<double>(obs);
            inputs.AddRange(gesture);
            actions = this.optima_net.activate(inputs, interactive_time,evolutionSession,reward);
            //显示推理链
            this.txtMsg.Text += this.optima_net.showActionPlan();

            interactive_time += 1;
            inferencing = true;
            this.Refresh();


        }
        
        /// <summary>
        /// 显示行动效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            (obs,gesture,actions,reward,end) = ((IEnv)this.demoMaze).action(this.optima_net,
                this.optima_net.Effectors.ConvertAll(x => x.Value[0]));
            (int ptx, int pty) = MeasureTools.Position.poscodeSplit(obs[11]);

            this.txtMsg.Text += "第" + interactive_time.ToString() + "次交互" + System.Environment.NewLine;
            this.txtMsg.Text += "障碍=" + Utility.toString(obs.GetRange(0, 6)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "目标=" + Utility.toString(obs.GetRange(6, 4)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "位置=" + obs[11].ToString("F4")+"("+ ptx.ToString()+","+pty.ToString()+")"+System.Environment.NewLine;
            this.txtMsg.Text += "朝向=" + MeasureTools.Direction.headingToDegree(gesture[0]).ToString("F2") + "(" + gesture[0].ToString("F2") + ")" + System.Environment.NewLine;
            this.txtMsg.Text += "奖励=" + this.reward+ System.Environment.NewLine;
            this.txtMsg.Text += "碰撞=" + this.optimaAgent.PrevCollided.ToString() + "->" + optimaAgent.HasCollided.ToString();
            this.txtMsg.Text += System.Environment.NewLine;

            //this.optima_net.setReward(reward, interactive_time);
            inferencing = false;
            this.Refresh();
           
        }

        
        private void runStep5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int steps = int.Parse(item.Tag.ToString());

            while(true)
            {
                toolStripButton5_Click(null,null);
                toolStripButton7_Click(null, null);
                if(steps > 0)
                {
                    steps -= 1;
                    if (steps <= 0) break;
                }
                else
                {
                    if (reward >= 100) return;
                }
                

            }
        }
        #endregion

        #region 当前个体的内部结构
        private void btnIndStructLevel1_Click(object sender, EventArgs e)
        {
            this.txtMsg.Text += System.Environment.NewLine;
            this.txtMsg.Text += "#####个体结构(Level1)#####";
            //打印推理记忆节点现状
            List<Inference> infs = this.optima_net.Inferences;
            for (int i = 0; i < infs.Count; i++)
            {
                Inference inf = (Inference)infs[i];
                this.txtMsg.Text += inf.ToString();
            }
            this.txtMsg.Text += "##############";
            this.txtMsg.Text += System.Environment.NewLine;
        }

        private void btnIndStructLevel2_Click(object sender, EventArgs e)
        {
            this.txtMsg.Text += System.Environment.NewLine;
            this.txtMsg.Text += "#####个体结构(Level2)#####";
            //打印推理记忆节点现状
            List<Inference> infs = this.optima_net.imagination.inferences;
            for (int i = 0; i < infs.Count; i++)
            {
                Inference inf = (Inference)infs[i];
                this.txtMsg.Text += inf.ToString();
            }
            this.txtMsg.Text += "##############";
            this.txtMsg.Text += System.Environment.NewLine;
        }

        private void btnPolicyShow_Click(object sender, EventArgs e)
        {
            
            if (!btnPolicyShow.Checked) return;
            this.Refresh();

        }
        #endregion
    }
}
