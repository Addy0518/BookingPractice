using System.Net.Http.Json;

/// <summary>
/// 用來測試多筆預定訂位同時發生 ( 併發查詢 )
/// </summary>
///
// 建立 Http 客戶端 , 對後端 API　發請求 ( 要一次發送多支請求 )
var client = new HttpClient();

// args 是參數陣列 , 比如 :  dotnet run -- cancel 28
//                                        ↑     ↑
//                                    args[0]  args[1]
var mode = args.Length > 0 ? args[0] : "";

// 判斷終端機是要用哪個測試方法 ( dotnet run -- create   或   dotnet run -- cancel 28 等等 ... )
switch (mode)
{
    case "create":
        await TestCreate();
        break;
    case "cancel":
        var id = args.Length > 1 ? args[1] : "0";
        await TestCancel(id);
        break;
    default:
        Console.WriteLine("請指定: create 或 cancel <bookingId>");
        break;
}

// 同時送多筆 「 新增訂位 」
async Task TestCreate()
{
    // 要測試的後端 API
    var url = "http://localhost:5197/api/Booking/CreateBooking";

    // 建立 10 個執行緒
    // Enumerable.Range 決定要跑幾次 ,  .Select(_ => ...)：每個數字都建立一個請求，_ 代表「用不到這個數字」
    // 這裡只是建立任務 , 等 PostAsJsonAsync 被呼叫才會一次送出
    // PostAsJsonAsync 會把要送的資料轉 Json 送出 ( API　參數是　Json )
    var tasks = Enumerable
        .Range(0, 10)
        .Select(_ =>
            client.PostAsJsonAsync(
                url,
                new
                {
                    userName = "test",
                    userPhone = "0900000000",
                    diningTableId = 1,
                    people = 2,
                    bookingTime = "2026-10-20T12:00:00Z",
                    note = "",
                }
            )
        );

    // 私有方法 : 同時執行並回報結果
    await PrintResult(await Task.WhenAll(tasks));
}

// 同時送多筆 「 取消同一筆訂位 」
async Task TestCancel(string bookingId)
{
    var cancelUrl = $"http://localhost:5197/api/Booking/CancelBooking?bookingId={bookingId}";

    // 這裡因為是修改所以用 Put
    var tasks = Enumerable.Range(0, 10).Select(_ => client.PutAsync(cancelUrl, null));
    await PrintResult(await Task.WhenAll(tasks));
}

// 同時執行並回報結果
// HttpResponseMessage 是每個請求回傳的結果 , Task.WhenAll 會再把它集合成陣列 , 所以這裡用 HttpResponseMessage 陣列接
async Task PrintResult(HttpResponseMessage[] responses)
{
    int ok = 0;
    int fail = 0;

    foreach (var r in responses)
    {
        var json = await r.Content.ReadAsStringAsync();

        // 看內容裡的 codeStatus：2000 代表真的成功，否則視為失敗
        if (json.Contains("\"codeStatus\":2000"))
            ok++;
        else
            fail++;
    }
    Console.WriteLine($"成功: {ok}");
    Console.WriteLine($"失敗: {fail}");
}
