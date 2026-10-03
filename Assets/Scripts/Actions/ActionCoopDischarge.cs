using UnityEngine;
using UnityEngine.AI;
using ThinkEngine.Planning;
using System.Collections;

public class ActionCoopDischarge : Action
{
    public int patientId { get; set; }
    public int doctorId { get; set; }
    public int nurseId { get; set; }
    private float waitDuration = 4f;

    public override State Prerequisite()
    {
        if(IdRegistry.Instance != null &&
            IdRegistry.Instance.Patients.ContainsKey(patientId) && 
            IdRegistry.Instance.Doctors.ContainsKey(doctorId) &&
            IdRegistry.Instance.Nurses.ContainsKey(nurseId))
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
            !IdRegistry.Instance.Nurses.ContainsKey(nurseId))
        {
            return;
        }

        PatientAI patient = IdRegistry.Instance.Patients[patientId].GetComponent<PatientAI>();
        StaffAI doctor = IdRegistry.Instance.Doctors[doctorId].GetComponent<StaffAI>();
        StaffAI nurse = IdRegistry.Instance.Nurses[nurseId].GetComponent<StaffAI>();

        patient.UpdateState(PatientState.in_discharge);
        doctor.AssignTask(patientId);
        nurse.AssignTask(patientId);

        Console.Instance.LogPatientAction($"Discharge: Doctor {doctorId} within Nurse {nurseId} are about to discharge Patient {patientId}.", patient.triageColor);
        patient.StartCoroutine(CoopDischargeRoutine(patient, doctor, nurse));
    }

    public override State Done()
    {
        return State.READY; 
    }

    private IEnumerator CoopDischargeRoutine(PatientAI p, StaffAI d, StaffAI n)
    {
        p.WakeUp(); 

        NavMeshAgent dAgent = d.GetComponent<NavMeshAgent>();
        NavMeshAgent nAgent = n.GetComponent<NavMeshAgent>();
        Vector3 targetPos = p.transform.position;

        if(dAgent != null && dAgent.isActiveAndEnabled && dAgent.isOnNavMesh)
        {
            dAgent.isStopped = false;
            dAgent.SetDestination(targetPos);
        }
        if(nAgent != null && nAgent.isActiveAndEnabled && nAgent.isOnNavMesh)
        {
            nAgent.isStopped = false;
            nAgent.SetDestination(targetPos);
        }

        float timeout = 10f;
        while((!HasArrived(dAgent, targetPos) || !HasArrived(nAgent, targetPos)) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null; 
        }

        yield return new WaitForSeconds(waitDuration);
        
        RoomManager room = p.GetRoomByPatientId(p.patientId);
        if(room != null)
        {
            room.ClearRoom();
        }

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

        p.LeaveHospital(); 
    }

    private bool HasArrived(NavMeshAgent agent, Vector3 target)
    {
        if(agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return true; 
        
        if(Vector3.Distance(agent.transform.position, target) <= agent.stoppingDistance + 0.2f)
            return true;

        if(agent.pathPending) return false;
        if(!agent.hasPath) return false;
        return agent.remainingDistance <= agent.stoppingDistance + 0.2f;
    }
}