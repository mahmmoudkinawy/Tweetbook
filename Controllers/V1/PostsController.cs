using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Cache;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extenstions;
using Tweetbook.Services;
using Tweetbook.SwaggerExamples.Responses;

namespace Tweetbook.Controllers.V1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly IMapper _mapper;

    public PostsController(IPostService postService, IMapper mapper)
    {
        _postService = postService;
        _mapper = mapper;
    }

    [HttpGet(ApiRoutes.Posts.GetAll)]
    [Cached(60)]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _postService.GetPostsAsync();
        return Ok(_mapper.Map<IEnumerable<PostResponse>>(posts));
    }

    /// <summary>
    /// Returns a post if if exists
    /// </summary>
    /// <param name="postId">Guid Id</param>
    /// <returns></returns>
    [HttpGet(ApiRoutes.Posts.Get)]
    [Cached(60)]
    [ProducesResponseType(typeof(PostResponseExample), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid postId)
    {
        var post = await _postService.GetPostByIdAsync(postId);

        if (post == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PostResponse>(post));
    }

    [HttpPost(ApiRoutes.Posts.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostRequest postRequest)
    {
        var post = new Post
        {
            Name = postRequest.Name,
            UserId = HttpContext.GetUserId(),
            Tags = postRequest.Tags.Select(a => new Tag
            {
                Id = Guid.NewGuid(),
                Name = a
            }).ToList()
        };

        await _postService.CreatePostAsync(post);

        var baseUrl =
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

        return Created(locationUri, _mapper.Map<PostResponse>(post));
    }

    [HttpPut(ApiRoutes.Posts.Update)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid postId,
        [FromBody] UpdatePostRequest request)
    {
        var userOwenPost = await _postService
            .UserOwenPostAsync(postId, HttpContext.GetUserId());

        if (!userOwenPost)
        {
            return BadRequest(new
            {
                error = "You do not owen this post"
            });
        }

        var post = await _postService.GetPostByIdAsync(postId);
        post.Name = request.Name;

        var updated = await _postService.UpdatePostAsync(post);

        if (updated)
        {
            return Ok(_mapper.Map<PostResponse>(post));
        }

        return NotFound();
    }

    [HttpDelete(ApiRoutes.Posts.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid postId)
    {
        var userOwenPost = await _postService
            .UserOwenPostAsync(postId, HttpContext.GetUserId());

        if (!userOwenPost)
        {
            return BadRequest(new
            {
                error = "You do not owen this post"
            });
        }

        var deleted = await _postService.DeletePostAsync(postId);

        if (deleted)
        {
            return NoContent();
        }

        return NotFound();
    }

}
