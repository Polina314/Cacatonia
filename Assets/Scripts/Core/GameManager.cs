using System.Collections;
using UnityEngine;
using TMPro;

// "Главный" объект игры: помнит последнюю контрольную точку,
// обрабатывает смерть игрока и показывает сообщения.
// На сцене должен быть ОДИН пустой объект GameManager с этим скриптом.
public class GameManager : MonoBehaviour
{
    // Синглтон: из любого скрипта можно написать GameManager.Instance.PlayerDied()
    public static GameManager Instance { get; private set; }

    public FirstPersonController player;
    public CanvasGroup deathScreen;   // чёрный экран (необязательно)
    public DeathScreen deathGif;      // гифка с глазами (необязательно)
    public TMP_Text messageText;      // текст для сообщений (необязательно)
    public float respawnDelay = 2f;   // минимум; если гифка длиннее — ждём её до конца

    Vector3 respawnPosition;
    Quaternion respawnRotation;
    bool isDead;
    Coroutine messageRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Стартовая позиция — первая "контрольная точка"
        respawnPosition = player.transform.position;
        respawnRotation = player.transform.rotation;
        if (deathScreen != null) deathScreen.alpha = 0f;

        // Если поле Death Gif не заполнено в Inspector — ищем сами (true = искать и среди выключенных)
        if (deathGif == null) deathGif = FindObjectOfType<DeathScreen>(true);
        if (deathGif == null) Debug.LogWarning("DeathScreen не найден на сцене: гифку смерти некому прятать");
        if (deathGif != null) deathGif.Hide();

        if (messageText != null) messageText.text = "";
    }

    public void SetCheckpoint(Transform point)
    {
        respawnPosition = point.position;
        respawnRotation = point.rotation;
        Debug.Log("Контрольная точка сохранена");
    }

    public void PlayerDied()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(RespawnRoutine());
    }

    // Запасное имя, если где-то в событиях Inspector вызывается KillPlayer
    public void KillPlayer()
    {
        PlayerDied();
    }

    IEnumerator RespawnRoutine()
    {
        // 1. Забираем управление и показываем экран смерти
        player.canMove = false;
        if (deathScreen != null) deathScreen.alpha = 1f;

        float wait = respawnDelay;
        if (deathGif != null)
        {
            deathGif.ShowDeath();
            wait = Mathf.Max(wait, deathGif.TotalDuration);   // гифка успевает доиграть
        }

        // 2. Ждём, пока игрок смотрит на экран смерти
        yield return new WaitForSeconds(wait);

        // 3. Телепорт игрока на чекпоинт (CharacterController на время телепорта выключаем)
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.SetPositionAndRotation(respawnPosition, respawnRotation);
        cc.enabled = true;

        // 4. Монстры: на стартовые места + полный сброс (снимает блокировку камеры, включает скрипты игрока)
        foreach (MonsterAI m in FindObjectsByType<MonsterAI>(FindObjectsSortMode.None))
            m.RespawnReset();

        // 5. Прячем экран смерти и гифку
        if (deathGif != null) deathGif.Hide();
        if (deathScreen != null) deathScreen.alpha = 0f;

        // 6. Возвращаем управление: и сам компонент, и флаг
        player.enabled = true;
        player.canMove = true;
        isDead = false;
    }

    // Короткая версия для событий в Inspector (UnityEvent не видит методы с двумя параметрами)
    public void Say(string text)
    {
        ShowMessage(text, 3f);
    }

    public void ShowMessage(string text, float duration = 3f)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageText.text = text;
        messageRoutine = StartCoroutine(HideMessage(duration));
    }

    IEnumerator HideMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.text = "";
    }
}