using NWSEExperiment.maze;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace NWSEExperiment
{
    public partial class MainForm : Form
    {
        protected HardMaze maze;
        private CoordinateFrame frame;
        public MainForm()
        {
            
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            maze = HardMaze.loadEnvironment("QDMaze.xml");
            this.Refresh();
        }

        public void panel_Paint(object sender, PaintEventArgs e)
        {
            
            if (maze == null) return;
            if(frame == null)
                frame = new CoordinateFrame(0.0f, maze.AOIRectangle.Y, 1.1f, 0.0f);
            maze.draw(e.Graphics, frame);
        }

        private void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (maze == null || frame == null) return;
            float mazeX, mazeY;
            frame.convertFromDisplay(e.X, e.Y, out mazeX, out mazeY);
            this.statusXY.Text = String.Format("X={0:000.00},Y={1:000.00}", mazeX, mazeY);

        }
    }
}
