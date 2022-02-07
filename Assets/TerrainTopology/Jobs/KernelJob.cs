using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct GenericKernelJob<K, D> : IJobFor
        where K : struct, IMutateTiles, IKernelData
		where D : struct, IRWTile
        {

		K kernelPass;
	
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
		D data;

		public void Execute (int i) => kernelPass.Execute<D>(i, data);

		public static JobHandle ScheduleParallel (
			NativeSlice<float> src,
            int resolution,
            int kernelSize,
            NativeArray<float> kernelBody,
            float kernelFactor,
            JobHandle dependency
		)
        {
			var job = new GenericKernelJob<K, D>();
			job.kernelPass.Resolution = resolution;
            job.kernelPass.JobLength = resolution;
            job.kernelPass.Setup(kernelFactor, kernelSize, kernelBody);
			job.data.Setup(
				src, resolution
			);
			JobHandle handle = job.ScheduleParallel(
				job.kernelPass.JobLength, 32, dependency
			);
            return job.data.Dispose(handle);

		}
	}

    public enum KernelFilterType {
        Gauss5,
        Gauss3,
        Smooth3,
        Sobel3Horizontal,
        Sobel3Vertical,
        Sobel3_2D
    }

    public struct SeparableKernelFilter {
        public static float gauss5Factor = 1f/16f;
        public static float[] gauss5 = {1f,4f,6f,4f,1f};
        public static float gauss3Factor = 1f/4f;
        public static float[] gauss3 = {1f, 2f, 1f};
        public static float smooth3Factor =  1f / 3f ;
        public static float[] smooth3 = {1f, 1f, 1f};

        public static float sobel3Factor =  1f;
        public static float[] sobel3_HX = {-1f, 0f, 1f};
        public static float[] sobel3_HY = {1f, 2f, 1f};
         public static float[] sobel3_VY = {1f, 0f, -1f};
        public static float[] sobel3_VX = {1f, 2f, 1f};
        private static JobHandle ScheduleSeries(
            NativeSlice<float> src,
            int resolution,
            int kernelSize,
            NativeArray<float> kernelX,
            NativeArray<float> kernelZ,
            float kernelFactor,
            JobHandle dependency
        ){
            JobHandle first = GenericKernelJob<KernelTileMutation<KernelSampleXOperator>, RWTileData>.ScheduleParallel(
                src, resolution, kernelSize, kernelX, kernelFactor, dependency
            );
            return GenericKernelJob<KernelTileMutation<KernelSampleZOperator>, RWTileData>.ScheduleParallel(
                src, resolution, kernelSize, kernelZ, kernelFactor, first
            );
        }

        private static JobHandle SchedulePL<T>(
            NativeSlice<float> src,
            int resolution,
            int kernelSize,
            NativeArray<float> kernelX,
            NativeArray<float> kernelZ,
            float kernelFactor,
            JobHandle dependency
        ) where T: struct, IReduceTiles {
            NativeArray<float> original = new NativeArray<float>(src.Length, Allocator.TempJob);
            src.CopyTo(original);
            JobHandle xPass = GenericKernelJob<KernelTileMutation<KernelSampleXOperator>, RWTileData>.ScheduleParallel(
                src, resolution, kernelSize, kernelX, kernelFactor, dependency
            );
            JobHandle zPass = GenericKernelJob<KernelTileMutation<KernelSampleZOperator>, RWTileData>.ScheduleParallel(
                original, resolution, kernelSize, kernelZ, kernelFactor, dependency
            );
            JobHandle allPass = JobHandle.CombineDependencies(xPass, zPass);
            JobHandle reduce = ReductionJob<T, RWTileData, ReadTileData>.ScheduleParallel(
                src, original, resolution, allPass
            );
            return original.Dispose(reduce);
        }

        private static JobHandle ScheduleSobel2D(
            NativeSlice<float> src,
            int resolution,
            JobHandle dependency
        ){
            NativeArray<float> original = new NativeArray<float>(src.Length, Allocator.TempJob);
            src.CopyTo(original);
            NativeArray<float> kbx_h = new NativeArray<float>(sobel3_HX, Allocator.TempJob);
            NativeArray<float> kby_h = new NativeArray<float>(sobel3_HY, Allocator.TempJob);
            NativeArray<float> kbx_v = new NativeArray<float>(sobel3_VX, Allocator.TempJob);
            NativeArray<float> kby_v = new NativeArray<float>(sobel3_VY, Allocator.TempJob);
            JobHandle horiz = SchedulePL<MultiplyTiles>(src, resolution, 3, kbx_h, kby_h, 1f, dependency);
            JobHandle vert = SchedulePL<MultiplyTiles>(original, resolution, 3, kbx_v, kby_v, 1f, dependency);
            JobHandle allPass = JobHandle.CombineDependencies(horiz, vert);
            JobHandle reducePass =  ReductionJob<RootSumSquaresTiles, RWTileData, ReadTileData>.ScheduleParallel(
                src, original, resolution, allPass
            );
            // chain disposal of nativeArrays after job complete
            return kby_v.Dispose(
                kbx_v.Dispose(
                    kby_h.Dispose(
                        kbx_h.Dispose(
                            original.Dispose(reducePass)))));
        }

        public static JobHandle Schedule(NativeSlice<float> src, KernelFilterType filter, int resolution, JobHandle dependency){
            float[] kernelBodyX = smooth3;
            float[] kernelBodyY = smooth3;
            float kernelFactor = smooth3Factor;
            int kernelSize = 3;

            switch(filter){
                case KernelFilterType.Sobel3_2D:
                    return ScheduleSobel2D(src, resolution, dependency);
                case KernelFilterType.Smooth3:
                    break;
                case KernelFilterType.Gauss5:
                    kernelBodyX = gauss5;
                    kernelBodyY = gauss5;
                    kernelFactor = gauss5Factor;
                    kernelSize = 5;
                    break;
                case KernelFilterType.Gauss3:
                    kernelBodyX = gauss3;
                    kernelBodyY = gauss3;
                    kernelFactor = gauss3Factor;
                    break;;
                case KernelFilterType.Sobel3Horizontal:
                    kernelBodyX = sobel3_HX;
                    kernelBodyY = sobel3_HY;
                    kernelFactor = sobel3Factor;;
                    break;
                case KernelFilterType.Sobel3Vertical:
                    kernelBodyX = sobel3_VX;
                    kernelBodyY = sobel3_VY;
                    kernelFactor = sobel3Factor;
                    break;           
            }
            NativeArray<float> kbx = new NativeArray<float>(kernelBodyX, Allocator.TempJob);
            NativeArray<float> kby = new NativeArray<float>(kernelBodyY, Allocator.TempJob);
            JobHandle res;

            switch(filter){
                case KernelFilterType.Sobel3Horizontal:
                case KernelFilterType.Sobel3Vertical:
                    res = SchedulePL<MultiplyTiles>(src, resolution, kernelSize, kbx, kby, kernelFactor, dependency);
                    break;
                default:
                    res = ScheduleSeries(src, resolution, kernelSize, kbx, kby, kernelFactor, dependency);
                    break;
            }
            return kbx.Dispose(
                kby.Dispose(
                    res
            ));
            
        }
    }
    public delegate JobHandle SeperableKernelFilterDelegate (
        NativeSlice<float> src,
        KernelFilterType filter,
        int resolution,
        JobHandle dependency
	);
}