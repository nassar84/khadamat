using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

using Khadamat.Application.Common.Models;

namespace Khadamat.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdsController : ControllerBase
{
    [HttpGet("slider")]
    public async Task<IActionResult> GetSliderAds()
    {
        var ads = new List<object>
        {
            new { Id = 1, Title = "خصم خاص للصيانة", ImageUrl = "hero-gradient-1", Description = "خصم 20% على خدمات السباكة" },
            new { Id = 2, Title = "مدرسون متميزون", ImageUrl = "hero-gradient-2", Description = "أفضل المدرسين للمواد العلمية" },
            new { Id = 3, Title = "برمجة تطبيقات", ImageUrl = "hero-gradient-3", Description = "حول فكرتك إلى واقع" }
        };
        
        return Ok(ApiResponse<List<object>>.Succeed(ads));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAd([FromBody] object ad)
    {
        return Ok(ApiResponse<bool>.Succeed(true));
    }
}
