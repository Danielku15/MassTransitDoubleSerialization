using MassTransit;

namespace MassTransitDoubleSerialization;

public class PointActivityArguments
{
    public Point Point { get; set; } = new();
}

public class PointActivity : IExecuteActivity<PointActivityArguments>
{
    public Task<ExecutionResult> Execute(ExecuteContext<PointActivityArguments> context)
    {
        Console.WriteLine("Received point: " + context.Arguments.Point.X + "/" + context.Arguments.Point.Y);
        return Task.FromResult(context.Completed());
    }
}