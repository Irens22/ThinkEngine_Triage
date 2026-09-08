using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AgentSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform staffSpawnPoint;
    public Transform patientSpawnPoint;

    [Header("Prefabs")]
    public GameObject[] doctorPrefabs;
    public GameObject[] nursePrefabs;
    public GameObject[] patientPrefabs;

    private List<GameObject> activeAgents = new List<GameObject>();
    private int currentPatientId = 1000; 
    private int currentStaffId = 2000;

    public void SpawnDoctor() => SpawnStaff(doctorPrefabs, "Doctor");
    public void SpawnNurse() => SpawnStaff(nursePrefabs, "Nurse");
    
    private void SpawnStaff(GameObject[] prefabs, string role)
    {
        GameObject newStaff = SpawnAgent(prefabs, staffSpawnPoint);
        if(newStaff == null) return;

        StaffAI staff = newStaff.GetComponent<StaffAI>();
        if(staff != null)
        {            
            staff.staffId = currentStaffId;
            currentStaffId++;
            staff.name = $"{role}_{staff.staffId}";
        }
    }

    public void SpawnPatient(bool randomColor, TriageColor specificColor = TriageColor.green)
    {
        GameObject newPatient = SpawnAgent(patientPrefabs, patientSpawnPoint);
        if(newPatient == null) return;

        PatientAI ai = newPatient.GetComponent<PatientAI>();
        if(ai != null)
        {
            ai.patientId = currentPatientId;
            currentPatientId++;

            TriageColor assignedColor = randomColor ? (TriageColor)Random.Range(0, 5) : specificColor;
            ai.UpdateColor(assignedColor);
            newPatient.name = $"Patient_{ai.patientId}_{assignedColor}";
        }
    }

    private GameObject SpawnAgent(GameObject[] prefabArray, Transform spawnLocation)
    {
        if(prefabArray == null || prefabArray.Length == 0 || spawnLocation == null) 
        {
            return null;
        }
        
        GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];
        if(prefabToSpawn == null) return null;
        
        Vector3 spawnPos = spawnLocation.position;
        NavMeshHit hit;
        if(NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }
        
        GameObject spawned = Instantiate(prefabToSpawn, spawnPos, spawnLocation.rotation);
        activeAgents.RemoveAll(a => a == null);
        activeAgents.Add(spawned);
        return spawned;
    }

    public void RemoveLastStaff(string staffType)
    {
        for(int i = activeAgents.Count - 1; i >= 0; i--)
        {
            if(activeAgents[i] != null && activeAgents[i].name.Contains(staffType))
            {
                Destroy(activeAgents[i]);
                activeAgents.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveLastPatient()
    {
        for(int i = activeAgents.Count - 1; i >= 0; i--)
        {
            if(activeAgents[i] != null && activeAgents[i].name.Contains("Patient"))
            {
                Destroy(activeAgents[i]);
                activeAgents.RemoveAt(i);
                break;
            }
        }
    }

    public void DespawnAll()
    {
        foreach(var agent in activeAgents)
        {
            if(agent != null) Destroy(agent);
        }
        activeAgents.Clear();
        currentPatientId = 1000; 
        currentStaffId = 2000;

        if(IdRegistry.Instance != null)
        {
            IdRegistry.Instance.ResetSimulationState();
        }
        
        if(WaitingRoomManager.Instance != null)
        {
            WaitingRoomManager.Instance.ResetAllSeats();
        }
    }
}