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

    public interface IMakeNoise {
        public float zoom {get; set;}
        public float2 offset {get; set;}
        float2 per {get; set;}
        float rot {get; set;}
        float GetNoiseValue(int x, int z);
    }

    public interface IMutateTiles {
        // total resolution including margin
        int JobLength {get; set;}
        int Resolution {get; set;}
        void Execute<T>(int i, T tile) where  T : struct, ImTileData, ISetTileData, IGetTileData; 
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

    public interface IKernelTiles: IMutateTiles {
        float KernelFactor {get; set;}
        int KernelSize {get; set;}
        NativeArray<float> Kernel {get; set;}
    }
    public interface IMathTiles : IMutateTiles {}

    public interface ISeparableKernel: IKernelTiles{}
    public interface IFixedKernel: IKernelTiles{}

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