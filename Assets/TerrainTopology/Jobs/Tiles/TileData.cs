using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

    public struct RWTileData: IRWTile{

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        NativeArray<float> src;

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
        NativeSlice<float> dst;
        int resolution;

        public void Setup(NativeSlice<float> source, int resolution_){
            dst = source;
            // This can't be allocated with temp because some times it lives too long :-D
            // Call dispose after the Reads are complete
            src = new NativeArray<float>(source.Length, Allocator.TempJob);
            source.CopyTo(src);
            resolution = resolution_;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getIdx(int x, int z){
            // overflows safely
            x = clamp(x, 0, resolution - 1);
            z = clamp(z, 0, resolution - 1);
            return (z * resolution) + x;   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetData(int x, int z){
            return src[getIdx(x,z)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int z, float value){
            dst[getIdx(x,z)] = value;
        }

        public JobHandle Dispose(JobHandle dep){
            return src.Dispose(dep);
        }
    }

    public struct ReadTileData: ImTileData, IGetTileData{

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        NativeSlice<float> src;
        int resolution;

        public void Setup(NativeSlice<float> source, int resolution_){
            src = source;
            resolution = resolution_;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getIdx(int x, int z){
            // overflows safely
            x = clamp(x, 0, resolution - 1);
            z = clamp(z, 0, resolution - 1);
            return (z * resolution) + x;   
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetData(int x, int z){
            return src[getIdx(x,z)];
        }

    }
    public struct WriteTileData: ImSliceTileData, ISetTileData{

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
        NativeSlice<float> dst;
        int resolution;

        public void Setup(NativeSlice<float> source, int resolution_){
            dst = source;
            resolution = resolution_;

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int getIdx(int x, int z){
            // shouldn't need to overflow at all
            return (z * (resolution)) + x;  
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int x, int z, float value){
            dst[getIdx(x,z)] = value;
        }
    }
}