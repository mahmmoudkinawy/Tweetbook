using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TagsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IMapper _mapper;

    public TagsController(IPostService postService, IMapper mapper)
    {
        _postService = postService;
        _mapper = mapper;
    }

    /// <summary>
    /// Returns all the tags in the system
    /// </summary>
    /// <response code="200">Returns all the tags in the system</response>
    [HttpGet(ApiRoutes.Tags.GetAll)]
    //[Authorize(Policy = "MustWorkForKinawy")]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _postService.GetAllTagsAsync();

        return Ok(_mapper.Map<IEnumerable<TagResponse>>(tags));
    }

    ///// <summary>
    ///// Creates the tags in the system
    ///// </summary>
    ///// <remarks>
    /////     Sample request:
    /////     
    /////         POST /api/v1/tags
    /////          {
    /////            "name":"Some tag" 
    /////          }
    ///// </remarks>
    ///// <response code="200">Creates the tags in the system<summary>   
    ///// <response code="400">Unable to create the tag due to validation errores</response>
    //[HttpPost(ApiRoutes.Tags.Create)]
    //public async Task<IActionResult> CreateTag()
    //{
    //    return Created("", new { });
    //}
}
