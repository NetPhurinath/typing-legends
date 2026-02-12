using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPortraitUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Image image;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defeatSprite;

    [Header("Timings")]
    [SerializeField] private float hitDuration = 0.15f;
    [SerializeField] private float attackDuration = 0.2f;

    private Coroutine flashRoutine;
    private bool defeated;

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        SetIdle();
    }

    public void SetIdle()
    {
        if (defeated) return;
        SetSprite(idleSprite);
    }

    public void PlayHit()
    {
        if (defeated) return;
        Flash(hitSprite, hitDuration);
    }

    public void PlayAttack()
    {
        if (defeated) return;
        Flash(attackSprite, attackDuration);
    }

    public void PlayDefeat()
    {
        defeated = true;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;
        SetSprite(defeatSprite);
    }

    private void Flash(Sprite sprite, float seconds)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(sprite, seconds));
    }

    private IEnumerator FlashRoutine(Sprite sprite, float seconds)
    {
        SetSprite(sprite);
        yield return new WaitForSecondsRealtime(seconds); // works even when Time.timeScale = 0
        SetIdle();
        flashRoutine = null;
    }

    private void SetSprite(Sprite s)
    {
        if (image == null || s == null) return;
        image.sprite = s;
    }
}