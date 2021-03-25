# GameOfLife

ライフゲームをJobsystemとVisual Scriptingで実装したサンプル

![b65938e4eef0332106149d75e17b3489](https://user-images.githubusercontent.com/29646672/112272214-3de51e80-8cbf-11eb-80cf-0b6ebc587438.gif)

## 動作確認環境

Unity2021.1.0b11
(Visual Scriptを使用している為、Unity2021以降となっていますが、Jobsystemに関してはそれ以前のバージョンでも実行可能です。)

## 実行方法

Assets/Scenesフォルダ以下にある各Sceneを開いて実行して下さい。

- [Jobsystem.unity](https://github.com/katsumasa/GameOfLife/blob/main/Assets/Scenes/Jobsystem.unity)
- [VisualScripting.unity](https://github.com/katsumasa/GameOfLife/blob/main/Assets/Scenes/VisualScripting.unity)
- [CS.unity](https://github.com/katsumasa/GameOfLife/blob/main/Assets/Scenes/CS.unity)

Visual Scriptingは極端にパフォーマンスが悪く、UnityEditor上で実行する場合は、cellMapのサイズを30x30未満にすることをお勧めします。(cellMapのサイズを100x100にした際、CPUがIntel(R) Core(TM) i7-10875Hの場合でもUnityEditorがフリーズしました。)

### 参考URL

https://ja.wikipedia.org/wiki/%E3%83%A9%E3%82%A4%E3%83%95%E3%82%B2%E3%83%BC%E3%83%A0

