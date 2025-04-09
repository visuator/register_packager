using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int maxLimit, int[] registers)
    {
        var root = Chunk(maxLimit, registers).Next;
        ArgumentNullException.ThrowIfNull(root);
        var node = JoinRecursive(maxLimit, GetNumberWithZeros(maxLimit), root, false);
        return GetChunks(node).ToArray();
    }
    private static int GetNumberWithZeros(int x) => (int)Math.Pow(10, (int)Math.Floor(Math.Log10(x)) + 1);
    private static IEnumerable<int[]> GetChunks(Node node)
    {
        private Node(int[] registers)
        {
            if (current.Registers.Length != 0)
            {
                yield return current.Registers;
            }
            current = current.Next;
        }
    }
    
    public class Node
    {
        private Node(int[] registers)
        {
            Registers = registers;
        }
        public static Node CreateFictiveNode() => new([]);
        public static Node Prepend(Node? tail, params int[][] chunks)
        {
            ArgumentOutOfRangeException.ThrowIfZero(chunks.Length);
            
            var node = tail;
            for (var i = chunks.Length - 1; i >= 0; i--)
            {
                var registers = chunks[i];
                if (registers.Length != 0)
                {
                    node = new Node(registers)
                    {
                        Next = node,
                        Registers = registers
                    };   
                }
            }
            ArgumentNullException.ThrowIfNull(node);
            
            return node;
        }
        public int[] Registers { get; private set; }
        public Node? Next { get; set; }
        public void Append(int[] registers)
        {
            var current = this;
            while (current.Next is not null)
            {
                current = current.Next;
            }
            current.Next = new(registers);
        }
        public void Replace(Node node)
        {
            Registers = node.Registers;
            Next = node.Next;
        }
        public int CalculateGarbageOfTail()
        {
            var garbage = 0;
            var current = this;
            while (current is not null)
            {
                garbage += CalculateGarbage(current.Registers);
                current = current.Next;
            }
            return garbage;
        }
        public int CalculateDepth()
        {
            var height = 0;
            var current = this;
            while (current is not null)
            {
                if (current.Registers.Length != 0)
                {
                    height++;
                }
                current = current.Next;
            }
            return height;
        }
    }
    
    private static Node JoinRecursive(int maxLimit, int decimalOrderMaxLimit, Node root, bool rearrange)
    {
        var node = root;
        while (node is not null)
        {
            var current = node.Registers;
            if (node.Next is not null)
            {
                var follow = node.Next.Registers;
                if (follow.Length != 0)
                {
                    var tail = node.Next.Next;

                    var minWeight = CalculateWeight(decimalOrderMaxLimit, tail, current, follow);
                    var candidate = node;
                    foreach (var (trimLeft, joinRight) in CalculateMinGarbageCombination(current, follow))
                    {
                        if (trimLeft.Length != 0 && ExcessLimit(maxLimit, joinRight, out var taken, out var rest))
                        {
                            if (rearrange)
                            {
                                continue;
                            }
                            var next = JoinRecursive(maxLimit, decimalOrderMaxLimit, Node.Prepend(tail, rest), true);
                            if (next.CalculateDepth() <= (tail?.CalculateDepth() ?? 0))
                            {
                                var weight = CalculateWeight(decimalOrderMaxLimit, next, trimLeft, taken);
                                if (weight < minWeight)
                                {
                                    minWeight = weight;
                                    candidate = Node.Prepend(next, trimLeft, taken);
                                }
                            }
                        }
                        if (!ExcessLimit(maxLimit, joinRight))
                        {
                            var weight = CalculateWeight(decimalOrderMaxLimit, tail, trimLeft, joinRight);
                            if (weight < minWeight)
                            {
                                minWeight = weight;
                                candidate = Node.Prepend(tail, trimLeft, joinRight);
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

    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk) => chunk[^1] - chunk[0] + 1 > maxLimit;
    private static bool ExcessLimit(int maxLimit, ReadOnlySpan<int> chunk, out int[] taken, out int[] rest)
    {
        ArgumentOutOfRangeException.ThrowIfZero(chunk.Length);
        
        if (ExcessLimit(maxLimit, chunk))
        {
            var index = chunk.Length - 1;
            while (index >= 0)
            {
                if (!ExcessLimit(maxLimit, chunk[..index]))
                {
                    break;
                }
                index--;
            }
            rest = chunk[index..].ToArray();
            taken = chunk[..index].ToArray();
            return true;
        }
        rest = [];
        taken = chunk.ToArray();
        return false;
    }
    
    private static int CalculateWeight(int decimalOrderMaxLimit, Node? tail, params int[][] chunks)
    {
        var garbage = tail?.CalculateGarbageOfTail() ?? 0;
        var depth = tail?.CalculateDepth() ?? 0;
        foreach (var registers in chunks)
        {
            if (registers.Length != 0)
            {
                garbage += CalculateGarbage(registers);
            }
            else
            {
                depth = Math.Max(0, depth - 1);
            }
        }
        return garbage + decimalOrderMaxLimit * depth;
    }
    private static int CalculateGarbage(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2) => chunk1.Length == 0 ? CalculateGarbage(chunk2) : CalculateGarbage(chunk1) + CalculateGarbage(chunk2);
    private static int CalculateGarbage(ReadOnlySpan<int> chunk)
    {
        ref var pv = ref MemoryMarshal.GetReference(chunk);
        nint length = chunk.Length;
        nint i = 0;
        nint bound512 = length & ~(Vector256<int>.Count * 2 - 1);
        int res = 0;
        for (; i < bound512; i += Vector256<int>.Count)
        {
            var vec1 =  Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref pv, i + 1));
            var vec2 = Unsafe.As<int, Vector256<int>>(ref Unsafe.Add(ref pv, i));
            var vec3 = Vector256.Subtract(vec1, vec2);
            var vec4 = Vector256.Subtract(vec3, Vector256<int>.One);
            res += Vector256.Sum(vec4);
        }
        i++;
        for (; i < length; i++)
        {
            res += chunk[(int)i] - chunk[(int)(i - 1)] - 1;
        }
        return res;
    }
    
    private static (int[] TrimLeft, int[] JoinRight)[] CalculateMinGarbageCombination(ReadOnlySpan<int> chunk1, ReadOnlySpan<int> chunk2)
    {
        List<(int[] TrimLeft, int[] JoinRight)> res = [];
        var min = CalculateGarbage(chunk1, chunk2);
        ReadOnlySpan<int> concat = [..chunk1, ..chunk2];
        for (var splitPoint = chunk1.Length - 1; splitPoint >= 0; splitPoint--)
        {
            var trimLeft = concat[..splitPoint];
            var joinRight = concat[splitPoint..];
            var garbage = CalculateGarbage(trimLeft, joinRight);
            if (garbage < min || trimLeft.Length == 0)
            {
                min = garbage;
                res.Add((trimLeft.ToArray(), joinRight.ToArray()));
            }
        }
        return res.ToArray();
    }
    
    private static Node Chunk(int maxLimit, int[] registers)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLimit);
        ArgumentOutOfRangeException.ThrowIfZero(registers.Length);

        var root = Node.CreateFictiveNode();
        var chunkStart = 0;
        var currentLimit = 1;
        var index = 1;
        while (index < registers.Length)
        {
            var distance = registers[index] - registers[index - 1];
            currentLimit += distance;
            if (currentLimit > maxLimit)
            {
                root.Append(registers[chunkStart..index]);
                currentLimit = 1;
                chunkStart = index;
            }
            index++;
        }
        root.Append(registers[chunkStart..index]);
        return root;
    }
}