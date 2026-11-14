using UnityEngine;

// Качнуть ВСЕ маятниковые двери на нужном этаже одной строкой в событии.
// Вешается на пустой объект (например, SwingDoors_F2).
// В событии: SwingDoorsScare → SwingAll (float) → сколько секунд качаться.
public class SwingDoorsScare : MonoBehaviour
{
    [Tooltip("Двери ниже этой высоты не трогаем (2 этаж: 3)")]
    public float minY = 3f;
    [Tooltip("Двери выше этой высоты не трогаем (2 этаж: 7)")]
    public float maxY = 7f;

    public void SwingAll(float seconds)
    {
        foreach (SwingDoor d in FindObjectsOfType<SwingDoor>())
        {
            float y = d.transform.position.y;
            if (y >= minY && y <= maxY) d.GhostSwing(seconds);
        }
    }
}
