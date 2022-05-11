using System.Collections.Generic;
using System.Collections.Immutable;
using HotChocolate.Language;

namespace HotChocolate.Stitching.Types.Pipeline;

public sealed class SchemaMergeContext : ISchemaMergeContext
{
    public SchemaMergeContext(IReadOnlyList<ServiceConfiguration> configurations)
    {
        Configurations = configurations;
    }

    public IReadOnlyList<ServiceConfiguration> Configurations { get; }

    public IImmutableList<DocumentNode> Documents { get; set; } = ImmutableList<DocumentNode>.Empty;

    public ICollection<IError> Errors { get; } = new List<IError>();
}
