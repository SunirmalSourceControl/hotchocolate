using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;

namespace HotChocolate.Stitching.Schemas.Customers;

public class ConsultantType : ObjectType<Consultant>
{
    protected override void Configure(
        IObjectTypeDescriptor<Consultant> descriptor)
    {
        descriptor
            .ImplementsNode()
            .IdField(t => t.Id)
            .ResolveNode((ctx, id) => Task.FromResult(
                ctx.Service<CustomerRepository>().Consultants
                    .Find(t => t.Id.Equals(id))));

        descriptor.Field(t => t.Name).Type<NonNullType<StringType>>();
        descriptor.Field("customers")
            .UsePaging<CustomerType>()
            .Resolve(ctx =>
            {
                Consultant consultant = ctx.Parent<Consultant>();
                return ctx.Service<CustomerRepository>().Customers
                    .Where(t => t.ConsultantId == consultant.Id);
            });
    }
}
