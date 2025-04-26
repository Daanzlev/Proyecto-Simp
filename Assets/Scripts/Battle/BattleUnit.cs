using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Security.Permissions;

public class BattleUnit : MonoBehaviour {

    /* -------------------------------------------------------
    >>>>>>>>> VARIABLES
    -------------------------------------------------------  */

    //[SerializeField] SimpBase _base;
    //[SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    Image image;
    Vector3 originalPos;
    Color originalColor;



    /* -------------------------------------------------------
    >>>>>>>>> METHODS
    -------------------------------------------------------  */


    /* ---------------- GETTERS -----------------------------*/
    public Simp Simp { get; set; }

    public bool IsPlayerUnit {
        get { return isPlayerUnit; }
    }

    public BattleHud Hud {
        get { return hud; }
    }



    private void Awake() {

        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;

    }

    public void Setup(Simp simp)  {

        Simp = simp;

        if (isPlayerUnit) {
            image.sprite = Simp.Base.BackSprite;
        }
        else  {
            image.sprite = Simp.Base.FrontSprite;
        }
        //hud.gameObject.SetActive(true);
        hud.SetData(simp);

        image.color = originalColor;
        PlayEnterAnimation();

    }


    public void PlayEnterAnimation() {

        if (isPlayerUnit) {
            image.transform.localPosition = new Vector3(-500, originalPos.y);
        }
        else {
            image.transform.localPosition = new Vector3(500, originalPos.y);
        }

        image.transform.DOLocalMoveX(originalPos.x, 1f);

    }

    public void PlayAtackAnimation() {

        var sequence = DOTween.Sequence();

        if (isPlayerUnit) {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50, 0.25f));
        }
        else {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50, 0.25f));
        }

        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));

    }

    public void PlayHitAnimation() {

        var sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));

    }

    public void PlayFaintAnimation() {

        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));

    }

}