using UnityEngine;

public class MonsterSpawnInfo : MonoBehaviour
{
    public Vector3 StartPosition { get; private set; }

    void Awake()
    {
        // «апоминаем позицию монстра при старте игры
        StartPosition = transform.position;
    }
}