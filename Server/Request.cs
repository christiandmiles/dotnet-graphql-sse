using GraphQL;

public class Request
{
    public string Query { get; set; }
    public string OperationName { get; set; }
    public Inputs Variables { get; set; }
}