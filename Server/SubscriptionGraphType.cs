using GraphQL.Resolvers;
using GraphQL.Types;
using System.Reactive.Linq;

namespace Server
{
    public class SubscriptionGraphType : ObjectGraphType
    {
        public SubscriptionGraphType()
        {
            AddField(new FieldType
            {
                Name = "messageAdded",
                Type = typeof(StringGraphType),
                Resolver = new FuncFieldResolver<string>(ctx => ctx.Source as string),
                StreamResolver = new SourceStreamResolver<string>(ctx => Observable
                                    .Interval(TimeSpan.FromSeconds(1))
                                    .Select(_ => DateTime.Now.ToString())
                                 )
            });

            AddField(new FieldType
            {
                Name = "fasterMessageAdded",
                Type = typeof(StringGraphType),
                Resolver = new FuncFieldResolver<string>(ctx => ctx.Source as string),
                StreamResolver = new SourceStreamResolver<string>(ctx => Observable
                                    .Interval(TimeSpan.FromSeconds(0.5))
                                    .Select(_ => DateTime.Now.ToString())
                                 )
            });
        }
    }
}
