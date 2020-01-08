using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

using log4net;

using NWSELib.evolution;
using NWSELib.genome;
using NWSELib.net;
using NWSELib.env;

namespace NWSELib
{
    public delegate void EventHandler(String eventName, params Object[] states);

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
        public IdGenerator idGenerator = new IdGenerator();

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
        

        /// <summary>
        /// 事件触发函数
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="values"></param>
        public void triggerEvent(String eventName,params Object[] values)
        {

            if (handler == null) return;
            handler(eventName,values);
        }

        #endregion

        #region 执行
        public void run(IEnv env,EventHandler handler)
        {
            //初始化
            this.handler = handler;
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
                        this.triggerEvent("do_action", net);
                        traces.Add((actions,obs, reward));
                        logger.Info("gamerun ind=" + net.Genome.id +",time="+time+ ",action"+actions+ ",obs=" + curobs+",reward="+reward);
                        if (reward >= Session.GetConfiguration().evaluation.max_reward)
                        {
                            logger.Info("evolution_end reason=maxreward ind="+net.Genome.id+",generation="+Generation);
                            triggerEvent("evolution_end", "maxreward",net,traces);
                            return;
                        }
                    }
                    this.triggerEvent(Session.EVT_NAME_END_ACTION, net);

                }
                this.triggerEvent(Session.EVT_NAME_CLEAR_AGENT);

                //进化过程
                Evolution evolution = new Evolution();
                evolution.execute(inds, this);

                //是否达到最大迭代次数
                this.generation += 1;
                if(this.generation >= Session.GetConfiguration().evolution.iter_count)
                {
                    logger.Info("evolution_end reason=max_iter_count");
                    triggerEvent("evolution_end", "max_iter_count");
                    return;
                }
            }

        }

        public const String EVT_NAME_DO_ACTION = "do_action";
        public const String EVT_NAME_END_ACTION = "end_action";
        public const String EVT_NAME_CLEAR_AGENT = "clear_agent";

    }
    #endregion
}