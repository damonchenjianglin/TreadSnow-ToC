using TreadSnow.Samples;
using Xunit;

namespace TreadSnow.EntityFrameworkCore.Applications;

[Collection(TreadSnowTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<TreadSnowEntityFrameworkCoreTestModule>
{

}
