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
        public HardMaze maze;
        private CoordinateFrame frame;
        public Session evolutionSession;
        private ILog logger = LogManager.GetLogger(typeof(MainForm));

        private Network optima_net;
        private int optima_generation;

        public MainForm()
        {
            Form.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure();
            this.Width = Session.GetConfiguration().view.width;
            this.Height = Session.GetConfiguration().view.height;


            maze = HardMaze.loadEnvironment("QDMaze.xml");
            this.panel2.Width = Session.GetConfiguration().view.width - maze.AOIRectangle.Width - 10;
            //this.Refresh();

            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            
        }

        public void panel_Paint(object sender, PaintEventArgs e)
        {
            
            if (maze == null) return;
            if(frame == null)
                frame = new CoordinateFrame(0.0f, maze.AOIRectangle.Y, 1.1f, 0.0f);
            
            maze.draw(e.Graphics, frame);
        }

        private void showOptimaInd(Graphics g)
        {
            if (this.optima_net == null) return;
            Network net = optima_net;
            gbInd.Text = "generation=" + this.optima_generation + ",network=" + net.Id;

            int hspace = 5, vspace = 10;
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (maze == null || frame == null) return;
            float mazeX, mazeY;
            frame.convertFromDisplay(e.X, e.Y, out mazeX, out mazeY);
            this.statusXY.Text = String.Format("X={0:000.00},Y={1:000.00}", mazeX, mazeY);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (evolutionSession != null) return;
            try
            {
                evolutionSession = new Session(this.maze, EventHandler);
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
            if(eventName == Session.EVT_NAME_DO_ACTION)
            {
                Network net = (Network)states[0];
                maze.updateAgent(net);
                this.Refresh();
            }else if(eventName == Session.EVT_NAME_END_ACTION)
            {
                Network net = (Network)states[0];
                //maze.removeAgent(net);
                this.Refresh();
            }
            else if(eventName == Session.EVT_NAME_CLEAR_AGENT)
            {
                //maze.clearAgent();
                this.Refresh();
            }else if(eventName == Session.EVT_NAME_OPTIMA_IND)
            {
                this.optima_generation = generation;
                this.optima_net = (Network)states[0];
            }else if(eventName == Session.EVT_NAME_MESSAGE)
            {
                txtLog.Text += states[0].ToString() + System.Environment.NewLine;
            }else if(eventName == Session.EVT_NAME_INVAILD_GENE)
            {
                Network net = (Network)states[0];
                NodeGene gene = (NodeGene)states[1];
                txtMilestone.Text += "########" + generation.ToString() + "########" + System.Environment.NewLine;
                txtMilestone.Text += "gene was eliminated in generation" + generation.ToString() + ":" + System.Environment.NewLine;
                txtMilestone.Text += gene.ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_NAME_VAILD_GENE)
            {
                Network net = (Network)states[0];
                NodeGene gene = (NodeGene)states[1];
                txtMilestone.Text += "########" + generation.ToString() + "########" + System.Environment.NewLine;
                txtMilestone.Text += "gene was considered valid in generation" + generation.ToString() + ":" + System.Environment.NewLine;
                txtMilestone.Text += gene.ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_NAME_REABILITY)
            {
                txtLog.Text += "reablity avg=" + states[0].ToString() + ",variance=" + states[1].ToString() + System.Environment.NewLine;
            }
            else if(eventName == Session.EVT_NAME_IND_COUNT)
            {
                this.lblindcount.Text = "size of population:"+states[1].ToString() + "(Preceding " + states[0].ToString()+")";
                txtLog.Text += "Elimination stat:" + states[1].ToString() + "(Preceding " + states[0].ToString() + ")" + ",reability limit=" + states[2].ToString();
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (this.evolutionSession == null) return;
            this.evolutionSession.paused = !this.evolutionSession.paused;
        }

        private void btnshowTrail_Click(object sender, EventArgs e)
        {
            if (this.maze == null) return;
            this.maze.ShowTrail = btnshowTrail.Checked ;
        }

        private int interactive_time = 0;
        private List<double> obs;
        private List<double> gesture;
        private List<double> actions;
        /// <summary>
        /// 交互式环境重置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            this.maze = new HardMaze();
            this.evolutionSession = new Session(maze,null);
            this.optima_net = new Network(NWSEGenome.create(evolutionSession));
            evolutionSession.root = new EvolutionTreeNode(optima_net);
            interactive_time = 0;
            (obs, gesture) = maze.reset(optima_net);

            this.txtMsg.Text = "第" + interactive_time.ToString() + "次交互" + System.Environment.NewLine;
            this.txtMsg.Text += "障碍=" + Utility.toString(obs.GetRange(0, 6)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "目标=" + Utility.toString(obs.GetRange(6, 4)) + System.Environment.NewLine; ;
            this.txtMsg.Text += "朝向=" + gesture[0].ToString("F3") + System.Environment.NewLine;
            this.txtMsg.Text += System.Environment.NewLine;
        }
        /// <summary>
        /// 接收输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //网络执行
            actions = this.optima_net.activate(obs, interactive_time);
            //打印推理记忆节点现状
            List<Node> infs = this.optima_net.Inferences;
            for(int i=0;i<infs.Count;i++)
            {
                Inference inf = (Inference)infs[i];
                this.txtMsg.Text += "推理节点=" + inf.Gene.Text+ System.Environment.NewLine;
                this.txtMsg.Text += "   记录数=" + inf.Records.Count.ToString() + System.Environment.NewLine;
                for(int j=0;j< inf.Records.Count;j++)
                {
                    this.txtMsg.Text += "   mean" + j.ToString() + "=" + Utility.toString(inf.Records[j].means.flatten().Item1.ToList()) + System.Environment.NewLine;
                }
                
            }
            this.txtMsg.Text += System.Environment.NewLine;
        }
        /// <summary>
        /// 查看执行规划
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            this.txtMsg.Text += "执行规划:" + System.Environment.NewLine;
            if (this.optima_net.actionPlanChain == null)
            {
                this.txtMsg.Text += "随机动作=" +
                this.optima_net.Effectors.ConvertAll(x => x.Value[0].ToString("F3")).Aggregate((a, b) => a + "," + b)
                + System.Environment.NewLine;

                return;
            }
            this.txtMsg.Text += this.optima_net.actionPlanChain.curPlanItem.printCurrentItem() + System.Environment.NewLine;


        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            ((IEnv)this.maze).action(this.optima_net,
                this.optima_net.Effectors.ConvertAll(x => x.Value[0]));
        }
    }
}
