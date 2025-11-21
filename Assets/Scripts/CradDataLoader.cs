using UnityEngine;

//カード辞典
public class CardDataLoader : MonoBehaviour
{
    public CardDatabase database; // cardsdata.cs の外枠クラス

    void Awake()
    {
        Load();
    }

    public void Load()
    {
        //carddata.jsonを読み込んで、unity内のTextAssetに変換する
        TextAsset json = Resources.Load<TextAsset>("carddata"); 
        if (json == null)
        {
            Debug.LogError("carddata.json が Resources にありません");
            return;
        }

        //JSONをcardsdata.csで作った型にまるごと変換する
        database = JsonUtility.FromJson<CardDatabase>(json.text);
        Debug.Log("データ読み込み完了：カード数 " + (database.characters.Length + database.attack.Length + database.buff.Length));
    }

    //IDでキャラクターカード検索
    public CharacterCardData GetCharacterById(string id)
    {
        foreach (var c in database.characters)
            if (c.id == id) return c;
        return null;
    }

    //IDで攻撃カード検索
    public AttackCardData GetAttackById(string id)
    {
        foreach (var a in database.attack)
            if (a.id == id) return a;
        return null;
    }

    //IDでバフカード検索
    public BuffCardData GetBuffById(string id)
    {
        foreach (var b in database.buff)
            if (b.id == id) return b;
        return null;
    }
}