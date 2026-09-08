using UnityEngine;
using System.Collections.Generic;

public class HospitalSensorHub : MonoBehaviour
{
    [Header("Hospital Resources Lists")]
    public List<RoomManager> rooms = new List<RoomManager>();
    public List<StaffAI> staffMembers = new List<StaffAI>();
    public List<PatientAI> activePatients = new List<PatientAI>();
    private int lastPatientCount = -1;
    private int lastStaffCount = -1;

    private void Start()
    {
        //le stanze sono statiche, quindi al momento è vuoto
    }

    private void Update()
    {
        if(IdRegistry.Instance == null) return;

        //forza la sincronizzazione ogni 30 fotogrammi per gestire gli scambi 1 a 1 in cui il conteggio non cambia
        bool forceSync = (Time.frameCount % 30 == 0);

        if(forceSync || IdRegistry.Instance.Patients.Count != lastPatientCount)
        {
            SyncPatients();
        }

        int currentStaffTotal = IdRegistry.Instance.Doctors.Count + IdRegistry.Instance.Nurses.Count;
        if(forceSync || currentStaffTotal != lastStaffCount)
        {
            SyncStaff();
        }
        
        if(rooms.Count == 0 && IdRegistry.Instance.Rooms.Count > 0)
        {
            SyncRooms();
        }
    }

    private void SyncRooms()
    {
        rooms.Clear();
        foreach(var t in IdRegistry.Instance.Rooms.Values)
        {
            if(t != null && t.TryGetComponent(out RoomManager r)) rooms.Add(r);
        }
    }

    private void SyncStaff()
    {
        staffMembers.Clear();
        foreach(var t in IdRegistry.Instance.Doctors.Values)
        {
            if(t != null && t.TryGetComponent(out StaffAI d)) staffMembers.Add(d);
        }
        foreach(var t in IdRegistry.Instance.Nurses.Values)
        {
            if(t != null && t.TryGetComponent(out StaffAI n)) staffMembers.Add(n);
        }
        lastStaffCount = IdRegistry.Instance.Doctors.Count + IdRegistry.Instance.Nurses.Count;
    }

    private void SyncPatients()
    {
        activePatients.Clear();
        foreach(var t in IdRegistry.Instance.Patients.Values)
        {
            if(t != null && t.TryGetComponent(out PatientAI p)) activePatients.Add(p);
        }
        lastPatientCount = IdRegistry.Instance.Patients.Count;
    }
}