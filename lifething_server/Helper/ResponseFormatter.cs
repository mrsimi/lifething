using lifething_server.DTO.Responses;

namespace lifething_server.Helper
{
    public static class ResponseFormatter
    {
        public static GenericResponse<T> BadRequestResponse<T>(T? data)
        {
            var response = new GenericResponse<T>
            {
                HttpStatusCode = 400
            };

            return response;
        }

        public static string GetUsernameFromEmail(string email)
        {
            string[] splitttedStrings = email.Split("@"); 

            return  splitttedStrings[0];
        }
    }
}