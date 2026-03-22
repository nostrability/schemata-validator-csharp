using Xunit;
using Nostrability.Schemata.Validator;

public class ValidatorTest {
    [Fact] public void GetSchema() => Assert.NotNull(SchemataValidator.GetSchema("kind1Schema"));
    [Fact] public void GetNonexistent() => Assert.Null(SchemataValidator.GetSchema("nonexistent"));
}
