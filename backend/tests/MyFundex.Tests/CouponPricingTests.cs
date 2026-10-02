using MyFundex.Payments;
using Xunit;
namespace MyFundex.Tests;
public sealed class CouponPricingTests
{
    [Theory]
    [InlineData("Percentage",10,999,99.9)]
    [InlineData("Fixed",200,999,200)]
    [InlineData("Percentage",100,999,998)]
    [InlineData("Fixed",2000,999,998)]
    [InlineData("Percentage",33.33,100,33.33)]
    public void DiscountsUsePaiseAndPreserveMinimumPayable(string type,decimal value,decimal fee,decimal expected)
        =>Assert.Equal(expected,CouponPricing.Discount(type,value,fee));
    [Theory]
    [InlineData("Percentage",101)]
    [InlineData("Fixed",0)]
    [InlineData("Other",1)]
    [InlineData("Fixed",1.001)]
    public void InvalidDiscountsAreRejected(string type,decimal value)
        =>Assert.Throws<ArgumentException>(()=>CouponPricing.Discount(type,value,100));
}
