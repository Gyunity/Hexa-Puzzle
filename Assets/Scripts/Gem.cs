using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("색깔별 보석들(Sprite)")]
    [SerializeField]
    private Sprite redGem;
    [SerializeField]
    private Sprite greenGem;
    [SerializeField] 
    private Sprite orangeGem;
    [SerializeField] 
    private Sprite pinkGem;
    [SerializeField]
    private Sprite purpleGem;
    [SerializeField]
    private Sprite yellowGem;

    public GemType GemType { get; private set; }

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(GemType type)
    {
        GemType = type;
        spriteRenderer.sprite = GetSprite(type);
    }

    private Sprite GetSprite(GemType type)
    {
        return type switch
        {
            GemType.Red => redGem,
            GemType.Green => greenGem,
            GemType.Orange => orangeGem,
            GemType.Pink => pinkGem,
            GemType.Purple => purpleGem,
            GemType.Yellow => yellowGem
        };
    }
}
