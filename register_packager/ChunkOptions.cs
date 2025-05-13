namespace register_packager;

public class ChunkOptions(ChunkPreparerOptions options)
{
    public bool IsLegacy_CoilsCompatible(ReadOnlySpan<int> registers) => !options.Legacy_CoilsCompatibility || Chunk.IsLegacy_CoilsCompatible(registers);
    public bool IsLegacy_CoilsCompatible(int distance) => !options.Legacy_CoilsCompatibility || Chunk.IsLegacy_CoilsCompatible(distance);
}
