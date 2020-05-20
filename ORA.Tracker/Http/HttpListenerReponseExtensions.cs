using System.Net;

namespace ORA.Tracker.Http
{
    public static class HttpListenerResponseExtensions
    {
        public static void BadRequest(this HttpListenerResponse response, byte[] body)
            => sendResponse(response, 400, body);

        public static void Forbidden(this HttpListenerResponse response, byte[] body)
            => sendResponse(response, 403, body);

        public static void NotFound(this HttpListenerResponse response, byte[] body)
            => sendResponse(response, 404, body);

        public static void MethodNotAllowed(this HttpListenerResponse response, byte[] body)
            => sendResponse(response, 405, body);

        public static void UnknownError(this HttpListenerResponse response, byte[] body)
            => sendResponse(response, 520, body);

        private static void sendResponse(HttpListenerResponse response, int code, byte[] body)
        {
            response.StatusCode = code;
            response.Close(body, true);
        }
    }
}
