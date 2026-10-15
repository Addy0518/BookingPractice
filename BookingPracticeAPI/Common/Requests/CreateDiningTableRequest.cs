namespace BookingPractice.API.Common.Requests
{
    public class CreateDiningTableRequest
    {
        /// <summary>
        /// 預約桌名稱
        /// </summary>
        [Required(ErrorMessage = "預約桌名稱不能為空 !")]
        [MaxLength(200, ErrorMessage = "預約桌名稱 長度不能超過200字 !")]
        public string TableName { get; set; }

        /// <summary>
        /// 座位數量
        /// </summary>
        [Required(ErrorMessage = "座位數量不能為空 !")]
        [Range(1, 25, ErrorMessage = "座位數量必須大於 0 且不能超過 25 位 !")]
        public int Capacity { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
