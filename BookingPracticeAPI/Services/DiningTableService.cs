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

        await context.DiningTables.AddAsync(diningTable);
        await context.SaveChangesAsync();
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
}
