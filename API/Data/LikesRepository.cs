using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class LikesRepository : ILikesRepository
  {

    private readonly DataContext _context;

    public LikesRepository(DataContext context)
    {
        _context = context;
    }
    public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
    {
        return await _context.Likes.FindAsync(sourceUserId,targetUserId);
    }

    public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
    {
        // gets the users from the db order by username but
        // asqueryable means it still not executed
        var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();

        var likes = _context.Likes.AsQueryable();

        // check if the predicate is liked to get the users that have
        // been liked by the current userId user
        if(likesParams.Predicate == "liked")
        {
            likes = likes.Where(l=>l.SourceUserId == likesParams.UserId);
            users = likes.Select(l => l.TargetUser);
        }
        // check if the predicate is likedBy to get the users that have
        // liked the current current userId user
        if(likesParams.Predicate == "likedBy")
        {
            likes = likes.Where(l=>l.TargetUserId == likesParams.UserId);
            users = likes.Select(l => l.SourceUser);
        }

        var likedUsers = users.Select(user => new LikeDTO
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Age = user.DateOfBirth.CalculateAge(),
            PhotoUrl = user.Photos.FirstOrDefault(x=> x.IsMain).Url,
            City = user.City,
            Id = user.Id
        });

        return await PagedList<LikeDTO>.CreateAsync(likedUsers,likesParams.PageNumber, likesParams.PageSize);
    }

    public async Task<AppUser> GetUserWithLikes(int userId)
    {
        // Get the user with the likst of the users he has liked
        return await _context.Users
            .Include(x => x.LikedUsers)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }
  }
}
