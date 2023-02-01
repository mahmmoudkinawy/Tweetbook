﻿using Microsoft.EntityFrameworkCore;
using Tweetbook.Data;
using Tweetbook.Domain;

namespace Tweetbook.Services;
public class PostService : IPostService
{
    private readonly DataContext _dataContext;

    public PostService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Post> GetPostByIdAsync(Guid id)
    {
        return await _dataContext.Posts.FindAsync(id);
    }

    public async Task<List<Post>> GetPostsAsync()
    {
        return await _dataContext.Posts.ToListAsync();
    }

    public async Task<bool> CreatePostAsync(Post post)
    {
        await _dataContext.Posts.AddAsync(post);
        var created = await _dataContext.SaveChangesAsync();
        return created > 0;
    }

    public async Task<bool> UpdatePostAsync(Post postToUpdate)
    {
        _dataContext.Posts.Update(postToUpdate);
        var updated = await _dataContext.SaveChangesAsync();
        return updated > 0;
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var post = await GetPostByIdAsync(id);

        if (post == null)
        {
            return false;
        }

        _dataContext.Posts.Remove(post);

        var deleted = await _dataContext.SaveChangesAsync();
        return deleted > 0;
    }

}