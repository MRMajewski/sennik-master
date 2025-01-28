using UnityEngine;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using System.Security.Cryptography;

public class UIManager : MonoBehaviour {
    // Public method to trigger the animation

    [SerializeField]
    private GameObject adGameObject;

    [SerializeField]
    private CanvasGroup winPanel;

    [SerializeField]
    private CanvasGroup headerPanel;

    [SerializeField]
    private CanvasGroup mainMenuPanel;

    [SerializeField]
    private CanvasGroup hostPanel;

    [SerializeField]
    private CanvasGroup clientPanel;

    [SerializeField]
    private CanvasGroup settingsPanel;

    [ContextMenu("Add Sequence")]
    public void PlayAdSequence() {
        PlayScaleAnimation(adGameObject);
    }

    public void PlayScaleAnimation(GameObject target) {

        target.transform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(target.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        sequence.Append(target.transform.DOScale(1.15f, 1f).SetEase(Ease.InOutCirc).SetLoops(4, LoopType.Yoyo));
        sequence.Append(target.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));

    }


    public void InitUI() {
        OpenPanelInstant(mainMenuPanel);

        HidePanelInstant(winPanel);
        HidePanelInstant(headerPanel);
        HidePanelInstant(hostPanel);
        HidePanelInstant(clientPanel);
        HidePanelInstant(settingsPanel);
    }

    public void CloseMainMenu() {
        HidePanel(mainMenuPanel);
        OpenPanel(headerPanel);


        HidePanelInstant(winPanel);     
        HidePanelInstant(hostPanel);
        HidePanelInstant(clientPanel);
        HidePanelInstant(settingsPanel);
    }


    public void ReinitUI() {
        OpenPanel(headerPanel);
        HidePanelInstant(winPanel);
    }

    public void ToggleSettingsPanelFromGame(bool openIt) {

        if(openIt) {
            OpenPanel(settingsPanel);           
        } else {
            HidePanel(settingsPanel);
        }
    }


    [ContextMenu("Open Win Panel")]
    public void OpenWinPanel() {
        OpenPanel(winPanel);
    }

    public void OpenPanel(CanvasGroup panel) {
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

        float currentAlpha = 0f;
        DOTween.To(() => currentAlpha, x => {
            currentAlpha = x;
            panel.alpha = currentAlpha;
        }, 1f, 0.5f).OnComplete(() => {

            panel.interactable = true;
            panel.blocksRaycasts = true;
        });
    }

    public void OpenPanelInstant(CanvasGroup panel) {
        panel.alpha = 1f;
        panel.interactable = true;
        panel.blocksRaycasts = true;

    }

    public void HidePanel(CanvasGroup panel) {
        panel.interactable = false;
        panel.blocksRaycasts = false;

        float currentAlpha = panel.alpha;
        DOTween.To(() => currentAlpha, x => {
            currentAlpha = x;
            panel.alpha = currentAlpha;
        }, 0f, 0.5f).OnComplete(() => {

          //  panel.gameObject.SetActive(false);
        });
    }


    public void HidePanelInstant(CanvasGroup panel) {
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;

    }
}