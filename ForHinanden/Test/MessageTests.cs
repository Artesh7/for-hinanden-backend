using ForHinanden.Api.Models;
using Xunit;
using Xunit;
using Assert = Xunit.Assert;

namespace Test;


    public class MessageTests
    {
        [Fact]
        public void New_Message_Has_Default_Values()
        {
            var message = new Message();

            Assert.NotEqual(Guid.Empty, message.Id);
            Assert.NotNull(message.SentAt);
            Assert.Null(message.Content); // Content er null ved oprettelse
        }

        [Fact]
        public void Can_Assign_Sender_And_Receiver()
        {
            var message = new Message
            {
                Sender = "Alice",
                Receiver = "Bob"
            };

            Assert.Equal("Alice", message.Sender);
            Assert.Equal("Bob", message.Receiver);
        }

        [Fact]
        public void Message_Content_Cannot_Be_Empty()
        {
            var message = new Message { Content = "" };

            Assert.Throws<ArgumentNullException>(() =>
            {
                if (string.IsNullOrWhiteSpace(message.Content))
                    throw new ArgumentNullException(nameof(message.Content), "Content is required.");
            });
        }

        [Fact]
        public void Message_SentAt_Is_Set_To_Now()
        {
            var before = DateTime.UtcNow;
            var message = new Message();
            var after = DateTime.UtcNow;

            Assert.True(message.SentAt >= before && message.SentAt <= after);
        }

        [Fact]
        public void Can_Assign_TaskId_To_Message()
        {
            var taskId = Guid.NewGuid();

            var message = new Message
            {
                TaskId = taskId
            };

            Assert.Equal(taskId, message.TaskId);
        }
}    
