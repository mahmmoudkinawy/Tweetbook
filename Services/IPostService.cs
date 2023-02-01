using Tweetbook.Domain;

namespace Tweetbook.Services;
public interface IPostService
{
    List<Post> GetPosts();
    Post GetPostById(Guid id);
    bool UpdatePost(Post postToUpdate);
    bool DeletePost(Guid id);
}
