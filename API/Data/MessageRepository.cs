using API.DTOs;
using API.Entities;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
      _mapper = mapper;
        _context = context;
    }
    public void AddMessage(Message message)
    {
        _context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        _context.Messages.Remove(message);
    }

    public async Task<Message> GetMessage(int id)
    {
        return await _context.Messages.FindAsync(id);
    }

    public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
    {
      var query = _context.Messages
        .OrderByDescending(m=>m.MessageSent).AsQueryable();

      query = messageParams.Container switch
      {
        "Inbox" => query.Where(u=>u.RecipientUsername == messageParams.Username
        && u.RecipientDeleted == false),
        "Outbox" => query.Where(u=>u.SenderUsername == messageParams.Username
        && u.SenderDeleted == false),
        _ => query.Where(u=>u.RecipientUsername == messageParams.Username && u.DateRead == null
        && u.RecipientDeleted == false)
      };

      var messages =query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

      return await PagedList<MessageDTO>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);
    }

    public async  Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
    {
      var messages = await _context.Messages
      .Include(u=> u.Sender).ThenInclude(u=>u.Photos)
      .Include(u=> u.Recipient).ThenInclude(u=>u.Photos)
      .Where(
        // We are going to look for messages where the user is the recipient
        // and where the user is the sender
        m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false &&
        m.SenderUsername == recipientUserName ||
        m.RecipientUsername == recipientUserName && m.SenderDeleted == false &&
        m.SenderUsername == currentUserName
      ).OrderBy(m => m.MessageSent)
      .ToListAsync();

      // get the messages that are unread and store then in a new list
      var unreadMessages = messages.Where(m => m.DateRead == null
      && m.RecipientUsername == currentUserName).ToList();

      // if there are any unread messages
      if(unreadMessages.Any())
      {
        // loop over the unread messages and update the date read to UTC current time
        foreach(var message in unreadMessages)
        {
          message.DateRead = DateTime.UtcNow;
        }
        // save the updates of date read to the db
        await _context.SaveChangesAsync();
      }
      // return a list of the mapping of messages to messageDTO
      return _mapper.Map<IEnumerable<MessageDTO>>(messages);
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
  }
}
