using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  public class MessagesController : BaseApiController
    {
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(MessageDTO messageDTO)
        {
            // Get the username of the user that is making the request
            var username = User.GetUsername();
            // If the user tries to message itself return bad request
            if(username == messageDTO.RecipientUsername.ToLower())
                return BadRequest("You can't send a message to yourself");
            // get the sender
            var sender = await _userRepository.GetUserByUsernameAsync(username);
            // get the recipient
            var recipient = await _userRepository.GetUserByUsernameAsync(messageDTO.RecipientUsername);

            // If recipient equals null then return not found
            if(recipient == null) return NotFound();

            // Create the new message object
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = messageDTO.Content,
            };

            // add the message to the messages table
            _messageRepository.AddMessage(message);

            // Save the changes and return a message dto
            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

            // Something went wrong
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser(
            [FromQuery] MessageParams messageParams)
        {
            // set the messages params username to the username that made the request
            messageParams.Username = User.GetUsername();

            // Get the user messages
            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            // Create out pagination headers
            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
            messages.PageSize,messages.TotalCount,messages.TotalPages));

            // return the response
            return messages;

        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessageThread(string username)
        {
            // Get the user whom is making the request
            var currentUserName = User.GetUsername();

            // Return the message thread
            return Ok(await _messageRepository.GetMessageThread(currentUserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            // get the username of the user who is trying to delete the message
            var username = User.GetUsername();
            // get the message from the db
            var message = await _messageRepository.GetMessage(id);
            // check if the username is not of the sender or the recipient then return unauthorized
            if(username != message.SenderUsername && username != message.RecipientUsername) return Unauthorized();
            // if the sender is deleting raise the flag that sender is deleting
            if(message.SenderUsername == username) message.SenderDeleted = true;
            // if the recipient is deleting raise the flag that recipient is deleting
            if(message.RecipientUsername == username) message.RecipientDeleted = true;
            // if both the sender and the recipient have deleted then we delete from db
            if(message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            // save changes to the database and return ok
            if(await _messageRepository.SaveAllAsync()) return Ok();

            // Something went wrong
            return BadRequest("Failed to delete message");

        }
    }
}
