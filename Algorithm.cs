// r: required, p: package, g: garbage, ml: max length
// priority = count(p) > sum(g)
// distance = r_end - r_start + 1
// garbage = required_end - required_start - 1

// 1 8 9 12 ml: 8
// ok(1, 8-12)
// 1:r 10:r 21:r ml:12
// ok(1-10:p 21:p -> 9:g), bad(1-1:p 10-21:p -> 10:g)
// 1:r 10:r 25:r ml:12
// ok(1-10:p 25:p -> 9:g), bad(<empty>)
// 1:r 10:r 25:r ml:16
// ok(1-10:p 25:p -> 9:g), bad(1:p 10-25:p -> 14:g)
// 1:r 10:r 25:r ml:26
// ok(1-25:p -> 14:g), bad(<empty>)
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

namespace register_packager;

public class Algorithm
{
    public static int[][] Solve(int[] vertices, (int Start, int End)[] holes, int limit)
    {
        var slices = Slice(limit, holes, vertices, false);
        return slices;
    }
    private static bool BinarySearch((int Start, int End)[] array, int searchedValue, int first, int last)
    {
        if (first > last)
        {
            return false;
        }
        var middle = (first + last) / 2;
        var middleValue = array[middle];
        if (searchedValue >= middleValue.Start && searchedValue <= middleValue.End)
        {
            return true;
        }
        if (middleValue.End > searchedValue)
        {
            return BinarySearch(array, searchedValue, first, middle - 1);
        }
        return BinarySearch(array, searchedValue, middle + 1, last);
    }
    //int[] отрезок
    //int[][] массив отрезков - пакетов
    private static int[][] Slice(int limit, (int Start, int End)[] holes, int[] vertices, bool split)
    {
        var slices = vertices.Select((t, i) => new[] { vertices[..i], vertices[i..] }.Where(x => x.Length != 0).ToArray()).Where(x => split ? !x.Any(x => x.SequenceEqual(vertices)) : true).ToArray();
        var slicesWithDistance = slices.Select(x => (Distance: x.Select(CalculateDistance).Max(), Src: x)).ToArray();
        var minLength = slicesWithDistance.Min(x => x.Src.Length);
        var minSlices = slicesWithDistance.Where(x => x.Src.Length == minLength).ToArray();
        var minMax = minSlices.Min(x => x.Distance);
        var bestSlices = slicesWithDistance.Where(x => x.Distance == minMax).ToArray();
        List<(int Distance, List<int[]>)> results = [];
        foreach (var (_, src) in bestSlices)
        {
            List<int[]> temp = [];
            foreach (var chunk in src)
            {
                if (CalculateDistance(chunk) <= limit)
                {
                    temp.Add(chunk);
                    continue;
                }
                if (CalculateDistance(chunk) > limit)
                {
                    temp.AddRange(Slice(limit, holes, chunk, true));
                }
            }
            var c = temp.Select(CalculateDistance).Sum();
            results.Add((c, temp));
        }
        return results.MinBy(x => x.Distance).Item2.ToArray();
    }

    private static int CalculateDistance(int[] chunk) => chunk.Length == 0 ? 0 : chunk[^1] - chunk[0] + 1;
}