namespace BookingPractice.Common.Responses
{
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public int DiningTableId { get; set; }

        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public int People { get; set; }
        public int BookingStatus { get; set; }

        public DateTime BookingTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Note { get; set; }
    }
}
