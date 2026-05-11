namespace SmartCenter.Service.Staff;

public class Response
{
    public class ConsultationResponse()
    {
        public int pendingConsultations { get; set; }
        public int newStudentsToday { get; set; }
        public int pendingOrders { get; set; }
    }
        
   
}