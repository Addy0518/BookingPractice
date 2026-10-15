using BookingPractice.Common.Responses;

namespace BookingPractice.API.Services;

public class DiningTableService(BookingPracticeContext context) : IDiningTableService
{
    /// <summary>
    /// 取得指定預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>桌位資訊</returns>
    public async Task<ApiResponse<DiningTableResponse>> GetDiningTable(int diningTableId)
    {
        var diningTable = await context.DiningTables.FindAsync(diningTableId);
        if (diningTable == null)
        {
            var errors = new Dictionary<string, string[]> { { "DiningTableId", new[] { "此桌位不存在！" } } };
            return ApiResponseHelper.RequestError<DiningTableResponse>(errors);
        }

        var response = new DiningTableResponse
        {
            DiningTableId = diningTable.DiningTableId,
            TableName = diningTable.TableName,
            Capacity = diningTable.Capacity,
            IsActive = diningTable.IsActive,
        };

        return ApiResponseHelper.Success(response);
    }

    /// <summary>
    /// 取得預約桌列表
    /// </summary>
    /// <returns>桌位資訊列表</returns>
    public async Task<ApiResponse<List<DiningTableResponse>>> GetDiningTables()
    {
        var dningTables = await context.DiningTables.ToListAsync();
        var responseList = dningTables
            .Select(dt => new DiningTableResponse
            {
                DiningTableId = dt.DiningTableId,
                TableName = dt.TableName,
                Capacity = dt.Capacity,
                IsActive = dt.IsActive,
            })
            .ToList();
        return ApiResponseHelper.Success(responseList);
    }

    /// <summary>
    /// 新增預約桌
    /// </summary>
    /// <param name="registerRequest">桌位資訊</param>
    /// <returns>桌位資訊</returns>
    public async Task<ApiResponse<DiningTableResponse>> CreateDiningTable(CreateDiningTableRequest registerRequest)
    {
        // 檢查桌位是否已存在
        bool isExist = await context.DiningTables.AnyAsync(dt => dt.TableName == registerRequest.TableName);
        if (isExist)
        {
            var errors = new Dictionary<string, string[]> { { "TableName", new[] { "此桌位已存在！" } } };
            return ApiResponseHelper.RequestError<DiningTableResponse>(errors);
        }

        var diningTable = new DiningTable
        {
            TableName = registerRequest.TableName,
            Capacity = registerRequest.Capacity,
            IsActive = registerRequest.IsActive,
        };
        try
        {
            await context.DiningTables.AddAsync(diningTable);
            await context.SaveChangesAsync();
        }
        // DbUpdateException : 資料庫異常
        catch (DbUpdateException ex)
            // 當異常例外是 SqlException ( 資料庫的問題 ) 且錯誤號碼是 2601 或 2627 時，表示違反了唯一性約束 ，代表有人嘗試插入重複的資料 , 就擋下來並報錯誤
            when (ex.InnerException is SqlException s && (s.Number == 2601 || s.Number == 2627))
        {
            var errors = new Dictionary<string, string[]> { { "BookingTime", new[] { "此時段已被預約！" } } };
            return ApiResponseHelper.RequestError<DiningTableResponse>(errors);
        }

        var response = new DiningTableResponse
        {
            DiningTableId = diningTable.DiningTableId,
            TableName = diningTable.TableName,
            Capacity = diningTable.Capacity,
            IsActive = diningTable.IsActive,
        };

        return ApiResponseHelper.Success(response);
    }

    /// <summary>
    /// 停用預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>是否成功停用</returns>
    public async Task<ApiResponse<bool>> CancelDiningTable(int diningTableId)
    {
        // 檢查桌位是否存在
        var diningTable = await context.DiningTables.FindAsync(diningTableId);
        if (diningTable == null)
        {
            var errors = new Dictionary<string, string[]> { { "DiningTableId", new[] { "此桌位不存在！" } } };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        var futureBooking = await context.Bookings.AnyAsync(b =>
            b.DiningTableId == diningTableId
            && b.BookingTime > DateTime.UtcNow
            && b.BookingStatus == (int)BookingStatusEnum.預約成功
        );

        if (futureBooking)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Bookings", new[] { "此桌位有未來預約 , 無法停用 ！" } },
            };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        diningTable.IsActive = false;
        await context.SaveChangesAsync();

        return ApiResponseHelper.Success(true);
    }

    /// <summary>
    /// 啟用預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>是否成功啟用</returns>
    public async Task<ApiResponse<bool>> EnableDiningTable(int diningTableId)
    {
        // 檢查桌位是否存在
        var diningTable = await context.DiningTables.FindAsync(diningTableId);
        if (diningTable == null)
        {
            var errors = new Dictionary<string, string[]> { { "DiningTableId", new[] { "此桌位不存在！" } } };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        diningTable.IsActive = true;
        await context.SaveChangesAsync();

        return ApiResponseHelper.Success(true);
    }
}
