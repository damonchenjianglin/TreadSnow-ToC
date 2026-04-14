using TreadSnow.Books;
using Xunit;

namespace TreadSnow.EntityFrameworkCore.Applications.Books;

[Collection(TreadSnowTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<TreadSnowEntityFrameworkCoreTestModule>
{

}