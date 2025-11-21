using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCardInput : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [Header("Game")]
    public BattleManager battleManager;
    public CardDataLoader dataLoader;

    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // 新しく見つかった画像
        foreach (var img in args.added)
        {
            HandleImage(img);
        }

        // すでに検出済みで、位置が更新されたりした画像
        foreach (var img in args.updated)
        {
            HandleImage(img);
        }

        // 見えなくなった画像（とりあえず今回は何もしない）
        // foreach (var img in args.removed) { ... }
    }

    void HandleImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.trackingState != TrackingState.Tracking)
            return;

        // Reference Image Library で付けた Name がここで取れる
        string key = trackedImage.referenceImage.name;
        Debug.Log("検出した画像キー: " + key);

        // ① まず攻撃カードとして探す
        var attack = dataLoader.GetAttackById(key);
        if (attack != null)
        {
            // プレイヤーの攻撃として実行
            battleManager.PlayerUseAttack(key);
            return;
        }

        // ② 見つからなければバフカードとして探す
        var buff = dataLoader.GetBuffById(key);
        if (buff != null)
        {
            battleManager.PlayerUseBuff(key);
            return;
        }

        // ③ それでもなければキャラクターカードかもしれない
        //    （キャラ選択に使いたければここで処理）
        var character = dataLoader.GetCharacterById(key);
        if (character != null)
        {
            Debug.Log("キャラクターカードを検出: " + character.name);
            // ここで「このキャラをプレイヤーにセット」なども後で書ける
            return;
        }

        Debug.LogWarning("キー " + key + " に対応するカードが見つかりませんでした");
    }
}