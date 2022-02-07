using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct MutationJob<G, D> : IJobFor
		where G : struct, IMutateTiles
		where D : struct, IRWTile {

		G generator;

		[NativeDisableParallelForRestriction]
		[NativeDisableContainerSafetyRestriction]
		D data;

		public void Execute (int i) => generator.Execute(i, data);

		public static JobHandle ScheduleParallel (
			NativeArray<float> src, int resolution, Vector2 per, float rot, Vector2 offset, float zoom, JobHandle dependency
		) {
			var job = new MutationJob<G, D>();
			job.generator.Resolution = resolution;
            job.generator.JobLength = resolution;
			job.data.Setup(
				src, resolution
			);
			JobHandle handle = job.ScheduleParallel(
				job.generator.JobLength, 1, dependency
			);
			return job.data.Dispose(handle);
		}
	}

	public delegate JobHandle MutationJobScheduleDelegate (
		NativeArray<float> src, int resolution, Vector2 per, float rot, Vector2 offset, float zoom, JobHandle dependency
	);
}