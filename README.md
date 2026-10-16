# BookingPractice 訂位系統

練習「併發控制」的餐廳訂位系統

## 技術

- 後端：ASP.NET Core 8 Web API、EF Core、SQL Server、Serilog
- 前端：Vue 3、Vite、Pinia、Axios、Tailwind CSS
- 測試：xUnit 整合測試（`WebApplicationFactory` + 獨立測試資料庫）、並行測試小程式（ConcurrencyTest）

## 專案結構

```
BookingPractice/
├─ BookingPracticeAPI/        後端 Web API
├─ BookingPracticeClient/     前端 Vue 專案
├─ BookingPractice.Tests/     xUnit 整合測試
└─ ConcurrencyTest/           命令列並行測試小程式
```

## 功能

- 桌位管理：新增、查詢、停用
- 訂位：新增、查詢（含分頁與篩選）、取消、查詢可訂時段
- 取消訂位時寫入異動紀錄（BookingLog）

## 併發處理（核心）

| 問題 | 解法 | 位置 |
|---|---|---|
| 多人同時搶同一桌同一時段 | 唯一索引（篩選索引：僅限預約成功） + 捕捉錯誤碼 2601/2627 | 資料庫 + `CreateBooking` |
| 多人同時修改同一筆訂位 | 樂觀鎖（`RowVersion`） + 捕捉 `DbUpdateConcurrencyException` | `CancelBooking` |
| 重複取消、狀態不合法 | 狀態檢查（狀態機） | `CancelBooking` |
| 更新訂位與寫紀錄必須同成功同失敗 | 交易（`BeginTransactionAsync`） | `CancelBooking` |
| 桌名重複 | 唯一索引 + 捕捉 2601/2627 | 資料庫 + `CreateDiningTable` |


## 測試

### xUnit 整合測試（`BookingPractice.Tests`）

使用 `WebApplicationFactory` 在記憶體中啟動整個 API，連到**獨立的測試資料庫**（`BookingPractice_Test`），不影響開發資料。每個測試自行準備資料（隨機桌名、隨機未來時間），彼此互不干擾。

| 測試情境 | 驗證重點 |
|---|---|
| 同桌同時段同時新增（10 個請求） | API 僅 1 個成功，且資料庫該時段只有 1 筆 |
| 同時取消同一筆訂位（10 個請求） | API 僅 1 個成功，且 BookingLog 只有 1 筆 |
| 不同桌同時段同時新增（5 個請求） | 全部成功，資料庫共 5 筆（防重複不會誤擋） |
| Smoke Test | 系統可啟動、可連線資料庫 |

每個測試除了檢查 API 回應，也**直接查詢資料庫**確認結果，不只相信回應內容。

執行：

```
dotnet test BookingPractice.Tests
```

測試資料庫連線字串放在測試專案自己的 User Secrets：

```
dotnet user-secrets set "ConnectionStrings:DBConnecting" "<測試資料庫連線字串>" --project BookingPractice.Tests
```

> 破壞實驗：刪除唯一索引後重跑，「同桌同時段」測試會變紅；重建索引後恢復綠燈，證明測試確實在保護併發規則。

### 並行測試小程式（`ConcurrencyTest`）

開發階段手動驗證用，需先啟動後端：

```
cd ConcurrencyTest
dotnet run -- create
dotnet run -- cancel <bookingId>
```

## 資料表

- `DiningTable`：桌位（名稱、容納人數、是否啟用）；`TableName` 唯一索引
- `Booking`：訂位（桌位、時間、人數、狀態、`RowVersion`）；`(DiningTableId, BookingTime)` 篩選唯一索引（`BookingStatus = 1`）
- `BookingLog`：狀態異動紀錄

> 時間一律以 UTC 儲存，前端顯示時再轉為本地時間。

## 如何執行

1. 建立資料庫（開發用 `BookingPractice`、測試用 `BookingPractice_Test`），執行 `sql/` 內的建表與索引腳本
2. 設定連線字串（User Secrets）：
```
   dotnet user-secrets set "ConnectionStrings:DBConnecting" "<連線字串>" --project BookingPracticeAPI
```
3. 啟動後端：`dotnet run`（http，埠 5197），Swagger：`/swagger`
4. 啟動前端：
```
   cd BookingPracticeClient
   npm install
   npm run dev
```
   並確認 `.env.development` 的 `VITE_BASE_URL` 指向 `http://localhost:5197/api/`，後端 CORS 已允許前端網址
5. 執行測試：`dotnet test BookingPractice.Tests`

