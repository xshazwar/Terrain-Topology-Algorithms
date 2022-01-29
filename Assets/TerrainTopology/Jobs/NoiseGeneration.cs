using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    public struct PerlinNoiseGenerator: IMutateTiles, IMakeNoise {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float _terrain_width {get; set;}
        public float _terrain_height {get; set;}
        public float zoom {get; set;}
        public float2 offset {get; set;}
        public float2 per {get; set;}
        public float rot {get; set;}
        public float pixel_size_ws {get; set;}
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData {
            int idx = z * (Resolution - 1);
            float2 coord = float2(0, z);
            for( int x = 0; x < Resolution; x++){
                coord.x = x;
                tile.SetValue(idx++, GetNoiseValue(coord));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetNoiseValue(float2 coord){
            return noise.psrnoise((coord + offset ) * zoom, per, rot);
        }
    }

    public struct WriteOnlyTileData: ImTileData{

        [NativeDisableContainerSafetyRestriction]
        NativeSlice<float> data;
        public void Setup(NativeSlice<float> source){
            data = source;

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int idx, float value){
            data[idx] = value;
        }

    }
}