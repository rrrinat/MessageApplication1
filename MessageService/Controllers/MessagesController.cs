using MessageService.DAL;
using MessageService.Models;
using Microsoft.AspNetCore.Mvc;

namespace MessageService.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageRepository _messageRepository;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(MessageRepository messageRepository, ILogger<MessagesController> logger)
        {
            _messageRepository = messageRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrEmpty(message.Text))
            {
                _logger.LogWarning("Invalid message received: Text is missing.");
                return BadRequest("Message text is required.");
            }

            if (message.Text.Length > 128)
            {
                _logger.LogWarning("Invalid message received: Text exceeds length limit.");
                return BadRequest("Message text cannot exceed 128 characters.");
            }

            if (message.SequenceNumber <= 0)
            {
                _logger.LogWarning("Invalid message received: Sequence number must be greater than zero.");
                return BadRequest("Sequence number must be greater than zero.");
            }

            message.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Adding message to repository.");
            _messageRepository.AddMessage(message);

            var messageJson = System.Text.Json.JsonSerializer.Serialize(message);
            await WebSocketHandler.SendMessageToAllAsync(messageJson);
            _logger.LogInformation("Message broadcasted to all WebSocket clients.");

            return Ok();
        }

        [HttpGet]
        public IActionResult GetMessages([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            if (startTime == default || endTime == default)
            {
                _logger.LogWarning("Invalid time range received: StartTime or EndTime is missing.");
                return BadRequest("StartTime and EndTime are required.");
            }

            _logger.LogInformation("Retrieving messages from repository.");
            var messages = _messageRepository.GetMessages(startTime, endTime);

            return Ok(messages);
        }
    }
}
