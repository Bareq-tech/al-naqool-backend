using BareqAlNaqool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BareqAlNaqool.Api.Controllers;

[Route("api/home")]
public class HomeController(IHomeService homeService) : ApiControllerBase
{
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        => Ok(await homeService.GetHomeStatsAsync(cancellationToken));

    [HttpGet("latest-news")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLatestNews(CancellationToken cancellationToken)
        => Ok(await homeService.GetLatestNewsAsync(cancellationToken));
}

[Route("api/landing")]
public class LandingController(IHomeService homeService) : ApiControllerBase
{
    [HttpGet("slides")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSlides(CancellationToken cancellationToken)
        => Ok(await homeService.GetLandingSlidesAsync(cancellationToken));
}
