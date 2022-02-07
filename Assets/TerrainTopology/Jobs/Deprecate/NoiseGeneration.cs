// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using Unity.Collections.LowLevel.Unsafe;

// using Unity.Collections;
// using Unity.Jobs;
// using UnityEngine;

// using static Unity.Mathematics.math;

// namespace xshazwar.processing.cpu.mutate {
//     using Unity.Mathematics;
//     public struct TilingSimplexNoiseGenerator: ICreateTiles, IMakeNoise {

//         public int Resolution {get; set;}
//         public int JobLength {get; set;}
//         public float _terrain_width {get; set;}
//         public float _terrain_height {get; set;}
//         public float zoom {get; set;}
//         public float2 offset {get; set;}
//         public float2 per {get; set;}
//         public float rot {get; set;}
//         public float pixel_size_ws {get; set;}
//         public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
//             for( int x = 0; x < Resolution; x++){
//                 tile.SetValue(x, z, GetNoiseValue(x, z));
//             }
//         }

//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public float GetNoiseValue(int x, int z){
//             float2 coord = float2(x, z);
//             float2 normCoord = ((coord/ Resolution * _terrain_width) + (offset * _terrain_width)) / zoom;
//             // 2-D tiling simplex noise with rotating gradients
//             // but without the analytical derivative.
//             return noise.psrnoise(normCoord, per, rot);
//         }
//     }

//     public struct CellNoiseGenerator: ICreateTiles, IMakeNoise {

//         public int Resolution {get; set;}
//         public int JobLength {get; set;}
//         public float _terrain_width {get; set;}
//         public float _terrain_height {get; set;}
//         public float zoom {get; set;}
//         public float2 offset {get; set;}
//         // NON-OP
//         public float2 per {get; set;}
//         // NON-OP
//         public float rot {get; set;}
//         public float pixel_size_ws {get; set;}
//         public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
//             for( int x = 0; x < Resolution; x++){
//                 tile.SetValue(x, z, GetNoiseValue(x, z));
//             }
//         }

//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public float GetNoiseValue(int x, int z){
//             float2 coord = float2(x, z);
//             float2 normCoord = ((coord/ Resolution * _terrain_width) + (offset * _terrain_width)) / zoom;
//             return noise.cnoise(normCoord);
//         }
//     }

//     public struct PeriodicPerlinNoiseGenerator: ICreateTiles, IMakeNoise {

//         public int Resolution {get; set;}
//         public int JobLength {get; set;}
//         public float _terrain_width {get; set;}
//         public float _terrain_height {get; set;}
//         public float zoom {get; set;}
//         public float2 offset {get; set;}
//         // NON-OP
//         public float2 per {get; set;}
//         // NON-OP
//         public float rot {get; set;}
//         public float pixel_size_ws {get; set;}
//         public void Execute<T>(int z, T tile) where  T : struct, ImTileData, ISetTileData {
//             for( int x = 0; x < Resolution; x++){
//                 tile.SetValue(x, z, GetNoiseValue(x, z));
//             }
//         }

//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public float GetNoiseValue(int x, int z){
//             float2 period = float2(Resolution / zoom, Resolution / zoom);
//             float2 coord = float2(x, z);
//             // float2 normCoord = ((coord/ Resolution * _terrain_width) + (offset * _terrain_width)) / zoom;
            
//             return math.unlerp(-1, 1,  noise.pnoise(coord/ zoom, period));
//             // return math.unlerp(-1, 1, noise.cnoise(coord));
//         }
//     }
// }