using BookingPractice.Common.Requests;
using BookingPractice.Common.Responses;

namespace BookingPractice.API.Services;

public class BookingService(BookingPracticeContext context) : IBookingService
{
    /// <summary>
    /// 取得指定訂位資訊
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>訂位資訊</returns>
    public async Task<ApiResponse<BookingResponse>> GetBooking(int bookingId)
    {
        var booking = await context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            var errors = new Dictionary<string, string[]> { { "BookingId", new[] { "此訂位不存在！" } } };
            return ApiResponseHelper.RequestError<BookingResponse>(errors);
        }

        var response = new BookingResponse
        {
            BookingId = booking.BookingId,
            UserName = booking.UserName,
            UserPhone = booking.UserPhone,
            DiningTableId = booking.DiningTableId,
            People = booking.People,
            BookingStatus = booking.BookingStatus,
            BookingTime = booking.BookingTime,
            Note = booking.Note,
            CreateTime = booking.CreateTime,
            UpdateTime = booking.UpdateTime,
        };

        return ApiResponseHelper.Success(response);
    }

    /// <summary>
    /// 取得訂位資訊列表
    /// </summary>
    /// <param name="request">搜尋條件</param>
    /// <returns>訂位資訊列表</returns>
    public async Task<ApiResponse<List<BookingResponse>>> GetBookings(BookingsSearchRequest request)
    {
        // AsNoTracking 只讀查詢不追蹤 , 加上 AsQueryable 可以讓後續的 Where 查詢條件可以在資料庫端執行 , 避免先把所有資料撈出來再做篩選
        var query = context.Bookings.AsNoTracking().AsQueryable();

        if (request.BookingStatus.HasValue)
        {
            query = query.Where(b => b.BookingStatus == (int)request.BookingStatus);
        }

        if (request.BookingTime.HasValue)
        {
            // 取得指定日期的當天所有訂位資訊
            var start = request.BookingTime.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(b => b.BookingTime >= start && b.BookingTime < end);
        }
        // 根據頁數頁碼取得資訊
        var bookings = await query
            .OrderByDescending(b => b.BookingTime)
            .Skip(request.pageIndex * request.pageSize)
            .Take(request.pageSize)
            .ToListAsync();

        var responseList = bookings
            .Select(booking => new BookingResponse
            {
                BookingId = booking.BookingId,
                UserName = booking.UserName,
                UserPhone = booking.UserPhone,
                DiningTableId = booking.DiningTableId,
                People = booking.People,
                BookingStatus = booking.BookingStatus,
                BookingTime = booking.BookingTime,
                Note = booking.Note,
                CreateTime = booking.CreateTime,
                UpdateTime = booking.UpdateTime,
            })
            .ToList();

        return ApiResponseHelper.Success(responseList);
    }

    /// <summary>
    /// 取得可預約的訂位時段
    /// </summary>
    /// <param name="diningTableId">桌位 ID</param>
    /// <param name="date">日期</param>
    /// <returns>可預約時段</returns>
    public async Task<ApiResponse<List<DateTime>>> GetAvailableBookings(int diningTableId, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        // 取得指定桌位在指定日期的當天所有已預約時段
        var bookings = await context
            .Bookings.Where(d =>
                d.DiningTableId == diningTableId
                && d.BookingStatus == (int)BookingStatusEnum.預約成功
                && d.BookingTime >= start
                && d.BookingTime < end
            )
            .Select(b => b.BookingTime)
            .ToListAsync();

        // 再根據已預約時段排除 , 就能拿到當天剩下的可預約時段
        // Range 代表營業時間 ( 8:00 - 21:00 , 因為是 UTC 格式所以寫 0 , 14 ) , Select 代表每個小時的整點時間 ( AddHours 會把每個數字變小時 , 比如 11 => 11:00) , Where 代表排除已預約時段並排除過去的時間
        var slots = Enumerable
            .Range(0, 14)
            .Select(h => start.AddHours(h))
            .Where(t => !bookings.Contains(t) && t > DateTime.UtcNow)
            .ToList();

        return ApiResponseHelper.Success(slots);
    }

    /// <summary>
    /// 新增訂位
    /// </summary>
    /// <param name="bookingRequest">訂位資訊</param>
    /// <returns>訂位資訊</returns>
    public async Task<ApiResponse<BookingResponse>> CreateBooking(CreateBookingRequest bookingRequest)
    {
        var table = await context.DiningTables.FindAsync(bookingRequest.DiningTableId);

        if (table == null || table.IsActive == false || table.Capacity < bookingRequest.People)
        {
            var errors = new Dictionary<string, string[]> { { "DiningTableId", new[] { "無法預約此桌位！" } } };
            return ApiResponseHelper.RequestError<BookingResponse>(errors);
        }

        var time = bookingRequest.BookingTime;

        if (time <= DateTime.UtcNow || time.Minute != 0 || time.Second != 0 || time.Millisecond != 0)
        {
            var errors = new Dictionary<string, string[]>
            {
                { "BookingTime", new[] { "預約時間必須大於現在日期且必須是整點！" } },
            };
            return ApiResponseHelper.RequestError<BookingResponse>(errors);
        }

        var isExist = await context.Bookings.AnyAsync(b =>
            b.DiningTableId == bookingRequest.DiningTableId
            && b.BookingTime == bookingRequest.BookingTime
            && b.BookingStatus == (int)BookingStatusEnum.預約成功
        );

        if (isExist)
        {
            var errors = new Dictionary<string, string[]> { { "BookingTime", new[] { "此時段已被預約！" } } };
            return ApiResponseHelper.RequestError<BookingResponse>(errors);
        }

        var booking = new Booking
        {
            UserName = bookingRequest.UserName,
            UserPhone = bookingRequest.UserPhone,
            DiningTableId = bookingRequest.DiningTableId,
            People = bookingRequest.People,
            BookingStatus = (int)BookingStatusEnum.預約成功,
            BookingTime = bookingRequest.BookingTime,
            Note = bookingRequest.Note,
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };

        try
        {
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();
        }
        // DbUpdateException : 資料庫異常
        catch (DbUpdateException ex)
            // 當異常例外是 SqlException ( 資料庫的問題 ) 且錯誤號碼是 2601 或 2627 時，表示違反了唯一性約束 ，代表有人嘗試插入重複的資料 , 就擋下來並報錯誤
            when (ex.InnerException is SqlException s && (s.Number == 2601 || s.Number == 2627))
        {
            var errors = new Dictionary<string, string[]> { { "BookingTime", new[] { "此時段已被預約！" } } };
            return ApiResponseHelper.RequestError<BookingResponse>(errors);
        }

        var response = new BookingResponse
        {
            BookingId = booking.BookingId,
            DiningTableId = booking.DiningTableId,
            UserName = booking.UserName,
            UserPhone = booking.UserPhone,
            People = booking.People,
            BookingStatus = booking.BookingStatus,
            BookingTime = booking.BookingTime,
            CreateTime = booking.CreateTime,
            UpdateTime = booking.UpdateTime,
            Note = booking.Note,
        };
        return ApiResponseHelper.Success(response);
    }

    /// <summary>
    /// 取消訂位
    /// </summary>
    /// <param name="bookingId">訂位 ID</param>
    /// <returns>是否成功取消</returns>
    public async Task<ApiResponse<bool>> CancelBooking(int bookingId)
    {
        var booking = await context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            var errors = new Dictionary<string, string[]> { { "Booking", new[] { "無訂位資訊 ！" } } };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        if (booking.BookingStatus != (int)BookingStatusEnum.預約成功)
        {
            var errors = new Dictionary<string, string[]> { { "Booking", new[] { "此訂位已取消 ！" } } };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        // 啟動交易
        await using var trx = await context.Database.BeginTransactionAsync();

        try
        {
            var oldStatus = booking.BookingStatus;
            booking.BookingStatus = (int)BookingStatusEnum.取消訂位;
            booking.UpdateTime = DateTime.UtcNow;
            await context.SaveChangesAsync();

            context.BookingLogs.Add(
                new BookingLog
                {
                    BookingId = booking.BookingId,
                    OldStatus = oldStatus,
                    NewStatus = booking.BookingStatus,
                    CreateTime = DateTime.UtcNow,
                }
            );

            await context.SaveChangesAsync();
            await trx.CommitAsync();
        }
        // DbUpdateConcurrencyException 是 DbUpdateException 的子類別 ,  DbUpdateException 代表資料庫異常 , DbUpdateConcurrencyExcept 則代表資料庫 " 更新 " 異常
        catch (DbUpdateConcurrencyException)
        {
            await trx.RollbackAsync();
            var errors = new Dictionary<string, string[]>
            {
                { "Booking", new[] { "訂位資料已被他人修改，請重新整理後再試 ！" } },
            };
            return ApiResponseHelper.RequestError<bool>(errors);
        }

        return ApiResponseHelper.Success(true);
    }
}
