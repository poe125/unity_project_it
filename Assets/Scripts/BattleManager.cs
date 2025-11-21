using UnityEngine;
using System.Collections;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("データベース読み込み")]
    public CardDataLoader dataLoader;   // CardDataLoader をアタッチ

    // バトル中のキャラ
    private BattleCharacter player;
    private BattleCharacter enemy;

    // 先攻・後攻
    private bool isPlayerTurn = true;

    // UI への参照
    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI enemyHpText;

    void Start()
    {
        // テスト用に同じキャラで戦わせる
        SetupBattle("phenix_001", "phenix_001");

        // オートバトル開始（コンソールでログ確認用）
        StartCoroutine(AutoBattleTest());

    }

    //HPのUI更新
    void UpdateHpUI()
    {
        if (player != null && enemy != null)
        {
            playerHpText.text = $"Player HP: {player.currentHp}";
            enemyHpText.text = $"Enemy HP: {enemy.currentHp}";
        }
    }

    /// バトル開始時に呼ぶ：プレイヤー・敵キャラをセット
    public void SetupBattle(string playerCharId, string enemyCharId)
    {
        var pData = dataLoader.GetCharacterById(playerCharId);
        var eData = dataLoader.GetCharacterById(enemyCharId);

        if (pData == null || eData == null)
        {
            Debug.LogError("キャラIDが見つかりません．player:" + playerCharId + " enemy:" + enemyCharId);
            return;
        }

        player = new BattleCharacter(pData);
        enemy = new BattleCharacter(eData);

        // 先攻はとりあえずプレイヤーにしておく
        isPlayerTurn = true;

        Debug.Log($"バトル開始！ プレイヤー:{player.data.name} HP:{player.currentHp} vs 敵:{enemy.data.name} HP:{enemy.currentHp}");

        UpdateHpUI();
    }

    IEnumerator AutoBattleTest()
    {
        // 両方生きている間は交互に殴り合う
        while (!player.IsDead && !enemy.IsDead)
        {
            if (isPlayerTurn)
            {
                // プレイヤーのターン
                PlayerUseAttack("fire_001");  // 攻撃カードIDはお好みで
            }
            else
            {
                // 敵のターン
                EnemyUseAttack("fire_001");
            }

            // 1秒待ってから次のターンへ（速くしたければ0.5fとかに）
            yield return new WaitForSeconds(1f);
        }

        // ここに来たときにはどちらかが死んでいる
        if (player.IsDead && enemy.IsDead)
        {
            Debug.Log("引き分け！（両者戦闘不能）");
        }
        else if (player.IsDead)
        {
            Debug.Log("バトル終了：敵の勝ち");
        }
        else
        {
            Debug.Log("バトル終了：プレイヤーの勝ち");
        }
    }

    /// プレイヤーが攻撃カードを使う（UIボタンやARから呼ぶ想定）
    public void PlayerUseAttack(string attackCardId)
    {
        if (!isPlayerTurn)
        {
            Debug.Log("今はプレイヤーのターンではありません．");
            return;
        }

        var attackCard = dataLoader.GetAttackById(attackCardId);
        if (attackCard == null)
        {
            Debug.LogError("攻撃カードが見つかりません: " + attackCardId);
            return;
        }

        DoAttack(player, enemy, attackCard);

        if (enemy.IsDead)
        {
            Debug.Log("敵を倒した！ プレイヤーの勝ち！");
            return;
        }

        EndTurn();
        // ここで敵ターンの処理（AI行動）につなげてもいい
    }

    /// <summary>
    /// プレイヤーがバフカードを使う（UI/ARから呼ぶ）
    /// </summary>
    public void PlayerUseBuff(string buffCardId)
    {
        if (!isPlayerTurn)
        {
            Debug.Log("今はプレイヤーのターンではありません．");
            return;
        }

        var buffCard = dataLoader.GetBuffById(buffCardId);
        if (buffCard == null)
        {
            Debug.LogError("バフカードが見つかりません: " + buffCardId);
            return;
        }

        ApplyBuff(player, buffCard);

        EndTurn();
    }

    /// <summary>
    /// 攻撃処理：自分の攻撃力 × 攻撃カード倍率 − 相手防御
    /// </summary>
    private void DoAttack(BattleCharacter attacker, BattleCharacter defender, AttackCardData attackCard)
    {
        // ダメージ計算
        float raw = attacker.currentAttack * attackCard.attack_rate - defender.currentDefense;
        int damage = Mathf.Max(0, Mathf.RoundToInt(raw));   // マイナスは0に

        defender.currentHp -= damage;

        UpdateHpUI();

        Debug.Log($"{attacker.data.name} の {attackCard.name}！ {defender.data.name} に {damage} ダメージ！ (残りHP {defender.currentHp})");
    }

    /// <summary>
    /// バフ処理
    /// </summary>
    private void ApplyBuff(BattleCharacter target, BuffCardData buffCard)
    {
        bool hasBuff = false;

        if (buffCard.add_attack != 0)
        {
            target.currentAttack += buffCard.add_attack;
            Debug.Log($"{target.data.name} の攻撃力が {buffCard.add_attack} 上がった！ 現在攻撃力:{target.currentAttack}");
            hasBuff = true;
        }

        if (buffCard.add_defense != 0)
        {
            target.currentDefense += buffCard.add_defense;
            Debug.Log($"{target.data.name} の防御力が {buffCard.add_defense} 上がった！ 現在防御力:{target.currentDefense}");
            hasBuff = true;
        }

        if (buffCard.add_HP != 0)
        {
            target.currentHp += buffCard.add_HP;
            // 回復は最大HPを超えないようにしたければ↓
            // target.currentHp = Mathf.Min(target.currentHp, target.data.hp);

            Debug.Log($"{target.data.name} のHPが {buffCard.add_HP} 回復！ 現在HP:{target.currentHp}");
            hasBuff = true;
        }

        UpdateHpUI();

        if (!hasBuff)
        {
            Debug.LogWarning("このバフカードには効果値が設定されていません．");
        }
    }

    /// <summary>
    /// ターン終了処理
    /// </summary>
    private void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        Debug.Log(isPlayerTurn ? "プレイヤーのターン" : "敵のターン");

        // ここで敵AIの行動を呼ぶなど
        // EnemyTurnAction();
    }

    // 敵ターンの例（とりあえず攻撃カード1枚固定で殴るなど）
    public void EnemyUseAttack(string attackCardId)
    {
        if (isPlayerTurn)
        {
            Debug.Log("今は敵のターンではありません．");
            return;
        }

        var attackCard = dataLoader.GetAttackById(attackCardId);
        if (attackCard == null)
        {
            Debug.LogError("攻撃カードが見つかりません: " + attackCardId);
            return;
        }

        DoAttack(enemy, player, attackCard);

        if (player.IsDead)
        {
            Debug.Log("プレイヤーは倒れた…… 敵の勝ち！");
            return;
        }

        EndTurn();
    }
}