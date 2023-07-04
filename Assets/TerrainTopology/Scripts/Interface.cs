using Unity.Collections;

namespace xshazwar.processing.cpu.mutate {
    public interface IProvideTiles {
        public void GetData(out NativeSlice<float> data, out int resolution, out int tileSize);
    }

    public interface IUpdateImageChannel {
        public void UpdateImageChannel();
    }

    public interface IUpdateAllChannels {
        public void UpdateImageAllChannels();
    }
}