using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;
    
    // public interface ICreateTiles {
    //     // total resolution including margin
    //     int JobLength {get; set;}
    //     int Resolution {get; set;}
    //     float _terrain_width {get; set;}
    //     float _terrain_height {get; set;}

    //     float pixel_size_ws {get; set;} // => _terrain_width / Resolution;
    //     void Execute<T>(int i, T tile) where  T : struct, IWriteOnlyTile; 
    // }

    public interface ICommonTileSettings {
        int JobLength {get; set;}
        int Resolution {get; set;}
    }

    public interface ITileSource: ICommonTileSettings {
        void SetPosition(int x, int z);
        void Execute<T>(int i, T tile) where  T : struct, IWriteOnlyTile; 
    }

    public interface IFractalSettings {
        public float Hurst {get; set;}
        public int OctaveCount {get; set;}
        public int NoiseSize {get; set;}
        public float NormalizationValue {get; set;}
    }

    public interface IMakeNoise {
        float NoiseValue(float x, float z);
    }

    public interface IMutateTiles: ICommonTileSettings {
        // total resolution including margin
        void Execute<T>(int i, T tile) where  T : struct, IRWTile; 
    }

    public interface IReduceTiles : ICommonTileSettings {
        // tile A is left side, B is right
        // result put onto A
        void Execute<T, V>(int i, T tileA, V tileB) 
                where  T : struct, IRWTile
                where V: struct, IReadOnlyTile; 
    }

    public interface IKernelData {
        public void Setup(float kernelFactor, int kernelSize, NativeArray<float> kernel);
    }
    
    public interface IKernelOperator {
        public void ApplyKernel<T>(int x, int z, T tile) where  T : struct, IRWTile;
    }
    public interface IMathTiles : IMutateTiles {}

    public interface IComputeFlowData: ICommonTileSettings {
        ReadTileData water {get; set;}
        RWTileData outN {get; set;}
        RWTileData outS {get; set;}
        RWTileData outE {get; set;}
        RWTileData outW {get; set;}

        void Execute<T>(int i, T tile) where  T : struct, IReadOnlyTile;
    }

   public interface IComputeWaterLevel: ICommonTileSettings {
        RWTileData water {get; set;}
        ReadTileData outN {get; set;}
        ReadTileData outS {get; set;}
        ReadTileData outE {get; set;}
        ReadTileData outW {get; set;}

        void Execute<T>(int i, T tile) where  T : struct, IReadOnlyTile;
    }

    

    public interface IWriteFlowMap: ICommonTileSettings {
        ReadTileData outN {get; set;}
        ReadTileData outS {get; set;}
        ReadTileData outE {get; set;}
        ReadTileData outW {get; set;}

        void Execute<T>(int i, T tile) where  T : struct, IWriteOnlyTile;
    }

}