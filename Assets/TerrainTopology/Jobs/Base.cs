// namespace xshazwar.processing.cpu.mutate {
//     public struct BaseJob: IKernelTiles{

//         int JobLength {get; set;}
//         int Resolution {get; set;}
//         float _terrain_width {get; set;}
//         float _terrain_height {get; set;}

//         float pixel_size_ws => _terrain_width / Resolution;

//         protected float GetNormalizedHeight(int x, int z)
//         {
//             x = clamp(x, 0, Resolution - 1);
//             z = clamp(z, 0, Resolution - 1);

//             return m_heights[x + z * Resolution];
//         }

//         protected float GetHeight(int x, int y)
//         {
//             return GetNormalizedHeight(x, y) * _terrain_height;
//         }

//         protected float2 GetFirstDerivative(int x, int y)
//         {
//             float w = pixel_size_ws;
//             float z1 = GetHeight(x - 1, y + 1);
//             float z2 = GetHeight(x + 0, y + 1);
//             float z3 = GetHeight(x + 1, y + 1);
//             float z4 = GetHeight(x - 1, y + 0);
//             float z6 = GetHeight(x + 1, y + 0);
//             float z7 = GetHeight(x - 1, y - 1);
//             float z8 = GetHeight(x + 0, y - 1);
//             float z9 = GetHeight(x + 1, y - 1);

//             //p, q
//             float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
//             float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

//             return new float2(-zx, -zy);
//         }

//         protected void GetDerivatives(int x, int y, out float2 d1, out float3 d2)
//         {
//             float w = m_cellLength;
//             float w2 = w * w;
//             float z1 = GetHeight(x - 1, y + 1);
//             float z2 = GetHeight(x + 0, y + 1);
//             float z3 = GetHeight(x + 1, y + 1);
//             float z4 = GetHeight(x - 1, y + 0);
//             float z5 = GetHeight(x + 0, y + 0);
//             float z6 = GetHeight(x + 1, y + 0);
//             float z7 = GetHeight(x - 1, y - 1);
//             float z8 = GetHeight(x + 0, y - 1);
//             float z9 = GetHeight(x + 1, y - 1);

//             //p, q
//             float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
//             float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);

//             //r, t, s
//             float zxx = (z1 + z3 + z4 + z6 + z7 + z9 - 2.0f * (z2 + z5 + z8)) / (3.0f * w2);
//             float zyy = (z1 + z2 + z3 + z7 + z8 + z9 - 2.0f * (z4 + z5 + z6)) / (3.0f * w2);
//             float zxy = (z3 + z7 - z1 - z9) / (4.0f * w2);

//             d1 = new float2(-zx, -zy);
//             d2 = new float3(-zxx, -zyy, -zxy); //is zxy or -zxy?
//         }

//         protected void ApplyKernelUnseparableRow(
//             NativeArray<float> src,
//             D target,
//             NativeArray<float> kernel,
//             int kernel_dim_x,
//             float kernel_const
//             ) where D : struct, ImTileData
//         {
//             var heights = new float[m_width * m_height];

//             // var gaussianKernel5 = new float[,]
//             // {
//             //     {1,4,6,4,1},
//             //     {4,16,24,16,4},
//             //     {6,24,36,24,6},
//             //     {4,16,24,16,4},
//             //     {1,4,6,4,1}
//             // };

//             float gaussScale = 1.0f / 256.0f;

//             for (int y = 0; y < m_height; y++)
//             {
//                 for (int x = 0; x < m_width; x++)
//                 {
//                     float sum = 0;

//                     for (int i = 0; i < 5; i++)
//                     {
//                         for (int j = 0; j < 5; j++)
//                         {
//                             int xi = x - 2 + i;
//                             int yi = y - 2 + j;

//                             sum += GetNormalizedHeight(xi, yi) * gaussianKernel5[i, j] * gaussScale;
//                         }
//                     }

//                     heights[x + y * m_width] = sum;
//                 }
//             }

//             m_heights = heights;
//         }

//     }
// }