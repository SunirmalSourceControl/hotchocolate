using System.Linq;
using HotChocolate.Language;
using HotChocolate.Types;
using Snapshooter.Xunit;
using Xunit;

namespace HotChocolate.Caching.Tests;

public class CacheControlDirectiveTypeTests
{
    [Fact]
    public void CreateCacheControlDirective()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("Query")
                .Field("field")
                .Type<StringType>())
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();
        CacheControlDirectiveType directive =
            schema.DirectiveTypes.OfType<CacheControlDirectiveType>().FirstOrDefault()!;

        Assert.NotNull(directive);
        Assert.IsType<CacheControlDirectiveType>(directive);
        Assert.Equal("cacheControl", directive.Name);
        Assert.Collection(directive.Arguments,
            t =>
            {
                Assert.Equal("maxAge", t.Name);
                Assert.IsType<IntType>(t.Type);
            },
            t =>
            {
                Assert.Equal("scope", t.Name);
                Assert.IsType<CacheControlScopeType>(t.Type);
            },
            t =>
            {
                Assert.Equal("inheritMaxAge", t.Name);
                Assert.IsType<BooleanType>(t.Type);
            });
        Assert.Collection(directive.Locations,
            t => Assert.Equal(Types.DirectiveLocation.Object, t),
            t => Assert.Equal(Types.DirectiveLocation.FieldDefinition, t),
            t => Assert.Equal(Types.DirectiveLocation.Interface, t),
            t => Assert.Equal(Types.DirectiveLocation.Union, t));
    }

    [Fact]
    public void CacheControlDirective_Cannot_Be_Applied_Multiple_Times()
    {
        ISchemaBuilder builder = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("ObjectType")
                .Field("field")
                .Type<StringType>()
                .CacheControl(500)
                .CacheControl(1000))
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _);

        var act = () => builder.Create();

        var expectedException = Assert.Throws<SchemaException>(act);
        expectedException.Message.MatchSnapshot();
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectField_CodeFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("ObjectType")
                .Field("field")
                .Type<StringType>()
                .CacheControl(500, CacheControlScope.Private, true))
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Fields["field"].Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectField_SchemaFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddDocumentFromString(
            @"
            type Query {
                field: ObjectType
            }
            
            type ObjectType {
                field: String @cacheControl(maxAge: 500 scope: PRIVATE inheritMaxAge: true)
            }
            ")
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Fields["field"].Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectField_Annotation()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType<ObjectQuery>()
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Fields["field"].Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectType_CodeFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("ObjectType")
                .CacheControl(500, CacheControlScope.Private, true)
                .Field("field")
                .Type<StringType>())
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectType_SchemaFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddDocumentFromString(
            @"
            type Query {
                field: ObjectType
            }
            
            type ObjectType @cacheControl(maxAge: 500 scope: PRIVATE inheritMaxAge: true) {
                field: String
            }
            ")
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_ObjectType_Annotation()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType<ObjectQuery>()
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        ObjectType type = schema.GetType<ObjectType>("ObjectType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceField_CodeFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("Query")
                .Field("field")
                .Type<StringType>())
            .AddInterfaceType(d => d
                .Name("InterfaceType")
                .Field("field")
                .Type<StringType>()
                .CacheControl(500, CacheControlScope.Private, true))
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Fields["field"].Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceField_SchemaFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddDocumentFromString(
            @"
            type Query {
                field: InterfaceType
            }

            interface InterfaceType {
                field: String @cacheControl(maxAge: 500 scope: PRIVATE inheritMaxAge: true)
            }
            
            type ObjectType implements InterfaceType {
                field: String
            }
            ")
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Fields["field"].Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceField_Annotation()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType<InterfaceQuery>()
            .AddType<InterfaceObjectType>()
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceType_CodeFirst()
    {
        // todo: unrelated to the feature, but shouldn't this crash since there
        //       is no object type implementing the interface?
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("Query")
                .Field("field")
                .Type<StringType>())
            .AddInterfaceType(d => d
                .Name("InterfaceType")
                .CacheControl(500, CacheControlScope.Private, true)
                .Field("field")
                .Type<StringType>())
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceType_SchemaFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddDocumentFromString(
            @"
            type Query {
                field: InterfaceType
            }

            interface InterfaceType @cacheControl(maxAge: 500 scope: PRIVATE inheritMaxAge: true) {
                field: String
            }
            
            type ObjectType implements InterfaceType {
                field: String
            }
            ")
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_InterfaceType_Annotation()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType<InterfaceQuery>()
            .AddType<InterfaceObjectType>()
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        InterfaceType type = schema.GetType<InterfaceType>("InterfaceType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_UnionType_CodeFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType(d => d
                .Name("Query")
                .Field("field")
                .Type<StringType>())
            .AddUnionType(d => d
                .Name("UnionType")
                .CacheControl(500, CacheControlScope.Private, true)
                .Type(new NamedTypeNode("ObjectType")))
            .AddObjectType(d => d
                .Name("ObjectType")
                .Field("field")
                .Type<StringType>())
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        UnionType type = schema.GetType<UnionType>("UnionType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_UnionType_SchemaFirst()
    {
        ISchema schema = SchemaBuilder.New()
            .AddDocumentFromString(
            @"
            type Query {
                field: UnionType
            }
            
            union UnionType @cacheControl(maxAge: 500 scope: PRIVATE inheritMaxAge: true) = ObjectType

            type ObjectType {
                field: String
            }
            ")
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        UnionType type = schema.GetType<UnionType>("UnionType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [Fact]
    public void CacheControlDirectiveType_UnionType_Annotation()
    {
        ISchema schema = SchemaBuilder.New()
            .AddQueryType<UnionQuery>()
            .AddType<UnionObjectType>()
            .AddDirectiveType<CacheControlDirectiveType>()
            .Use(_ => _)
            .Create();

        UnionType type = schema.GetType<UnionType>("UnionType");
        IDirective directive = type.Directives
            .Single(d => d.Name == "cacheControl");
        CacheControlDirective obj = directive.ToObject<CacheControlDirective>();

        Assert.Equal(500, obj.MaxAge);
        Assert.Equal(CacheControlScope.Private, obj.Scope);
        Assert.Equal(true, obj.InheritMaxAge);
    }

    [ObjectType("ObjectType")]
    [CacheControl(500, Scope = CacheControlScope.Private, InheritMaxAge = true)]
    public class ObjectQuery
    {
        [CacheControl(500, Scope = CacheControlScope.Private, InheritMaxAge = true)]
        public string? Field { get; set; }
    }

    [InterfaceType("InterfaceType")]
    [CacheControl(500, Scope = CacheControlScope.Private, InheritMaxAge = true)]
    public interface IInterfaceType
    {
        [CacheControl(500, Scope = CacheControlScope.Private, InheritMaxAge = true)]
        public string? Field { get; set; }
    }

    public class InterfaceObjectType : IInterfaceType
    {
        public string? Field { get; set; }
    }

    public class InterfaceQuery
    {
        public IInterfaceType? GetField() => null;
    }

    [UnionType("UnionType")]
    [CacheControl(500, Scope = CacheControlScope.Private, InheritMaxAge = true)]
    public interface IUnionType
    {
    }

    public class UnionObjectType : IUnionType
    {
        public string? Field { get; set; }
    }

    public class UnionQuery
    {
        public IUnionType? GetField() => null;
    }
}
