using System.Text;

namespace OneFlowApis.Models;

public sealed record UpstreamResponse(int StatusCode, string Content, string ContentType)
{
    public IResult ToResult()
    {
        return Results.Content(Content, ContentType, Encoding.UTF8, StatusCode);
    }
}
