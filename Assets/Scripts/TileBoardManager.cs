using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBoardManager : MonoBehaviour
{

    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private GemFactory gemFactory;
    [SerializeField]
    private TileMatchFinder tileMatchFinder;
    [SerializeField]
    private Transform gemsRoot;

    private Dictionary<Vector3Int, Gem> gemMap = new Dictionary<Vector3Int, Gem>();


    private GemType[] allTypes;
    public bool moveCheck = true;

    private void Start()
    {
        allTypes = (GemType[])Enum.GetValues(typeof(GemType));
        InitializeBoard();
    }


    public Gem GetGemMap(Vector3Int cell)
    {
       return gemMap[cell];
    }
    //타일맵에 있는 자리들을 바탕으로 Gem을 생성하고 Type를 랜덤으로 부여한다.
    //gemMap에 cell(Vector3Int)을 키로하여 Gem을 넣는다.
    private void InitializeBoard()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        var bounds = tilemap.cellBounds;
        foreach (var p in bounds.allPositionsWithin)
            if (tilemap.HasTile(p))
                positions.Add(p);

        positions.Sort((a, b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));


        foreach (var cell in positions)
        {

            List<GemType> candidates = new List<GemType>(allTypes);
            candidates.RemoveAll(t => tileMatchFinder.WouldFormLineOf3At(cell, t, gemMap));

            GemType chosen = candidates.Count > 0 ?
                candidates[UnityEngine.Random.Range(0, candidates.Count)] : allTypes[UnityEngine.Random.Range(0, allTypes.Length)];

            gemMap[cell] = gemFactory.CreateGemOfType(chosen, tilemap, cell, gemsRoot);

        }
    }

    private Vector3 WorldCenterOf(Vector3Int cell)
    {
        Vector3 w = tilemap.CellToWorld(cell) + tilemap.tileAnchor;
        return new Vector3(w.x, w.y, tilemap.transform.position.z);
    }

    //aGem과 bGem을 교체한다
    public void TrySwap(Vector3Int a, Vector3Int b)
    {
        if (!gemMap.ContainsKey(a) || !gemMap.ContainsKey(b) || a == b)
            return;

        StartCoroutine(SwapPosition(a, b));

    }
    //두개의 지점을 교체
    private IEnumerator SwapPosition(Vector3Int cellA, Vector3Int cellB, float duration = 0.25f)
    {
        moveCheck = false;
        if (!gemMap[cellA] || !gemMap[cellB])
            yield break;
        Vector3 startAPos = WorldCenterOf(cellA);
        Vector3 startBPos = WorldCenterOf(cellB);

        (gemMap[cellA], gemMap[cellB]) = (gemMap[cellB], gemMap[cellA]);


        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float du = Mathf.Clamp01(time / duration);

            gemMap[cellA].transform.position = Vector3.LerpUnclamped(startBPos, startAPos, du);
            gemMap[cellB].transform.position = Vector3.LerpUnclamped(startAPos, startBPos, du);
            yield return null;
        }
        gemMap[cellA].transform.position = startAPos;
        gemMap[cellB].transform.position = startBPos;

        yield return new WaitForSeconds(duration + 0.1f);

       List<Vector3Int> matches = tileMatchFinder.FindMatches(gemMap);
       if (matches.Count == 0)
       {
           (gemMap[cellA], gemMap[cellB]) = (gemMap[cellB], gemMap[cellA]);

           time = 0f;
           while (time < duration)
           {
               time += Time.deltaTime;
               float du = Mathf.Clamp01(time / duration);

               gemMap[cellA].transform.position = Vector3.LerpUnclamped(startBPos, startAPos, du);
               gemMap[cellB].transform.position = Vector3.LerpUnclamped(startAPos, startBPos, du);
               yield return null;
           }
           gemMap[cellA].transform.position = startAPos;
           gemMap[cellB].transform.position = startBPos;

           if (gemsRoot)
               gemMap[cellA].transform.SetParent(gemsRoot, worldPositionStays: true);
           gemMap[cellB].transform.SetParent(gemsRoot, worldPositionStays: true);
       }
       ResolveCascades(matches);
        moveCheck = true;
    }


    private bool _isResolving;

    private void ResolveCascades(List<Vector3Int> initialMatches = null)
    {
        if (_isResolving) return;        // 재진입 방지(안전)
        _isResolving = true;

        var matches = initialMatches ?? tileMatchFinder.FindMatches(gemMap);

        while (matches.Count > 0)
        {
            // 1) 매칭 제거
            foreach (var p in matches)
            {
                var g = gemMap[p];
                if (g != null)
                {
                    Destroy(g.gameObject);
                    gemMap[p] = null;
                }
            }

            // 2) 중력+보충 (열 단위 압축만 사용! ⬇️)
            ApplyGravityAndRefill();

            // 3) 다음 연쇄 검사
            matches = tileMatchFinder.FindMatches(gemMap);
        }

        _isResolving = false;
    }
    private void SnapGemToCell(Gem gem, Vector3Int cell)
    {
        if (!gem) return;
        gem.transform.position = WorldCenterOf(cell);
        if (gemsRoot) gem.transform.SetParent(gemsRoot, true);
    }
    private void FillEmptyChack()
    {
        // 1) 보드 위 모든 유효 셀 수집 (정렬: '아래→위', 그 다음 좌→우)
        var cells = new List<Vector3Int>();
        foreach (var p in tilemap.cellBounds.allPositionsWithin)
            if (tilemap.HasTile(p)) cells.Add(p);

        // 아래가 -x 이므로 x 오름차순(작은 x가 더 아래), y 오름차순
        cells.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));

        // 2) 빈칸마다 위쪽에서 가장 가까운 보석을 끌어내려 채운다
        foreach (var cell in cells)
        {
            if (gemMap.TryGetValue(cell, out var here) && here == null)
            {
                // 위쪽으로 스캔 (Up = 축0의 forward)
                var scan = cell;
                while (true)
                {
                    var upDelta = HexDirections.GetAxisDeltas(scan, axis: 0).fwd; // (+1,0,0) 또는 parity에 따른 값
                    var above = scan + upDelta;
                    if (!tilemap.HasTile(above)) break;

                    if (gemMap.TryGetValue(above, out var g) && g != null)
                    {
                        // 끌어내리기
                        gemMap[cell] = g;
                        gemMap[above] = null;
                        SnapGemToCell(g, cell);
                        break;
                    }

                    scan = above;
                }
            }
        }

        // 3) 아직도 비어있는 칸들(맨 위쪽에 남은 빈칸)에 새 보석 생성
        var empties = cells.Where(c => gemMap[c] == null).ToList();
        foreach (var cell in empties)
        {
            // 즉시 3매치 방지
            var candidates = new List<GemType>(allTypes);
            candidates.RemoveAll(t => tileMatchFinder.WouldFormLineOf3At(cell, t, gemMap));

            var chosen = (candidates.Count > 0)
                ? candidates[UnityEngine.Random.Range(0, candidates.Count)]
                : allTypes[UnityEngine.Random.Range(0, allTypes.Length)];

            gemMap[cell] = gemFactory.CreateGemOfType(chosen, tilemap, cell, gemsRoot);
            // 스폰 위치를 '보드 위 한 칸 더 위'에서 떨어뜨리고 싶다면:
            //   var spawnCell = cell + HexDirections.GetAxisDeltas(cell, 0).fwd;
            //   gem = gemFactory.CreateGemOfType(chosen, tilemap, spawnCell, gemsRoot);
            //   gemMap[cell] = gem;  // 이후 DOTween 등으로 cell까지 내려오게 연출
        }
    }
    /// 보드의 모든 열(column, 동일 y)별로 '아래로 압축'하고, 맨 위 빈칸은 새로 스폰
    private void ApplyGravityAndRefill()
    {
        // 1) 유효 셀 수집 및 y(열)별 그룹핑
        var allCells = new List<Vector3Int>();
        foreach (var p in tilemap.cellBounds.allPositionsWithin)
            if (tilemap.HasTile(p)) allCells.Add(p);

        // y(열) → 그 열의 셀들(아래→위: x 오름차순)로 정렬
        var columns = allCells
            .GroupBy(c => c.y)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(c => c.x).ToList() // x가 작을수록 '아래'
            );

        // 2) 각 열을 '아래로 압축'
        foreach (var kv in columns)
        {
            var col = kv.Value;        // 아래→위 순
            int write = 0;             // 다음으로 채울 '아래쪽' 인덱스

            for (int read = 0; read < col.Count; read++)
            {
                var fromCell = col[read];
                if (!gemMap.TryGetValue(fromCell, out var g) || g == null) continue;

                var toCell = col[write];
                if (fromCell != toCell)
                {
                    // 이동: 데이터 갱신 + 화면 스냅
                    gemMap[toCell] = g;
                    gemMap[fromCell] = null;
                    SnapGemToCell(g, toCell);
                }
                write++;
            }

            // write ~ 끝까지는 빈칸(null)로 남김 (여기에만 새로 스폰)
            for (int i = write; i < col.Count; i++)
                gemMap[col[i]] = null;
        }

        // 3) 맨 위 빈칸만 스폰 (즉시 3매치 방지 포함)
        foreach (var kv in columns)
        {
            var col = kv.Value;
            // 위쪽부터 내려오며 빈칸만 생성 (col은 아래→위 정렬이므로 역순)
            for (int i = col.Count - 1; i >= 0; i--)
            {
                var cell = col[i];
                if (gemMap[cell] != null) continue;

                var candidates = new List<GemType>(allTypes);
                candidates.RemoveAll(t => tileMatchFinder.WouldFormLineOf3At(cell, t, gemMap));

                var chosen = (candidates.Count > 0)
                    ? candidates[UnityEngine.Random.Range(0, candidates.Count)]
                    : allTypes[UnityEngine.Random.Range(0, allTypes.Length)];

                gemMap[cell] = gemFactory.CreateGemOfType(chosen, tilemap, cell, gemsRoot);

                // 연출을 원하면: 스폰을 '셀 위'에서 만들고 DOTween 등으로 cell까지 떨어뜨리세요.
                // var up = HexDirections.GetAxisDeltas(cell, 0).fwd; // (+1,0,0) 쪽 한 칸 위
                // var spawnCell = cell + up;
                // var gem = gemFactory.CreateGemOfType(chosen, tilemap, spawnCell, gemsRoot);
                // gemMap[cell] = gem;
                // gem.transform.DOMove(WorldCenterOf(cell), 0.15f).SetEase(Ease.InQuad);
            }
        }
    }
    public bool TryGetGemAtCell(Vector3Int cell, out Gem gem)
    {
        return gemMap.TryGetValue(cell, out gem) && gem != null;
    }
}
