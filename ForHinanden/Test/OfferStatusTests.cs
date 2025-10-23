using ForHinanden.Api.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace Test;

public class OfferStatusTests
{
    [Fact]
    public void OfferStatus_Enum_Has_Expected_Values()
    {
        Assert.Equal(0, (int)OfferStatus.Pending);
        Assert.Equal(1, (int)OfferStatus.Accepted);
        Assert.Equal(2, (int)OfferStatus.Rejected);
    }
    
    [Fact]
    public void Can_Convert_OfferStatus_To_String()
    {
        Assert.Equal("Pending", OfferStatus.Pending.ToString());
        Assert.Equal("Accepted", OfferStatus.Accepted.ToString());
        Assert.Equal("Rejected", OfferStatus.Rejected.ToString());
    }
    
    [Fact]
    public void Can_Parse_String_To_OfferStatus()
    {
        Assert.Equal(OfferStatus.Pending, Enum.Parse<OfferStatus>("Pending"));
        Assert.Equal(OfferStatus.Accepted, Enum.Parse<OfferStatus>("Accepted"));
        Assert.Equal(OfferStatus.Rejected, Enum.Parse<OfferStatus>("Rejected"));
    }
    
    [Fact]
    public void Invalid_String_Throws_Exception_On_Parse()
    {
        Assert.Throws<ArgumentException>(() => Enum.Parse<OfferStatus>("InvalidStatus"));
    }
}