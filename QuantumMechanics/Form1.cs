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
        WavefunctionConfig config = new WavefunctionConfig();

        float simulationtime = 0;
        float timestep = 0.02f;
        bool isRunning = false;

        private readonly Timer _timer = new Timer();

        public mainForm()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = config;

            DebugInterface.WriteToLogCallback = (string s) => { debugLogTextBox.AppendText(s); debugLogTextBox.Refresh(); };


            DoubleBuffered = true;          // avoid flicker
            _timer.Interval = 16;           // ~1000 ms / 60 fps
            _timer.Tick += (s, e) => glControl.Invalidate();
            _timer.Start();
        }



        public double RadialInversePotential(double x, double y)
        {


            return -0.01 / (0.005 + Math.Sqrt(Math.Pow(x - 0.5, 2) + Math.Pow(y - 0.5, 2)));
        }

        public double RadialHarmonicPotential(double x, double y)
        {
            x -= 0.5;
            y -= 0.5;
            return x * x + y * y;
        }

        public double LinearPotential(double x, double y)
        {
            return x;
        }

        public double WallPotential(double x, double y)
        {
            if (x > 0.48 && (x < 0.52)) return 0.2;
            if (x > 0.55) return x - 0.55;
            return 0;
        }

        public double SlitPotential(double x, double y)
        {
            if (x > 0.48 && (x < 0.52) && !(y > 0.45 && (y < 0.55))) return 1000;
            if (x > 0.55) return x - 0.55;
            return 0;
        }

        public double DoubleSlitPotential(double x, double y) // Works well!
        {
            float d = 0.1f;
            float s = 0.06f;

            if (x > 0.48 && (x < 0.52) && !(y > 0.5 - d - s && (y < 0.5 - d + s)) && !(y > 0.5 + d - s && (y < 0.5 + d + s))) return 100;
            
            
            //if (x > 0.55) return 2 * (x - 0.55);
            return 0;
        }

        private System.Numerics.Complex Wavefunction(double x, double y)
        {
            double posx = config.posx;
            double posy = config.posy;

            double velx = config.velx;
            double vely = config.vely;

            x -= posx;
            y -= posy;

            double magnitude = config.amplitude * Math.Exp(- 1 / (2 * config.sigma*config.sigma) * (x * x + y * y));

            return new System.Numerics.Complex(Math.Cos(velx * x + vely * y) * magnitude, Math.Sin(velx * x + vely * y) * magnitude);
        }

        private void SetupSimulation()
        {
            timestep = 0.02f;

            simulation = new CustomPotentialBoundedSimulation(1, 64);

            simulation.hbar = 0.03;
            simulation.mass = 0.1;
            simulation.eigenfunctionCount = 512;

            simulation.potential = DoubleSlitPotential;//(double x, double y) => { return simulation.mass * x; };
            simulation.targetWavefunction = Wavefunction;//(double x, double y) => { x -= 0.8; y -= 0.5; return new System.Numerics.Complex(0.7 * , 0); };
            simulation.SolveForEigenfunctions();
            simulation.CalculateComponents();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 8, 8));
            glControlPanel.Controls.Add(glControl);
            glControl.Parent = glControlPanel;
            glControl.Dock = DockStyle.Fill;

            glControl.Resize += glControl_Resize;
            glControl.Paint += glControl_Paint;
            GL.Viewport(0, 0, glControlPanel.Width, glControlPanel.Height);
            GL.Enable(EnableCap.DepthTest);

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
            isRunning = !isRunning;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            simulationtime = 0;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            simulation.CalculateComponents();
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {

            if (isRunning) simulationtime += config.timestep;

            simulation.ConstructWavefunction(simulationtime);

            glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Use();
            Matrix4 mat = cam.GetViewTransform() * cam.GetProjectionTransform();
            GL.BindVertexArray(simulation.vao);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, simulation.ebo);


            GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "mat"), false, ref mat);

            GL.DrawElements(PrimitiveType.Triangles, (simulation.spacialResolution - 1) * (simulation.spacialResolution - 1) * 6,
                DrawElementsType.UnsignedInt, 0);

            //GL.DrawArrays(PrimitiveType.Points, 0, simulation.spacialResolution* simulation.spacialResolution);
            GL.BindVertexArray(0);

            glControl.SwapBuffers();


            cam.theta += 0.005f;
        }
    }



    public class WavefunctionConfig
    {
        public double posx { get; set; }
        public double posy { get; set; }
        public double velx { get; set; }
        public double vely { get; set; }

        public double sigma { get; set; }
        public double amplitude { get; set; }

        public float timestep { get; set; }
    
        public WavefunctionConfig()
        {
            posx = 0.5;
            posy = 0.5;
            velx = 0;
            vely = 0;

            sigma = 0.1;
            amplitude = 0.7;

            timestep = 0.02f;
        }
    }
}
