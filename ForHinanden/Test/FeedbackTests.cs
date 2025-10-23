using ForHinanden.Api.Models;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Test;

public class FeedbackTests
{
        [Fact]
        public void Feedback_DeviceId_Cannot_Be_Empty()
        {
            var feedback = new Feedback { Rating = 3 };
            Assert.Throws<ArgumentNullException>(() => 
            {
                if (string.IsNullOrWhiteSpace(feedback.DeviceId))
                    throw new ArgumentNullException("DeviceId er påkrævet.");
            });
        }
        
        [Fact]
        public void Feedback_Rating_Must_Be_Between_1_And_5()
        {
            var feedback = new Feedback { DeviceId = "test" };

            Assert.Throws<ArgumentOutOfRangeException>(() => 
            {
                feedback.Rating = 0; // Invalid
                if (feedback.Rating < 1 || feedback.Rating > 5)
                    throw new ArgumentOutOfRangeException("Bedømmelse skal være mellem 1 og 5.");
            });

            Assert.Throws<ArgumentOutOfRangeException>(() => 
            {
                feedback.Rating = 6; // Invalid
                if (feedback.Rating < 1 || feedback.Rating > 5)
                    throw new ArgumentOutOfRangeException("Bedømmelse skal være mellem 1 og 5.");
            });
        }
        
}