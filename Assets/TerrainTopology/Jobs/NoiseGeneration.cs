using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    public struct TilingSimplexNoiseGenerator: ICreateTiles, IMakeNoise {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float _terrain_width {get; set;}
        public float _terrain_height {get; set;}
        public float zoom {get; set;}
        public float2 offset {get; set;}
        public float2 per {get; set;}
        public float rot {get; set;}
        public float pixel_size_ws {get; set;}
        private float scaleFactor =>  (_terrain_width / 10000) * (1 / zoom);
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                tile.SetValue(x, z, GetNoiseValue(x, z));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetNoiseValue(int x, int z){
            float2 coord = float2(x, z);
            float2 normCoord = (coord * scaleFactor) + offset;
            // 2-D tiling simplex noise with rotating gradients
            // but without the analytical derivative.
            return noise.psrnoise(normCoord, per, rot);
        }
    }

    public struct CellNoiseGenerator: ICreateTiles, IMakeNoise {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float _terrain_width {get; set;}
        public float _terrain_height {get; set;}
        public float zoom {get; set;}
        public float2 offset {get; set;}
        // NON-OP
        public float2 per {get; set;}
        // NON-OP
        public float rot {get; set;}
        public float pixel_size_ws {get; set;}
        private float scaleFactor =>  (_terrain_width / 10000) * (1 / zoom);
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                tile.SetValue(x, z, GetNoiseValue(x, z));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetNoiseValue(int x, int z){
            float2 coord = float2(x, z);
            float2 normCoord = (coord * scaleFactor) + offset;
            return noise.cnoise(normCoord);
        }
    }

    public struct PeriodicPerlinNoiseGenerator: ICreateTiles, IMakeNoise {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float _terrain_width {get; set;}
        public float _terrain_height {get; set;}
        public float zoom {get; set;}
        public float2 offset {get; set;}
        // NON-OP
        public float2 per {get; set;}
        // NON-OP
        public float rot {get; set;}
        public float pixel_size_ws {get; set;}
        private float scaleFactor =>  (_terrain_width / 10000) * (1 / zoom);
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                tile.SetValue(x, z, GetNoiseValue(x, z));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetNoiseValue(int x, int z){
            float2 coord = float2(x, z);
            float2 normCoord = (coord * scaleFactor) + offset;
            return noise.pnoise(normCoord, per);
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