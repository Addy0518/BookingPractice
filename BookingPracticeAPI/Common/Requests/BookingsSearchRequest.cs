namespace BookingPractice.Common.Requests
{
    public class BookingsSearchRequest
    {
        /// 分頁
        /// ==================================================
        /// <summary>
        /// 頁碼
        /// </summary>
        [Display(Name = "頁碼")]
        public int pageIndex { get; set; } = 0;

        /// <summary>
        /// 每頁顯示數量
        /// </summary>
        [Display(Name = "每頁顯示數量")]
        public int pageSize { get; set; } = 10;

        /// 搜尋條件
        /// ==================================================
        /// <summary>
        /// 預定狀態
        /// </summary>
        [Display(Name = "預定狀態")]
        public BookingStatusEnum? BookingStatus { get; set; }

        /// <summary>
        /// 預定時間
        /// </summary>
        [Display(Name = "預定時間")]
        public DateTime? BookingTime { get; set; }

        /// 排序條件
        /// ==================================================
        /// <summary>
        /// 分類排序
        /// </summary>
        [Display(Name = "分類排序")]
        public string? sortBy { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        [Display(Name = "排序方向")]
        public string? sortOrder { get; set; }
    }
}
