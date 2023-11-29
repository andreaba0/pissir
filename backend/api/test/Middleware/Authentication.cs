using Middleware;
using Moq;
using System.Web;
using Microsoft.AspNetCore.Http;

public static class TestAuthentication
{

    [Test]
    public static async Task TestJwtCheck()
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        int statusCode = 0;
        string messageResponse = string.Empty;
        context.Setup(x => x.Request).Returns(request.Object);
        context.Setup(x => x.Request.Headers["Authorization"]).Returns(string.Empty);
        context.SetupSet(x => x.Response.StatusCode).Callback((int code) => {
            statusCode = code;
        });

        await Authentication.JwtCheck(context.Object, async () =>
        {
            await Task.CompletedTask;
        });

        Assert.AreEqual(statusCode, 401);
    }

}