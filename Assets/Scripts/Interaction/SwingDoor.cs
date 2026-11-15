using UnityEngine;

// Маятниковая дверь (как в больницах между отделениями): без ручки.
// Когда подходит игрок ИЛИ монстр — створка сама отходит от него, когда уходят — закрывается.
// Вешается на каждую створку (pivot должен стоять на петле). Кнопка E не нужна.
//
// Монстр через маятниковые двери проходит ВСЕГДА (он их толкает), поэтому
// NavMesh Obstacle на маятниковых дверях НЕ нужен — если он есть, он выключится сам.
public class SwingDoor : MonoBehaviour
{
    public float openAngle = 80f;
    [Tooltip("На каком расстоянии от петли дверь начинает открываться")]
    public float openDistance = 1.8f;
    public float speed = 4f;

    [Header("Звук (необязательно)")]
    public AudioSource audioSource;
    public AudioClip swingSound;

    Transform player;
    MonsterAI[] monsters;
    Quaternion closedRotation;
    Vector3 upAxis;
    int currentSide;        // 0 = закрыта, +1 / -1 = открыта в одну из сторон
    float ghostUntil = -1f; // до какого времени дверь «качается сама»

    // Дверь сама качается несколько секунд, будто кто-то пробежал.
    // Вызывать из события: SwingDoor → GhostSwing (float), например 3.
    public void GhostSwing(float seconds)
    {
        ghostUntil = Time.time + seconds;
        if (audioSource != null && swingSound != null) audioSource.PlayOneShot(swingSound);
    }

    void Start()
    {
        closedRotation = transform.localRotation;
        upAxis = transform.parent != null
            ? transform.parent.InverseTransformDirection(Vector3.up).normalized
            : Vector3.up;

        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null) player = fpc.transform;
        monsters = FindObjectsOfType<MonsterAI>();

        // на всякий случай выключаем старый obstacle, если он остался на двери
        var obs = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obs != null) obs.enabled = false;
    }

    void Update()
    {
        // ближайший к двери: игрок или монстр
        Vector3 toNearest = Vector3.zero;
        float best = float.MaxValue;
        Consider(player, ref toNearest, ref best);
        if (monsters != null)
            foreach (var m in monsters) if (m != null) Consider(m.transform, ref toNearest, ref best);

        bool near = best < openDistance;

        if (!near)
        {
            currentSide = 0;
        }
        else if (currentSide == 0)
        {
            // Сторона выбирается один раз, когда кто-то подошёл, чтобы дверь не дёргалась
            currentSide = AwaySign(toNearest);
            if (audioSource != null && swingSound != null) audioSource.PlayOneShot(swingSound);
        }

        Quaternion target = currentSide == 0
            ? closedRotation
            : Quaternion.AngleAxis(openAngle * currentSide, upAxis) * closedRotation;

        // «Призрачное» качание (от подруги): работает, только если рядом никого нет
        if (Time.time < ghostUntil && currentSide == 0)
        {
            float left = ghostUntil - Time.time;
            float angle = Mathf.Sin(Time.time * 6f) * Mathf.Min(1f, left) * 45f;
            target = Quaternion.AngleAxis(angle, upAxis) * closedRotation;
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * speed);
    }

    void Consider(Transform t, ref Vector3 toNearest, ref float best)
    {
        if (t == null) return;
        Vector3 d = t.position - transform.position; d.y = 0f;
        float dist = d.magnitude;
        if (dist < best) { best = dist; toNearest = d; }
    }

    int AwaySign(Vector3 toWho)
    {
        Renderer r = GetComponentInChildren<Renderer>();
        Vector3 edge = (r != null ? r.bounds.center : transform.position + transform.right) - transform.position;
        edge.y = 0f;
        Vector3 move = Vector3.Cross(Vector3.up, edge);
        return Vector3.Dot(move, toWho) > 0f ? -1 : 1;
    }
}