using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tweetbook.Contracts.V1.Requests.Queries;
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

    public async Task<List<Post>> GetPostsAsync(
        GetAllPostsFilter query = null,
        PaginationFilter paginationFilter = null)
    {
        var queryable = _dataContext.Posts.AsQueryable();

        if (query == null)
        {
            return await queryable
                .ToListAsync();
        }

        queryable = AddFiltersQuery(query, queryable);

        var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

        return await queryable
            .Skip(skip)
            .Take(paginationFilter.PageSize)
            .ToListAsync();
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

    public async Task<bool> UserOwenPostAsync(Guid postId, string userId)
    {
        return await _dataContext.Posts.AnyAsync(
            a =>
                a.Id == postId &&
                a.UserId == userId
               );
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _dataContext.Tags.ToListAsync();
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

    private static IQueryable<Post> AddFiltersQuery(GetAllPostsFilter query, IQueryable<Post> queryable)
    {
        if (!string.IsNullOrEmpty(query.UserId))
        {
            queryable = queryable.Where(a => a.UserId == query.UserId);
        }

        return queryable;
    }

}
