# BookingPractice 訂位系統

練習「併發控制」的餐廳訂位系統。

## 技術

- 後端：ASP.NET Core 8 Web API、EF Core、SQL Server、Serilog
- 前端：Vue 3、Vite、Pinia、Axios
- 測試：並行測試程式（ConcurrencyTest）

## 功能

- 桌位管理：新增、查詢、停用
- 訂位：新增、查詢、取消、查詢可訂時段
- 取消訂位時寫入異動紀錄（BookingLog）

## 併發處理（核心）

| 問題 | 解法 | 位置 |
|---|---|---|
| 多人同時搶同一桌同一時段 | 唯一索引（篩選索引：僅限預約成功） + 捕捉錯誤碼 2601/2627 | 資料庫 + `CreateBooking` |
| 多人同時修改同一筆訂位 | 樂觀鎖（`RowVersion`） + 捕捉 `DbUpdateConcurrencyException` | `CancelBooking` |
| 重複取消、狀態不合法 | 狀態檢查（狀態機） | `CancelBooking` |
| 更新訂位與寫紀錄必須同成功同失敗 | 交易（`BeginTransactionAsync`） | `CancelBooking` |

### 驗證結果

同時送出 10 個請求（`Task.WhenAll`）：

| 情境 | 預期 | 結果 |
|---|---|---|
| 同桌同時段新增訂位 | 成功 1、失敗 9 | 成功 1、失敗 9 |
| 同時取消同一筆訂位 | 成功 1、失敗 9，BookingLog 僅 1 筆 | 符合 |

> 只用樂觀鎖仍會出現多筆成功（較晚到的請求讀到的已是新版本），因此需搭配狀態檢查。

## 資料表

- `DiningTable`：桌位（名稱、容納人數、是否啟用）
- `Booking`：訂位（桌位、時間、人數、狀態、`RowVersion`）
- `BookingLog`：狀態異動紀錄

## 如何執行

1. 建立資料庫並執行 SQL 建表與索引
2. 設定連線字串（User Secrets）：
```
   dotnet user-secrets set "ConnectionStrings:DBConnecting" "<連線字串>"
```
3. 啟動後端：`dotnet run`（http，埠 5197），Swagger：`/swagger`
4. 執行並行測試：
```
   cd ConcurrencyTest
   dotnet run -- create
   dotnet run -- cancel <bookingId>
```

## 後續規劃

- xUnit 整合測試
- 前端訂位頁面
- 登入與角色權限
