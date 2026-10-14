namespace BookingPractice.Common.Responses
{
    public class DiningTableResponse
    {
        public int DiningTableId { get; set; }

        public string TableName { get; set; }

        public int Capacity { get; set; }

        public bool IsActive { get; set; }
    }
}
