using betterme_web_gui.Services;
using betterme_web_gui.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace betterme_web_gui.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationRequestsController : ControllerBase
    {
        private readonly VerificationRequestsService _service;
        private readonly ILogger<VerificationRequestsController> _logger;

        public VerificationRequestsController(ILogger<VerificationRequestsController> logger, VerificationRequestsService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVerificationRequest([FromForm] VerificationRequestDTO request)
        {
            Response<VerificationRequestDTO> response;

            if (request.IsValid())
            {
                response = await _service.AddVerificationRequest(request);
            }
            else
            {
                response = new()
                {
                    Success = false,
                    Message = "Campos inválidos"
                };
                _logger.LogWarning("Controller request with empty fields: {request}", request);
            }
            
            return new JsonResult(response); 
        }
    }
}