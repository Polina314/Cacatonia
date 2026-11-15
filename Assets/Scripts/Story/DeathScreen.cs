using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Гифка смерти: листает кадры. Показывает и прячет её GameManager.
public class DeathScreen : MonoBehaviour
{
    public Image deathImage;
    public Sprite[] frames;
    public float frameDelay = 0.6f;

    Coroutine playRoutine;

    // Сколько длится вся анимация (GameManager ждёт столько перед респавном)
    public float TotalDuration => frames != null ? frames.Length * frameDelay : 0f;

    public void ShowDeath()
    {
        gameObject.SetActive(true);
        if (playRoutine != null) StopCoroutine(playRoutine);   // повторный вызов просто начнёт заново
        playRoutine = StartCoroutine(PlayFrames());
    }

    public void Hide()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = null;
        gameObject.SetActive(false);
    }

    IEnumerator PlayFrames()
    {
        foreach (Sprite frame in frames)
        {
            deathImage.sprite = frame;
            yield return new WaitForSeconds(frameDelay);
        }
    }
}