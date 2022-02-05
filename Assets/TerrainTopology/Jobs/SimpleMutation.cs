using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;

namespace xshazwar.processing.cpu.mutate {
    using Unity.Mathematics;

    public struct MultiplyTiles: IReduceTiles{
        public int JobLength {get; set;}
        public int Resolution {get; set;}
        // tile A is left side, B is right
        // result put onto A
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoOp<T, V>(int x, int z, T tileA, V tileB)
                where T : struct, ImTileData, ISetTileData, IGetTileData
                where V : struct, ImTileData, IGetTileData {
            float val = tileA.GetData(x, z) * tileB.GetData(x, z);
            tileA.SetValue(x, z, val);
        }

        public void Execute<T, V>(int z, T tileA, V tileB)
                where  T : struct, ImTileData, ISetTileData, IGetTileData
                where V: struct, ImTileData, IGetTileData{
            for( int x = 0; x < Resolution; x++){
                DoOp<T, V>(x, z, tileA, tileB);
            }
        }
    }
}