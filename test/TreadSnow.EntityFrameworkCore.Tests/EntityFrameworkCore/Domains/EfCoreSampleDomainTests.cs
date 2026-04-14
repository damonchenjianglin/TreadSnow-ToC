using TreadSnow.Samples;
using Xunit;

namespace TreadSnow.EntityFrameworkCore.Domains;

[Collection(TreadSnowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<TreadSnowEntityFrameworkCoreTestModule>
{

}
