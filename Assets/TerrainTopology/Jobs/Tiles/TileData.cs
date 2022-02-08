using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

    public struct RWTileData: IRWTile{

        [NoAlias]
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        NativeArray<float> src_backing;
        
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        NativeSlice<float> src;

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
        NativeSlice<float> dst;
        int resolution;

        public void Setup(NativeSlice<float> source, int resolution_){
            dst = source;
            // This can't be allocated with temp because some times it lives too long :-D
            // Call dispose after the Reads are complete
            src_backing = new NativeArray<float>(source.Length, Allocator.TempJob);
            src = new NativeSlice<float>(src_backing);
            src.CopyFrom(source);
            resolution = resolution_;
        }

        public void SetupNoAlloc(NativeSlice<float> source, NativeSlice<float> holder, int resolution_){
            dst = source;
            // in this case we can avoid the alloc by passing in properly sized slice for reuse
            src = holder;
            src.CopyFrom(source);
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
            return src_backing.Dispose(dep);
        }
    }

    public struct ReadTileData: IReadOnlyTile{

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
    public struct WriteTileData: IWriteOnlyTile{

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