namespace BookingPractice.API.Common.Requests
{
    public class CreateBookingRequest
    {
        /// <summary>
        /// 訂位人姓名
        /// </summary>
        [Required(ErrorMessage = "訂位人姓名不能為空 ！")]
        [MaxLength(50, ErrorMessage = "{0}不能超過{1}個字 !")]
        public string UserName { get; set; }

        /// <summary>
        /// 訂位人電話
        /// </summary>
        [Required(ErrorMessage = "訂位人電話不能為空！")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請符合手機號碼格式 0912345678")]
        public string UserPhone { get; set; }

        /// <summary>
        /// 預約桌 ID
        /// </summary>
        [Required(ErrorMessage = "預約桌 ID 不能為空 ！")]
        public int DiningTableId { get; set; }

        /// <summary>
        /// 用餐人數
        /// </summary>
        [Required(ErrorMessage = "用餐人數不能為空 ！")]
        [Range(1, 25, ErrorMessage = "用餐人數必須大於 0 且不能超過 25 位 ！")]
        public int People { get; set; }

        /// <summary>
        /// 預約狀態
        /// </summary>
        [Required(ErrorMessage = "預約狀態不能為空 ！")]
        public BookingStatusEnum BookingStatus { get; set; }

        /// <summary>
        /// 預約時間
        /// </summary>
        [Required(ErrorMessage = "預約時間不能為空 ！")]
        public DateTime BookingTime { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [MaxLength(500, ErrorMessage = "備註長度不能超過 500 字 ！")]
        public string Note { get; set; }
    }
}
