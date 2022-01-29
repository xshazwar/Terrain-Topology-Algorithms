using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    public struct PerlinNoiseGenerator: ICreateTiles, IMakeNoise {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float _terrain_width {get; set;}
        public float _terrain_height {get; set;}
        public float zoom {get; set;}
        public float2 offset {get; set;}
        public float2 per {get; set;}
        public float rot {get; set;}
        public float pixel_size_ws {get; set;}
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
            int max = Resolution;
            float2 coord = float2(0, z);
            for( int x = 0; x < max; x++){
                coord.x = x;
                tile.SetValue(x, z, GetNoiseValue(coord));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetNoiseValue(float2 coord){
            return noise.psrnoise((coord * zoom) + offset, per, rot);
        }
    }

    public struct RWTileData: ImTileData, IGetTileData, ISetTileData{

        [ReadOnly]
        NativeArray<float> src;

        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
        NativeArray<float> dst;
        int resolution;

        public void Setup(NativeArray<float> source, int resolution_){
            src = new NativeArray<float>(source.Length, Allocator.Temp);
            NativeArray<float>.Copy(source, src);
            dst = source;
            resolution = resolution_;

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getIdx(int x, int z){
            // overflows safely
            x = clamp(x, 0, resolution - 1);
            z = clamp(z, 0, resolution - 1);
            return (z * (resolution - 1)) + x;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetData(int x, int z){
            return src[getIdx(x,z)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int z, float value){
            dst[getIdx(x,z)] = value;
        }
    }

    public struct WriteOnlyTileData: ImTileData, ISetTileData{

        [ReadOnly]
        NativeArray<float> src;

        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
        NativeArray<float> dst;
        int resolution;

        public void Setup(NativeArray<float> source, int resolution_){
            dst = source;
            resolution = resolution_;

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getIdx(int x, int z){
            // overflows safely
            x = clamp(x, 0, resolution - 1);
            z = clamp(z, 0, resolution - 1);
            return (z * (resolution - 1)) + x;
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int z, float value){
            dst[getIdx(x,z)] = value;
        }
    }
}