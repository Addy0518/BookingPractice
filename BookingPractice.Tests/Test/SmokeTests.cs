using System;
using System.Collections.Generic;
using System.Text;

namespace BookingPractice.Tests.Test
{
    // 基本測試 , 用來測試這個專案能否啟動 , 連資料庫跟 API 是否能正常運作
    // IClassFixture 就類似注入建構子 , 把 CustomWebApplicationFactory 注入到測試類別裡 , 讓測試類別可以使用它
    public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
    {
        // 用來發送 HTTP 請求
        private readonly HttpClient _client;

        public SmokeTests(CustomWebApplicationFactory factory)
        {
            // CreateClient：向工廠要一個 HttpClient，請求會直接送進記憶體中的測試網站
            _client = factory.CreateClient();
        }

        // [Fact]：標記這是一個測試，dotnet test 會找到並執行它
        [Fact]
        public async Task GetDiningTables_應回傳成功()
        {
            // 發送 GET 請求到 /api/DiningTable/GetDiningTables
            var response = await _client.GetAsync("/api/DiningTable/GetDiningTables");

            // 把回傳內容 ( Json ) 讀出來
            var json = await response.Content.ReadAsStringAsync();

            // Assert（斷言）：檢查結果是否符合預期，不符合就讓測試變紅
            // 1. HTTP 狀態碼是 2xx
            Assert.True(response.IsSuccessStatusCode);
            // 2. API 成功時 codeStatus 為 2000，內容要包含它
            Assert.Contains("\"codeStatus\":2000", json);
        }
    }
}
