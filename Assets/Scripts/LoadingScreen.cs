using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour {
    CanvasGroup _canvasGroup;
    public float animationDuration = 0.5f;
    public static LoadingScreen I { get; private set; }
    private void Start() {
        if (I != null && I != this) {
            Destroy(gameObject);
            return;
        }
        if (I == this) return;
        I = this;
        DontDestroyOnLoad(gameObject);
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1;
        _ = Close();
    }
    public async void LoadScene(int buildIndex) {
        await Open();
        if (buildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(buildIndex);
        else
            SceneManager.LoadScene(0);
        await Close();
    }
    async Task Open() {
        await DOTween.To(() => _canvasGroup.alpha, (a) => _canvasGroup.alpha = a, 1, animationDuration).AsyncWaitForCompletion();
    }
    async Task Close() {
        await DOTween.To(() => _canvasGroup.alpha, (a) => _canvasGroup.alpha = a, 0, animationDuration).AsyncWaitForCompletion();
    }
}
