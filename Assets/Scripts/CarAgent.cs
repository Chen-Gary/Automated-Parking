using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using static CarController;

public class CarAgent : BaseAgent
{
    private Vector3 originalPosition;

    private BehaviorParameters behaviorParameters;

    private CarControllerNew carController;

    private Rigidbody carControllerRigidBody;

    private CarSpots carSpots;

    public override void Initialize()
    {
        originalPosition = transform.localPosition;
        behaviorParameters = GetComponent<BehaviorParameters>();
        carController = GetComponent<CarControllerNew>();
        carControllerRigidBody = GetComponent<Rigidbody>();
        carSpots = transform.parent.GetComponentInChildren<CarSpots>();

        ResetParkingLotArea();    
    }

    public override void OnEpisodeBegin()
    {
        ResetParkingLotArea();
    }

    private void ResetParkingLotArea()
    {
        // important to set car to automonous during default behavior
        //carController.IsAutonomous = behaviorParameters.BehaviorType == BehaviorType.Default;
        transform.localPosition = originalPosition;
        transform.localRotation = Quaternion.identity;
        carControllerRigidBody.velocity = Vector3.zero;
        carControllerRigidBody.angularVelocity = Vector3.zero;

        // reset which cars show or not show
        carSpots.Setup();
    }

    void Update()
    {
        if(transform.localPosition.y <= 0)
        {
            TakeAwayPoints();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation);

        sensor.AddObservation(carSpots.CarGoal.transform.position);
        sensor.AddObservation(carSpots.CarGoal.transform.rotation);

        sensor.AddObservation(carControllerRigidBody.velocity);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        carController.SetInput(
            actionBuffers.ContinuousActions[0],
            actionBuffers.ContinuousActions[1],
            actionBuffers.DiscreteActions[0]);

        AddReward(-1f / MaxStep);
    }

    public void GivePoints(float amount = 1.0f, bool isFinal = false)
    {
        AddReward(amount);

        if(isFinal)
        {
            StartCoroutine(SwapGroundMaterial(successMaterial, 0.5f));

            EndEpisode();
        }
    }

    public void TakeAwayPoints()
    {
        StartCoroutine(SwapGroundMaterial(failureMaterial, 0.5f));

        AddReward(-0.01f);
        
        EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
