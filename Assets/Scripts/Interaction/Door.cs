using UnityEngine;
using UnityEngine.AI;

// Дверь, которая плавно поворачивается на петлях. Открывается на E.
// Скрипт вешается на объект, у которого pivot стоит на петле.
// Дверь всегда крутится вокруг вертикальной оси мира, даже если модель повёрнута.
//
// МОНСТР И ДВЕРЬ: на дверь нужен NavMesh Obstacle (Shape = Box, Carve = вкл).
// Закрыта — obstacle включён, прохода нет. Открыта — выключен, монстр проходит.
// Если поле Obstacle пустое, скрипт сам берёт NavMesh Obstacle с этого объекта.
public class Door : MonoBehaviour, IInteractable
{
    public float openAngle = 90f;
    public NavMeshObstacle obstacle;
    public float speed = 3f;

    [Tooltip("Дверь сама выбирает сторону и открывается ОТ игрока (не в лицо). " +
             "Если выключено — открывается всегда на Open Angle")]
    public bool openAwayFromPlayer = false;

    [Header("Замок (необязательно)")]
    public bool isLocked = false;
    [Tooltip("ID ключа, который открывает дверь, например \"key_basement\". Пусто = не открыть ничем")]
    public string requiredKeyId = "";
    [Tooltip("Текст, когда заперто")]
    public string lockedPrompt = "Заперто";

    [Header("Двойная дверь (необязательно)")]
    [Tooltip("Вторая створка. Если указать — обе открываются и закрываются вместе")]
    public Door linkedDoor;

    [Header("Звуки (необязательно)")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    bool isOpen;
    Quaternion closedRotation;
    Quaternion targetRotation;
    Vector3 upAxis;

    public bool IsOpen => isOpen;

    void Awake()
    {
        if (obstacle == null) obstacle = GetComponent<NavMeshObstacle>();
        if (obstacle != null) obstacle.carving = true;
    }

    void Start()
    {
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
        upAxis = transform.parent != null
            ? transform.parent.InverseTransformDirection(Vector3.up).normalized
            : Vector3.up;
        UpdateObstacle();
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * speed);
    }

    public string GetPrompt()
    {
        if (isLocked) return lockedPrompt;
        return isOpen ? "Закрыть дверь" : "Открыть дверь";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isLocked)
        {
            if (!string.IsNullOrEmpty(requiredKeyId) && player.inventory != null && player.inventory.HasItem(requiredKeyId))
            {
                isLocked = false;
                if (linkedDoor != null) linkedDoor.isLocked = false;
            }
            else
            {
                Play(lockedSound);
                return;
            }
        }

        SetOpen(!isOpen, player.transform.position);
        Play(isOpen ? openSound : closeSound);

        if (linkedDoor != null) linkedDoor.SetOpen(isOpen, player.transform.position);
    }

    public void SetOpen(bool open, Vector3 fromPosition)
    {
        isOpen = open;
        UpdateObstacle();

        if (!open)
        {
            targetRotation = closedRotation;
            return;
        }

        float angle = openAngle;
        if (openAwayFromPlayer)
            angle = Mathf.Abs(openAngle) * AwaySign(fromPosition);

        targetRotation = Quaternion.AngleAxis(angle, upAxis) * closedRotation;
    }

    void UpdateObstacle()
    {
        if (obstacle != null) obstacle.enabled = !isOpen;
    }

    int AwaySign(Vector3 playerPosition)
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 hinge = transform.position;
        Vector3 edge = (r != null ? r.bounds.center : hinge + transform.right) - hinge;
        edge.y = 0f;
        Vector3 move = Vector3.Cross(Vector3.up, edge);
        Vector3 toPlayer = playerPosition - hinge;
        toPlayer.y = 0f;
        return Vector3.Dot(move, toPlayer) > 0f ? -1 : 1;
    }

    void Play(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    // Чтобы монстр сам открывал незапертые двери, вызывайте это из MonsterAI.
    public void OpenForMonster(Vector3 monsterPosition)
    {
        if (isLocked || isOpen) return;
        SetOpen(true, monsterPosition);
        Play(openSound);
        if (linkedDoor != null) linkedDoor.SetOpen(true, monsterPosition);
    }
}