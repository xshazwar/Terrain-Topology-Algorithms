using Unity.Collections;

namespace xshazwar.processing.cpu.mutate {
    public interface IProvideTiles {
        public void GetData(out NativeArray<float> data, out int resolution, out int tileSize);
    }

    public interface IUpdateImage {
        public void UpdateImage();
    }
}