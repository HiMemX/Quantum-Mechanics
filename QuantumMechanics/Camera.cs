using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace QuantumMechanics
{
    internal class Camera
    {
        public Vector3 orbit; // Position of the camera orbit
        
        public float phi = (float)Math.PI/3f;
        public float theta = 0;
        public float radius = 1.2f;

        public float fov = (float)Math.PI / 4f;
        public float aspectratio { get { return GetAspect(); } }


        public Func<float> GetAspect; // Callback instead of fixed value for easy linking

        public Camera()
        {
            orbit = new Vector3(0.5f,0f, 0.5f);
        }

        public Vector3 GetPosition()
        {
            return orbit + new Vector3(
                radius * (float)(Math.Sin(phi) * Math.Cos(theta)),
                radius * (float)(Math.Cos(phi)),
                radius * (float)(Math.Sin(phi) * Math.Sin(theta))

            );
        }

        public Matrix4 GetProjectionTransform()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, aspectratio, 0.1f, 100); // Hardcoded near and far values for now

        }

        public Matrix4 GetViewTransform()
        {
            return Matrix4.LookAt(GetPosition(), orbit, new Vector3(0, 1, 0));

        }
    }
}
