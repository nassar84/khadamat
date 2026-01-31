using System.Security.Claims;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using Khadamat.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly KhadamatDbContext _context;
    private readonly IHubContext<ChatHub> _chatHub;

    public MessagesController(KhadamatDbContext context, IHubContext<ChatHub> chatHub)
    {
        _context = context;
        _chatHub = chatHub;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ConversationDto>>>> GetConversations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var conversationData = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new 
            {
                OtherUserId = g.Key,
                LastMessage = g.OrderByDescending(m => m.CreatedAt).First(),
                UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
            })
            .ToListAsync();

        var conversations = new List<ConversationDto>();
        var userIds = conversationData.Select(c => c.OtherUserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new { u.UserName, ImageUrl = u.ProfileImageUrl }); // Use ProfileImageUrl

        foreach (var conv in conversationData.OrderByDescending(x => x.LastMessage.CreatedAt))
        {
            var name = users.ContainsKey(conv.OtherUserId) ? users[conv.OtherUserId].UserName : "Unknown";
            // var photo = users.ContainsKey(conv.OtherUserId) ? users[conv.OtherUserId].ImageUrl : null;

            conversations.Add(new ConversationDto
            {
                OtherPartyId = conv.OtherUserId,
                OtherPartyName = name ?? "User",
                LastMessage = conv.LastMessage.Content,
                LastMessageTime = conv.LastMessage.CreatedAt,
                UnreadCount = conv.UnreadCount
            });
        }

        return Ok(ApiResponse<List<ConversationDto>>.Succeed(conversations));
    }

    [HttpGet("{otherUserId}")]
    public async Task<ActionResult<ApiResponse<List<MessageDto>>>> GetMessages(string otherUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var messages = await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        // Mark as read (simple approach: mark all from otherUser as read)
        var unread = await _context.Messages
            .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
            .ToListAsync();
        
        if (unread.Any())
        {
            foreach (var m in unread) m.MarkAsRead();
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<List<MessageDto>>.Succeed(messages));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var message = new Message(userId, request.ReceiverId, request.Content);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var dto = new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };

        // Real-time Push
        await _chatHub.Clients.Group(request.ReceiverId).SendAsync("ReceiveMessage", dto);

        return Ok(ApiResponse<MessageDto>.Succeed(dto));
    }
}
