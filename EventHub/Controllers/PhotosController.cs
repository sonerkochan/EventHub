using EventHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers
{
    [AllowAnonymous]
    [Route("photos")]
    public class PhotosController(
        ApplicationDbContext context) : Controller
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var photo = await context.CoverPhotos.FirstOrDefaultAsync(photo => photo.Id == id);

            if (photo == null)
            {
                return NotFound();
            }

            return File(photo.Data, photo.ContentType);
        }
    }
}
