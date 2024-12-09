using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class NodeGrabMovementProvider : MonoBehaviour, IMovementProvider
{
    private NodeGrabMovement movement;

    void Start()
    {
        // CreateMovement で NodeGrabMovement インスタンスを作成
        movement = (NodeGrabMovement)CreateMovement();
    }

    void Update()
    {
        bool isRightPressed = true; // テスト用の例

        if (isRightPressed)
        {
            MoveBackward(0.1f * Time.deltaTime); // 後ろに動かす
        }

        // オブジェクトの位置を更新
        transform.position = movement.Pose.position;
        transform.rotation = movement.Pose.rotation;
    }

    public IMovement CreateMovement()
    {
        return new NodeGrabMovement();
    }

    public void MoveBackward(float distance)
    {
        // 現在の Pose を取得して、後方（Z軸のマイナス方向）に移動
        Pose currentPose = movement.Pose;
        Vector3 newPosition = currentPose.position - transform.forward * distance;
        Pose newPose = new Pose(newPosition, currentPose.rotation);

        // 新しい Pose を適用
        movement.MoveTo(newPose);
    }

    public class NodeGrabMovement : IMovement
    {
        public Pose Pose { get; private set; } = Pose.identity;

        public bool Stopped => true;

        public void StopMovement()
        {
        }

        public void MoveTo(Pose target)
        {
            Pose = target;
        }

        public void UpdateTarget(Pose target)
        {
            Pose = target;
        }

        public void StopAndSetPose(Pose source)
        {
            Pose = source;
        }

        public void Tick()
        {
        }
    }
}