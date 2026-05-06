namespace SmartCenter.Service.Payment;

public class Response
{
    public class CreatePaymentResponse
    {
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
    }
}