using System.Net;
using Common.API.Auth;
using Common.API.Services;
using Common.Data.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Common.API.Controllers
{
    [ApiController]
    [Route("api/allergies")]
    [AuthorizeRole("Admin", "Medic")]
    public class AllergyController : ControllerBase
    {
        private readonly IAllergyService allergyService;
        private readonly ILogger<AllergyController> logger;

        public AllergyController(IAllergyService allergyService, ILogger<AllergyController> logger) : base()
        {
            this.allergyService = allergyService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Allergy>>> GetAllAllergies()
        {
            try
            {
                var result = await allergyService.GetAllergiesAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to fetch all allergies.");

                return Problem(
                    detail: "Failed to fetch all allergies.",
                    statusCode: (int)HttpStatusCode.InternalServerError,
                    title: "Could not fetch allergies.");
            }
        }
    }
}
