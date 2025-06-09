using betterme_web_gui.DTOS;
using System.Net;

namespace betterme_web_gui.Services
{
    public class VerificationRequestsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly ILogger<VerificationRequestsService> _logger;

        public VerificationRequestsService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<VerificationRequestsService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("VerificationRequestsAPI");
            _token = httpContextAccessor.HttpContext?.Session.GetString("token")?.ToString() ?? string.Empty;
            _logger = logger;
        }

        public async Task<Response<VerificationRequestDTO>> AddVerificationRequest(VerificationRequestDTO verificationRequest)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            HttpResponseMessage httpResponse;
            Response<VerificationRequestDTO> response = new();

            try
            {
                httpResponse = await _httpClient.PostAsJsonAsync("", verificationRequest);
            }
            catch (HttpRequestException error)
            {
                _logger.LogError("Error while attempting to establish connection with API Gateway: {error}", error);
                response.Success = false;
                response.Message = "No es posible conectarse al servidor en este momento. Intentélo de nuevo más tarde.";
                return response;
            }

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.Created:
                    response.Success = true;
                    response.Message = "Se ha registrado la solicitud con éxito.";
                    break;
                case HttpStatusCode.BadRequest:
                    response.Success = false;
                    response.Message = "Ocurrió un error interno en la aplicación. Vuelve a intentar más tarde.";
                    _logger.LogWarning("Error while attempting to create a verification request: {message}", await httpResponse.Content.ReadAsStringAsync());
                    break;
                case HttpStatusCode.Unauthorized:
                    response.Success = false;
                    response.Message = "Es necesario volver a iniciar sesión antes de enviar la solicitud.";
                    response.RedirectPage = "/Login";
                    _logger.LogWarning("Attempt to create a verification request with invalid token");
                    break;
                case HttpStatusCode.Conflict:
                    response.Success = false;
                    response.Message = "Existe una solicitud enviada anteriormente que no ha sido calificada. Solo puedes tener una en revisión.";
                    break;
                case HttpStatusCode.InternalServerError:
                    response.Success = false;
                    response.Message = "Ocurrió un error con el servidor. Inténtelo más tarde.";
                    break;
            }

            return response;
        }
    }
}