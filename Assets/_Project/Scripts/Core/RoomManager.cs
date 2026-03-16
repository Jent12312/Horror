using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private List<Room> allRooms;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Room GetRandomRoom()
    {
        if (allRooms.Count == 0) return null;
        return allRooms[Random.Range(0, allRooms.Count)];
    }
}

[System.Serializable]
public class Room
{
    public string roomId;
    public Transform roomCenter;
    public List<Transform> itemSpawnPoints;
    public List<Transform> playerSpawnPoints;

    [HideInInspector] public bool isAltarRoom;
    [HideInInspector] public bool isDevilLair;
}