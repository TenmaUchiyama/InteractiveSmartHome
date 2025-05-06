using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveToPosition : Singleton<MoveToPosition>
{
   [SerializeField] GameObject cameraRig; 
   [SerializeField] GameObject positionToMove; 
   [SerializeField] OVRScreenFade oVRScreenFade;


  public async UniTask MoveCameraRigAsync()
    {
        // フェードアウトを開始
        oVRScreenFade.FadeOut();

        // フェードアウトが完了するまで待機
        await UniTask.WaitUntil(() => oVRScreenFade.currentAlpha >= 1.0f);

        // cameraRigをpositionToMoveの位置に移動
        cameraRig.transform.SetPositionAndRotation(positionToMove.transform.position, positionToMove.transform.rotation);

        // フェードインを開始
        oVRScreenFade.FadeIn();

        // フェードインが完了するまで待機
        await UniTask.WaitUntil(() => oVRScreenFade.currentAlpha <= 0.0f);
    }


}
