namespace NWSEExperiment
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.btnERun = new System.Windows.Forms.ToolStripButton();
            this.btnEPause = new System.Windows.Forms.ToolStripButton();
            this.btnEReset = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.btnORun = new System.Windows.Forms.ToolStripButton();
            this.btnOStop = new System.Windows.Forms.ToolStripButton();
            this.btnOStrucuture = new System.Windows.Forms.ToolStripButton();
            this.btnoShowTrail = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.btniOpen = new System.Windows.Forms.ToolStripButton();
            this.btnIEnvReset = new System.Windows.Forms.ToolStripButton();
            this.btnIInference = new System.Windows.Forms.ToolStripButton();
            this.btnIActions = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusXY = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblindcount = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.gbInd = new System.Windows.Forms.GroupBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.pnlMaze = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtMilestone = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.runStep5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stepsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stepsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.stepsToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.stepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stepToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.stepToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.untilEndToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.btnERun,
            this.btnEPause,
            this.btnEReset,
            this.toolStripSeparator1,
            this.toolStripLabel2,
            this.btnORun,
            this.btnOStop,
            this.btnOStrucuture,
            this.btnoShowTrail,
            this.toolStripSeparator2,
            this.toolStripLabel3,
            this.btniOpen,
            this.btnIEnvReset,
            this.btnIInference,
            this.btnIActions,
            this.toolStripSplitButton1,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(916, 40);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(64, 37);
            this.toolStripLabel1.Text = "Evoultion:";
            // 
            // btnERun
            // 
            this.btnERun.Image = ((System.Drawing.Image)(resources.GetObject("btnERun.Image")));
            this.btnERun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnERun.Name = "btnERun";
            this.btnERun.Size = new System.Drawing.Size(34, 37);
            this.btnERun.Text = "Run";
            this.btnERun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnERun.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // btnEPause
            // 
            this.btnEPause.CheckOnClick = true;
            this.btnEPause.Image = ((System.Drawing.Image)(resources.GetObject("btnEPause.Image")));
            this.btnEPause.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEPause.Name = "btnEPause";
            this.btnEPause.Size = new System.Drawing.Size(46, 37);
            this.btnEPause.Text = "Pause";
            this.btnEPause.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnEPause.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // btnEReset
            // 
            this.btnEReset.Image = ((System.Drawing.Image)(resources.GetObject("btnEReset.Image")));
            this.btnEReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEReset.Name = "btnEReset";
            this.btnEReset.Size = new System.Drawing.Size(44, 37);
            this.btnEReset.Text = "Reset";
            this.btnEReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(57, 37);
            this.toolStripLabel2.Text = "Optimal:";
            // 
            // btnORun
            // 
            this.btnORun.Image = ((System.Drawing.Image)(resources.GetObject("btnORun.Image")));
            this.btnORun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnORun.Name = "btnORun";
            this.btnORun.Size = new System.Drawing.Size(34, 37);
            this.btnORun.Text = "Run";
            this.btnORun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnOStop
            // 
            this.btnOStop.Image = ((System.Drawing.Image)(resources.GetObject("btnOStop.Image")));
            this.btnOStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOStop.Name = "btnOStop";
            this.btnOStop.Size = new System.Drawing.Size(39, 37);
            this.btnOStop.Text = "Stop";
            this.btnOStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnOStrucuture
            // 
            this.btnOStrucuture.Image = ((System.Drawing.Image)(resources.GetObject("btnOStrucuture.Image")));
            this.btnOStrucuture.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOStrucuture.Name = "btnOStrucuture";
            this.btnOStrucuture.Size = new System.Drawing.Size(63, 37);
            this.btnOStrucuture.Text = "structure";
            this.btnOStrucuture.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnOStrucuture.Click += new System.EventHandler(this.toolStripButton6_Click_1);
            // 
            // btnoShowTrail
            // 
            this.btnoShowTrail.CheckOnClick = true;
            this.btnoShowTrail.Image = ((System.Drawing.Image)(resources.GetObject("btnoShowTrail.Image")));
            this.btnoShowTrail.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnoShowTrail.Name = "btnoShowTrail";
            this.btnoShowTrail.Size = new System.Drawing.Size(44, 37);
            this.btnoShowTrail.Text = "Track";
            this.btnoShowTrail.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnoShowTrail.Click += new System.EventHandler(this.btnshowTrail_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 40);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(71, 37);
            this.toolStripLabel3.Text = "Intercative:";
            // 
            // btniOpen
            // 
            this.btniOpen.Image = ((System.Drawing.Image)(resources.GetObject("btniOpen.Image")));
            this.btniOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btniOpen.Name = "btniOpen";
            this.btniOpen.Size = new System.Drawing.Size(44, 37);
            this.btniOpen.Text = "Open";
            this.btniOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // btnIEnvReset
            // 
            this.btnIEnvReset.Image = ((System.Drawing.Image)(resources.GetObject("btnIEnvReset.Image")));
            this.btnIEnvReset.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIEnvReset.Name = "btnIEnvReset";
            this.btnIEnvReset.Size = new System.Drawing.Size(68, 37);
            this.btnIEnvReset.Text = "Env Reset";
            this.btnIEnvReset.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIEnvReset.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // btnIInference
            // 
            this.btnIInference.Image = ((System.Drawing.Image)(resources.GetObject("btnIInference.Image")));
            this.btnIInference.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIInference.Name = "btnIInference";
            this.btnIInference.Size = new System.Drawing.Size(65, 37);
            this.btnIInference.Text = "inference";
            this.btnIInference.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIInference.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // btnIActions
            // 
            this.btnIActions.Image = ((System.Drawing.Image)(resources.GetObject("btnIActions.Image")));
            this.btnIActions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIActions.Name = "btnIActions";
            this.btnIActions.Size = new System.Drawing.Size(53, 37);
            this.btnIActions.Text = "actions";
            this.btnIActions.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnIActions.Click += new System.EventHandler(this.toolStripButton7_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 40);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusXY,
            this.lblindcount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(916, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusXY
            // 
            this.statusXY.Name = "statusXY";
            this.statusXY.Size = new System.Drawing.Size(26, 17);
            this.statusXY.Text = "X;Y";
            // 
            // lblindcount
            // 
            this.lblindcount.Name = "lblindcount";
            this.lblindcount.Size = new System.Drawing.Size(62, 17);
            this.lblindcount.Text = "ind count";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 40);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(916, 388);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(908, 362);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Maze";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel
            // 
            this.panel.Controls.Add(this.panel2);
            this.panel.Controls.Add(this.pnlMaze);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(3, 3);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(902, 356);
            this.panel.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tabControl2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(597, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(305, 356);
            this.panel2.TabIndex = 1;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(305, 356);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.gbInd);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(297, 330);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Network";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // gbInd
            // 
            this.gbInd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbInd.Location = new System.Drawing.Point(3, 3);
            this.gbInd.Name = "gbInd";
            this.gbInd.Size = new System.Drawing.Size(291, 324);
            this.gbInd.TabIndex = 1;
            this.gbInd.TabStop = false;
            this.gbInd.Text = "最优个体";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.txtMsg);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(297, 330);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Message";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtMsg
            // 
            this.txtMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMsg.Location = new System.Drawing.Point(3, 3);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(291, 324);
            this.txtMsg.TabIndex = 0;
            // 
            // pnlMaze
            // 
            this.pnlMaze.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMaze.Location = new System.Drawing.Point(0, 0);
            this.pnlMaze.Name = "pnlMaze";
            this.pnlMaze.Size = new System.Drawing.Size(902, 356);
            this.pnlMaze.TabIndex = 0;
            this.pnlMaze.Paint += new System.Windows.Forms.PaintEventHandler(this.panel_Paint);
            this.pnlMaze.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_MouseMove);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtMilestone);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 362);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Mailstone";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtMilestone
            // 
            this.txtMilestone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMilestone.Location = new System.Drawing.Point(3, 3);
            this.txtMilestone.Multiline = true;
            this.txtMilestone.Name = "txtMilestone";
            this.txtMilestone.Size = new System.Drawing.Size(786, 356);
            this.txtMilestone.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.panel1);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(792, 362);
            this.tabPage5.TabIndex = 2;
            this.tabPage5.Text = "Log";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtLog);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(786, 356);
            this.panel1.TabIndex = 0;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(786, 356);
            this.txtLog.TabIndex = 0;
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runStep5ToolStripMenuItem,
            this.stepsToolStripMenuItem,
            this.stepsToolStripMenuItem1,
            this.stepsToolStripMenuItem2,
            this.stepToolStripMenuItem,
            this.stepToolStripMenuItem1,
            this.stepToolStripMenuItem2,
            this.untilEndToolStripMenuItem});
            this.toolStripSplitButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSplitButton1.Image")));
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(105, 37);
            this.toolStripSplitButton1.Text = "multiple steps";
            this.toolStripSplitButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // runStep5ToolStripMenuItem
            // 
            this.runStep5ToolStripMenuItem.Name = "runStep5ToolStripMenuItem";
            this.runStep5ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.runStep5ToolStripMenuItem.Tag = "5";
            this.runStep5ToolStripMenuItem.Text = "5 steps";
            this.runStep5ToolStripMenuItem.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepsToolStripMenuItem
            // 
            this.stepsToolStripMenuItem.Name = "stepsToolStripMenuItem";
            this.stepsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.stepsToolStripMenuItem.Tag = "10";
            this.stepsToolStripMenuItem.Text = "10 steps";
            this.stepsToolStripMenuItem.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepsToolStripMenuItem1
            // 
            this.stepsToolStripMenuItem1.Name = "stepsToolStripMenuItem1";
            this.stepsToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.stepsToolStripMenuItem1.Tag = "15";
            this.stepsToolStripMenuItem1.Text = "15 steps";
            this.stepsToolStripMenuItem1.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepsToolStripMenuItem2
            // 
            this.stepsToolStripMenuItem2.Name = "stepsToolStripMenuItem2";
            this.stepsToolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this.stepsToolStripMenuItem2.Tag = "25";
            this.stepsToolStripMenuItem2.Text = "25 steps";
            this.stepsToolStripMenuItem2.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepToolStripMenuItem
            // 
            this.stepToolStripMenuItem.Name = "stepToolStripMenuItem";
            this.stepToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.stepToolStripMenuItem.Tag = "50";
            this.stepToolStripMenuItem.Text = "50 step";
            this.stepToolStripMenuItem.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepToolStripMenuItem1
            // 
            this.stepToolStripMenuItem1.Name = "stepToolStripMenuItem1";
            this.stepToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.stepToolStripMenuItem1.Tag = "100";
            this.stepToolStripMenuItem1.Text = "100 step";
            this.stepToolStripMenuItem1.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // stepToolStripMenuItem2
            // 
            this.stepToolStripMenuItem2.Name = "stepToolStripMenuItem2";
            this.stepToolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
            this.stepToolStripMenuItem2.Tag = "200";
            this.stepToolStripMenuItem2.Text = "200 step";
            this.stepToolStripMenuItem2.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // untilEndToolStripMenuItem
            // 
            this.untilEndToolStripMenuItem.Name = "untilEndToolStripMenuItem";
            this.untilEndToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.untilEndToolStripMenuItem.Tag = "0";
            this.untilEndToolStripMenuItem.Text = "until end";
            this.untilEndToolStripMenuItem.Click += new System.EventHandler(this.runStep5ToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(916, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStripStatusLabel statusXY;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlMaze;
        private System.Windows.Forms.ToolStripButton btnERun;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox gbInd;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.TextBox txtMilestone;
        private System.Windows.Forms.ToolStripStatusLabel lblindcount;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ToolStripButton btnEPause;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnoShowTrail;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnIEnvReset;
        private System.Windows.Forms.ToolStripButton btnIInference;
        private System.Windows.Forms.ToolStripButton btnIActions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnOStrucuture;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton btnEReset;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton btnORun;
        private System.Windows.Forms.ToolStripButton btnOStop;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripButton btniOpen;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem runStep5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stepsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stepsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem stepsToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem stepToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stepToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem stepToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem untilEndToolStripMenuItem;
    }
}

