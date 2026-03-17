# AIos.Core.Tests

Unit tests for AIos.Core, using **xUnit**, **FluentAssertions**, and **Moq**.

## Behaviour-driven design (BDD)

Tests follow a **Given / When / Then** style:

- **Given** — setup (helpers named `Given_...` or shared data).
- **When** — the action under test (often in the test body or a `When_...` helper).
- **Then** — a single assertion (one `Should()` or `Verify()` per test).

### Rules

1. **One assert per test** — each test has exactly one logical assertion. To check multiple outcomes, reuse the same setup in several tests.
2. **Reuse setup** — shared context (e.g. `Given_orchestrator_with_mocked_provider()`, `When_StreamAsync_capturing_provider_request(history)`) is used across tests to avoid duplication.
3. **Test names** — name tests as `When_<action>_Then_<expected_outcome>` so the scenario is clear.

### Example

```csharp
[Fact]
public async Task When_StreamAsync_with_history_Then_factory_receives_provider_id()
{
    var (orchestrator, providerMock, factoryMock) = Given_orchestrator_with_mocked_provider();
    providerMock.Setup(...).Returns(FakeStreamAsync());
    var history = Given_message_history_with_one_user_message();

    await foreach (var _ in orchestrator.StreamAsync(history, "OpenAI")) { }

    factoryMock.Verify(f => f.GetProvider("OpenAI"), Times.Once);
}
```
