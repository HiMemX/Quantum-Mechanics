using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using static OpenTK.Graphics.OpenGL.GL;

namespace QuantumMechanics
{
    internal class CustomPotentialBoundedSimulation
    {

        public double hbar = 0.02;//0.662607015; // Unit: 10^(-33) kg * m²/s
        public double mass = 1;//910.93837; // Unit: 10^(-33) kg

        double domainLength;
        public int spacialResolution;
        public int eigenfunctionCount;

        public Func<double, double, double> potential;
        public Func<double, double, System.Numerics.Complex> targetWavefunction;

        int eigenvaluesSSBO;
        int eigenfunctionsSSBO;

        int componentsSSBO;
        int targetwaveSSBO;

        int targeteigenproductSSBO;
        int wavefunctionSSBO;
        Shader precalculateTargetEigen = new Shader("Shaders\\PrecalculateTargetEigen.glsl");
        Shader reduceToComponents = new Shader("Shaders\\ReduceToComponents.glsl");
        Shader combineEigenwaves = new Shader("Shaders\\CombineEigenwaves.glsl");
        Shader constructMesh = new Shader("Shaders\\ConstructMeshFromProbability.glsl");

        public int meshSSBO, vao; // General purpose, to be rendered
        public int ebo;


        // So, what do we need to do?
        // 1: Solve for Eigenfunctions -> Done by python scipy script
        // 2: Calculate components -> Parallel reduction done by compute shaders
        // 3: Reconstruct -> Not sure, maybe done in the vertex shader?

        public CustomPotentialBoundedSimulation(double domainLength, int spacialResolution)
        {
            this.domainLength = domainLength;
            this.spacialResolution = spacialResolution;

            eigenvaluesSSBO = GL.GenBuffer();
            eigenfunctionsSSBO = GL.GenBuffer();
            componentsSSBO = GL.GenBuffer();
            targetwaveSSBO = GL.GenBuffer();
            targeteigenproductSSBO = GL.GenBuffer();
            wavefunctionSSBO = GL.GenBuffer();


            uint[] indexdata = MeshTools.BuildGridIndices((uint)spacialResolution, (uint)spacialResolution);

            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          indexdata.Length * sizeof(uint),
                          indexdata, BufferUsageHint.StaticDraw);


            vao = GL.GenVertexArray();

            meshSSBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, meshSSBO);

            GL.BindVertexArray(vao);

            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 48, 0);
            GL.EnableVertexAttribArray(0);


            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 48, 16);
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 48, 32);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);


        }

        public void RunPythonProgram()
        {
            BinaryWriter file = new BinaryWriter(File.Create("siminfo.dat"));

            file.Write(eigenfunctionCount);
            file.Write(hbar);
            file.Write(mass);
            file.Write(domainLength);
            file.Write(spacialResolution);

            double ds = (double)domainLength / (double)spacialResolution;
            for (int y = 0; y < spacialResolution; y++)
            {
                for (int x = 0; x < spacialResolution; x++)
                {
                    file.Write(potential(x * ds, y * ds));
                }
            }
            file.Close();

            // Code copied from ChatGPT 
            //string pythonExe = "C:\\Users\\felix\\AppData\\Local\\Microsoft\\WindowsApps\\python.exe";
            string pythonExe = @"C:\Users\felix\AppData\Local\Programs\Python\Python39\python.exe"; // Adjust accordingly
                                                                                                      // Path to the Python script
            string scriptPath = Directory.GetCurrentDirectory() + "\\SolveForEigenfunctions.py";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\"",  // Quote paths in case they contain spaces
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();

                    process.WaitForExit(); // Waits for the script to finish

                    DebugInterface.WriteLine("Output:");
                    DebugInterface.WriteLine(output);

                    if (!string.IsNullOrWhiteSpace(errors))
                    {
                        DebugInterface.WriteLine("Errors:");
                        DebugInterface.WriteLine(errors);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugInterface.WriteLine("Error running Python script:");
                DebugInterface.WriteLine(ex.Message);
            }
        }

        public void SolveForEigenfunctions(bool solve=true)
        {
            if (solve) { RunPythonProgram(); } // Quicko debugging way

            BinaryReader reader = new BinaryReader(File.OpenRead("computed.dat"));
            double[] eigenvalues = new double[eigenfunctionCount];
            double[] eigenfunctions = new double[eigenfunctionCount * spacialResolution * spacialResolution];

            for (int i = 0; i < eigenfunctionCount; i++)
            {
                eigenvalues[i] = reader.ReadDouble();
            }

            double ds = (double)domainLength / (double)spacialResolution;
            for (int i=0; i<eigenfunctions.Length; i++)
            {
                eigenfunctions[i] = reader.ReadDouble() / ds;
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, eigenvaluesSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, eigenvalues.Length * sizeof(double), eigenvalues, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, eigenfunctionsSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, eigenfunctions.Length * sizeof(double), eigenfunctions, BufferUsageHint.DynamicDraw);


            // Construct debug mesh
            int dbgindex = 0;
            
        }

        public void ConstructWavefunction(float t)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, wavefunctionSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 2 * spacialResolution * spacialResolution * sizeof(double), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, meshSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, spacialResolution * spacialResolution * 12 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);


            combineEigenwaves.Use();
            combineEigenwaves.SetDouble("hbar", hbar);
            combineEigenwaves.SetFloat("t", t);
            combineEigenwaves.SetInt("datapointCount", spacialResolution * spacialResolution);
            combineEigenwaves.SetInt("eigenfunctionCount", eigenfunctionCount);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, eigenvaluesSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, eigenfunctionsSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, targeteigenproductSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, wavefunctionSSBO);

            GL.DispatchCompute((spacialResolution * spacialResolution + 127) / 128, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);


            

            constructMesh.Use();
            constructMesh.SetInt("spacialResolution", spacialResolution);
            constructMesh.SetDouble("domainLength", domainLength);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, wavefunctionSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, meshSSBO);

            GL.DispatchCompute((spacialResolution + 31) / 32, (spacialResolution + 31) / 32, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.VertexAttribArrayBarrierBit);
            /*
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, meshSSBO);
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            float[] data = new float[8 * spacialResolution * spacialResolution];
            Marshal.Copy(ptr, data, 0, 8 * spacialResolution * spacialResolution);
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

            foreach (float f in data)
            {
                DebugInterface.WriteLine(f.ToString());
            }*/

        }

        public void ConstructMesh(float[] heights)
        {
            // TEMP: Show an eigenfunction
            float[] meshdata = new float[3 * spacialResolution * spacialResolution];
            for (int y = 0; y < spacialResolution; y++)
            {
                for (int x = 0; x < spacialResolution; x++)
                {
                    float value = heights[spacialResolution * y + x];//(float)eigenfunctions[dbgindex * spacialResolution * spacialResolution + y * spacialResolution + x];


                    meshdata[3 * (y * spacialResolution + x)] = x / (float)spacialResolution * (float)domainLength;
                    meshdata[3 * (y * spacialResolution + x) + 2] = y / (float)spacialResolution * (float)domainLength;
                    meshdata[3 * (y * spacialResolution + x) + 1] = (float)Math.Pow(value, 1);
                }
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, meshSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 3 * spacialResolution * spacialResolution * sizeof(float), meshdata, BufferUsageHint.StaticDraw);
        }

        public void CalculateComponents()
        {
            // Step 1: Precalculate wavefunction at every x,y location
            double[] targetwave = new double[2 * spacialResolution * spacialResolution];
            for (int y = 0; y < spacialResolution; y++)
            {
                for (int x = 0; x < spacialResolution; x++)
                {
                    Complex value = targetWavefunction(x / (double)spacialResolution * (double)domainLength, y / (double)spacialResolution * (double)domainLength);
                    targetwave[2*y * spacialResolution + 2*x] = value.Real;
                    targetwave[2*y * spacialResolution + 2*x + 1] = value.Imaginary;
                }
            }
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, targetwaveSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 2 * spacialResolution * spacialResolution * sizeof(double), targetwave, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, targeteigenproductSSBO);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, 2 * spacialResolution * spacialResolution * eigenfunctionCount * sizeof(double), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Step 2: Precalculate wavefunction * eigenfunction for every eigenfunction
            // How much data is that? 16*spacialRes*spacialRes * eigenfunctioncount [Managable for reasonable sim sizes]
            precalculateTargetEigen.Use();
            precalculateTargetEigen.SetInt("datapointCount", spacialResolution*spacialResolution);
            precalculateTargetEigen.SetInt("eigenfunctionCount", eigenfunctionCount);
            precalculateTargetEigen.SetFloat("ds", (float)domainLength / (float)spacialResolution * (float)domainLength / (float)spacialResolution);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, targetwaveSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, eigenfunctionsSSBO);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, targeteigenproductSSBO);

            GL.DispatchCompute((spacialResolution*spacialResolution + 127) / 128, (eigenfunctionCount + 7) / 8, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);


            // test
            /*
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, targeteigenproductSSBO);
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            double[] data = new double[2 * eigenfunctionCount * spacialResolution * spacialResolution];
            Marshal.Copy(ptr, data, 0, 2 * eigenfunctionCount * spacialResolution * spacialResolution);
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

            float[] heights = new float[spacialResolution * spacialResolution];

            float integral = 0;
            for(int y=0; y<spacialResolution; y++)
            {
                for(int x=0; x < spacialResolution; x++)
                {
                    float value = (float)data[2 * (y * spacialResolution + x)];

                    integral += value ;

                    heights[y * spacialResolution + x] = 1 * value;
                }
            }
            ConstructMesh(heights);

            DebugInterface.WriteLine(integral.ToString());*/

            // Step 3: Reduce!
            reduceToComponents.Use();
            reduceToComponents.SetInt("datapointCount", spacialResolution * spacialResolution);
            reduceToComponents.SetInt("eigenfunctionCount", eigenfunctionCount);

            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, targeteigenproductSSBO);

            int blocksize = (int)Math.Pow(2, Math.Ceiling(Math.Log(spacialResolution * spacialResolution, 2) - 1));
            for(int b=blocksize; b>=1; b /= 2)
            {
                DebugInterface.WriteLine("Reducing blocksize: " + b.ToString());
                reduceToComponents.SetInt("currentBlockSize", b);

                GL.DispatchCompute((b + 127) / 128, (eigenfunctionCount + 7) / 8, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
            }

            // The first values of the targeteigenproductSSBOs previous products are now the components!

            // Debug
            /*GL.BindBuffer(BufferTarget.ShaderStorageBuffer, targeteigenproductSSBO);
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            double[] data = new double[2 * eigenfunctionCount *spacialResolution * spacialResolution];
            Marshal.Copy(ptr, data, 0, 2 * eigenfunctionCount * spacialResolution * spacialResolution);
            GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);

            for(int i=0; i<eigenfunctionCount; i++)
            {
                DebugInterface.WriteLine(data[2 * i * spacialResolution * spacialResolution].ToString() + " + " + data[2 * i * spacialResolution * spacialResolution + 1].ToString() + "j");
            }*/
            
        }


    }
}
