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
    public delegate void EventHandler(String eventName, int generation,params Object[] states);

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
        /// 配置
        /// </summary>
        public static Configuration GetConfiguration()
        {
            if(config == null)
            {
                config = (Configuration)new XmlSerializer(typeof(Configuration)).Deserialize(new FileStream("config.xml",FileMode.Open));
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
        
        public Session(IEnv env,EventHandler handler)
        {
            this.env = env;
            this.handler = handler;
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
            orginGenome = NWSEGenome.create(this);
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
                    logger.Info("gamebegin ind=" + net.Genome.id);
                    this.triggerEvent(Session.EVT_NAME_MESSAGE, "Network("+net.Id+") begin" );
                    List<double> curobs = env.reset(net);

                    List<(List<double>, List<double>,double)> traces = new List<(List<double>, List<double>,double)>();
                    traces.Add((null, curobs, 0));
                    logger.Info("gamereset ind=" + net.Genome.id + ",obs="+ curobs);
                    for (int time = 0; time <= Session.GetConfiguration().evaluation.run_count; time++) 
                    {
                        List<double> actions = net.activate(curobs, time);
                        (List<double> obs,double reward) = env.action(net,actions);

                        curobs = obs;
                        net.setReward(reward);
                        this.triggerEvent(Session.EVT_NAME_MESSAGE, "time="+time.ToString()+",action=" + actions.ConvertAll(x => x.ToString()).Aggregate((a, b) => String.Format("{0:##.###}", a) + "," + String.Format("{0:##.###}", b))
                            + ",reward = " + reward.ToString() + ", obs="+Utility.toString(curobs));
                        this.triggerEvent(Session.EVT_NAME_MESSAGE, " mental process:" + net.getInferenceChainText());
                        

                        traces.Add((actions,obs, reward));
                        logger.Info("gamerun ind=" + net.Genome.id +",time="+time+ ",action"+actions+ ",obs=" + curobs+",reward="+reward);
                        if (reward >= Session.GetConfiguration().evaluation.max_reward)
                        {
                            logger.Info("evolution_end reason=maxreward ind="+net.Genome.id+",generation="+Generation);
                            triggerEvent("evolution_end", "maxreward",net,traces);
                            return;
                        }
                    }
                    this.triggerEvent(Session.EVT_NAME_END_ACTION, net,this.generation);
                    this.triggerEvent(Session.EVT_NAME_MESSAGE, "Network(" + net.Id + ") end"+System.Environment.NewLine);

                    judgePaused();
                }
                this.triggerEvent(Session.EVT_NAME_CLEAR_AGENT);

                //最优个体
                int indIndex = this.inds.ConvertAll(ind => ind.Reability).argmax();
                this.triggerEvent(Session.EVT_NAME_OPTIMA_IND, this.inds[indIndex]);

                this.generation += 1;
                //是否达到最大迭代次数
                if (this.generation >= Session.GetConfiguration().evolution.iter_count)
                {
                    logger.Info("evolution_end reason=max_iter_count");
                    triggerEvent("evolution_end", "max_iter_count");
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
        

        public const String EVT_NAME_DO_ACTION = "do_action";
        public const String EVT_NAME_END_ACTION = "end_action";
        public const String EVT_NAME_CLEAR_AGENT = "clear_agent";
        public const String EVT_NAME_OPTIMA_IND = "optima_ind";
        public const String EVT_NAME_MESSAGE = "message";
        public const String EVT_NAME_INVAILD_GENE = "invaild_gene";
        public const String EVT_NAME_VAILD_GENE = "vaild_gene";
        public const String EVT_NAME_IND_COUNT = "ind_count";
        public const String EVT_NAME_REABILITY = "reability";
    }
    #endregion
}