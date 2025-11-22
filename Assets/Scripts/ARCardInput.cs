using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARCardInput : MonoBehaviour
{
    [Header("AR Components")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Game Components")]
    public BattleManager battleManager;
    public CardDataLoader dataLoader;

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // 追加された画像と更新された画像の両方をチェック
        ProcessImages(args.added);
        ProcessImages(args.updated);
    }

    void ProcessImages(IEnumerable<ARTrackedImage> images)
    {
        foreach (var img in images)
        {
            // 認識状態が Tracking (安定して認識中) の時のみ処理
            if (img.trackingState == TrackingState.Tracking)
            {
                HandleFoundImage(img);
            }
        }
    }

    void HandleFoundImage(ARTrackedImage img)
    {
        string cardId = img.referenceImage.name;

        // ========================================================
        // 1. キャラクター召喚処理
        // ========================================================
        // まだデータ読み込み前やマネージャー設定漏れがあれば中断
        if (dataLoader == null || battleManager == null) return;

        var charData = dataLoader.GetCharacterById(cardId);
        if (charData != null)
        {
            // Player1のカードで、まだPlayer1がいなければセット
            if (charData.owner == "player1" && !battleManager.Player1Set)
            {
                battleManager.SetPlayer1Card(cardId, img.transform.position, img.transform.rotation);
            }
            // Player2のカードで、まだPlayer2がいなければセット
            else if (charData.owner == "player2" && !battleManager.Player2Set)
            {
                battleManager.SetPlayer2Card(cardId, img.transform.position, img.transform.rotation);
            }
            // キャラクターカードの場合はここで処理終了
            return;
        }

        // ========================================================
        // 2. 戦闘アクション処理 (攻撃・バフ)
        // ========================================================
        
        // 両方のプレイヤーが揃っていないとバトルアクションは不可
        if (!battleManager.Player1Set || !battleManager.Player2Set) return;

        // 攻撃カードかチェック
        var attackData = dataLoader.GetAttackById(cardId);
        if (attackData != null)
        {
            // Player1のカード → Player1が未行動なら実行
            if (attackData.owner == "player1" && !battleManager.Player1Acted)
            {
                battleManager.Player1UseAttack(cardId);
            }
            // Player2のカード → Player2が未行動なら実行
            else if (attackData.owner == "player2" && !battleManager.Player2Acted)
            {
                battleManager.Player2UseAttack(cardId);
            }
            return;
        }

        // バフカードかチェック
        var buffData = dataLoader.GetBuffById(cardId);
        if (buffData != null)
        {
            if (buffData.owner == "player1" && !battleManager.Player1Acted)
            {
                battleManager.Player1UseBuff(cardId);
            }
            else if (buffData.owner == "player2" && !battleManager.Player2Acted)
            {
                battleManager.Player2UseBuff(cardId);
            }
            return;
        }
    }

    // BattleManagerから呼ばれるが、入力制限フラグを廃止したので中身は空でOK
    // (削除するとBattleManager側でエラーになるため残しておく)
    public void ResetActionFlag()
    {
        // 処理なし
    }
}