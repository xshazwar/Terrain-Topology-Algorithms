using Unity.Collections.LowLevel.Unsafe;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

	[BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
	public struct FlowMapStepComputeFlow<F, D> : IJobFor
        where F : struct, IComputeFlowData
		where D : struct, IReadOnlyTile
        {
		F flowOperator;

        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
		D data;

		public void Execute (int i) => flowOperator.Execute<D>(i, data);

		public static JobHandle ScheduleParallel (
            // compute outflow this is RO
			NativeSlice<float> src,  
            // compute outflow this is RO
            NativeSlice<float> waterMap,
            // compute outflow then these are RW
            NativeSlice<float> flowMapN,
            NativeSlice<float> flowMapN__buff,
            NativeSlice<float> flowMapS,
            NativeSlice<float> flowMapS__buff,
            NativeSlice<float> flowMapE,
            NativeSlice<float> flowMapE__buff,
            NativeSlice<float> flowMapW,
            NativeSlice<float> flowMapW__buff,
            int resolution,
            JobHandle dependency
		)
        {
            var job = new FlowMapStepComputeFlow<F, D>();
			job.data.Setup(
				src, resolution
			);
            job.flowOperator.Resolution = resolution;
            job.flowOperator.JobLength = resolution;
            job.flowOperator.water.Setup(waterMap, resolution);
            job.flowOperator.outN.SetupNoAlloc(flowMapN, flowMapN__buff, resolution);
            job.flowOperator.outS.SetupNoAlloc(flowMapS, flowMapS__buff, resolution);
            job.flowOperator.outE.SetupNoAlloc(flowMapE, flowMapE__buff, resolution);
            job.flowOperator.outW.SetupNoAlloc(flowMapW, flowMapW__buff, resolution);

            // no temporary allocations, so no need to dispose
			return job.ScheduleParallel(
                job.flowOperator.JobLength, 8, dependency
			);
		}
	}

    public delegate JobHandle FlowMapStepComputeFlowDelegate(
        // compute outflow this is RO
        NativeSlice<float> src,  
        // compute outflow this is RO
        NativeSlice<float> waterMap,
        // compute outflow then these are RW
        NativeSlice<float> flowMapN,
        NativeSlice<float> flowMapN__buff,
        NativeSlice<float> flowMapS,
        NativeSlice<float> flowMapS__buff,
        NativeSlice<float> flowMapE,
        NativeSlice<float> flowMapE__buff,
        NativeSlice<float> flowMapW,
        NativeSlice<float> flowMapW__buff,
        int resolution,
        JobHandle dependency
    );

    public struct FlowMapStepUpdateWater<F, D> : IJobFor
        where F : struct, IComputeWaterLevel
		where D : struct, IReadOnlyTile
        {

		F flowOperator;
        int Resolution;
	
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
		D data;

		public void Execute (int i) => flowOperator.Execute<D>(i, data);

		public static JobHandle ScheduleParallel (
            // compute outflow this is RO
			NativeSlice<float> src,  
            // compute outflow this is RO
            NativeSlice<float> waterMap,
            NativeSlice<float> waterMap__buff,
            // compute outflow then these are RW
            NativeSlice<float> flowMapN,
            NativeSlice<float> flowMapS,
            NativeSlice<float> flowMapE,
            NativeSlice<float> flowMapW,
            int resolution,
            JobHandle dependency
		)
        {
            var job = new FlowMapStepUpdateWater<F, D>();
			job.data.Setup(
				src, resolution
			);
            job.flowOperator.Resolution = resolution;
            job.flowOperator.JobLength = resolution;
            job.flowOperator.water.SetupNoAlloc(waterMap, waterMap__buff, resolution);

            job.flowOperator.outN.Setup(flowMapN, resolution);
            job.flowOperator.outS.Setup(flowMapS, resolution);
            job.flowOperator.outE.Setup(flowMapE, resolution);
            job.flowOperator.outW.Setup(flowMapW, resolution);

            // no temporary allocations, so no need to dispose
			return job.ScheduleParallel(
                job.flowOperator.JobLength, 8, dependency
			);
		}
	}

    public delegate JobHandle FlowMapStepUpdateWaterDelegate(
        NativeSlice<float> src,  
        // compute outflow this is RO
        NativeSlice<float> waterMap,
        NativeSlice<float> waterMap__buff,
        // compute outflow then these are RW
        NativeSlice<float> flowMapN,
        NativeSlice<float> flowMapS,
        NativeSlice<float> flowMapE,
        NativeSlice<float> flowMapW,
        int resolution,
        JobHandle dependency
    );

    public struct FlowMapWriteValues<F, D> : IJobFor
        where F : struct, IWriteFlowMap
		where D : struct, IWriteOnlyTile
        {

		F flowOperator;
        int Resolution;
	
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        [WriteOnly]
		D data;

		public void Execute (int i) => flowOperator.Execute<D>(i, data);

		public static JobHandle ScheduleParallel (
            // compute outflow this is RO
			NativeSlice<float> src,  
            // compute outflow then these are RW
            NativeSlice<float> flowMapN,
            NativeSlice<float> flowMapS,
            NativeSlice<float> flowMapE,
            NativeSlice<float> flowMapW,
            int resolution,
            JobHandle dependency
		)
        {
            var job = new FlowMapWriteValues<F, D>();
			job.data.Setup(
				src, resolution
			);
            job.flowOperator.Resolution = resolution;
            job.flowOperator.JobLength = resolution;
            job.flowOperator.outN.Setup(flowMapN, resolution);
            job.flowOperator.outS.Setup(flowMapS, resolution);
            job.flowOperator.outE.Setup(flowMapE, resolution);
            job.flowOperator.outW.Setup(flowMapW, resolution);

            // no temporary allocations, so no need to dispose
			return job.ScheduleParallel(
                job.flowOperator.JobLength, 8, dependency
			);
		}
	}

    public delegate JobHandle FlowMapWriteValuesDelegate(
        NativeSlice<float> src,  
        // compute outflow then these are RW
        NativeSlice<float> flowMapN,
        NativeSlice<float> flowMapS,
        NativeSlice<float> flowMapE,
        NativeSlice<float> flowMapW,
        int resolution,
        JobHandle dependency
    );
}