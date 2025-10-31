using UnityEngine;

public static class HexDirections
{
    // Flat-Top + Odd-R (행 오프셋)
    public static bool IsOddRow(Vector3Int c) => (c.y & 1) == 1;

    // 현재 셀에서 3개 축의 "앞/뒤" 델타를 넘겨준다.
    // 축0: E-W, 축1: NE-SW, 축2: NW-SE
    public static (Vector3Int fwd, Vector3Int back) GetAxisDeltas(Vector3Int cell, int axis)
    {
        bool odd = IsOddRow(cell);

        switch (axis)
        {
            case 0: // E <-> W (항상 동일)
                return (new Vector3Int(+1, 0, 0), new Vector3Int(-1, 0, 0));

            case 1: // NE <-> SW : 짝/홀 행에서 번갈아 달라짐
                // 전진(NE) 방향
                var ne = odd ? new Vector3Int(+1, +1, 0) : new Vector3Int(0, +1, 0);
                // 반대(SW) 방향
                var sw = odd ? new Vector3Int(0, -1, 0) : new Vector3Int(-1, -1, 0);
                return (ne, sw);

            case 2: // NW <-> SE : 짝/홀 행에서 번갈아 달라짐
                // 전진(NW) 방향
                var nw = odd ? new Vector3Int(0, +1, 0) : new Vector3Int(-1, +1, 0);
                // 반대(SE) 방향
                var se = odd ? new Vector3Int(+1, -1, 0) : new Vector3Int(0, -1, 0);
                return (nw, se);
        }

        return (Vector3Int.zero, Vector3Int.zero);
    }
    //// 현재 셀의 짝/홀에 맞춘 6이웃 오프셋 반환
    //public static Vector3Int[] GetNeighbor6(Vector3Int cell)
    //{
    //    return new[]
    //                {
    //            new Vector3Int(+1,  0, 0), // 위 (E)
    //            new Vector3Int( 0, +1, 0), // 오른쪽 위 (NE)
    //            new Vector3Int(-1, +1, 0), // 오른쪽 아래 (SE)
    //            new Vector3Int(-1,  0, 0), // 아래 (W)
    //            new Vector3Int(-1, -1, 0), // 왼쪽 아래 (SW)
    //            new Vector3Int( 0, -1, 0), // 왼쪽 위 (NW)
    //        };
    //}
    //
    //// '같은 라인'을 따라갈 때 현재 셀 기준으로 동일 방향 벡터를 다시 선택
    //public static Vector3Int NextStepDir(Vector3Int atCell, Vector3Int prevDir)
    //{
    //    var dirs = GetNeighbor6(atCell);
    //    foreach (var d in dirs) if (d == prevDir) return d;
    //    return dirs[0]; // fallback
    //}
    //
    //public static Vector3Int OppositeDir(Vector3Int atCell, Vector3Int prevDir)
    //{
    //    var want = -prevDir;
    //    var dirs = GetNeighbor6(atCell);
    //    foreach (var d in dirs) if (d == want) return d;
    //    return want; // fallback
    //}
}
