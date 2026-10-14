using BookingPractice.API.Services;
using BookingPractice.Common.Responses;

namespace BookingPractice.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<ProblemDetails>))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<Dictionary<string, string[]>>))]
public class DiningTableController(IDiningTableService diningTableService) : ControllerBase
{
    /// <summary>
    /// 取得指定預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>桌位資訊</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DiningTableResponse>))]
    public async Task<IActionResult> GetDiningTable([FromQuery] int diningTableId)
    {
        return Ok(await diningTableService.GetDiningTable(diningTableId));
    }

    /// <summary>
    /// 取得預約桌列表
    /// </summary>
    /// <returns>桌位資訊列表</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<DiningTableResponse>>))]
    public async Task<IActionResult> GetDiningTables()
    {
        return Ok(await diningTableService.GetDiningTables());
    }

    /// <summary>
    /// 新增預約桌
    /// </summary>
    /// <param name="registerRequest">桌位資訊</param>
    /// <returns>桌位資訊</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DiningTableResponse>))]
    public async Task<IActionResult> CreateDiningTable([FromBody] CreateDiningTableRequest registerRequest)
    {
        return Ok(await diningTableService.CreateDiningTable(registerRequest));
    }

    /// <summary>
    /// 停用預約桌
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <returns>是否成功停用</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
    public async Task<IActionResult> CancelDiningTable([FromQuery] int diningTableId)
    {
        return Ok(await diningTableService.CancelDiningTable(diningTableId));
    }
}
