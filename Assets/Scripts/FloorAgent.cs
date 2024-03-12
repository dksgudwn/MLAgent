using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class FloorAgent : Agent
{
    public Transform ballTransform;
    private Rigidbody ballRigidbody;

    //환경이 처음 시작될 때 한번만 실행 Start()와 비슷한 역할
    public override void Initialize()
    {
        ballRigidbody = ballTransform.GetComponent<Rigidbody>();
    }

    //각 에피소드가 시작될 때 호출되는 함수, 환경의 상태를 초기화하는 역할
    public override void OnEpisodeBegin()
    {
        //Floor를 x축, z축 기준으로 무작위하게 살짝 회전시킨다.
        transform.rotation = new Quaternion(0, 0, 0, 0);
        transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));

        //Ball은 Velocity와 위치를 초기화한다.
        ballRigidbody.velocity = new Vector3(0, 0, 0);
        ballTransform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), 1.5f);
    }

    //에이전트의 관측을 설정하는 함수
    public override void CollectObservations(VectorSensor sensor)
    {
        //Vector Observation(벡터 관측) : 값을 통해 관측시키는 것
        //관측되는 값의 개수는 8개이다.
        sensor.AddObservation(transform.rotation.x);
        sensor.AddObservation(transform.rotation.z);
        sensor.AddObservation(ballTransform.transform.position - transform.position);
        sensor.AddObservation(ballRigidbody.velocity);
    }

    //행동값을 이용해서 에이전트의 행동을 설정하는 함수
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Continuous Action(연속적인 행동) : 2개의 값을 사용해서 행동을 설정
        var ContinuousActions = actions.ContinuousActions;
        float z_rotation = Mathf.Clamp(ContinuousActions[0], -1f, 1f);
        float x_rotation = Mathf.Clamp(ContinuousActions[1], -1f, 1f);

        transform.Rotate(new Vector3(0, 0, 1), z_rotation);
        transform.Rotate(new Vector3(1, 0, 0), x_rotation);

        if (ballTransform.position.y - transform.position.y < -2f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (Mathf.Abs(ballTransform.position.x - transform.position.x) > 25f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (Mathf.Abs(ballTransform.position.z - transform.position.z) > 2.5f)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else
        {
            SetReward(0.1f);
        }
    }

    //사람이 수동으로 에이전트의 행동을 제어하는 방범을 설정하는 함수
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var ContinuosActionsOut = actionsOut.ContinuousActions;
        ContinuosActionsOut[0] = -Input.GetAxis("Horizontal");
        ContinuosActionsOut[1] = Input.GetAxis("Vertical");
    }
}
