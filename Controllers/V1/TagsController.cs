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

    [HttpGet(ApiRoutes.Tags.GetAll)]
    //[Authorize(Policy = "MustWorkForKinawy")]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _postService.GetAllTagsAsync();

        return Ok(_mapper.Map<IEnumerable<TagResponse>>(tags));
    }
}
