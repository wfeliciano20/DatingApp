using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
    public class UsersController: BaseApiController
    {

    private readonly IUserRepository _userRepository;

    private readonly IMapper _mapper;

    private readonly IPhotoServices _photoService;

    public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoServices photoService)
    {
        _photoService = photoService;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<MemberDTO>>> GetUsers([FromQuery]UserParams userParams)
    {
        var currentUser = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        userParams.CurrentUsername = currentUser.UserName;

        if(string.IsNullOrEmpty(userParams.Gender)){
            userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
        }
        var users = await _userRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages));
        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDTO>> getUserById(string username){

        return await _userRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        _mapper.Map(memberUpdateDTO,user);

        if(await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
        // Get the user who is uploading the photo
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if(user == null) return NotFound();

        // use the photoservice to upload file
        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        // Create a photo object
        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        // if the user is uploading its first photo make that one the main photo
        if(user.Photos.Count == 0) photo.IsMain = true;
        // add the new photo to the users photos
        user.Photos.Add(photo);
        // save the changes to the user and map to photoDTO
        if(await _userRepository.SaveAllAsync())
        {
            return CreatedAtAction(nameof(getUserById), new {username  = user.UserName}, _mapper.Map<PhotoDTO>(photo));
        }
        // if there is any error this will run
        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        // get the user
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        // if not found return not found
        if (user == null) return NotFound("Did not found the user.");

        // get the photo  the user wants to main
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        // if not found return not found
        if(photo == null) return NotFound("Did not found the photo.");


        // If phot is main return bad request
        if(photo.IsMain) return BadRequest("This is already your main photo.");

        // look for the main photo
        var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);

        // change the status of the main photo to false
        if(currentMain != null) currentMain.IsMain = false;

        // set the photo we got from the photo id to true
        photo.IsMain = true;

        // save changes to db and return no content
        if(await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("A problem occurred while changing the main photo.");

    }


    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        // Find the user
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        // if not found return not found
        if (user == null) return NotFound("Did not found the user.");

        // See if the user has this photo in its photos collection
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        // if not found return not found
        if(photo == null) return NotFound("Did not found the photo.");

        if(photo.IsMain) return BadRequest("Change the main photo to a different one to be able to delete this one.");

        // Check that the photo is not one of the seeded ones
        if(photo.PublicId != null)
        {
            // Call the deleteAsync to delete phpto from cloudinary
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        // Remove the photo from the user photo collection
        user.Photos.Remove(photo);

        // Try and save the changes
        if(await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting photo.");
    }


    }
}
