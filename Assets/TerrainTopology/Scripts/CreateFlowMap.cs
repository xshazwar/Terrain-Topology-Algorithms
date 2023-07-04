using System.Collections;

using System.Collections.Generic;

using UnityEngine;



namespace TerrainTopology
{

    public class CreateFlowMap : CreateTopology
    {

        private const int WEST = 0;
        private const int EAST = 1;
        private const int SOUTH = 2;
        private const int NORTH = 3;

        private const float TIME = 0.2f;

        public int m_iterations = 5;

        protected override void CreateMap()
        {

            float[,] waterMap = new float[m_width, m_height];
            float[,,] outFlow = new float[m_width, m_height, 4];

            FillWaterMap(0.0001f, waterMap, m_width, m_height);

            for(int i = 0; i < m_iterations; i++)
            {
                ComputeOutflow(waterMap, outFlow, m_heights, m_width, m_height);
                UpdateWaterMap(waterMap, outFlow, m_width, m_height);
            }

            float[,] velocityMap = new float[m_width, m_height];

            CalculateVelocityField(velocityMap, outFlow, m_width, m_height);
            NormalizeMap(velocityMap, m_width, m_height);

            Texture2D flowMap = new Texture2D(m_width, m_height);

            for (int y = 0; y < m_height; y++)
            {
                for (int x = 0; x < m_width; x++)
                {
                    float v = velocityMap[x, y];
                    flowMap.SetPixel(x, y, new Color(v, v, v,  EAST));
                }
            }

            flowMap.Apply();
            m_material.mainTexture = flowMap;

        }

        private void FillWaterMap(float amount, float[,] waterMap, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    waterMap[x, y] = amount;
                }
            }
        }

        private void ComputeOutflow(float[,] waterMap, float[,,] outFlow, float[] heightMap, int width, int height)
        {

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xW = (x == 0) ? 0 : x - 1;
                    int xE = (x == width - 1) ? width - 1 : x + 1;
                    int zS = (y == 0) ? 0 : y - 1;
                    int zN = (y == height - 1) ? height - 1 : y + 1;

                    float waterHt = waterMap[x, y];
                    float waterH_W = waterMap[xW, y];
                    float waterH_E = waterMap[xE, y];
                    float waterH_S = waterMap[x, zS];
                    float waterH_N = waterMap[x, zN];

                    float landHt = heightMap[x + y * width];
                    float landH_W = heightMap[xW + y * width];
                    float landH_E = heightMap[xE + y * width];
                    float landH_S = heightMap[x + zS * width];
                    float landH_N = heightMap[x + zN * width];

                    float totalHt = (waterHt + landHt) ;

                    float diff0 = totalHt - (waterH_W + landH_W);
                    float diff1 = totalHt - (waterH_E + landH_E);
                    float diff2 = totalHt - (waterH_S + landH_S);
                    float diff3 = totalHt - (waterH_N + landH_N);

                    //out flow is previous flow plus flow for this time step.
                    float flow0 = Mathf.Max(0, outFlow[x, y,  WEST] + diff0);
                    float flow1 = Mathf.Max(0, outFlow[x, y,  EAST] + diff1);
                    float flow2 = Mathf.Max(0, outFlow[x, y,  SOUTH] + diff2);
                    float flow3 = Mathf.Max(0, outFlow[x, y,  NORTH] + diff3);

                    float sum = flow0 + flow1 + flow2 + flow3;

                    if (sum > 0.0f)
                    {
                        //If the sum of the outflow flux exceeds the amount in the cell
                        //flow value will be scaled down by a factor K to avoid negative update.
                        float K = waterHt / (sum * TIME);
                        if (K > 1.0f) K = 1.0f;
                        if (K < 0.0f) K = 0.0f;

                        outFlow[x, y,  WEST] = flow0 * K;
                        outFlow[x, y,  EAST] = flow1 * K;
                        outFlow[x, y,  SOUTH] = flow2 * K;
                        outFlow[x, y,  NORTH] = flow3 * K;
                    }
                    else
                    {
                        outFlow[x, y,  WEST] = 0.0f;
                        outFlow[x, y,  EAST] = 0.0f;
                        outFlow[x, y,  SOUTH] = 0.0f;
                        outFlow[x, y,  NORTH] = 0.0f;
                    }

                }
            }

        }

        private void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height)
        {

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float flowOUT = outFlow[x, y,  WEST] + outFlow[x, y,  EAST] + outFlow[x, y,  SOUTH] + outFlow[x, y,  NORTH];
                    float flowIN = 0.0f;

                    //Flow in is inflow from neighour cells. Note for the cell on the WEST you need 
                    //thats cells flow to the EAST (ie it flows into this cell)
                    flowIN += (x == 0) ? 0.0f : outFlow[x - 1, y, EAST];
                    flowIN += (x == width - 1) ? 0.0f : outFlow[x + 1, y, WEST];
                    flowIN += (y == 0) ? 0.0f : outFlow[x, y - 1, NORTH];
                    flowIN += (y == height - 1) ? 0.0f : outFlow[x, y + 1, SOUTH];

                    float ht = waterMap[x, y] + (flowIN - flowOUT) * TIME;
                    if (ht < 0.0f) ht = 0.0f;

                    //Result is net volume change over time
                    waterMap[x, y] = ht;
                }
            }

        }

        private void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height)
        {

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x <width; x++)
                {
                    float dl = (x == 0) ? 0.0f : outFlow[x - 1, y, EAST] - outFlow[x, y, WEST];

                    float dr = (x == width - 1) ? 0.0f : outFlow[x, y, EAST] - outFlow[x + 1, y, WEST];

                    float dt = (y == height - 1) ? 0.0f : outFlow[x, y + 1, SOUTH] - outFlow[x, y, NORTH];

                    float db = (y == 0) ? 0.0f : outFlow[x, y, SOUTH] - outFlow[x, y - 1, NORTH];

                    float vx = (dl + dr) * 0.5f;
                    float vy = (db + dt) * 0.5f;

                    velocityMap[x, y] = Mathf.Sqrt(vx * vx + vy * vy);
                }

            }

        }

        public static void NormalizeMap(float[,] map, int width, int height)
        {

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = map[x, y];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            float size = max - min;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = map[x, y];

                    if (size < 1e-12f)
                        v = 0;
                    else
                        v = (v - min) / size;

                    map[x, y] = v;
                }
            }

        }

    }

}
