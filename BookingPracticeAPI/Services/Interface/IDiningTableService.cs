using BookingPractice.Common.Responses;

namespace BookingPractice.API.Services.Interface;

public interface IDiningTableService
{
    /// <summary>
    /// 取得指定預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>桌位資訊</returns>
    Task<ApiResponse<DiningTableResponse>> GetDiningTable(int diningTableId);

    /// <summary>
    /// 取得預約桌列表
    /// </summary>
    /// <returns>桌位資訊列表</returns>
    Task<ApiResponse<List<DiningTableResponse>>> GetDiningTables();

    /// <summary>
    /// 新增預約桌
    /// </summary>
    /// <param name="registerRequest">桌位資訊</param>
    /// <returns>桌位資訊</returns>
    Task<ApiResponse<DiningTableResponse>> CreateDiningTable(CreateDiningTableRequest registerRequest);

    /// <summary>
    /// 停用預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>是否成功停用</returns>
    Task<ApiResponse<bool>> CancelDiningTable(int diningTableId);
}
