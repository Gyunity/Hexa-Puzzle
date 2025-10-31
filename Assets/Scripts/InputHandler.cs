using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InputHandler : MonoBehaviour
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private LayerMask gemLayer;
    [SerializeField]
    private TileBoardManager tileBoardManager;

    private Vector3Int slectGem;
    private Vector3Int swapGem;

    private Vector3Int resetVec = new Vector3Int(99999,9999,9999);

    private void Start()
    {
       SlectedReset();
    }

    void Update()
    {
        if (!tileBoardManager.moveCheck || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {

            TryPickGemCell(Input.mousePosition, out slectGem);
            Debug.Log($"slectGem : {slectGem}");
            Debug.Log($"slectGemDic: {tileBoardManager.GetGemMap(slectGem).GemType}");
            Debug.Log($"slectGemPos: {tileBoardManager.GetGemMap(slectGem).transform.position}");
        }
        else if (Input.GetMouseButton(0) && slectGem != resetVec)
        {
            if(TryPickGemCell(Input.mousePosition, out swapGem))
            {
            //Debug.Log($"Ω∫ø“¿Î:{swapGem}");
            tileBoardManager.TrySwap(slectGem, swapGem);
            SlectedReset();

            }
        }
    }

   

    private Vector3 ScreenToWorldOnTilePlane(Vector3 screenPos)
    {
        float planeZ = tilemap.transform.position.z;
        Vector3 sp = new(screenPos.x, screenPos.y, cam.nearClipPlane);
        return cam.ScreenToWorldPoint(sp);
    }
    private bool TryPickGemCell(Vector3 screenPos, out Vector3Int cell)
    {
        Gem gem;
        Vector3 world = ScreenToWorldOnTilePlane(screenPos);
        var col = Physics2D.OverlapPoint(world, gemLayer); 
        if (col)
        {
            gem = col.GetComponentInParent<Gem>();
            if (gem != null)
            {
                cell = tilemap.WorldToCell(gem.transform.position);
                if (cell == slectGem)
                    return false;
                return true;
            }
        }
        gem = null;
        cell = default;
        return false;
    }
    private void SlectedReset()
    {
        slectGem = resetVec;
        swapGem = resetVec;

    }
}
