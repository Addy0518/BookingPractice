using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BookingPractice.Tests
{
    // WebApplicationFactory<Program>：微軟提供的「測試用網站啟動器」
    // <Program> 是型別標籤，用來指定「要啟動哪個專案」（API 專案的 Program）
    // 這裡繼承的是 WebApplicationFactory，並覆寫它的設定方法來調整網站的啟動設定
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // 覆寫 WebApplicationFactory 的 ConfigureWebHost 方法，修改啟動設定
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // 用 Development 環境啟動（沿用 API 的設定與 UserSecrets 載入規則）
            builder.UseEnvironment("Development");

            // ConfigureAppConfiguration : 設定 appsettings.json、appsettings.Development.json、user-secrets 等設定檔
            builder.ConfigureAppConfiguration(
                (context, config) =>
                {
                    // 加入 「 測試專案自己的 user-secrets 」
                    // <CustomWebApplicationFactory> 用來告訴系統：請去這個組件（測試專案）的 UserSecretsId 找 secrets
                    // 後加入的設定會 「 覆蓋 」 先前同名的設定，所以測試專案裡的 ConnectionStrings:DBConnecting（指向 BookingPractice_Test）
                    // 就會蓋掉 API 本來的連線字串，API 就會改連測試資料庫
                    config.AddUserSecrets<CustomWebApplicationFactory>();
                }
            );
        }
    }
}
