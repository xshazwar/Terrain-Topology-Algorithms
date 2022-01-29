using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	public struct MutationJob<G, S> : IJobFor
		where G : struct, IMutateTiles, IMakeNoise
		where S : struct, ImTileData {

		G generator;

		[NativeDisableContainerSafetyRestriction]
        [WriteOnly]
		S streams;

		public void Execute (int i) => generator.Execute(i, streams);

		public static JobHandle ScheduleParallel (
			NativeArray<float> src, int resolution, Vector2 per, float rot, Vector2 offset, float zoom, JobHandle dependency
		) {
			var job = new MutationJob<G, S>();
			job.generator.Resolution = resolution;
            job.generator.JobLength = resolution;
            job.generator.per = float2(per.x, per.y);
            job.generator.rot = rot;
            job.generator.zoom = zoom;
            job.generator.offset = float2(offset.x, offset.y);
			job.streams.Setup(
				src
			);
			return job.ScheduleParallel(
				job.generator.JobLength, 1, dependency
			);
		}
	}

	public delegate JobHandle MutationJobScheduleDelegate (
		NativeArray<float> src, int resolution, Vector2 per, float rot, Vector2 offset, float zoom, JobHandle dependency
	);
}