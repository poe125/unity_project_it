using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [Header("参照")]
    public CardDataLoader dataLoader;
    public ARCardInput arCardInput; // ターン終了時にフラグをリセットするために必要

    [Header("UI")]
    public TextMeshProUGUI player1HpText;
    public TextMeshProUGUI player2HpText;
    public TextMeshProUGUI infoText; // ターン情報などを表示

    [Header("プレハブ")]
    public GameObject player1Prefab; // Player1用の生成モデル
    public GameObject player2Prefab; // Player2用の生成モデル

    // バトル中のステータス管理
    private BattleCharacter player1;
    private BattleCharacter player2;

    // 生成されたオブジェクトの管理
    private GameObject player1GO;
    private GameObject player2GO;

    // 状態フラグ
    public bool Player1Set { get; private set; } = false;
    public bool Player2Set { get; private set; } = false;
    public bool Player1Acted { get; private set; } = false;
    public bool Player2Acted { get; private set; } = false;

    private bool isBattleOver = false;

    private int turnNumber = 1;

    void Start()
    {
        UpdateUI("Scan cards to summon characters.");
    }

    // ============================================================
    // キャラクター召喚
    // ============================================================
    public void SetPlayer1Card(string id, Vector3 pos, Quaternion rot)
    {
        if (Player1Set) return; // すでに召喚済みなら無視

        var data = dataLoader.GetCharacterById(id);
        if (data == null) return;

        player1 = new BattleCharacter(data);
        Player1Set = true;

        // モデル生成
        if (player1Prefab != null)
            player1GO = Instantiate(player1Prefab, pos, rot);
        
        UpdateUI("P1 SUMMONEDED");
        UpdateHpUI();
    }

    public void SetPlayer2Card(string id, Vector3 pos, Quaternion rot)
    {
        if (Player2Set) return;

        var data = dataLoader.GetCharacterById(id);
        if (data == null) return;

        player2 = new BattleCharacter(data);
        Player2Set = true;

        if (player2Prefab != null)
            player2GO = Instantiate(player2Prefab, pos, rot);

        UpdateUI("P2 SUMMONDED");
        UpdateHpUI();
    }

    // ============================================================
    // 攻撃アクション
    // ============================================================
    public void Player1UseAttack(string key)
    {
        if (!CanAct(isPlayer1: true)) return;

        var atk = dataLoader.GetAttackById(key);
        if (atk == null) return;

        DoAttack(player1, player2, atk);
        Player1Acted = true;
        UpdateUI($"P1 ATTACK:{atk.name}");
        CheckEndOfTurn();
    }

    public void Player2UseAttack(string key)
    {
        if (!CanAct(isPlayer1: false)) return;

        var atk = dataLoader.GetAttackById(key);
        if (atk == null) return;

        DoAttack(player2, player1, atk);
        Player2Acted = true;
        UpdateUI($"P2 Attack: {atk.name}");
        CheckEndOfTurn();
    }

    // ============================================================
    // バフアクション
    // ============================================================
    public void Player1UseBuff(string key)
    {
        if (!CanAct(isPlayer1: true)) return;

        var buff = dataLoader.GetBuffById(key);
        if (buff == null) return;

        ApplyBuff(player1, buff);
        Player1Acted = true;
        UpdateUI($"P1 BUFF: {buff.name}");
        CheckEndOfTurn();
    }

    public void Player2UseBuff(string key)
    {
        if (!CanAct(isPlayer1: false)) return;

        var buff = dataLoader.GetBuffById(key);
        if (buff == null) return;

        ApplyBuff(player2, buff);
        Player2Acted = true;
        UpdateUI($"P2 BUFF: {buff.name}");
        CheckEndOfTurn();
    }

    // ============================================================
    // 内部処理
    // ============================================================
    
    // 行動可能かどうかの判定
    private bool CanAct(bool isPlayer1)
    {
        //バトルが終わっていたら行動不可
        if (isBattleOver) 
        {
            Debug.Log("バトルは終了しています");
            return false; 
        }

        if (!Player1Set || !Player2Set) 
        {
            // 戦闘開始前
            return false;
        }
        
        if (isPlayer1 && Player1Acted) return false; // すでに行動済み
        if (!isPlayer1 && Player2Acted) return false;

        return true;
    }

    // ============================================================
    // ★変更箇所: 攻撃処理 (属性計算を追加)
    // ============================================================
    private void DoAttack(BattleCharacter attacker, BattleCharacter defender, AttackCardData atk)
    {
        // 1. 属性倍率を取得 (攻撃カードの属性 vs 防御キャラの属性)
        float attributeMultiplier = GetAttributeMultiplier(atk.attribute, defender.data.attribute);

        // 2. 攻撃力計算
        // 基本攻撃力(バフ込み) × カード倍率
        float basePower = attacker.currentAttack * atk.attack_rate;

        // 3. 属性倍率を適用
        float powerWithAttribute = basePower * attributeMultiplier;

        // 4. 最終ダメージ計算 (防御力を引く)
        // 数式: ( (Base ATK) * Attribute Multiplier ) - Enemy DEF
        int damage = Mathf.Max(0, Mathf.RoundToInt(powerWithAttribute - defender.currentDefense));

        // HP更新
        defender.currentHp -= damage;
        UpdateHpUI();

        // 相手が死んだかチェック
        if (defender.currentHp <= 0)
        {
            // HPを0で止める（マイナス表示にならないように）
            defender.currentHp = 0; 
            UpdateHpUI();

            // ゲーム終了処理へ (勝者は攻撃した側)
            EndBattle(winnerName: attacker.data.owner); 
        }
        else
        {
            // まだ生きているなら、効果抜群などのメッセージを出してターン継続
            if (attributeMultiplier > 1.0f){
                UpdateUI($"Super Effective! Dealt {damage} dmg！");
            }
            else{
                UpdateUI($"{damage} dmg！");
            }

            // ターン終了判定へ
            CheckEndOfTurn(); 
        }

        // ログ表示 (デバッグ用)
        string typeMsg = (attributeMultiplier > 1.0f) ? " <color=red>Super Efective!(x2)!</color>" : "";
        Debug.Log($"Attack: {atk.name} (Attribute:{atk.attribute}) -> Defense: {defender.data.name} (Attribute:{defender.data.attribute})\n" +
                  $"Rate: {attributeMultiplier}, Damage: {damage}{typeMsg}");
        
        if (attributeMultiplier > 1.0f) UpdateUI($"Super Effective！{damage} dmg！");
    }

    // 属性相性の判定ロジック
    private float GetAttributeMultiplier(string attackAttr, string defenseAttr)
    {
        // 属性有利の定義: 炎 > 草 > 水 > 炎 ...
        if (attackAttr == "fire" && defenseAttr == "grass") return 2.0f;
        if (attackAttr == "grass" && defenseAttr == "water") return 2.0f;
        if (attackAttr == "water" && defenseAttr == "fire") return 2.0f;

        // それ以外は等倍 (不利な場合の0.5倍などを入れたい場合はここに追記)
        return 1.0f;
    }

    private void ApplyBuff(BattleCharacter target, BuffCardData buff)
    {
        target.currentAttack += buff.add_attack;
        target.currentDefense += buff.add_defense;
        target.currentHp += buff.add_HP;
        UpdateHpUI();
    }

    private void UpdateHpUI()
    {
        if (player1 != null) player1HpText.text = $"P1 HP: {player1.currentHp}";
        if (player2 != null) player2HpText.text = $"P2 HP: {player2.currentHp}";
    }

    private void UpdateUI(string msg)
    {
        if (infoText != null) infoText.text = msg;
        Debug.Log(msg);
    }

    // ターン終了判定
    private void CheckEndOfTurn()
    {
        if (isBattleOver) return;

        if (Player1Acted && Player2Acted)
        {
            // 両者行動済みならターンリセット
            Player1Acted = false;
            Player2Acted = false;
            turnNumber++;
            
            UpdateUI($"--- Turn {turnNumber} Start ---");

            // ARCardInput側の「1フレーム1回制限」などのフラグもリセットさせる
            if (arCardInput != null)
            {
                arCardInput.ResetActionFlag();
            }
        }
        
    }

    //ゲーム終了処理
    private void EndBattle(string winnerName)
    {
        isBattleOver = true;

        string winMsg = "";
        if (winnerName == "player1")
        {
            winMsg = "P1 WIN"; 
        }
        else
        {
            winMsg = "P2 WIN";
        }

        UpdateUI(winMsg);
        Debug.Log($"GameSet: Winner {winnerName}");
        SceneManager.LoadScene("GameOverScene");
    }
}