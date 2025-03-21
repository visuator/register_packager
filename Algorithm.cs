// r: required, p: package, g: garbage, ml: max length
// priority = count(p) > sum(g)
// distance = r_end - r_start + 1
// garbage = required_end - required_start - 1

// 1:r 10:r 21:r ml:12
// ok(1-10:p 21:p -> 9:g), bad(1-1:p 10-21:p -> 10:g)
// 1:r 10:r 25:r ml:12
// ok(1-10:p 25:p -> 9:g), bad(<empty>)
// 1:r 10:r 25:r ml:16
// ok(1-10:p 25:p -> 9:g), bad(1:p 10-25:p -> 14:g)
// 1:r 10:r 25:r ml:26
// ok(1-25:p -> 14:g)
// 1:r 25:r 45:r ml:26
// ok(1:p 25-45:p -> 19:g), bad(1-25:p 45:p -> 23:g)

// inference the garbage-based pair select formula: garbage(x_0, x_1) < garbage(x_1, x_2) ? (x_0, x_1) : (x_1, x_2)
// inference the recurrent package construction: пробуем взять нулевой пакет,
//                                               пытаемся взять сначала нулевой элемент и следующий,
//                                               так делаем до тех пор, пока не закончится длина пакета,
//                                               для следующего пакета следующей вариации от каждой точки предыдущего будем считать мусор
//                                               (аналогично для нового пакета считаются точки) -
//                                               если по приведенной выше формуле количество мусора становится меньше,
//                                               создаем новую вариацию пакета и дописываем его как следующую ноду связного списка пакетов

using System.Text;

namespace register_packager;

public class Algorithm
{
    public static Package[] Solve(Register[] registers, int RegistersLimit, out string debugPrintedTree)
    {
        var list = new List<Node>();
        var remainingLength = RegistersLimit;
        var index = 0;
        NextPackageRecursive(registers, list, null, index, remainingLength, default);
        var sb = new StringBuilder();
        foreach (var node in list)
        {
            PrintRecursive(list, sb, node, 0);
        }
        debugPrintedTree = sb.ToString();
        return [];
    }
    private static void PrintRecursive(List<Node> list, StringBuilder sb, Node? value, int space)
    {
        if (value is not null)
        {
            sb.Append(string.Concat(Enumerable.Range(0, space).Select(x => '\t')));
            sb.AppendLine(value.ToString());
            PrintRecursive(list, sb, value.Next, space + 2);
        }
    }
    private static void NextPackageRecursive(
        Register[] registers,
        List<Node> list,
        Node? parent,
        int index,
        int remainingLength,
        in RegisterPair previousJoin)
    {
        var takenRegisters = new List<RegisterPair>();
        var package = new Package([]);
        var copy = index;
        while (TryTakeNextPair(registers, ref index, ref remainingLength, out var pair) && remainingLength > 0)
        {
            takenRegisters.Add(pair);
            package.Joins.Add(takenRegisters.ToArray());
            var lessGarbageJoin = previousJoin.CompareByGarbage(pair);
            var node = parent?.SetNext(package) ?? new Node(null, package);
            list.Add(node);
            NextPackageRecursive(registers, list, node, index, remainingLength, in lessGarbageJoin);
        }
    }
    private static bool TryTakeNextPair(Register[] registers, ref int index, ref int remainingLength, out RegisterPair pair)
    {
        var registersCount = registers.Length;
        if (index < registersCount)
        {
            var start = registers[index];
            if (index + 1 < registersCount)
            {
                var next = registers[index + 1];
                pair = new(start, next);
                index += 2;
                remainingLength -= 2;
                return true;
            }
            pair = RegisterPair.CreateSingle(start);
            index += 1;
            remainingLength--;
            return true;
        }
        pair = default;
        return false;
    }
}

public class Node(Node? parent, Package? value)
{
    public Node? Next { get; set; }
    public Node? Parent { get; init; } = parent;
    public Package? Value { get; init; } = value;
    public Node SetNext(Package package) => Next = new(this, package);
    public override string ToString() => $"| {Value}";
}