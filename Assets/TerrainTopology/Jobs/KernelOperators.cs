using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

    public struct KernelSampleXOperator: IKernelOperator, IKernelData
    {        
        float KernelFactor;
        int KernelSize;
        [ReadOnly]
        public NativeArray<float> Kernel;

        public void Setup(float kernelFactor, int kernelSize, NativeArray<float> kernel){
            KernelFactor = kernelFactor;
            KernelSize = kernelSize;
            Kernel = kernel;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyKernel<T>(int x, int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData{
            
            int k_off = (KernelSize - 1) / 2;
            float total = 0;
            for (int k = -k_off; k < k_off; k++){
                int xi = x + k;
                total += tile.GetData(xi, z) * Kernel[k + k_off];
            }
            tile.SetValue(x, z, total * KernelFactor);
        }
    }

    public struct KernelSampleZOperator: IKernelOperator, IKernelData
    {        
        public float KernelFactor;
        public int KernelSize;
        [ReadOnly]
        public NativeArray<float> Kernel;
        
        public void Setup(float kernelFactor, int kernelSize, NativeArray<float> kernel){
            KernelFactor = kernelFactor;
            KernelSize = kernelSize;
            Kernel = kernel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyKernel<T>(int x, int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData{
            int k_off = (KernelSize - 1) / 2;
            float total = 0;
            for (int k = -k_off; k < k_off; k++){
                int zi = z + k;
                total += tile.GetData(x, zi) * Kernel[k + k_off];
            }
            tile.SetValue(x, z, total * KernelFactor);
        }
    }
}