using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Linq;
using System.Threading;
    

using log4net;

using NWSELib.common;
using NWSELib.evolution;
using NWSELib.genome;
using NWSELib.net;
using NWSELib.env;

namespace NWSELib
{
    /// <summary>
    /// 推送事件消息
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="generation"></param>
    /// <param name="states"></param>
    public delegate void EventHandler(String eventName, int generation,params Object[] states);
    /// <summary>
    /// 生成本能动作（总是朝向目标方向走）
    /// </summary>
    /// <param name="net"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public delegate List<double> InstinctActionHandler(Network net, int time);
    /// <summary>
    /// 计算适应度方法
    /// </summary>
    /// <param name="net"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public delegate double FitnessHandler(Network net, Session session);
    /// <summary>
    /// 执行任务
    /// </summary>
    public class Session
    {
        #region 基本信息
        /// <summary>
        /// 配置
        /// </summary>
        public static Configuration config;
        /// <summary>
        /// 进化树
        /// </summary>
        public EvolutionTreeNode root;
        /// <summary>
        /// ID生成器
        /// </summary>
        public static IdGenerator idGenerator = new IdGenerator();
        /// <summary>
        /// 本能行为处理器
        /// </summary>
        public static InstinctActionHandler instinctActionHandler;
        /// <summary>
        /// 适应度函数
        /// </summary>
        public static FitnessHandler fitnessHandler;

        /// <summary>
        /// 配置
        /// </summary>
        public static Configuration GetConfiguration()
        {
            if(config == null)
            {
                config = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(new FileStream("config.xml",FileMode.Open));
                MeasureTools.init();
            }
            return config;
        }
        /// <summary>
        /// 进化树
        /// </summary>
        public EvolutionTreeNode getEvolutionRootNode()
        {
            return root;
        }
        /// <summary>
        /// ID生成器
        /// </summary>
        public IdGenerator GetIdGenerator()
        {
            return idGenerator;
        }

        
        #endregion

        #region 进化状态
        public bool paused;
        /// <summary>
        /// 进化年代
        /// </summary>
        protected int generation = 0;
        /// <summary>
        /// 进化年代
        /// </summary>
        public int Generation { get => generation; set => generation = value; }

        /// <summary>
        /// 日志
        /// </summary>
        public ILog logger;
        /// <summary>
        /// 创世染色体
        /// </summary>
        NWSEGenome orginGenome;
        /// <summary>
        /// 创世个体
        /// </summary>
        Network orginNet;
        /// <summary>
        /// 个体集
        /// </summary>
        List<Network> inds = new List<Network>();
        /// <summary>
        /// 事件处理
        /// </summary>
        private EventHandler handler;
        /// <summary>
        /// 交互环境
        /// </summary>
        private IEnv env;
        /// <summary>
        /// 交互环境
        /// </summary>
        public IEnv Env { get=> env; }
        
        public Session(IEnv env, FitnessHandler fitness,EventHandler handler,InstinctActionHandler instinctHandler)
        {
            this.env = env;
            this.handler = handler;
            Session.instinctActionHandler = instinctHandler;
            Session.fitnessHandler = fitness;
        }

        /// <summary>
        /// 事件触发函数
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="values"></param>
        public void triggerEvent(String eventName,params Object[] values)
        {
            if (handler == null) return;
            handler(eventName,this.generation,values);
        }

        #endregion

        #region 执行
        private Thread thread;
        public void run()
        {
            if (thread != null) return;
            thread = new Thread(new ThreadStart(do_run));
            thread.Start();
        }
        public void do_run()
        {
            //初始化
            
            logger = LogManager.GetLogger(typeof(Session));
            this.generation = 1;

            //初始化初代个体
            orginGenome = new NWSEGenomeFactory().createSimpleGenome(this);
            orginNet = new Network(orginGenome);
            inds.Clear();
            inds.Add(orginNet);
            this.root = new EvolutionTreeNode(orginNet);

            //反复迭代
            while(true)
            {
                logger.Info("#################" + this.generation.ToString() + "#################");
                //环境交互过程
                foreach(Network net in inds)
                {
                    this.triggerEvent(Session.EVT_MESSAGE, "Network("+net.Id+") begin" );
                    (List<double> obs,List<double> gesture) = env.reset(net);
                    double reward = 0.0;
                    bool end = false;
                    for (int time = 0; time <= Session.GetConfiguration().evaluation.run_count; time++) 
                    {
                        List<double> actions = net.activate(obs, time,this, reward);
                        (obs, gesture, actions,reward,end) = env.action(net,actions);
                        this.triggerEvent(Session.EVT_MESSAGE, "time="+time.ToString()+",action=" + actions.ConvertAll(x => x.ToString()).Aggregate((a, b) => String.Format("{0:##.###}", a) + "," + String.Format("{0:##.###}", b))
                            + ",reward = " + reward.ToString() + ", obs="+Utility.toString(obs)+ ",gesture="+Utility.toString(gesture));
                        this.triggerEvent(Session.EVT_MESSAGE, " mental process:" + net.showActionPlan());


                        if (end) break;
                        judgePaused();
                    }

                    net.Fitness = Session.fitnessHandler == null ? 0 : Session.fitnessHandler(net, this);
                    this.triggerEvent(Session.EVT_MESSAGE, "Network(" + net.Id + ") end"+System.Environment.NewLine);

                    judgePaused();
                }
                

                //最优个体
                int indIndex = this.inds.ConvertAll(ind => ind.Fitness).argmax();
                this.triggerEvent(Session.EVT_OPTIMA_IND, this.inds[indIndex]);

                //是否达到最大迭代次数
                this.generation += 1;
                if (this.generation >= Session.GetConfiguration().evolution.iter_count)
                {
                    triggerEvent(EVT_EVOLUTION_END, this);
                    return;
                }

                //进化过程
                Evolution evolution = new Evolution();
                evolution.execute(inds, this);

                judgePaused();

            }

        }
        /// <summary>
        /// 判断是否暂停
        /// </summary>
        /// <returns></returns>
        public bool judgePaused()
        {
            while(paused)
            {
                try
                {
                    Thread.Sleep(1000);

                }catch(ThreadInterruptedException e1)
                {
                    return false;
                }catch(Exception e2)
                {
                    return true;
                }
            }
            return true;
        }

        
        

        public const String EVT_OPTIMA_IND = "optima_ind";
        public const String EVT_MESSAGE = "message";
        public const String EVT_INVAILD_GENE = "invaild_gene";
        public const String EVT_VAILD_GENE = "vaild_gene";
        public const String EVT_IND_COUNT = "ind_count";
        public const String EVT_REABILITY = "reability";
        public const String EVT_EVOLUTION_END = "evolution_end";
    }
    #endregion
}