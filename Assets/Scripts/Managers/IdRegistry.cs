using UnityEngine;
using System.Collections.Generic;

public class IdRegistry : MonoBehaviour
{
    public static IdRegistry Instance { get; private set; }

    [Header("Static Waypoints")]
    public Transform exitPoint;
    public Transform staffRoomPoint;

    public Dictionary<int, Transform> Patients { get; private set; } = new Dictionary<int, Transform>();
    public Dictionary<int, Transform> Doctors { get; private set; } = new Dictionary<int, Transform>();
    public Dictionary<int, Transform> Nurses { get; private set; } = new Dictionary<int, Transform>();
    public Dictionary<int, Transform> Rooms { get; private set; } = new Dictionary<int, Transform>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //metodi iscrizione
    public void RegisterPatient(int id, Transform agentTransform)
    {
        if(!Patients.ContainsKey(id)) Patients.Add(id, agentTransform);
    }

    public void RegisterDoctor(int id, Transform agentTransform)
    {
        if(!Doctors.ContainsKey(id)) Doctors.Add(id, agentTransform);
    }

    public void RegisterNurse(int id, Transform agentTransform)
    {
        if(!Nurses.ContainsKey(id)) Nurses.Add(id, agentTransform);
    }

    public void RegisterRoom(int id, Transform roomTransform)
    {
        if(!Rooms.ContainsKey(id)) Rooms.Add(id, roomTransform);
    }

    //metodi disiscrizione
    public void UnregisterPatient(int id)
    {
        if(Patients.ContainsKey(id)) Patients.Remove(id);
    }

    public void UnregisterDoctor(int id)
    {
        if(Doctors.ContainsKey(id)) Doctors.Remove(id);
    }

    public void UnregisterNurse(int id)
    {
        if(Nurses.ContainsKey(id)) Nurses.Remove(id);
    }

    public void ResetSimulationState()
    {
        Patients.Clear();
        Doctors.Clear();
        Nurses.Clear();

        foreach(var r in Rooms)
        {
            if(r.Value != null && r.Value.TryGetComponent(out RoomManager roomManager))
            {
                roomManager.ClearRoom();
            }
        }
    }
}