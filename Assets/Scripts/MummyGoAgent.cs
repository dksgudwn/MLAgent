using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MummyGoAgent : Agent
{
    public Material goodMat;
    public Material badMat;
    public Material originMat;
    private Renderer floorRenderer;

    public Transform Target;
    private Rigidbody rigidbody;
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        originMat = floorRenderer.material;
    }

    public override void OnEpisodeBegin()
    {
        rigidbody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-2, 2), 0.05f, Random.Range(-2, 2));
        Target.transform.localPosition = new Vector3(Random.Range(-4, 4), 0.05f, Random.Range(-4, 4));
        StartCoroutine(RecoverFloor());
    }

    private IEnumerator RecoverFloor()
    {
        yield return new WaitForSeconds(0.2f);
        floorRenderer.material = originMat;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.transform.position);
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rigidbody.velocity.x);
        sensor.AddObservation(rigidbody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var ContinuousActions = actions.ContinuousActions;
        Vector3 direction = (Vector3.forward * ContinuousActions[0]) + Vector3.right * ContinuousActions[1];
        direction.Normalize();
        rigidbody.AddForce(direction * 50);

        SetReward(-0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ContinuosActionsOut = actionsOut.ContinuousActions;
        ContinuosActionsOut[0] = Input.GetAxis("Vertical");
        ContinuosActionsOut[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Target"))
        {
            floorRenderer.material = goodMat;
            SetReward(1f);
            EndEpisode();
        }
        if (collision.collider.CompareTag("Wall"))
        {
            floorRenderer.material = badMat;
            SetReward(-1f);
            EndEpisode();
        }
    }
}
