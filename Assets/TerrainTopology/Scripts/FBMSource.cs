using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using static Unity.Mathematics.math;
using Unity.Mathematics;

using Unity.Collections;
using Unity.Jobs;

using xshazwar.processing.cpu.mutate;

[RequireComponent(typeof(Texture2D))]
public class FBMSource : MonoBehaviour, IProvideTiles, IUpdateImage
{
    public enum FractalNoise {
        Sin,
        Perlin,
        PeriodicPerlin,
        Simplex,
        RotatedSimplex,
        Cellular
    }

    static FractalJobDelegate[] jobs = {
		FractalJob<FractalGenerator<SinGetter>, WriteTileData>.ScheduleParallel,
        FractalJob<FractalGenerator<PerlinGetter>, WriteTileData>.ScheduleParallel,
        FractalJob<FractalGenerator<PeriodicPerlinGetter>, WriteTileData>.ScheduleParallel,
        FractalJob<FractalGenerator<SimplexGetter>, WriteTileData>.ScheduleParallel,
        FractalJob<FractalGenerator<RotatedSimplexGetter>, WriteTileData>.ScheduleParallel,
        FractalJob<FractalGenerator<CellularGetter>, WriteTileData>.ScheduleParallel
	};
    public Renderer mRenderer;
    public FractalNoise noiseType;
    public int resolution = 512;

    int tileSize = 1000;
    
    [Range(0f, 1f)]
    public float hurst = 0f;
    
    [Range(1, 24)]
    public int octaves = 1;

    [Range(0, 1000)]
    public int xpos = 0;
    [Range(0, 1000)]
    public int zpos = 0;

    [Range(5, 10000)]
    public int noiseSize = 1000;

    Texture2D texture;
    NativeSlice<float> data;
    JobHandle jobHandle;

    public bool enabled;
    bool triggered;


    // Start is called before the first frame update
    void Start()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false);
        mRenderer.material.mainTexture = texture;
        data =  new NativeSlice<float4>(texture.GetRawTextureData<float4>()).SliceWithStride<float>(8);
        Debug.Log($"{data.Length} ==? {resolution * resolution}");
        // renderTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.RFloat);
    }

    void GenerateTexture () {
		jobHandle = jobs[(int)noiseType](
            data, resolution, hurst, octaves, xpos, zpos, noiseSize, default);
    }
    void OnValidate () => enabled = true;

    // Update is called once per frame
    void Update()
    {
		if (triggered){
            if (!jobHandle.IsCompleted){
                Debug.Log("Generator Working");
                return;
            }
            jobHandle.Complete();
            UnityEngine.Profiling.Profiler.BeginSample("Apply Texture");
            UpdateImage();
            UnityEngine.Profiling.Profiler.EndSample();
            Debug.Log("Generator Complete");
            triggered = false;
        }
        
        if (enabled && !triggered){
            UnityEngine.Profiling.Profiler.BeginSample("Start Job");
            triggered = true;
            GenerateTexture();
            UnityEngine.Profiling.Profiler.EndSample();
            enabled = false;
        }
    }

    public void GetData(out NativeSlice<float> d, out int res, out int ts){
        d = this.data;
        res = resolution;
        ts = tileSize;
    }

    public void UpdateImage(){
        texture.Apply();
    }

    public void OnDestroy(){
        // data.Dispose();
    }
}
