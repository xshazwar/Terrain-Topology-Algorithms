using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    
    public struct KernelTileMutation<O>: IMutateTiles, IKernelData
        where O: struct, IKernelOperator, IKernelData
        {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public O kernelOp;

        public void Setup(float kernelFactor, int kernelSize, NativeArray<float> kernel){
            kernelOp.Setup(kernelFactor, kernelSize, kernel);
        }
        public void Execute<T>(int z, T tile) where  T : struct, IRWTile {
            for( int x = 0; x < Resolution; x++){
                kernelOp.ApplyKernel<T>(x, z, tile);
            }
        }
    }
}