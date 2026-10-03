using UnityEngine;
using UnityEngine.AI;
using ThinkEngine.Planning;
using System.Collections;

public class ActionDischarge : Action
{
    public int patientId { get; set; }
    public int doctorId { get; set; }
    private float waitDuration = 4f;

    public override State Prerequisite()
    {
        if(IdRegistry.Instance != null &&
            IdRegistry.Instance.Patients.ContainsKey(patientId) && 
            IdRegistry.Instance.Doctors.ContainsKey(doctorId))
        {
            return State.READY;
        }
        return State.ABORT;
    }

    public override void Do()
    {
        if(IdRegistry.Instance == null || 
            !IdRegistry.Instance.Patients.ContainsKey(patientId) || 
            !IdRegistry.Instance.Doctors.ContainsKey(doctorId))
        {
            return;
        }

        PatientAI patient = IdRegistry.Instance.Patients[patientId].GetComponent<PatientAI>();
        StaffAI doctor = IdRegistry.Instance.Doctors[doctorId].GetComponent<StaffAI>();

        patient.UpdateState(PatientState.in_discharge);
        doctor.AssignTask(patientId);

        Console.Instance.LogPatientAction($"Discharge: Doctor {doctorId} is about to discharge Patient {patientId}.", patient.triageColor);
        patient.StartCoroutine(DischargeRoutine(patient, doctor));
    }

    public override State Done()
    {
        return State.READY;
    }

    private IEnumerator DischargeRoutine(PatientAI p, StaffAI d)
    {
        p.WakeUp();

        NavMeshAgent dAgent = d.GetComponent<NavMeshAgent>();
        Vector3 targetPos = p.transform.position;

        if(dAgent != null && dAgent.isActiveAndEnabled && dAgent.isOnNavMesh)
        {
            dAgent.isStopped = false;
            dAgent.SetDestination(targetPos);
        }

        float timeout = 10f;
        while(!HasArrived(dAgent, targetPos) && timeout > 0)
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