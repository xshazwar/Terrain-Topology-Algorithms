// using Unity.Collections.LowLevel.Unsafe;

// using Unity.Burst;
// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;

// using static Unity.Mathematics.math;

// namespace xshazwar.processing.cpu.mutate {
//     using Unity.Mathematics;

// 	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
// 	public struct CreationJob<G, D> : IJobFor
// 		where G : struct, ICreateTiles, IMakeNoise
// 		where D : struct, IWriteOnlyTile {

// 		G generator;

// 		[NativeDisableContainerSafetyRestriction]
// 		D data;

// 		public void Execute (int i) => generator.Execute(i, data);

// 		public static JobHandle ScheduleParallel (
// 			NativeArray<float> src,
//             int resolution,
//             int tileSize,
//             Vector2 per,
//             float rot,
//             Vector2 offset,
//             float zoom,
//             JobHandle dependency
// 		) {
// 			var job = new CreationJob<G, D>();
// 			job.generator.Resolution = resolution;
//             job.generator._terrain_width = (float) tileSize;
//             job.generator._terrain_height = (float) tileSize;
//             job.generator.JobLength = resolution;
//             job.generator.per = float2(per.x, per.y);
//             job.generator.rot = rot;
//             job.generator.zoom = zoom;
//             job.generator.offset = float2(offset.x, offset.y);
// 			job.data.Setup(
// 				src, resolution
// 			);
// 			return job.ScheduleParallel(
// 				job.generator.JobLength, 1, dependency
// 			);
// 		}
// 	}

// 	public delegate JobHandle CreationJobScheduleDelegate (
// 		NativeArray<float> src,
//         int resolution,
//         int tileSize,
//         Vector2 per,
//         float rot,
//         Vector2 offset,
//         float zoom,
//         JobHandle dependency
// );
// }