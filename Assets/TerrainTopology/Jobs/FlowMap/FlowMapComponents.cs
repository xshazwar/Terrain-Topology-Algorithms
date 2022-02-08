using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

    public struct ComputeFlowStep: IComputeFlowData {
        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public ReadTileData water {get; set;}
        public RWTileData outN {get; set;}
        public RWTileData outS {get; set;}
        public RWTileData outE {get; set;}
        public RWTileData outW {get; set;}
        private const float TIMESTEP = 0.2f;

        public void CalculateCell<T>(int x, int z, T land) where  T : struct, IReadOnlyTile {
            float water_0 = water.GetData(x, z);
            float land_0 = land.GetData(x, z);

            int xE = x + 1;
            int xW = x - 1;
            int zN = z + 1;
            int zS = z - 1;
            
            float4 diff = new float4( //W, E, S, N
                water_0 + land_0 - (water.GetData(xW, z) + land.GetData(xW, z)),
                water_0 + land_0 - (water.GetData(xE, z) + land.GetData(xE, z)),
                water_0 + land_0 - (water.GetData(zS, x) + land.GetData(zS, x)),
                water_0 + land_0 - (water.GetData(zN, x) + land.GetData(zN, x))
            );

            float4 flow = new float4( //W, E, S, N
                max(0, outW.GetData(x, z) + diff.x ),
                max(0, outE.GetData(x, z) + diff.y ),
                max(0, outS.GetData(x, z) + diff.z ),
                max(0, outN.GetData(x, z) + diff.w )
            );

            float sum_ = csum(flow);

            if (sum_ > 0){
                float K = water_0 / (sum_ * TIMESTEP);
                K = clamp(K, 0, 1);

                outW.SetValue(x, z, flow.x * K);
                outE.SetValue(x, z, flow.y * K);
                outS.SetValue(x, z, flow.z * K);
                outN.SetValue(x, z, flow.w * K);
            }else{
                outW.SetValue(x, z, 0);
                outE.SetValue(x, z, 0);
                outS.SetValue(x, z, 0);
                outN.SetValue(x, z, 0);
            }
        }

        public void Execute<T>(int z, T tile) where  T : struct, IReadOnlyTile {
            for( int x = 0; x < Resolution; x++){
                CalculateCell<T>(x, z, tile);
            }
        }
    }

    public struct UpdateWaterStep: IComputeWaterLevel {
        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public RWTileData water {get; set;}
        public ReadTileData outN {get; set;}
        public ReadTileData outS {get; set;}
        public ReadTileData outE {get; set;}
        public ReadTileData outW {get; set;}
        private const float TIMESTEP = 0.2f;

        public void CalculateCell<T>(int x, int z, T land) where  T : struct, IReadOnlyTile {
            float flowOUT = outW.GetData(x, z) +
                outE.GetData(x, z) +
                outS.GetData(x, z) +
                outN.GetData(x, z);
            float flowIN = 0;
            //from E flowing W
            flowIN += outW.GetData(x + 1, z);
            //from W flowing E
            flowIN += outE.GetData(x - 1, z);
            //from N flowing S
            flowIN += outS.GetData(x, z + 1);
            //from S flowing N
            flowIN += outN.GetData(x, z - 1);

            float ht = water.GetData(x, z) + (flowIN - flowOUT) * TIMESTEP;
            ht = max(0, ht);
            water.SetValue(x, z, ht);
            
        }

        public void Execute<T>(int z, T tile) where  T : struct, IReadOnlyTile {
            for( int x = 0; x < Resolution; x++){
                CalculateCell<T>(x, z, tile);
            }
        }
    }

    public struct CreateVelocityField: IWriteFlowMap {
        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public ReadTileData outN {get; set;}
        public ReadTileData outS {get; set;}
        public ReadTileData outE {get; set;}
        public ReadTileData outW {get; set;}
        private const float TIMESTEP = 0.2f;

        public void CalculateCell<T>(int x, int z, T map) where  T : struct, IWriteOnlyTile {
            float4 dd = new float4(
                outW.GetData(x + 1, z) - outE.GetData(x, z),
                outE.GetData(x - 1, z) - outW.GetData(x, z),
                outS.GetData(x, z + 1) - outN.GetData(x, z),
                outN.GetData(x, z - 1) - outS.GetData(x, z)
            );
            float vx = (dd.x +dd.y) * 0.5f;
            float vy = (dd.z +dd.w) * 0.5f;

            map.SetValue(x, z, sqrt(vx*vx + vy*vy));
        }

        public void Execute<T>(int z, T tile) where  T : struct, IWriteOnlyTile {
            for( int x = 0; x < Resolution; x++){
                CalculateCell<T>(x, z, tile);
            }
        }
    }

}