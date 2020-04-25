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
using NWSELib.net.policy;

namespace NWSELib
{
    #region 事件句柄
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
    /// 最优姿态
    /// </summary>
    /// <param name="net"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public delegate Vector OptimaGestureHandler(Network net, int time);

    /// <summary>
    /// 计算适应度方法
    /// </summary>
    /// <param name="net"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public delegate double FitnessHandler(Network net, Session session);

    /// <summary>
    /// 任务启动句柄
    /// </summary>
    /// <param name="net"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public delegate List<double> TaskBeginHandler(Network net, Session session);

    /// <summary>
    /// 推理准确度计算句柄
    /// </summary>
    /// <param name="net"></param>
    /// <param name="session"></param>
    /// <param name="observation"></param>
    /// <param name="actions"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public delegate List<Vector> InferenceVerifyHandler(Network net, Session session, List<Vector> observation,List<double> actions,List<Vector> result);


    
    #endregion


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
        /// 推理验证句柄
        /// </summary>
        public static InferenceVerifyHandler inferenceVerifyHandler;

        /// <summary>
        /// 取得与任务有关的最优姿态
        /// </summary>
        public static OptimaGestureHandler handleGetOptimaGesture;

        public static TaskBeginHandler taskBeginHandler;

        

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
        public List<Network> inds = new List<Network>();
        /// <summary>
        /// 可以完成任务的个体集
        /// </summary>
        public List<Network> taskCompletedNets = new List<Network>();


        /// <summary>
        /// 无效推理基因
        /// </summary>
        public readonly static List<InferenceGene> invalidInfGenes = new List<InferenceGene>();
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
        /// 运行中
        /// </summary>
        private bool running;
        /// <summary>
        /// 运行中
        /// </summary>
        public bool Running 
        { 
            get => running; 
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="env"></param>
        /// <param name="fitness"></param>
        /// <param name="handler"></param>
        /// <param name="instinctHandler"></param>
        public Session(IEnv env, FitnessHandler fitness,EventHandler handler,InstinctActionHandler instinctHandler,OptimaGestureHandler handleGetOptimaGesture, TaskBeginHandler taskBeginHandler)
        {
            this.env = env;
            this.handler = handler;
            Session.instinctActionHandler = instinctHandler;
            Session.fitnessHandler = fitness;
            Session.handleGetOptimaGesture = handleGetOptimaGesture;
            Session.taskBeginHandler = taskBeginHandler;
        }
        #endregion

        #region 事件处理

        public const String EVT_POP_INIT = "population init...";
        public const String EVT_EVAULATION_BEGIN = "net evaulation begin";
        public const String EVT_EVAULATION_END = "net evaulation end";
        public const String EVT_EVAULATION_IND_BEGIN = "individual evaulation begin";
        public const String EVT_EVAULATION_IND_END = "The result of individual evaulation";
        public const String EVT_EVOLUTION_END = "evolution end";

        public const String EVT_INVAILD_GENE = "invaild_gene";
        public const String EVT_VAILD_GENE = "vaild_gene";

        public const String EVT_SELECTION = "evolution selection";
        public const String EVT_GENERATION_END = "generation_end";

        public const String EVT_LOG = "log";
        public const String EVT_STEP = "step";
        

        public const String EVT_MSG = "msg";
        
        public const String EVT_IND_COUNT = "ind_count";
        public const String EVT_REABILITY = "reability";
        
        

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

        public String currentSessionPath;
        private void prepareStorePath()
        {
            String path = System.AppDomain.CurrentDomain.BaseDirectory;
            String sessionPath = "session_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            sessionPath = path + "\\" + sessionPath;
            Directory.CreateDirectory(sessionPath);

            currentSessionPath = sessionPath;
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

        public void stop()
        {
            if (thread == null) return;
            try
            {
                thread.Abort();
            }
            finally
            {
                thread = null;
            }
        }
        public void do_run()
        {
            //1.preparing...
            this.running = true;
            MeasureTools.init();
            logger = LogManager.GetLogger(typeof(Session));
            this.generation = 1;
            prepareStorePath();

            //2.population init... There is only a origin individual in first generation population
            orginGenome = new NWSEGenomeFactory().createOriginGenome(this);
            orginNet = new Network(orginGenome);
            inds.Clear();
            inds.Add(orginNet);
            this.root = new EvolutionTreeNode(orginNet);
            invalidInfGenes.Clear();
            this.triggerEvent(Session.EVT_POP_INIT, inds);


            //3.Interation
            while(true)
            {
                this.triggerEvent(Session.EVT_EVAULATION_BEGIN);
                //evaluation
                foreach (Network net in inds)
                {
                    this.doNetworkEvaluation(net);
                    judgePaused();
                    
                }

                //record optima individual
                double maxFitness = inds.ConvertAll(ind=>ind.Fitness).Max();
                List<Network> optimaNets = inds.FindAll(ind => ind.Fitness == maxFitness);
                if (optimaNets.Count > 0)
                    optimaNets.ForEach(net => net.Save(this.currentSessionPath, this.generation));
                this.triggerEvent(Session.EVT_EVAULATION_END, optimaNets);

                this.root.Save(this.currentSessionPath + "\\tree");


                //是否达到最大迭代次数
                this.generation += 1;
                if (this.generation >= Session.GetConfiguration().evolution.iter_count)
                {
                    triggerEvent(EVT_EVOLUTION_END, this);
                    running = false;
                    return;
                }

                //do evolution
                Evolution evolution = new Evolution();
                evolution.execute(inds, this);

                judgePaused();
                running = false;

            }

        }

        private double doNetworkEvaluation(Network net)
        {
            //1 skip those haved been evaluated. We need 
            //We need to evaluate many times only in noisy environments
            if (!double.IsNaN(net.Fitness) && !Session.GetConfiguration().evaluation.repeat) return net.Fitness;

            int runcount = 0;
            bool end = false;
            while (true)
            {
                //init enviorment and network
                (List<double> obs, List<double> gesture) = env.reset(net);
                net.Reset();
                double reward = 0.0;
                this.triggerEvent(EVT_EVAULATION_IND_BEGIN, net);
                this.triggerEvent(EVT_STEP, net, 0, obs, gesture, null, reward, end);

                //2 Run the network until the maximum number of iterations is reached or the end signal returns from the environment
                int maxtime = Session.GetConfiguration().evaluation.run_count;
                for (int time = 0; time < maxtime; time++)
                {
                    List<double> inputs = new List<double>(obs);
                    inputs.AddRange(gesture);
                    List<double> actions = net.Activate(inputs, time, this, reward);
                    (obs, gesture, actions, reward, end) = env.action(net, actions);
                    this.triggerEvent(EVT_STEP, net, time, obs, gesture, actions, reward, end);
                    if (end) break;
                    judgePaused();
                }
                runcount += 1;
                if (!end) break;
                if (end && runcount >= 2) break;
                
            }

            //评估可靠性和适应度
            net.Inferences.ForEach(inf => inf.CheckReability());
            var reability = net.Reability;
            net.Fitness = Session.fitnessHandler == null ? 0 : Session.fitnessHandler(net, this);
            net.TaskCompleted = end;
            if(end)
            {
                this.taskCompletedNets.Add(net);
                net.Save(this.currentSessionPath, this.generation);
            }
            //评估无效基因
            List<Inference> invaildInference = net.FindInvaildInference();
            List<InferenceGene> invaildInferenceGenes = invaildInference.ConvertAll(inf => inf.GetGene());
            Session.putInvalieInferenceGenes(invaildInferenceGenes.ToArray());
            triggerEvent(Session.EVT_INVAILD_GENE, net, invaildInference);
            //评估结束
            this.triggerEvent(EVT_EVAULATION_IND_END, net);
            return net.Fitness;
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
        #endregion

        #region 无效基因
        public static void putInvalieInferenceGenes(params InferenceGene[] inferenceGenes)
        {
            if (inferenceGenes == null || inferenceGenes.Length <= 0) return;
            foreach(InferenceGene gene in inferenceGenes)
            {
                if (IndexOfInvalidInferenceGenes(gene) >= 0) continue;
                invalidInfGenes.Add(gene.clone<InferenceGene>());


            }
        }

        public static int IndexOfInvalidInferenceGenes(InferenceGene inferenceGene)
        {
            if (inferenceGene == null) return -1;
            String text = inferenceGene.Text;
            for(int i=0;i<invalidInfGenes.Count;i++)
            {
                if (text.Equals(invalidInfGenes[i].Text)) return i;
            }
            return -1;
        }

        public static bool IsInvaildGene(InferenceGene inferenceGene)
        {
            return IndexOfInvalidInferenceGenes(inferenceGene) >= 0;
        }
        #endregion

    }
}