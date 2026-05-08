using System.Net;
using Common.API.Services;
using Common.Data.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/allergies")]
    public class AllergyController : ControllerBase
    {
        private readonly IAllergyService _allergyService;
        private readonly ILogger<AllergyController> _logger;

        public AllergyController(IAllergyService allergyService, ILogger<AllergyController> logger) : base()
        {
            _allergyService = allergyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Allergy>>> GetAllAllergies()
        {
            try
            {
                var result = await _allergyService.GetAllergiesAsync();
                return Ok(result);
            }
            catch (Exception e )
            {
                _logger.LogWarning(e, "Failed to fetch all allergies.");

                return Problem(
                    detail: "Failed to fetch all allergies.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch allergies.");
            }
        }
    }
}
