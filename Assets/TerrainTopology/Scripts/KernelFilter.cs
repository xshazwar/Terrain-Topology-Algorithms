using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

using xshazwar.processing.cpu.mutate;

public class DataSource<T> : IProvideTiles, IUpdateImage where T: MonoBehaviour, IProvideTiles, IUpdateImage {
    public T source;
    
    public void GetData(out NativeSlice<float> d, out int res, out int ts){
        source.GetData(out d, out res, out ts);
    }

    public void UpdateImage(){
        source.UpdateImage();
    }

}

[RequireComponent(typeof(FBMSource))]
public class KernelFilter : MonoBehaviour
{
    static SeperableKernelFilterDelegate job = SeparableKernelFilter.Schedule;

    public KernelFilterType filter;
    public DataSource<FBMSource> dataSource;

    JobHandle jobHandle;
    
    [Range(1, 32)]
    public int iterations;
    public bool enabled;
    bool triggered;


    // Start is called before the first frame update
    void Start(){
        dataSource = new DataSource<FBMSource>();
        dataSource.source = GetComponent<FBMSource>();
        triggered = false;
        iterations = 5;
    }

    void FilterTexture () {
        NativeSlice<float> src;
        int res; int tileSize;
        UnityEngine.Profiling.Profiler.BeginSample("Get Upstream Handle");
        dataSource.GetData(out src, out res, out tileSize);
        UnityEngine.Profiling.Profiler.EndSample();
        JobHandle[] handles = new JobHandle[iterations];
        for (int i = 0; i < iterations; i++){
            UnityEngine.Profiling.Profiler.BeginSample("Enqueue Step");
            if (i == 0){
                handles[i] = job(src, filter, res, default);
            }else{
                handles[i] = job(src, filter, res, handles[i - 1]);
            }
            UnityEngine.Profiling.Profiler.EndSample();
            
        }
		jobHandle = handles[iterations - 1];
    }
    // void OnValidate () => enabled = false;

    // Update is called once per frame
    void Update()
    {
		if (triggered){
            if (!jobHandle.IsCompleted){
                Debug.Log("Filter Working");
                return;
            }
            UnityEngine.Profiling.Profiler.BeginSample("Apply Filter");
            jobHandle.Complete();
            dataSource?.UpdateImage();
            Debug.Log("Filter Complete");
            UnityEngine.Profiling.Profiler.EndSample();
            triggered = false;
        }
        
        if (enabled && !triggered){
            UnityEngine.Profiling.Profiler.BeginSample("Start Filter Job");
            triggered = true;
            FilterTexture();
            UnityEngine.Profiling.Profiler.EndSample();
            enabled = false;
        }
    }
}
