using UnityEngine;
using UnityEngine.AI;
using ThinkEngine.Planning; 
using System.Collections;

public class ActionVisit : ThinkEngine.Planning.Action
{
    public int patientId { get; set; }
    public int doctorId { get; set; }
    public int roomId { get; set; }

    private PatientAI patient;
    private StaffAI doctor;
    private RoomManager room;
    private NavMeshAgent patientAgent;
    private NavMeshAgent doctorAgent;

    private float arrivalTime;
    private float waitDuration = 6f;

    public override State Prerequisite()
    {        
        if(IdRegistry.Instance != null &&
            IdRegistry.Instance.Patients.ContainsKey(patientId) && 
            IdRegistry.Instance.Doctors.ContainsKey(doctorId) &&
            IdRegistry.Instance.Rooms.ContainsKey(roomId))
        {
            return State.READY;
        }
        return State.ABORT;
    }

    public override void Do()
    {
        if(IdRegistry.Instance == null || 
            !IdRegistry.Instance.Patients.ContainsKey(patientId) || 
            !IdRegistry.Instance.Doctors.ContainsKey(doctorId) ||
            !IdRegistry.Instance.Rooms.ContainsKey(roomId)) 
        {
            return;
        }

        patient = IdRegistry.Instance.Patients[patientId].GetComponent<PatientAI>();
        doctor = IdRegistry.Instance.Doctors[doctorId].GetComponent<StaffAI>();
        room = IdRegistry.Instance.Rooms[roomId].GetComponent<RoomManager>();

        if (patient.currentState != PatientState.waiting || doctor.aspStatus != "idle")
        {
            return; 
        }

        patient.UpdateState(PatientState.in_visit);
        doctor.AssignTask(patientId);
        room.OccupyRoom(patientId);

        Console.Instance.LogPatientAction($"Patient {patientId} called to Visit Room {roomId} by Doctor {doctorId}.", patient.triageColor);
        patient.StartCoroutine(VisitRoutine(patient, doctor, room));
        
    }

    public override State Done()
    {
        return State.READY;
    }

    private IEnumerator VisitRoutine(PatientAI p, StaffAI d, RoomManager r)
    {
        p.LeaveWaitingRoom();
        d.WakeUp();

        NavMeshAgent pAgent = p.GetComponent<NavMeshAgent>();
        NavMeshAgent dAgent = d.GetComponent<NavMeshAgent>();

        Vector3 pTarget = r.patientPosition.position;
        Vector3 dTarget = r.doctorPosition.position;

        if(pAgent != null && pAgent.isActiveAndEnabled && pAgent.isOnNavMesh)
        {
            pAgent.isStopped = false;
            pAgent.SetDestination(pTarget);
        }

        if(dAgent != null && dAgent.isActiveAndEnabled && dAgent.isOnNavMesh)
        {
            dAgent.isStopped = false;
            dAgent.SetDestination(dTarget);
        }

        float timeout = 10f;
        while((!HasArrived(pAgent, pTarget) || !HasArrived(dAgent, dTarget)) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null; 
        }

        p.AlignAndPose(r.patientPosition, PatientPose.LieDown);
        yield return new WaitForSeconds(waitDuration);

        if(d != null)
        {
            d.ClearTask();
            d.GoToStaffRoom();
        }
        
        p.UpdateState(PatientState.needs_therapy);
    }

    private bool HasArrived(NavMeshAgent agent, Vector3 target)
    {
        if(agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return true;
        if(Vector3.Distance(agent.transform.position, target) <= agent.stoppingDistance + 0.2f)
        {
            return true;
        }
        if(agent.pathPending) return false;
        if(!agent.hasPath) return false;
        return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
    }
}