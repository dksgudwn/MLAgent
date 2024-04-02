using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MummyRayAgent : Agent
{
    public float moveSpeed = 30f;
    public float turnSpeed = 100f;

    public Material originMat;
    public Material goodMat;
    public Material badMat;
    private Renderer floorRenderer;

    public ItemSpawner itemSpawner;

    public override void Initialize()
    {
        itemSpawner = GetComponentInParent<ItemSpawner>();
        floorRenderer = transform.parent.Find("Floor").GetComponent<Renderer>();
        originMat = floorRenderer.material;
    }

    public override void OnEpisodeBegin()
    {
        itemSpawner.SpawnItems();
        transform.localPosition = new Vector3(Random.Range(-22, 22), 0.05f, Random.Range(-22f, 22));
        transform.localRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // DiscreteActions 이산적인 행동 : 정해진 개수의 선택지 중에서 행동 선택
        var DiscreteActions = actions.DiscreteActions;
        Vector3 dir = Vector3.zero;
        Vector3 rot = Vector3.zero;

        switch (DiscreteActions[0])
        {
            case 0:
                dir = Vector3.zero;
                break;
            case 1:
                dir = transform.forward;
                break;
            case 2:
                dir = -transform.forward;
                break;
        }

        switch (DiscreteActions[1])
        {
            case 0:
                rot = Vector3.zero;
                break;
            case 1:
                rot = Vector3.down;
                break;
            case 2:
                rot = Vector3.up;
                break;
        }

        transform.Rotate(rot, turnSpeed * Time.fixedDeltaTime);
        transform.localPosition += dir * Time.fixedDeltaTime * moveSpeed;

        AddReward(-1f / MaxStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var DiscreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W))
        {
            DiscreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            DiscreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            DiscreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            DiscreteActionsOut[1] = 2;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("GoodItem"))
        {
            Destroy(collision.gameObject);
            StartCoroutine(ChangeFloorColor(goodMat));
            AddReward(1);
        }
        if (collision.collider.CompareTag("BadItem"))
        {
            AddReward(-1);
            StartCoroutine(ChangeFloorColor(badMat));
            EndEpisode();
        }
        if (collision.collider.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            StartCoroutine(ChangeFloorColor(badMat));
            EndEpisode();
        }
    }

    IEnumerator ChangeFloorColor(Material m)
    {
        floorRenderer.material = m;
        yield return new WaitForSeconds(0.2f);
        floorRenderer.material = originMat;
    }
}