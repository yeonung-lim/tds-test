using UnityEngine;

public class BackgroundTileScroller : MonoBehaviour
{
    public float scrollSpeed = 1f;
    private float tileWidth;

    private Transform[] tiles;

    void Start()
    {
        if (transform.childCount < 2)
        {
            Debug.LogError("자식 타일이 2개 있어야 합니다.");
            return;
        }

        tiles = new Transform[2];
        tiles[0] = transform.GetChild(0);
        tiles[1] = transform.GetChild(1);

        // SpriteRenderer에서 tileWidth 자동 추출
        var spriteRenderer = tiles[0].GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            tileWidth = spriteRenderer.sprite.bounds.size.x * tiles[0].lossyScale.x;
        }
        else
        {
            Debug.LogError("SpriteRenderer 또는 Sprite가 없습니다. tileWidth 계산 실패.");
        }
    }

    void Update()
    {
        foreach (Transform tile in tiles)
        {
            tile.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        Transform leftTile = GetLeftmostTile();
        Transform rightTile = GetRightmostTile();

        if (leftTile.localPosition.x <= -tileWidth)
        {
            leftTile.localPosition = new Vector3(rightTile.localPosition.x + tileWidth, leftTile.localPosition.y, leftTile.localPosition.z);
            SwapTiles();
        }
    }

    Transform GetLeftmostTile()
    {
        return tiles[0].localPosition.x < tiles[1].localPosition.x ? tiles[0] : tiles[1];
    }

    Transform GetRightmostTile()
    {
        return tiles[0].localPosition.x > tiles[1].localPosition.x ? tiles[0] : tiles[1];
    }

    void SwapTiles()
    {
        if (tiles[0].localPosition.x > tiles[1].localPosition.x)
        {
            (tiles[0], tiles[1]) = (tiles[1], tiles[0]);
        }
    }
}