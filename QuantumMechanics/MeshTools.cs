using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumMechanics
{
    internal static class MeshTools
    {
        public static uint[] BuildGridIndices(uint cols, uint rows)
        {
            if (cols < 2 || rows < 2)
                throw new ArgumentException("Grid must be at least 2 × 2 vertices.");

            uint cellsPerRow = cols - 1;
            uint totalCells = cellsPerRow * (rows - 1);
            uint[] indices = new uint[totalCells * 6];

            uint i = 0;  // write pointer into indices[]
            for (uint y = 0; y < rows - 1; ++y)
            {
                uint rowStart = y * cols;
                uint nextRow = rowStart + cols;

                for (uint x = 0; x < cols - 1; ++x)
                {
                    uint v00 = rowStart + x;       // top‑left
                    uint v10 = v00 + 1;            // top‑right
                    uint v01 = nextRow + x;        // bottom‑left
                    uint v11 = v01 + 1;            // bottom‑right

                    // triangle 1: v00, v01, v11
                    indices[i++] = v00;
                    indices[i++] = v01;
                    indices[i++] = v11;
                    // triangle 2: v00, v11, v10
                    indices[i++] = v00;
                    indices[i++] = v11;
                    indices[i++] = v10;
                }
            }

            return indices;
        }
    }
}
