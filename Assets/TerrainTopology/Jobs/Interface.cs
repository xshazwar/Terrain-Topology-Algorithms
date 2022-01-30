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

    public interface IMutateTiles {
        // total resolution including margin
        int JobLength {get; set;}
        int Resolution {get; set;}
        float _terrain_width {get; set;}
        float _terrain_height {get; set;}

        float pixel_size_ws {get; set;} // => _terrain_width / Resolution;
        void Execute<T>(int i, T tile) where  T : struct, ImTileData, ISetTileData, IGetTileData; 
    }

    public interface IMakeNoise {
        public float zoom {get; set;}
        public float2 offset {get; set;}
        float2 per {get; set;}
        float rot {get; set;}
        float GetNoiseValue(int x, int z);
    }

    public interface IKernelTiles: IMutateTiles {
        int KernelSize {get; set;}
        bool Separable {get; set;}
        int passDirection {get; set;}
        float[] Kernel {get; set;}
    }

    public interface ImTileData {
        void Setup(NativeArray<float> source, int resolution);
        // public float GetData(int x, int z);
        // void SetValue(int x, int z, float value);

    }

    public interface IGetTileData {
        public float GetData(int x, int z);
    }

    public interface ISetTileData {
        void SetValue(int x, int z, float value);

    }

}