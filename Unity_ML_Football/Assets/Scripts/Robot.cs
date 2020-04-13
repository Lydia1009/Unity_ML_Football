using UnityEngine;
using UnityEngine.UI;
using MLAgents;
using MLAgents.Sensors;

public class Robot : Agent
{
    [Header("速度"), Range(1, 50)]
    public float speed = 10;

    /// <summary>
    /// 機器人剛體
    /// </summary>
    private Rigidbody rigRobot;
    /// <summary>
    /// 足球剛體
    /// </summary>
    private Rigidbody rigBall;
    /// <summary>
    /// 動畫控制器
    /// </summary>
    private Animator ani;
    /// <summary>
    /// 訊息文字介面
    /// </summary>
    private Text textMessage;
    /// <summary>
    /// 訓練次數
    /// </summary>
    private int countTotal;
    /// <summary>
    /// 成功次數
    /// </summary>
    private int countComplete;

    private void Start()
    {
        ani = GetComponent<Animator>();
        rigRobot = GetComponent<Rigidbody>();
        rigBall = GameObject.Find("足球").GetComponent<Rigidbody>();
        textMessage = GameObject.Find("訊息").GetComponent<Text>();
    }

    /// <summary>
    /// 事件開始時：重新設定機器人與足球位置
    /// </summary>
    public override void OnEpisodeBegin()
    {
        UpdateTextMessage();

        // 重設剛體加速度與角度加速度
        rigRobot.velocity = Vector3.zero;
        rigRobot.angularVelocity = Vector3.zero;
        rigBall.velocity = Vector3.zero;
        rigBall.angularVelocity = Vector3.zero;

        // 隨機機器人位置
        Vector3 posRobot = new Vector3(Random.Range(-1f, 1f), 0.1f, Random.Range(-1f, 0f));
        transform.position = posRobot;
        
        // 隨機足球位置
        Vector3 posBall = new Vector3(Random.Range(-0.5f, 0.5f), 0.1f, Random.Range(1f, 1.5f));
        rigBall.position = posBall;

        // 足球尚未進入球門
        Ball.complete = false;
    }

    /// <summary>
    /// 收集觀測資料
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // 加入觀測資料：機器人、足球座標、機器人加速度 X、Z
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rigBall.position);
        sensor.AddObservation(rigRobot.velocity.x);
        sensor.AddObservation(rigRobot.velocity.z);
    }

    /// <summary>
    /// 動作：控制機器人與回饋
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        // 使用參數控制機器人
        Vector3 control = Vector3.zero;
        control.x = vectorAction[0];
        control.z = vectorAction[1];
        rigRobot.AddForce(control * speed);
        ani.SetBool("跑步開關", rigRobot.velocity.magnitude > 0.1f);

        // 球進入球門，成功：加 1 分並結束
        if (Ball.complete)
        {
            countTotal++;
            countComplete++;
            UpdateTextMessage();
            SetReward(1);
            EndEpisode();
        }

        // 機器人或足球掉到地板下方，失敗：扣 1 分並結束
        if (transform.position.y < 0 || rigBall.position.y < 0)
        {
            countTotal++;
            SetReward(-1);
            EndEpisode();
        }
    }

    /// <summary>
    /// 探索：讓開發者測試環境
    /// </summary>
    /// <returns></returns>
    public override float[] Heuristic()
    {
        // 提供開發者控制的方式
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "足球")
        {
            SetReward(0.1f);
        }
    }

    /// <summary>
    /// 更新訊息文字介面
    /// </summary>
    private void UpdateTextMessage()
    {
        textMessage.text = "測試次數：" + countTotal + "\n成功次數：" + countComplete + "\n成功機率：" + ((float)countComplete / countTotal * 100).ToString("F0") + "%";
    }
}
