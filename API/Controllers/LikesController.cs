using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
    private readonly ILikesRepository _likesRepository;
    private readonly IUserRepository _userRepository;
    public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
    {
        _userRepository = userRepository;
        _likesRepository = likesRepository;
    }

    [HttpPost("{username}")]
    public async Task<ActionResult> AddLike(string username)
    {
        // Get the user using our extension method
        var sourceUserId = User.GetUserId();

        // get the liked user
        var likedUser = await _userRepository.GetUserByUsernameAsync(username);

        // Get the user with the list of liked users
        var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

        // IF liked user is null then return not found
        if(likedUser == null) return NotFound();

        // IF the user tries to liked itself then return bad request
        if(sourceUser.UserName == username) return BadRequest("You can't liked yourself");

        // Get the user that is being liked
        var userLike = await _likesRepository.GetUserLike(sourceUserId,likedUser.Id);

        // IF the userlike is not null then you have already liked this user
        if(userLike != null) return BadRequest("You already liked this user.");

        // Create a new user like with the source and target users id
        userLike = new UserLike
        {
            SourceUserId = sourceUserId,
            TargetUserId = likedUser.Id
        };

        // add the userlike to the source user liked user list
        sourceUser.LikedUsers.Add(userLike);

        // SAVE ALL THE CHANGES TO DB
        if(await _userRepository.SaveAllAsync() ) return Ok();

        // Something went wrong liking the user
        return BadRequest("Failed to Like the user");
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<LikeDTO>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {

        likesParams.UserId = User.GetUserId();
        // Get the user's list of users whom have liked him
        var users = await _likesRepository.GetUserLikes(likesParams);

        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,
        users.PageSize,users.TotalCount,users.TotalPages));

        return Ok(users);
    }

    }
}
