using GraphQL;
using GraphQL.Types;
using Server;
using GraphQL.SystemTextJson;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

var schema = new Schema {
    Query = new QueryGraphType(),
    Subscription = new SubscriptionGraphType()
};
var serializer = new GraphQLSerializer();
var documentExecuter = new DocumentExecuter();

app.MapPost("/gql/streams", async (HttpResponse response, Request request) =>
{
    response.Headers.CacheControl = "no-cache";
    response.Headers["X-Accel-Buffering"] = "no";
    response.ContentType = "text/event-stream";
    await response.Body.FlushAsync();

    var result = await documentExecuter.ExecuteAsync(options =>
    {
        options.Schema = schema;
        options.Query = request.Query;
        options.Variables = request.Variables;
        options.OperationName = request.OperationName;
    });

    if (result.Errors != null || result.Streams == null)
    {
        await response.WriteAsync($"event: error\n");
        await response.WriteAsync($"data: {serializer.Serialize(result)}\n\n");
        await response.CompleteAsync();
        return;
    }
    var obs = result.Streams.First().Value; // per spec there can only be 1 stream

    var buffer = new ActionBlock<ExecutionResult>(async data =>
    {
        await response.WriteAsync($"data: {serializer.Serialize(data)}\n\n");
        await response.Body.FlushAsync();
    });

    using var subscription = obs.Subscribe(buffer.AsObserver());
    await buffer.Completion;
    await response.CompleteAsync();
});

app.MapPost("/gql", async (Request request) =>
{
    return Results.Content(await schema.ExecuteAsync(serializer, options => {
        options.Query = request.Query;
        options.Variables = request.Variables;
        options.OperationName = request.OperationName;
    }), "application/json");
});

app.UseCors();

app.Run();
