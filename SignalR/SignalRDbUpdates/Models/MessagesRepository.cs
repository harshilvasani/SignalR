using SignalRDbUpdates.Hubs;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SignalRDbUpdates.Models
{
    public class MessagesRepository
    {
        private readonly string _connString =
            ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public IEnumerable<Messages> GetAllMessages()
        {
            var messages = new List<Messages>();
            using (var connection = new SqlConnection(_connString))
            {
                connection.Open();
                using (var command = new SqlCommand(@"SELECT [id],[message] FROM [hv].[dbo].[notification]", connection))
                {
                    command.Notification = null;

                    var dependency = new SqlDependency(command);
                    dependency.OnChange += dependency_OnChange;

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        messages.Add(item: new Messages
                        {
                            Id = (int)reader["id"],
                            Message = (string)reader["message"],
                        });
                    }
                }
            }
            return messages;
        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                MessagesHub.SendMessages("");
            }
        }
    }
}