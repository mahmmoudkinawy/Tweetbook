using Tweetbook.Domain;

namespace Tweetbook.Services;
public interface IPostService
{
    Task<Post> GetPostByIdAsync(Guid id);
    Task<List<Post>> GetPostsAsync(GetAllPostsFilter query = null, PaginationFilter paginationFilter = null);
    Task<bool> UpdatePostAsync(Post postToUpdate);
    Task<bool> DeletePostAsync(Guid id);
    Task<bool> CreatePostAsync(Post post);
    Task<bool> UserOwenPostAsync(Guid postId, string userId);
    Task<IEnumerable<Tag>> GetAllTagsAsync();
}
