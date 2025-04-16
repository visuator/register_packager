namespace register_packager;

public class Worker
{
    public static ChunkNode Work(ChunkPreparerOptions options, ChunkNode root) => WorkRecursive(options, root, false);
    private static ChunkNode WorkRecursive(ChunkPreparerOptions options, ChunkNode root, bool rearrange)
    {
        var node = root;
        while (node is not null)
        {
            var current = node.Chunk;
            if (node.Next is not null)
            {
                var follow = node.Next.Chunk;
                if (follow.Registers.Length != 0)
                {
                    var tail = node.Next.Next;
                    var candidate = node;
                    
                    Min<int> min = new(ChunkNode.CalculateWeight(options.MaxLimit, tail, current, follow));
                    foreach (var (trimLeft, joinRight) in current.GetMinGarbageCandidates(options, follow))
                    {
                        if (joinRight.ExcessLimit(options.MaxLimit, out var taken, out var rest))
                        {
                            if (trimLeft.Registers.Length != 0)
                            {
                                if (rearrange)
                                {
                                    continue;
                                }
                                var next = WorkRecursive(options, GreedyPreparer.Prepare(options, [..rest, ..tail?.GetChunks().SelectMany(x => x.Registers) ?? []]), true);
                                if (min.TryChange(ChunkNode.CalculateWeight(options.MaxLimit, next, trimLeft, taken)))
                                {
                                    candidate = ChunkNode.CreateHead(next, trimLeft, taken);
                                }
                            }
                        }
                        else
                        {
                            if (min.TryChange(ChunkNode.CalculateWeight(options.MaxLimit, tail, trimLeft, joinRight)))
                            {
                                candidate = ChunkNode.CreateHead(tail, trimLeft, joinRight);
                            }
                        }
                    }
                    node.Replace(candidate);
                }
            }
            else
            {
                return root;
            }
            node = node.Next;
        }
        return root;
    }
}