using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct ReductionJob<G, DL, DR> : IJobFor
		where G : struct, IReduceTiles
		where DL : struct, ImTileData, IGetTileData, ISetTileData
        where DR : struct, ImTileData, IGetTileData {

		G generator;

		[NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
		DL dataL;
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        DR dataR;

		public void Execute (int i) => generator.Execute(i, dataL, dataR);

		public static JobHandle ScheduleParallel (
			NativeArray<float> srcL, //receives output
            NativeArray<float> srcR,
            int resolution,
            JobHandle dependency
		) {
            Debug.Log($"{srcL.Length == srcR.Length && srcR.Length == (resolution * resolution)}");
			var job = new ReductionJob<G, DL, DR>();
			job.generator.Resolution = resolution;
            job.generator.JobLength = resolution;
			job.dataL.Setup(
				srcL, resolution
			);
            job.dataR.Setup(
				srcR, resolution
			);
			return job.ScheduleParallel(
				job.generator.JobLength, 1, dependency
			);
		}
	}

	public delegate JobHandle ReductionJobScheduleDelegate (
        NativeArray<float> srcL, //receives output
        NativeArray<float> srcR,
        int resolution,
        JobHandle dependency
	);
}