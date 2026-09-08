using UnityEngine;

public enum RoomType { triage, exam, resus }
public class RoomManager : MonoBehaviour
{
    [Header("Room Identity (ASP Sensors)")]
    public int roomId;              
    public RoomType roomType;      
    public string aspRoomType => roomType.ToString();
    public string aspStatus { get; private set; } = "idle";
    public int currentPatientId { get; private set; } = -1;

    [Header("Navigation Points")]
    public Transform patientPosition;
    public Transform doctorPosition;
    public Transform nursePosition;

    private void Start()
    {
        if(IdRegistry.Instance != null)
        {
            IdRegistry.Instance.RegisterRoom(roomId, this.transform);
        }
        
        if(patientPosition == null || doctorPosition == null || nursePosition == null)
        {
            Debug.LogError($"[RoomManager] Stanza {roomId} non ha dei navigation points");
        }
    }

    public void OccupyRoom(int patientId)
    {
        aspStatus = "busy";
        currentPatientId = patientId;
    }

    public void ClearRoom()
    {
        aspStatus = "idle";
        currentPatientId = -1;
    }
}