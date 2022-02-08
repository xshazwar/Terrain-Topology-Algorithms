using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

using xshazwar.processing.cpu.mutate;

[RequireComponent(typeof(FBMSource))]
public class FlowMapFilter : MonoBehaviour
{
    static FlowMapStepComputeFlowDelegate flowStage =  FlowMapStepComputeFlow<ComputeFlowStep, ReadTileData>.ScheduleParallel;
    static FlowMapStepUpdateWaterDelegate waterStage = FlowMapStepUpdateWater<UpdateWaterStep, ReadTileData>.ScheduleParallel;
    static FlowMapWriteValuesDelegate finalStage = FlowMapWriteValues<CreateVelocityField, WriteTileData>.ScheduleParallel;
    public DataSource<FBMSource> dataSource;
    
    bool arraysReady;
    NativeArray<float> waterMap;
    NativeArray<float> waterMap__buff;
    NativeArray<float> flowMapN;
    NativeArray<float> flowMapN__buff;
    NativeArray<float> flowMapS;
    NativeArray<float> flowMapS__buff;
    NativeArray<float> flowMapE;
    NativeArray<float> flowMapE__buff;
    NativeArray<float> flowMapW;
    NativeArray<float> flowMapW__buff;

    JobHandle jobHandle;
    
    [Range(1, 32)]
    public int iterations;
    public bool enabled;
    bool triggered;
    bool enqueueFinished;

    void InitArrays(int size){
        if(arraysReady){
            return;
        }
        UnityEngine.Profiling.Profiler.BeginSample("Allocate Arrays");
        float[] waterInit = new float[size];
        Helpers.Fill<float>(waterInit, size, 0.0001f);
        waterMap = new NativeArray<float>(waterInit, Allocator.Persistent);
        waterMap__buff = new NativeArray<float>(size, Allocator.Persistent);
        flowMapN = new NativeArray<float>(size, Allocator.Persistent);
        flowMapN__buff = new NativeArray<float>(size, Allocator.Persistent);
        flowMapS = new NativeArray<float>(size, Allocator.Persistent);
        flowMapS__buff = new NativeArray<float>(size, Allocator.Persistent);
        flowMapE = new NativeArray<float>(size, Allocator.Persistent);
        flowMapE__buff = new NativeArray<float>(size, Allocator.Persistent);
        flowMapW = new NativeArray<float>(size, Allocator.Persistent);
        flowMapW__buff = new NativeArray<float>(size, Allocator.Persistent);
        arraysReady = true;
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void Start(){
        dataSource = new DataSource<FBMSource>();
        dataSource.source = GetComponent<FBMSource>();
        triggered = false;
        enqueueFinished = true;
        arraysReady = false;
        iterations = 5;
    }

    public IEnumerator FilterSteps(){
        NativeSlice<float> src;
        int res; int tileSize;
        UnityEngine.Profiling.Profiler.BeginSample("Get Upstream Handle");
        dataSource.GetData(out src, out res, out tileSize);
        UnityEngine.Profiling.Profiler.EndSample();
        InitArrays(res * res);
        
        JobHandle[] handles = new JobHandle[iterations * 2];
        for (int i = 0; i < 2 * iterations; i += 2){
            UnityEngine.Profiling.Profiler.BeginSample("Enqueue Step");
            if (i == 0){
                handles[i] = flowStage(
                        src,  
                        new NativeSlice<float>(waterMap),
                        new NativeSlice<float>(flowMapN),
                        new NativeSlice<float>(flowMapN__buff),
                        new NativeSlice<float>(flowMapS),
                        new NativeSlice<float>(flowMapS__buff),
                        new NativeSlice<float>(flowMapE),
                        new NativeSlice<float>(flowMapE__buff),
                        new NativeSlice<float>(flowMapW),
                        new NativeSlice<float>(flowMapW__buff),
                        res,
                        default);
                handles[i + 1]  = waterStage(
                        src,  
                        new NativeSlice<float>(waterMap),
                        new NativeSlice<float>(waterMap__buff),
                        new NativeSlice<float>(flowMapN),
                        new NativeSlice<float>(flowMapS),
                        new NativeSlice<float>(flowMapE),
                        new NativeSlice<float>(flowMapW),
                        res,
                        handles[i]);
            }else{
                handles[i] = flowStage(
                        src,  
                        new NativeSlice<float>(waterMap),
                        new NativeSlice<float>(flowMapN),
                        new NativeSlice<float>(flowMapN__buff),
                        new NativeSlice<float>(flowMapS),
                        new NativeSlice<float>(flowMapS__buff),
                        new NativeSlice<float>(flowMapE),
                        new NativeSlice<float>(flowMapE__buff),
                        new NativeSlice<float>(flowMapW),
                        new NativeSlice<float>(flowMapW__buff),
                        res,
                        handles[i - 1]);
                handles[i + 1]  = waterStage(
                        src,
                        new NativeSlice<float>(waterMap),
                        new NativeSlice<float>(waterMap__buff),
                        new NativeSlice<float>(flowMapN),
                        new NativeSlice<float>(flowMapS),
                        new NativeSlice<float>(flowMapE),
                        new NativeSlice<float>(flowMapW),
                        res,
                        handles[i]);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            yield return null;   
        }

        jobHandle = finalStage(
                        src,
                        new NativeSlice<float>(flowMapN),
                        new NativeSlice<float>(flowMapS),
                        new NativeSlice<float>(flowMapE),
                        new NativeSlice<float>(flowMapW),
                        res,
                        handles[iterations * 2 - 1]);
        enqueueFinished = true;
    }
    void Update()
    {
		if (triggered){
            if (!enqueueFinished){
                return;
            }
            if (!jobHandle.IsCompleted){
                return;
            }
            UnityEngine.Profiling.Profiler.BeginSample("Apply Filter");
            jobHandle.Complete();
            dataSource?.UpdateImage();
            triggered = false;
            UnityEngine.Profiling.Profiler.EndSample();
        }
        
        if (enabled && !triggered){
            UnityEngine.Profiling.Profiler.BeginSample("Start Filter Job");
            triggered = true;
            enqueueFinished = false;
            StartCoroutine(FilterSteps());
            // FilterTexture();
            UnityEngine.Profiling.Profiler.EndSample();
            enabled = false;
        }
    }

    public void OnDestroy(){
        waterMap.Dispose();
        waterMap__buff.Dispose();
        flowMapN.Dispose();
        flowMapN__buff.Dispose();
        flowMapS.Dispose();
        flowMapS__buff.Dispose();
        flowMapE.Dispose();
        flowMapE__buff.Dispose();
        flowMapW.Dispose();
        flowMapW__buff.Dispose();
    }
}
