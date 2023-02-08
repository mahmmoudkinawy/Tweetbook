﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Domain;
using Tweetbook.Extenstions;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PostsController : Controller
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet(ApiRoutes.Posts.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _postService.GetPostsAsync());
    }

    [HttpGet(ApiRoutes.Posts.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid postId)
    {
        var post = await _postService.GetPostByIdAsync(postId);

        if (post == null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpPost(ApiRoutes.Posts.Create)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostRequest postRequest)
    {
        var post = new Post
        {
            Name = postRequest.Name,
            UserId = HttpContext.GetUserId()
        };

        await _postService.CreatePostAsync(post);

        var baseUrl =
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";

        var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

        var response = new PostResponse { Id = post.Id };

        return Created(locationUri, response);
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
            return Ok(post);
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
