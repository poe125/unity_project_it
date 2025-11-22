[System.Serializable]
public class CharacterCardData {
    public string id;
    public string name;
    public int hp;
    public int attack;
    public int defense;
    public string imagekey;
    public string owner; // "player1" or "player2"
    public string attribute;
}

[System.Serializable]
public class AttackCardData {
    public string id;
    public string name;
    public float attack_rate;
    public string attribute;
    public string owner; // "player1" or "player2"
}

[System.Serializable]
public class BuffCardData {
    public string id;
    public string name;
    public int add_attack;
    public int add_defense;
    public int add_HP;
    public string owner; // "player1" or "player2"
}

[System.Serializable]
public class CardDatabase {
    public CharacterCardData[] characters;
    public AttackCardData[] attack;
    public BuffCardData[] buff;
}
