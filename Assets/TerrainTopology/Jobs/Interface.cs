using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    
    public interface ICreateTiles {
        // total resolution including margin
        int JobLength {get; set;}
        int Resolution {get; set;}
        float _terrain_width {get; set;}
        float _terrain_height {get; set;}

        float pixel_size_ws {get; set;} // => _terrain_width / Resolution;
        void Execute<T>(int i, T tile) where  T : struct, ImTileData, ISetTileData; 
    }

    public interface ITileSource {

        int JobLength {get; set;}
        int Resolution {get; set;}

        void SetPosition(int x, int z);
        void Execute<T>(int i, T tile) where  T : struct, ImSliceTileData, ISetTileData; 
    }

    public interface IMakeNoise {
        public float zoom {get; set;}
        public float2 offset {get; set;}
        float2 per {get; set;}
        float rot {get; set;}
        float GetNoiseValue(int x, int z);
    }

    public interface IFractalSettings {
        public float Hurst {get; set;}
        public int OctaveCount {get; set;}
        public int NoiseSize {get; set;}
        public float NormalizationValue {get; set;}
    }

    public interface IMakeNoiseCoord {
        float NoiseValue(float x, float z);
    }

    public interface IMutateTiles {
        // total resolution including margin
        int JobLength {get; set;}
        int Resolution {get; set;}
        void Execute<T>(int i, T tile) where  T : struct, IRWTile; 
    }

    public interface IReduceTiles {
        // total resolution including margin
        int JobLength {get; set;}
        int Resolution {get; set;}
        // tile A is left side, B is right
        // result put onto A
        void Execute<T, V>(int i, T tileA, V tileB) 
                where  T : struct, ImTileData, ISetTileData, IGetTileData
                where V: struct, ImTileData, IGetTileData; 
    }

    public interface IKernelData {
        public void Setup(float kernelFactor, int kernelSize, NativeArray<float> kernel);
    }
    
    public interface IKernelOperator {
        public void ApplyKernel<T>(int x, int z, T tile) where  T : struct, IRWTile;
    }
    public interface IMathTiles : IMutateTiles {}

}