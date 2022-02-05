using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct SeperableKernelPassJob<K, D> : IJobFor
		where K : struct, ISeparableKernel
		where D : struct, ImTileData, IGetTileData, ISetTileData {

		K kernelPass;

		
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
		D data;

		public void Execute (int i) => kernelPass.Execute(i, data);

		public static JobHandle ScheduleParallel (
			NativeArray<float> src,
            int resolution,
            int kernelSize,
            NativeArray<float> kernelBody,
            float kernelFactor,
            JobHandle dependency
		) {
			var job = new SeperableKernelPassJob<K, D>();
			job.kernelPass.Resolution = resolution;
            job.kernelPass.JobLength = resolution;
            job.kernelPass.KernelFactor = kernelFactor;
            job.kernelPass.KernelSize = kernelSize;
            job.kernelPass.Kernel = kernelBody;
			job.data.Setup(
				src, resolution
			);
			return job.ScheduleParallel(
				job.kernelPass.JobLength, 1, dependency
			);

		}
	}

    public enum KernelFilterType {
        Gauss5,
        Gauss3,
        Smooth3,
        Sobel3Horizontal,
        Sobel3Vertical
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
         public static float[] sobel3_VY = {-1f, 0f, 1f};
        public static float[] sobel3_VX = {1f, 2f, 1f};
        private static JobHandle Schedule(
            NativeArray<float> src,
            int resolution,
            int kernelSize,
            NativeArray<float> kernelX,
            NativeArray<float> kernelZ,
            float kernelFactor,
            JobHandle dependency
        ){
            NativeArray<float> original = new NativeArray<float>(src, Allocator.TempJob);
            JobHandle xPass = SeperableKernelPassJob<XSeparableKernelTileMutation, RWTileData>.ScheduleParallel(
                src, resolution, kernelSize, kernelX, kernelFactor, dependency
            );
            JobHandle zPass = SeperableKernelPassJob<ZSeparableKernelTileMutation, RWTileData>.ScheduleParallel(
                original, resolution, kernelSize, kernelZ, kernelFactor, dependency
            );
            JobHandle allPass = JobHandle.CombineDependencies(xPass, zPass);
            return ReductionJob<MultiplyTiles, RWTileData, ReadOnlyTileData>.ScheduleParallel(
                src, original, resolution, allPass
            );
        }

        public static JobHandle Schedule(NativeArray<float> src, KernelFilterType filter, int resolution, JobHandle dependency){
            float[] kernelBodyX = smooth3;
            float[] kernelBodyY = smooth3;
            float kernelFactor = smooth3Factor;
            int kernelSize = 3;

            switch(filter){
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
            return Schedule(src, resolution, kernelSize, kbx, kby, kernelFactor, dependency);
        }
    }
    public delegate JobHandle SeperableKernelFilterDelegate (
        NativeArray<float> src,
        KernelFilterType filter,
        int resolution,
        JobHandle dependency
	);
}