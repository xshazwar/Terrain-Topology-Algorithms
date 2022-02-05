using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    
    public struct FixedKernelTileMutation: IFixedKernel {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float KernelFactor {get; set;}
        public int KernelSize {get; set;}
        public NativeArray<float> Kernel {get; set;}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyKernel<T>(int x, int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData{
            int k_off = (KernelSize - 1) / 2;
            float total = 0;
            for (int kx = -k_off; kx < k_off; kx++){
                int xi = x + kx;
                for (int kz = -k_off; kz < k_off; kz++){
                    int zi = z + kz;
                    int kidx = ((kz + k_off) * KernelSize) + kx + k_off;
                    total += tile.GetData(xi, zi) * Kernel[kidx];
                }
            }
            tile.SetValue(x, z, total * KernelFactor);
        }

        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                ApplyKernel<T>(x, z, tile);
            }
        }
    }
    
    public struct XSeparableKernelTileMutation: ISeparableKernel {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float KernelFactor {get; set;}
        public int KernelSize {get; set;}
        public NativeArray<float> Kernel {get; set;}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyKernel<T>(int x, int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData{
            int k_off = (KernelSize - 1) / 2;
            float total = 0;
            for (int k = -k_off; k < k_off; k++){
                int xi = x + k;
                total += tile.GetData(xi, z) * Kernel[k + k_off];
            }
            tile.SetValue(x, z, total * KernelFactor);
        }

        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                ApplyKernel<T>(x, z, tile);
            }
        }
    }

    public struct ZSeparableKernelTileMutation: ISeparableKernel {

        public int Resolution {get; set;}
        public int JobLength {get; set;}
        public float KernelFactor {get; set;}
        public int KernelSize {get; set;}
        public NativeArray<float> Kernel {get; set;}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyKernel<T>(int x, int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData{
            int k_off = (KernelSize - 1) / 2;
            float total = 0;
            for (int k = -k_off; k < k_off; k++){
                int zi = z + k;
                total += tile.GetData(x, zi) * Kernel[k + k_off];
            }
            tile.SetValue(x, z, total * KernelFactor);
        }

        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, IGetTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                ApplyKernel<T>(x, z, tile);
            }
        }
    }

    
}