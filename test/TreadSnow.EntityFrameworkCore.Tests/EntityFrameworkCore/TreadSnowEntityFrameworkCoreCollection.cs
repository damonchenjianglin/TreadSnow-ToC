using Xunit;

namespace TreadSnow.EntityFrameworkCore;

[CollectionDefinition(TreadSnowTestConsts.CollectionDefinitionName)]
public class TreadSnowEntityFrameworkCoreCollection : ICollectionFixture<TreadSnowEntityFrameworkCoreFixture>
{

}
