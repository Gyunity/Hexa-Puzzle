using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMatchFinder : MonoBehaviour
{

    public List<Vector3Int> FindMatches(Dictionary<Vector3Int, Gem> gemMap)
    {
        var result = new HashSet<Vector3Int>();

        foreach (var kv in gemMap)
        {
            var pos = kv.Key;
            var gem = kv.Value;
            if (gem == null) continue;

            // 3개 축만 검사 (양방향 확장)
            for (int axis = 0; axis < 3; axis++)
            {
                var line = new List<Vector3Int> { pos };

                // +쪽(Forward)
                var p = pos;
                for (int i = 0; i < 8; i++) // 보드 크기에 맞게 넉넉히
                {
                    var delta = HexDirections.GetAxisDeltas(p, axis).fwd;
                    p += delta;
                    if (!gemMap.TryGetValue(p, out var g) || g == null || g.GemType != gem.GemType) break;
                    line.Add(p);
                }

                // -쪽(Backward)
                p = pos;
                for (int i = 0; i < 8; i++)
                {
                    var delta = HexDirections.GetAxisDeltas(p, axis).back;
                    p += delta;
                    if (!gemMap.TryGetValue(p, out var g) || g == null || g.GemType != gem.GemType) break;
                    line.Add(p);
                }

                if (line.Count >= 3)
                    foreach (var c in line) result.Add(c);
            }
        }

        return result.ToList();
    }

    public bool WouldFormLineOf3At(Vector3Int pos, GemType t, IDictionary<Vector3Int, Gem> gemMap)
    {
        // 3개 축 중 하나라도 3이상 이어지면 true
        for (int axis = 0; axis < 3; axis++)
        {
            int count = 1;

            // +쪽
            var p = pos;
            for (int i = 0; i < 2; i++)
            {
                var d = HexDirections.GetAxisDeltas(p, axis).fwd;
                p += d;
                if (!gemMap.TryGetValue(p, out var g) || g == null || g.GemType != t) break;
                count++;
            }

            // -쪽
            p = pos;
            for (int i = 0; i < 2; i++)
            {
                var d = HexDirections.GetAxisDeltas(p, axis).back;
                p += d;
                if (!gemMap.TryGetValue(p, out var g) || g == null || g.GemType != t) break;
                count++;
            }

            if (count >= 3) return true;
        }

        return false;
    }
}
