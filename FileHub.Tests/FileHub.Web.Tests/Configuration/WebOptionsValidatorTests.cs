using FileHub.Web.Configuration;

namespace FileHub.Web.Tests.Configuration;

[TestFixture]
public class WebOptionsValidatorTests : TestBase
{
    private WebOptionsValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new WebOptionsValidator();
    }

    [TestCase(null!)]
    [TestCase("")]
    [TestCase(" ")]
    public void Validate_WhenRootUrlInvalid_ShouldReturnFailure(string rootUrl)
    {
        // Arrange
        var options = new WebOptions { RootUrl = rootUrl };

        // Act
        var result = _validator.Validate("test", options);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Failed, Is.True);
            Assert.That(result.FailureMessage, Is.EqualTo($"{nameof(options.RootUrl)} cannot be null or whitespace."));
        }
    }

    [Test]
    public void Validate_WhenRootUrlValid_ShouldReturnSuccess()
    {
        // Arrange
        var options = new WebOptions { RootUrl = "test.root.url" };

        // Act
        var result = _validator.Validate("test", options);

        // Assert
        Assert.That(result.Failed, Is.False);
    }
}
