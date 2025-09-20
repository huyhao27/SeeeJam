using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class Tutorial : Popup
{
    [SerializeField] private GameObject bg;
    [SerializeField] private RectTransform tutHand;

    void Start()
    {
        Vector2 startPos = tutHand.anchoredPosition;

        tutHand
            .DOAnchorPos(startPos + new Vector2(-30, -30), 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
        StartCoroutine(WaitUntilAddNote());
    }

    void OnEnable()
    {
        this.bg.SetActive(true);
        this.tutHand.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        this.bg.SetActive(false);
        this.tutHand.gameObject.SetActive(false);
    }

    private IEnumerator WaitUntilAddNote()
    {
        yield return new WaitUntil(() => PlayerStats.Instance.NoteCount <= 0);

        GameManager.Instance.ChangeState(GameState.Playing);
    }

    void OnDestroy()
    {
        tutHand.DOKill();
        StopAllCoroutines();
    }




}
