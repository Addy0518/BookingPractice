using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingPractice.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BookingPractice.Tests.Helper
{
    public static class TestHelper
    {
        // 建立一個桌位，並回傳桌位的 Id
        public static async Task<int> CreateTable(CustomWebApplicationFactory factory, int capacity = 4)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookingPracticeContext>();

            var table = new DiningTable
            {
                TableName = "T-" + Guid.NewGuid().ToString("N"),
                Capacity = capacity,
                IsActive = true,
            };
            db.DiningTables.Add(table);
            await db.SaveChangesAsync();
            return table.DiningTableId;
        }

        // 確認呼叫的 API 回傳的內容是否真的成功
        public static async Task<bool> IsAPIReturnOK(HttpResponseMessage responseMessage)
        {
            var json = await responseMessage.Content.ReadAsStringAsync();

            // 看內容裡的 codeStatus：2000 代表真的成功，否則視為失敗
            return json.Contains("\"codeStatus\":2000");
        }

        // 拿到一個未來的隨機時間給預約測試用
        public static DateTime RandomTime()
        {
            var days = Random.Shared.Next(30, 3000);
            var hour = Random.Shared.Next(0, 24);
            return DateTime.UtcNow.Date.AddDays(days).AddHours(hour);
        }
    }
}
