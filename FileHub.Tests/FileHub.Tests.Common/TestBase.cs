using System.Reflection;
using FileHub.Core.Common;
using Moq;

namespace FileHub.Tests.Common;

public abstract class TestBase
{
    [TearDown]
    public void BaseTearDown()
    {
        VerifyAllAndNoOtherCalls();
    }

    protected static void AssertSuccess(Result result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Status, Is.EqualTo(Status.Success));
        }
    }

    protected static void AssertSuccess<T>(Result<T> result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Status, Is.EqualTo(Status.Success));
        }
    }

    protected static void AssertFailed(Result result, string error)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Failed));
            Assert.That(result.Error, Is.EqualTo(error));
        }
    }

    protected static void AssertFailed<T>(Result<T> result, string error)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Failed));
            Assert.That(result.Error, Is.EqualTo(error));
        }
    }

    protected static void AssertInvalid(Result result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Invalid));
        }
    }

    protected static void AssertInvalid<T>(Result<T> result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Invalid));
        }
    }

    protected static void AssertNotFound(Result result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.Invalid));
        }
    }

    protected static void AssertNotFound<T>(Result<T> result)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Status, Is.EqualTo(Status.NotFound));
        }
    }

    private void VerifyAllAndNoOtherCalls()
    {
        var mockFields = GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.FieldType.IsGenericType
                        && f.FieldType.GetGenericTypeDefinition() == typeof(Mock<>));

        foreach (var mockField in mockFields)
        {
            var mock = mockField.GetValue(this);

            if (mock is null)
            {
                continue;
            }

            var type = mock.GetType();

            var verifyAllMethod = type.GetMethod("VerifyAll", Type.EmptyTypes);
            verifyAllMethod?.Invoke(mock, null);

            var verifyNoOtherCallsMethod = type.GetMethod("VerifyNoOtherCalls", Type.EmptyTypes);
            verifyNoOtherCallsMethod?.Invoke(mock, null);
        }
    }
}
