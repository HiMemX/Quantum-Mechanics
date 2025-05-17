using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace QuantumMechanics
{
    public partial class mainForm : Form
    {
        GLControl glControl;

        Shader shader;
        Camera cam;
        int ssbo, vao;

        CustomPotentialBoundedSimulation simulation;

        float simulationtime = 0;
        bool isRunning = false;

        private readonly Timer _timer = new Timer();

        public mainForm()
        {
            InitializeComponent();

            DebugInterface.WriteToLogCallback = (string s) => { debugLogTextBox.AppendText(s); debugLogTextBox.Refresh(); };


            DoubleBuffered = true;          // avoid flicker
            _timer.Interval = 16;           // ~1000 ms / 60 fps
            _timer.Tick += (s, e) => glControl.Invalidate();
            _timer.Start();
        }



        private void SetupSimulation()
        {

            simulation = new CustomPotentialBoundedSimulation(1, 64);

            simulation.mass = 1;
            simulation.eigenfunctionCount = 256;

            simulation.potential = (double x, double y) => { return -0.01 / (0.0001 + Math.Pow(x-0.5, 2) + Math.Pow(y - 0.5, 2)); };// return x * simulation.mass; };
            simulation.targetWavefunction = (double x, double y) => { x -= 0.8; y -= 0.5; return new System.Numerics.Complex(0.7 * Math.Exp(-80 * (x*x+y*y)), 0); };
            simulation.SolveForEigenfunctions();
            simulation.CalculateComponents();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            glControl = new GLControl();
            glControlPanel.Controls.Add(glControl);
            glControl.Parent = glControlPanel;
            glControl.Dock = DockStyle.Fill;

            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            GL.Viewport(0, 0, glControlPanel.Width, glControlPanel.Height);
            GL.PointSize(2);

            GL.ClearColor(Color4.Black);

            shader = new Shader("Shaders\\vertex.glsl", "Shaders\\frag.glsl");
            cam = new Camera();
            cam.GetAspect = () => { return (float)glControlPanel.Width / (float)glControlPanel.Height; };

            SetupSimulation();
        }

        private void glControl_Resize(object sender, EventArgs e)
        {

            glControl.MakeCurrent();
            GL.Viewport(0, 0, glControlPanel.Width, glControlPanel.Height);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            isRunning = true;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            isRunning = false;
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {

            if (isRunning) simulationtime += 0.02f;

            simulation.ConstructWavefunction(simulationtime);

            glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit);

            shader.Use();
            Matrix4 mat = cam.GetViewTransform() * cam.GetProjectionTransform();
            GL.BindVertexArray(simulation.vao);


            GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "mat"), false, ref mat);

            GL.DrawArrays(PrimitiveType.Points, 0, 64*64);
            GL.BindVertexArray(0);

            glControl.SwapBuffers();


            cam.theta += 0.005f;
        }
    }
}
