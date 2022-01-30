using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Unity.Collections;
using Unity.Jobs;

using xshazwar.processing.cpu.mutate;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(RenderTexture))]
[RequireComponent(typeof(Texture2D))]
public class NoiseSource : MonoBehaviour
{
    
    public enum NoiseType {
		Cellular,
        TilingSimplex,
        PeriodicPerlin
	};
    static CreationJobScheduleDelegate[] jobs = {
		CreationJob<CellNoiseGenerator, WriteOnlyTileData>.ScheduleParallel,
		CreationJob<TilingSimplexNoiseGenerator, WriteOnlyTileData>.ScheduleParallel,
        CreationJob<PeriodicPerlinNoiseGenerator, WriteOnlyTileData>.ScheduleParallel  
	};

    public NoiseType noiseType;
    public Renderer mRenderer;
    public int resolution;
    public int tileSize;
    public Vector2Int per;
    public Vector2Int offset;
    public float rot;
    public float zoom;

    Texture2D texture;
    NativeArray<float> data;
    JobHandle jobHandle;

    bool enabled;


    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RFloat, false);
        mRenderer.material.mainTexture = texture;
        data = texture.GetRawTextureData<float>(); 
    }

    void GenerateTexture () {
		jobHandle = jobs[(int) noiseType](
            data, resolution, tileSize, (Vector2) per, rot, (Vector2) offset, zoom, default);
    }
    void OnValidate () => enabled = true;

    // Update is called once per frame
    void Update()
    {
		if (!jobHandle.IsCompleted){
            jobHandle.Complete();
            UnityEngine.Profiling.Profiler.BeginSample("Apply Texture");
            texture.Apply();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        
        if (enabled && jobHandle.IsCompleted){
            UnityEngine.Profiling.Profiler.BeginSample("Start Job");
            GenerateTexture();
            UnityEngine.Profiling.Profiler.EndSample();
            enabled = false;
        }
    }
}
