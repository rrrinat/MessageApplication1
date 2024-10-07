using MessageService.Models;
using Npgsql;

namespace MessageService.DAL
{
    public class MessageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(string connectionString, ILogger<MessageRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void AddMessage(Message message)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    _logger.LogInformation("Connection to database opened successfully.");

                    string query = "INSERT INTO Messages (Text, Timestamp, SequenceNumber) VALUES (@text, @timestamp, @sequenceNumber)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@text", message.Text);
                        command.Parameters.AddWithValue("@timestamp", message.Timestamp);
                        command.Parameters.AddWithValue("@sequenceNumber", message.SequenceNumber);

                        command.ExecuteNonQuery();
                        _logger.LogInformation("Message added to the database successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding the message to the database.");
                }
            }
        }

        public List<Message> GetMessages(DateTime startTime, DateTime endTime)
        {
            var messages = new List<Message>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    _logger.LogInformation("Connection to database opened successfully.");

                    string query = "SELECT Id, Text, Timestamp, SequenceNumber FROM Messages WHERE Timestamp BETWEEN @startTime AND @endTime ORDER BY Timestamp ASC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startTime", startTime);
                        command.Parameters.AddWithValue("@endTime", endTime);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var message = new Message
                                {
                                    Id = reader.GetInt32(0),
                                    Text = reader.GetString(1),
                                    Timestamp = reader.GetDateTime(2),
                                    SequenceNumber = reader.GetInt32(3)
                                };

                                messages.Add(message);
                            }
                        }
                        _logger.LogInformation("Messages retrieved from the database successfully.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while retrieving messages from the database.");
                }
            }

            return messages;
        }
    }
}