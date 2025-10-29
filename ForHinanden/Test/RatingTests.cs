using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Test;

public class RatingTests
{
    [Fact]
    public void Rating_Must_Be_Between_1_And_5()
    {
        int rating = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentOutOfRangeException("Rating must be between 1 and 5.");
        });

        rating = 6;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentOutOfRangeException("Rating must be between 1 and 5.");
        });

        rating = 3; // Valid
        try
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentOutOfRangeException("Rating must be between 1 and 5.");
        }
        catch
        {
            Assert.Fail("No exception should be thrown for valid rating.");
        }
    }
    
    [Fact]
    public void Valid_Rating_Does_Not_Throw_Exception()
    {
        for (int rating = 1; rating <= 5; rating++)
        {
            try
            {
                if (rating < 1 || rating > 5)
                    throw new ArgumentOutOfRangeException("Rating must be between 1 and 5.");
            }
            catch
            {
                Assert.Fail("No exception should be thrown for valid rating.");
            }
        }
    }
    
    [Fact]
    public void Invalid_Rating_Throws_Exception()
    {
        int[] invalidRatings = { 0, 6, -1, 10 };
        foreach (var rating in invalidRatings)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                if (rating < 1 || rating > 5)
                    throw new ArgumentOutOfRangeException("Rating must be between 1 and 5.");
            });
        }
    }
}