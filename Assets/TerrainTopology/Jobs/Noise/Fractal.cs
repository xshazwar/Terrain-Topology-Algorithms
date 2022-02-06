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

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct FractalJob<G, N, D> : IJobFor
        where G: struct, ITileSource, IFractalSettings
        where N: struct, IMakeNoiseCoord
        where D : struct, ImTileData, IGetTileData, ISetTileData
        {
	
        N noiseGenerator;
        G generator;

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
		D data;
		public void Execute (int i) => generator.Execute(i, data);

        public void SetGenerator(N ng){
            noiseGenerator = ng;
        }
		public static JobHandle ScheduleParallel (
			NativeArray<float> src,
            int resolution,
            JobHandle dependency
		)
        {
			var job = new FractalJob<G, N, D>();
			job.generator.Resolution = resolution;
            job.generator.JobLength = resolution;
            job.generator.Hurst = 1;
            job.generator.OctaveCount = 5;
			job.data.Setup(
				src, resolution
			);
			return job.ScheduleParallel(
				job.generator.JobLength, 1, dependency
			);
		}
	}

    public delegate JobHandle FractalJobDelegate (
        NativeArray<float> src,
        int resolution,
        JobHandle dependency
	);

    public struct FractalGenerator<N>: ITileSource, IFractalSettings
        where N: IMakeNoiseCoord
    {
        public int JobLength {get; set;}
        public int Resolution {get; set;}

        // [0, 1]
        public float Hurst {get; set;}
        public int OctaveCount {get; set;}
        public N noiseGenerator;

        float NoiseValue(int x, int z){
            float G = math.exp2(-Hurst);
            float f = 1;
            float a = 1;
            float t = 0;
            for (int i = 0; i < OctaveCount; i++){
                t += a * noiseGenerator.NoiseValue(f * (float) x, f * (float) z);
                f *= 2;
                a *= G;
            }
            return t;
        }

        
        public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
            for( int x = 0; x < Resolution; x++){
                tile.SetValue(x, z, NoiseValue(x, z));
            }
        }
    }

    public struct PerlinGetter: IMakeNoiseCoord {
        public float NoiseValue(float x, float z){
            float2 coord = float2(x, z);
            return math.unlerp(-1, 1,  noise.pnoise(coord, 100));
        }
    }

}
