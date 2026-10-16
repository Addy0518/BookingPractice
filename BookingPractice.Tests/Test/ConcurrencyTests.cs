using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using BookingPractice.Models;
using BookingPractice.Tests.Helper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingPractice.Tests.Test
{
    // ConcurrencyTests : 整合測試
    // IClassFixture 就類似注入建構子 , 把 CustomWebApplicationFactory 注入到測試類別裡 , 讓測試類別可以使用它
    public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
    {
        // 測試網站工廠：用來取得資料庫 DbContext（準備資料、驗證結果）
        private readonly CustomWebApplicationFactory _factory;

        // 用來發送 HTTP 請求
        private readonly HttpClient _client;

        public ConcurrencyTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            // CreateClient：向工廠要一個 HttpClient，請求會直接送進記憶體中的測試網站
            _client = factory.CreateClient();
        }

        // [Fact]：標記這是一個測試，dotnet test 會找到並執行它
        [Fact]
        public async Task 同桌同時段同時新增_只有一個成功()
        {
            // 呼叫自訂的共用方法新增一個桌位 , 並拿一個未來的隨機時間
            var tableId = await TestHelper.CreateTable(_factory);
            var time = TestHelper.RandomTime();

            // 建立 10 個執行緒 ( 新增訂位 )
            // Enumerable.Range 決定要跑幾次 ,  .Select(i => ...)：每個數字都建立一個請求
            // 這裡只是建立任務 , 等 PostAsJsonAsync 被呼叫才會一次送出
            // PostAsJsonAsync 會把要送的資料轉 Json 送出 ( API　參數是　Json )
            var tasks = Enumerable
                .Range(0, 10)
                .Select(i =>
                    _client.PostAsJsonAsync(
                        "/api/Booking/CreateBooking",
                        new
                        {
                            userName = $"user{i}",
                            userPhone = "0900000000",
                            diningTableId = tableId,
                            people = 2,
                            bookingTime = time,
                            note = "",
                        }
                    )
                )
                .ToList();

            // Task.WhenAll 同時執行所有執行續
            var responses = await Task.WhenAll(tasks);

            // 統計成功 （codeStatus = 2000） 的請求數量
            var okCount = 0;
            foreach (var r in responses)
            {
                // 一樣呼叫自訂的共用方法確認 API 回傳是否成功
                if (await TestHelper.IsAPIReturnOK(r))
                    okCount++;
            }

            // Assert ( 斷言 ) : 這裡是第一階段比對 , 先比對他 API 回傳的成功結果是不是一次 ( 對應我們這次的測試 => 同桌同時段同時新增_只有一個成功 )
            // 如果等於 1 , 代表成功 , 其他則為失敗
            Assert.Equal(1, okCount);

            // 但不能只信任 API , 還要去資料庫確認 , 這也就是這次整合測試的重點
            // 手動建立 DI 容器並取得服務 , using 確保使用完關閉
            // 去資料庫查這個時間點的這個桌位
            using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingPracticeContext>();
            var tableCount = await db.Bookings.CountAsync(b =>
                b.DiningTableId == tableId && b.BookingTime == time && b.BookingStatus == 1
            );
            // 再次斷言 , 也就是第二階段比對 , 比對資料庫裡面是不是只有一筆
            Assert.Equal(1, tableCount);
        }

        [Fact]
        public async Task 同時取消同一筆訂位_只有一個成功且只寫一筆Log()
        {
            // 一樣新增一個桌位用來測試
            var tableId = await TestHelper.CreateTable(_factory);
            var time = TestHelper.RandomTime();

            var createBooking = await _client.PostAsJsonAsync(
                "/api/Booking/CreateBooking",
                new
                {
                    userName = $"u",
                    userPhone = "0900000000",
                    diningTableId = tableId,
                    people = 2,
                    bookingTime = time,
                    note = "",
                }
            );

            // 斷言 1 : 確認 API 回傳成功
            Assert.True(await TestHelper.IsAPIReturnOK(createBooking));

            // 拿到剛新增的訂位的 ID
            int bookingId;
            using (var scope = _factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingPracticeContext>();
                bookingId = (await db.Bookings.SingleAsync(b => b.DiningTableId == tableId)).BookingId;
            }
            ;

            // 取消十次 , 測試是不是只有取消一次並寫一次 Log
            var tasks = Enumerable
                .Range(0, 10)
                .Select(_ => _client.PutAsync($"/api/Booking/CancelBooking?bookingId={bookingId}", null))
                .ToList();
            var responses = await Task.WhenAll(tasks);
            var okCount = 0;
            foreach (var r in responses)
            {
                if (await TestHelper.IsAPIReturnOK(r))
                    okCount++;
            }

            // 斷言 2 : 確認 API 回傳成功的次數只有一次
            Assert.Equal(1, okCount);

            using var scope2 = _factory.Services.CreateAsyncScope();
            var db2 = scope2.ServiceProvider.GetRequiredService<BookingPracticeContext>();
            var tableCount = await db2.BookingLogs.CountAsync(b => b.BookingId == bookingId);

            // 斷言 3 : 確認資料庫裡面 Log 只有一筆
            Assert.Equal(1, tableCount);
        }

        [Fact]
        public async Task 不同桌同時段同時新增_全部成功()
        {
            // 測試同時間不同桌位能不能全預定成功
            var time = TestHelper.RandomTime();
            var tableIds = new List<int>();

            for (int i = 0; i < 5; i++)
            {
                tableIds.Add(await TestHelper.CreateTable(_factory));
            }

            var tasks = tableIds
                .Select(tableId =>
                    _client.PostAsJsonAsync(
                        "/api/Booking/CreateBooking",
                        new
                        {
                            userName = $"user{tableId}",
                            userPhone = "0900000000",
                            diningTableId = tableId,
                            people = 2,
                            bookingTime = time,
                            note = "",
                        }
                    )
                )
                .ToList();

            var responses = await Task.WhenAll(tasks);

            foreach (var r in responses)
            {
                // 斷言 1 : 確認 API 回傳成功
                Assert.True(await TestHelper.IsAPIReturnOK(r));
            }

            using var scope3 = _factory.Services.CreateAsyncScope();
            var db3 = scope3.ServiceProvider.GetRequiredService<BookingPracticeContext>();
            var tableCount = await db3.Bookings.CountAsync(b =>
                tableIds.Contains(b.DiningTableId) && b.BookingTime == time
            );

            // 斷言 2 : 確認資料庫裡面有 5 筆訂位
            Assert.Equal(5, tableCount);
        }
    }
}
