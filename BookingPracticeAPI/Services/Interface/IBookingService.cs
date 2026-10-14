using BookingPractice.Common.Requests;
using BookingPractice.Common.Responses;

namespace BookingPractice.API.Services.Interface;

public interface IBookingService
{
    /// <summary>
    /// 取得指定訂位資訊
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>訂位資訊</returns>
    Task<ApiResponse<BookingResponse>> GetBooking(int bookingId);

    /// <summary>
    /// 取得訂位資訊列表
    /// </summary>
    /// <param name="request">搜尋條件</param>
    /// <returns>訂位資訊列表</returns>
    Task<ApiResponse<List<BookingResponse>>> GetBookings(BookingsSearchRequest request);

    /// <summary>
    /// 取得可預約的訂位時段
    /// </summary>
    /// <returns>可預約時段</returns>
    Task<ApiResponse<List<DateTime>>> GetAvailableBookings(int diningTableId, DateTime date);

    /// <summary>
    /// 新增訂位
    /// </summary>
    /// <param name="bookingRequest">訂位資訊</param>
    /// <returns>訂位資訊</returns>
    Task<ApiResponse<BookingResponse>> CreateBooking(CreateBookingRequest bookingRequest);

    /// <summary>
    /// 取消訂位
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>訂位資訊</returns>
    Task<ApiResponse<bool>> CancelBooking(int bookingId);
}
