using BookingPractice.API.Services;
using BookingPractice.Common.Requests;
using BookingPractice.Common.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace BookingPractice.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<ProblemDetails>))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<Dictionary<string, string[]>>))]
public class BookingController(IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// 取得指定訂位資訊
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>訂位資訊</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    public async Task<IActionResult> GetBooking([FromQuery] int bookingId)
    {
        return Ok(await bookingService.GetBooking(bookingId));
    }

    /// <summary>
    /// 取得訂位資訊列表
    /// </summary>
    /// <param name="request">搜尋條件</param>
    /// <returns>訂位資訊列表</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<BookingResponse>>))]
    public async Task<IActionResult> GetBookings([FromBody] BookingsSearchRequest request)
    {
        return Ok(await bookingService.GetBookings(request));
    }

    /// <summary>
    /// 取得可預約的訂位時段
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <param name="date">日期</param>
    /// <returns>可預約時段</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<DateTime>>))]
    public async Task<ApiResponse<List<DateTime>>> GetAvailableBookings(
        [FromQuery] int diningTableId,
        [FromQuery] DateTime date
    )
    {
        return await bookingService.GetAvailableBookings(diningTableId, date);
    }

    /// <summary>
    /// 新增訂位
    /// </summary>
    /// <param name="bookingRequest">訂位資訊</param>
    /// <returns>訂位資訊</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BookingResponse>))]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest bookingRequest)
    {
        return Ok(await bookingService.CreateBooking(bookingRequest));
    }

    /// <summary>
    /// 取消訂位
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>是否成功取消</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
    public async Task<IActionResult> CancelBooking([FromQuery] int bookingId)
    {
        return Ok(await bookingService.CancelBooking(bookingId));
    }
}
