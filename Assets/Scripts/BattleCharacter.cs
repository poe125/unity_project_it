//1枚のキャラクターカードの、バトル中の状態を管理
public class BattleCharacter
{
    //JSONからカードの元データを読み取る
    public CharacterCardData data;

    //バフやダメージで変化するステータスを保持
    public int currentHp;
    public int currentAttack;
    public int currentDefense;

    //戦闘開始時に呼び出して、現在値を元データから初期化している
    public BattleCharacter(CharacterCardData baseData)
    {
        data = baseData;
        currentHp = baseData.hp;
        currentAttack = baseData.attack;
        currentDefense = baseData.defense;
    }

    //HPが0以下かどうか
    public bool IsDead => currentHp <= 0;
}