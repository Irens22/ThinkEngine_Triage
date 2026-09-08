using UnityEngine;
using UnityEngine.AI;
using ThinkEngine.Planning; 
using System.Collections;

public class ActionCoopVisit : ThinkEngine.Planning.Action
{
    public int patientId { get; set; }
    public int doctorId { get; set; }
    public int nurseId { get; set; }
    public int roomId { get; set; }

    private PatientAI patient;
    private StaffAI doctor;
    private StaffAI nurse;
    private RoomManager room;
    private NavMeshAgent patientAgent;
    private NavMeshAgent doctorAgent;
    private NavMeshAgent nurseAgent;

    private float arrivalTime;
    private float waitDuration = 6f;

    public override State Prerequisite()
    {        
        if(IdRegistry.Instance != null &&
            IdRegistry.Instance.Patients.ContainsKey(patientId) && 
            IdRegistry.Instance.Doctors.ContainsKey(doctorId) &&
            IdRegistry.Instance.Nurses.ContainsKey(nurseId) &&
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
            !IdRegistry.Instance.Nurses.ContainsKey(nurseId) ||
            !IdRegistry.Instance.Rooms.ContainsKey(roomId))
        {
            return;
        }

        patient = IdRegistry.Instance.Patients[patientId].GetComponent<PatientAI>();
        doctor = IdRegistry.Instance.Doctors[doctorId].GetComponent<StaffAI>();
        nurse = IdRegistry.Instance.Nurses[nurseId].GetComponent<StaffAI>();
        room = IdRegistry.Instance.Rooms[roomId].GetComponent<RoomManager>();

        doctor.AssignTask(patientId);
        nurse.AssignTask(patientId);
        room.OccupyRoom(patientId);
        patient.UpdateState(PatientState.in_visit);

        patient.StartCoroutine(CoopVisitRoutine(patient, doctor, nurse, room));
    }

    public override State Done()
    {
        return State.READY;
    }

    private IEnumerator CoopVisitRoutine(PatientAI p, StaffAI d, StaffAI n, RoomManager r)
    {
        p.LeaveWaitingRoom();
        d.WakeUp();
        n.WakeUp();

        NavMeshAgent pAgent = p.GetComponent<NavMeshAgent>();
        NavMeshAgent dAgent = d.GetComponent<NavMeshAgent>();
        NavMeshAgent nAgent = n.GetComponent<NavMeshAgent>();

        Vector3 pTarget = r.patientPosition.position;
        Vector3 dTarget = r.doctorPosition.position;
        Vector3 nTarget = r.nursePosition.position;

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

        if(nAgent != null && nAgent.isActiveAndEnabled && nAgent.isOnNavMesh)
        {
            nAgent.isStopped = false;
            nAgent.SetDestination(nTarget);
        }

        float timeout = 10f;
        while((!HasArrived(pAgent, pTarget) || !HasArrived(dAgent, dTarget) || !HasArrived(nAgent, nTarget)) && timeout > 0)
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
        if(n != null)
        {
            n.ClearTask();
            n.GoToStaffRoom();
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