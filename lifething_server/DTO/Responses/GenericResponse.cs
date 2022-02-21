namespace lifething_server.DTO.Responses
{
    public class GenericResponse <T> 
    {
        public  T? Data {get; set;}
        public string? ResponseCode {get; set;}
        public string? ResponseMessage {get; set;}
        public int? HttpStatusCode {get; set;}
    }
}