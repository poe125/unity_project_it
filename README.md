# Monster Summoner

To play this game, please clone this repo and open it in Unity. (version 6000.2.12f)
Monster Summoner is a turn-based card battle game that uses AR (Augmented Reality).
When the player scans a physical card with an Android smartphone, characters and attacks appear as 3D models in AR space, and the player can view stats such as HP and attack power.
The game consists of the following three card types:
- Character Cards
- Attack Cards
- Buff Cards (Attack Up / Defense Up / HP Recovery)
Players combine these cards to build strategies and fight battles.

## 概要
**Monster Summoner** は、AR（拡張現実）を用いたターン制カードバトルゲームです。  
物理カードを Android スマートフォンで読み取ると、キャラクターや攻撃が 3D モデルとして AR 上に表示され、HP や攻撃力などのステータスを確認できます。

本ゲームは以下の 3 種類のカードで構成されています：

- キャラクターカード
- 攻撃カード
- バフカード（攻撃力UP / 防御力UP / HP回復）

それらを組み合わせて戦略的にバトルを進めます。

---

## ゲームルール

### 1. バトル開始
- プレイヤーは物理カードから 2 体のキャラクターを選び、場に出します。
  - Player1 のキャラクター
  - Player2 のキャラクター

### 2. ターン制バトル
- プレイヤーは交互にターンを行います。
- 1ターンにつき 1 回の行動が可能です。

### 3. 選べる行動
- 攻撃
- 攻撃力バフ
- 防御力バフ
- HP回復

### 4. 攻撃計算式

ダメージ = （キャラの基礎攻撃力 + 攻撃カードの攻撃力）× 2（有利属性の場合） − 相手キャラの防御力

### 5. 勝敗条件
- どちらかのキャラクターの HP が 0 になった時点で敗北。

---

## 使用技術・開発環境
- Unity
- AR Foundation
- Google ARCore XR Plugin
- C#（バトル管理ロジック）
- JSON（カードデータベース）
- Figma（カードデザイン）

---

## カードデータベース仕様（JSON）
カードは **カードID（ImageID = DataID）** で管理し、  
JSON データベースから ID をキーにして情報を取得します。

### キャラクターカード
- キャラ名
- HP
- 基礎攻撃力
- 基礎防御力
- 属性

### 攻撃カード
- 攻撃名
- 攻撃力

### バフカード
- バフ名
- バフ効果
  - 攻撃力UP：追加攻撃力（基礎攻撃力に加算）
  - 防御力UP：追加防御力（基礎防御力に可算）
  - 回復：HP の回復量
  
