using UnityEngine;
using MLAgents;
using MLAgents.Sensors;

public class Robot : Agent
{
    [Header("目標物件")]
    public Rigidbody target;
    [Header("速度"), Range(1, 100)]
    public float speed = 10;

    private Rigidbody rig;

    private void Start()
    {
        rig = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rig.angularVelocity = Vector3.zero;
        rig.velocity = Vector3.zero;
        transform.position = new Vector3(0, 0.1f, 0);
        Ball.complete = false;
        target.position = new Vector3(Random.Range(-3f, 3f), 0.2f, Random.Range(1f, 3f));
        target.transform.eulerAngles = Vector3.zero;
        target.velocity = Vector3.zero;
        target.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.position);
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rig.velocity.x);
        sensor.AddObservation(rig.velocity.z);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        Vector3 control = Vector3.zero;
        control.x = vectorAction[0];
        control.z = vectorAction[1];
        rig.AddForce(control * speed);

        if (Ball.complete)
        {
            SetReward(1);
            EndEpisode();
        }

        if (transform.position.y < 0)
        {
            EndEpisode();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
}
