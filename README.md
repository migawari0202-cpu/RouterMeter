# RouterMeter

OpenRouterの利用状況をブラウザを開かずに確認できる、右下常駐型のWPFミニウィジェット。

## セットアップ

1. Windows + .NET 8 SDK がインストールされていること
2. このフォルダで以下を実行してビルド

   ```
   dotnet build
   ```

3. `bin/Debug/net8.0-windows/` にコピーされた `config.example.json` を `config.json` にリネームし、
   `ApiKey` に自分のAPIキー（`sk-or-...`）を設定
4. `dotnet run` または生成された `RouterMeter.exe` を実行

初回起動時に `config.json` が無い場合は雛形を自動生成して終了します（APIキー未設定のまま動かさないための挙動です）。

## 使っているAPIとその理由

`GET https://openrouter.ai/api/v1/auth/key` のみを使用しています。

- 通常のAPIキー（`sk-or-...`）でそのまま呼べる
- `usage`（累計消費額, USD）が既に計算済みで返ってくるので、トークン単価を自前で計算する必要がない
- `limit`（キーの上限）や `is_free_tier` も同じ呼び出しで取得できる

`GET /api/v1/credits` と `GET /api/v1/activity`（日別集計）は **Provisioning key（管理キー）専用**で、通常のAPIキーを渡すと `403 Only management keys can perform this operation` になります。今回は通常キーのみを前提にしているため、この2つは使っていません。

## 「本日の消費」をどう作っているか

OpenRouterのAPIは累計usageしか返さず、当日分だけを取り出す仕組みがありません
（`/api/v1/activity` も管理キー専用の上、"完了したUTC日"しか返さないため、そもそも進行中の当日は対象外です）。

そのため `daily_state.json`（実行ファイルと同じフォルダに自動生成）にローカル日付ベースで
「その日最初に観測したusage」をベースラインとして保存し、

```
本日の消費 = 現在のusage − その日のベースラインusage
```

で算出しています。日付判定はローカル時刻基準です。UTC基準にしたい場合は
`Services/DailyUsageTracker.cs` の `DateTime.Now` を `DateTime.UtcNow` に変更するだけで切り替えられます。

`Today Requests` はAPIから正確な件数が取れないため、「ポーリング間でusageが増えた回数」を
近似値としてカウントしています（短時間に複数リクエストが走ると実際より少なく出ます）。
正確な件数が必要になったら、管理キー＋`/api/v1/activity` を使った実装への切り替えが必要です。

## 構成（MVVM）

```
Models/      設定・APIレスポンス・永続化状態などのデータ構造
Services/    ConfigService（設定読込）/ OpenRouterApiService（API通信）/ DailyUsageTracker（当日消費の算出）
ViewModels/  MainViewModel（ポーリング・状態計算・表示用文字列の組み立て）
Converters/  Status(enum) → 色・文字列への変換（View用）
Controls/    PieChartControl（円弧描画。色分けロジックはこのコントロール内に閉じている）
MainWindow.* Viewとウィンドウ配置（右下固定・ドラッグ移動・右クリックで終了）のみ
```

ViewModelはAPI/永続化サービスを直接呼び出すシンプルな構成にしています
（DIコンテナは今回のスコープでは過剰と判断し未導入）。

## 今回あえて実装しなかったもの / 仕様からの補足

- 設定画面・保存ボタンなどは仕様通り未実装。ただしタイトルバーが無いとウィンドウを閉じる手段が無いため、
  右クリックメニューに「終了」のみ追加しています。
- オフライン時は最終取得値を表示したまま `API Status: Offline` に切り替わり、
  次回のタイマーTickで自動的に再取得を試みます（明示的なリトライボタンなし）。

## 将来の拡張ポイント（仕様書の「今後追加予定」に対応）

- 月間利用額・モデル別利用額・RPM/TPM → 管理キー対応 + `/api/v1/activity` 呼び出しを追加
- タスクトレイ常駐 → `App.xaml.cs` に `NotifyIcon` 相当の仕組みを追加
- 通知 / ダークモード切替 / 複数APIキー → `AppConfig` にフィールド追加 + 対応するサービス/ViewModelの拡張

いずれも `Models` / `Services` を増やす形で、既存のMVVM構成を崩さずに追加できるようにしています。
