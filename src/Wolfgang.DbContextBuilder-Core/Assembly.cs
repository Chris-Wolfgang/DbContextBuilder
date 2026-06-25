using System.Runtime.CompilerServices;
using Wolfgang.DbContextBuilderCore;

// ICreateRandomEntities moved to the Wolfgang.DbContextBuilder.Abstractions package.
// Forward the type so assemblies compiled against earlier versions keep resolving it.
[assembly: TypeForwardedTo(typeof(ICreateRandomEntities))]

[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder.AutoFixture.Tests.Unit")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit-EF6")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit-EF7")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit-EF8")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit-EF9")]
[assembly: InternalsVisibleTo("Wolfgang.DbContextBuilder-Core.Tests.Unit-EF10")]
