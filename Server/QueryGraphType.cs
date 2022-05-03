using GraphQL.Types;

namespace Server
{
    public class QueryGraphType: ObjectGraphType
    {
        public QueryGraphType()
        {
            Field<StringGraphType, string>("ApiName")
                .Resolve(_ => "Dotnet GraphQL Server Sent Events Example");
        }
    }
}
