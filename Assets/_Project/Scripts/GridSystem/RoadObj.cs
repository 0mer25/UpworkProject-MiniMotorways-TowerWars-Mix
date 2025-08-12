using UnityEngine;
using DG.Tweening;

public class RoadObj : GridObj
{
    [Header("Spawn Anim")]
    [SerializeField] float dropHeight = 1.5f;
    [SerializeField] float dropDuration = 0.35f;
    [SerializeField] Ease dropEase = Ease.OutCubic;
    [SerializeField] bool squash = true;

    private Sequence _spawnSeq;
    void OnEnable()
    {
        if (transform.parent.TryGetComponent<GridTile>(out var grid))
        {
            SetTile(grid.MapRoad);
        }

        PlaySpawnAnim();
    }   

    void OnDisable()
    {
        _spawnSeq?.Kill();
        transform.DOKill();
    }
    

    public void PlaySpawnAnim()
    {
        var t = transform;
        t.DOKill();
        _spawnSeq?.Kill();

        // hedef (zemin) pozisyonu mevcut pozisyonun
        Vector3 targetPos  = t.position;
        Quaternion targetRot = t.rotation;

        // biraz yukarıda ve minik tilt ile başlat
        t.position = targetPos + Vector3.up * dropHeight;

        _spawnSeq = DOTween.Sequence().SetLink(gameObject); // obje disable/destroy olunca otomatik kill

        // düşüş
        _spawnSeq.Append(t.DOMoveY(targetPos.y, dropDuration).SetEase(dropEase));

        // yere değince hafif "squash" (isteğe bağlı)
        if (squash)
        {
            _spawnSeq.Join(t.DOScaleY(0.9f, dropDuration * 0.5f)); // inerken biraz bas
            _spawnSeq.Append(t.DOScaleY(1f, 0.12f));               // sonra toparla
        }

        // tilt'i geri düzelt
        _spawnSeq.Join(t.DORotateQuaternion(targetRot, dropDuration * 0.9f));
    }
}
