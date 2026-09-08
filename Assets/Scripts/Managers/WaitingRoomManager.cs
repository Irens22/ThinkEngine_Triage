using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class WaitingRoomManager : MonoBehaviour
{
    //singleton per accesso facilitato da parte di PatientAI
    public static WaitingRoomManager Instance { get; private set; }

    [Header("Seat Management")]
    public List<Transform> availableSeats; 
    private List<Transform> occupiedSeats = new List<Transform>();

    [Header("Standing Area Fallback")]
    public Transform roomCenter;
    public float standingRadius = 4f;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Transform GetRandomAvailableSeat()
    {
        if(availableSeats.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSeats.Count);
            Transform assignedSeat = availableSeats[randomIndex];
            
            availableSeats.RemoveAt(randomIndex);
            occupiedSeats.Add(assignedSeat);
            
            return assignedSeat;
        }
        return null; 
    }

    public void ReleaseSeat(Transform seatToRelease)
    {
        if(seatToRelease != null && occupiedSeats.Contains(seatToRelease))
        {
            occupiedSeats.Remove(seatToRelease);
            availableSeats.Add(seatToRelease);
        }
    }

    public void ResetAllSeats()
    {
        availableSeats.AddRange(occupiedSeats);
        occupiedSeats.Clear();
    }

    public Vector3 GetRandomStandingPoint()
    {
        if(roomCenter != null)
        {
            Vector2 circle = Random.insideUnitCircle * standingRadius;
            Vector3 randomDirection = roomCenter.position + new Vector3(circle.x, 0f, circle.y);
            
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomDirection, out hit, standingRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return transform.position;
    }
}