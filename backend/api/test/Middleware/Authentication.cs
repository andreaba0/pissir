using Middleware;
using Moq;
using System.Web;
using Microsoft.AspNetCore.Http;
using Extension;

public static class TestAuthentication
{

    [Test]
    public static async Task TestJwtCheck()
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var response = new Mock<HttpResponse>();
        var httpResponseExtension = new Mock<IStaticWrapperHRE>();
        int statusCode = 0;
        string messageResponse = string.Empty;
        context.Setup(x => x.Request).Returns(request.Object);
        context.Setup(x => x.Response).Returns(response.Object);
        context.Setup(x => x.Request.Headers["Authorization"]).Returns(string.Empty);
        context.SetupSet(x => x.Response.StatusCode).Callback((int code) => {
            statusCode = code;
        });
        httpResponseExtension.Setup(x => x.WriteAsync(It.IsAny<HttpResponse>(), It.IsAny<string>())).Callback((HttpResponse response, string message) => {
            messageResponse = message;
        });

        await Authentication.JwtCheck(context.Object, async () =>
        {
            await Task.CompletedTask;
        }, httpResponseExtension.Object);

        Assert.AreEqual(statusCode, 401);
        Assert.AreEqual(messageResponse, "Missing Authorization header");
    }

}