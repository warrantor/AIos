using System.Runtime.CompilerServices;
using AIos.Core.Application.Chat;
using AIos.Core.Application.Llm;
using AIos.Sdk.Chat;
using FluentAssertions;
using Moq;

namespace AIos.Core.Tests.Chat;

/// <summary>
///     BDD-style tests for <see cref="ChatOrchestrator" />: Given/When/Then, one assert per test.
///     Shared setup is reused across tests.
/// </summary>
public sealed class ChatOrchestratorTests {
    private static readonly IReadOnlyList<ChatMessage> HistoryWithSystemAndUser = new List<ChatMessage> {
        new(ChatRole.System, "You are helpful"),
        new(ChatRole.User, "Hello")
    };

    private static IReadOnlyList<ChatMessage> Given_message_history_with_one_user_message(string content = "Say hello") {
        return new List<ChatMessage> { new(ChatRole.User, content) };
    }

    private static (IChatOrchestrator orchestrator, Mock<ILlmProvider> providerMock, Mock<ILlmProviderFactory> factoryMock)
        Given_orchestrator_with_mocked_provider() {
        var providerMock = new Mock<ILlmProvider>();
        var factoryMock = new Mock<ILlmProviderFactory>();
        factoryMock.Setup(f => f.GetProvider(It.IsAny<string?>())).Returns(providerMock.Object);
        var orchestrator = new ChatOrchestrator(factoryMock.Object);
        return (orchestrator, providerMock, factoryMock);
    }

    private static (IChatOrchestrator orchestrator, ILlmProvider provider) Given_orchestrator_with_fake_provider_yielding(params string[] tokens) {
        var provider = new FakeLlmProvider(tokens);
        var factory = new FakeLlmProviderFactory(provider);
        return (new ChatOrchestrator(factory), provider);
    }

    [Fact]
    public async Task When_StreamAsync_with_history_Then_factory_receives_provider_id() {
        var (orchestrator, providerMock, factoryMock) = Given_orchestrator_with_mocked_provider();
        providerMock.Setup(p => p.StreamCompleteAsync(It.IsAny<LlmCompletionRequest>(), It.IsAny<CancellationToken>())).Returns(FakeStreamAsync());
        var history = Given_message_history_with_one_user_message();

        await foreach (var _ in orchestrator.StreamAsync(history, "OpenAI")) {
        }

        factoryMock.Verify(f => f.GetProvider("OpenAI"), Times.Once);
    }

    [Fact]
    public async Task When_StreamAsync_with_null_provider_id_Then_factory_receives_null() {
        var (orchestrator, providerMock, factoryMock) = Given_orchestrator_with_mocked_provider();
        providerMock.Setup(p => p.StreamCompleteAsync(It.IsAny<LlmCompletionRequest>(), It.IsAny<CancellationToken>())).Returns(FakeStreamAsync());
        var history = Given_message_history_with_one_user_message();

        await foreach (var _ in orchestrator.StreamAsync(history)) {
        }

        factoryMock.Verify(f => f.GetProvider(null), Times.Once);
    }

    [Fact]
    public async Task When_StreamAsync_Then_provider_StreamCompleteAsync_is_invoked() {
        var (orchestrator, providerMock, _) = Given_orchestrator_with_mocked_provider();
        providerMock
            .Setup(p => p.StreamCompleteAsync(It.IsAny<LlmCompletionRequest>(), It.IsAny<CancellationToken>()))
            .Returns(FakeStreamAsync());
        var history = Given_message_history_with_one_user_message();

        await foreach (var _ in orchestrator.StreamAsync(history)) {
        }

        providerMock.Verify(
            p => p.StreamCompleteAsync(It.IsAny<LlmCompletionRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async IAsyncEnumerable<LlmStreamChunk> FakeStreamAsync([EnumeratorCancellation] CancellationToken ct = default) {
        yield return new LlmStreamChunk("ok");
        await Task.CompletedTask;
    }

    [Fact]
    public async Task When_StreamAsync_Then_yielded_tokens_match_provider_order() {
        string[] tokens = ["Hello", ", ", "world", "!"];
        var (orchestrator, _) = Given_orchestrator_with_fake_provider_yielding(tokens);
        var history = Given_message_history_with_one_user_message();
        var collected = new List<string>();

        await foreach (var token in orchestrator.StreamAsync(history)) {
            collected.Add(token);
        }

        collected.Should().ContainInOrder(tokens);
    }

    [Fact]
    public async Task When_StreamAsync_Then_yielded_count_equals_provider_chunk_count() {
        string[] tokens = ["Hello", ", ", "world", "!"];
        var (orchestrator, _) = Given_orchestrator_with_fake_provider_yielding(tokens);
        var history = Given_message_history_with_one_user_message();
        var collected = new List<string>();

        await foreach (var token in orchestrator.StreamAsync(history)) {
            collected.Add(token);
        }

        collected.Should().HaveCount(tokens.Length);
    }

    [Fact]
    public async Task When_StreamAsync_Then_first_yielded_token_is_first_provider_token() {
        var (orchestrator, _) = Given_orchestrator_with_fake_provider_yielding("first", "second");
        var history = Given_message_history_with_one_user_message();
        string? first = null;

        await foreach (var token in orchestrator.StreamAsync(history)) {
            first = token;
            break;
        }

        first.Should().Be("first");
    }

    [Fact]
    public async Task When_StreamAsync_with_empty_provider_stream_Then_no_tokens_yielded() {
        var (orchestrator, _) = Given_orchestrator_with_fake_provider_yielding();
        var history = Given_message_history_with_one_user_message();
        var collected = new List<string>();

        await foreach (var token in orchestrator.StreamAsync(history)) {
            collected.Add(token);
        }

        collected.Should().BeEmpty();
    }

    private static async Task<LlmCompletionRequest> When_StreamAsync_capturing_provider_request(IReadOnlyList<ChatMessage> history) {
        LlmCompletionRequest? capturedRequest = null;
        var (orchestrator, providerMock, _) = Given_orchestrator_with_mocked_provider();
        providerMock
            .Setup(p => p.StreamCompleteAsync(It.IsAny<LlmCompletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<LlmCompletionRequest, CancellationToken>((req, _) => capturedRequest = req)
            .Returns(FakeStreamAsync());

        await foreach (var _ in orchestrator.StreamAsync(history)) {
        }
        return capturedRequest!;
    }

    [Fact]
    public async Task When_StreamAsync_Then_provider_receives_request_with_same_message_count_as_history() {
        var request = await When_StreamAsync_capturing_provider_request(HistoryWithSystemAndUser);

        request.Messages.Should().HaveCount(2);
    }

    [Fact]
    public async Task When_StreamAsync_Then_provider_receives_first_message_with_role_and_content() {
        var request = await When_StreamAsync_capturing_provider_request(HistoryWithSystemAndUser);

        request.Messages[0].Should().BeEquivalentTo(new LlmMessage("system", "You are helpful"));
    }

    [Fact]
    public async Task When_StreamAsync_Then_provider_receives_second_message_with_role_and_content() {
        var request = await When_StreamAsync_capturing_provider_request(HistoryWithSystemAndUser);

        request.Messages[1].Should().BeEquivalentTo(new LlmMessage("user", "Hello"));
    }

    private sealed class FakeLlmProvider : ILlmProvider {
        private readonly string[] _tokens;

        public FakeLlmProvider(string[] tokens) {
            _tokens = tokens;
        }

        public Task<LlmCompletionResult> CompleteAsync(LlmCompletionRequest request, CancellationToken ct = default) {
            return Task.FromResult(new LlmCompletionResult(string.Concat(_tokens)));
        }

        public async IAsyncEnumerable<LlmStreamChunk> StreamCompleteAsync(LlmCompletionRequest request,
            [EnumeratorCancellation] CancellationToken ct = default) {
            foreach (var t in _tokens) {
                await Task.Yield();
                yield return new LlmStreamChunk(t);
            }
        }
    }

    private sealed class FakeLlmProviderFactory : ILlmProviderFactory {
        private readonly ILlmProvider _provider;

        public FakeLlmProviderFactory(ILlmProvider provider) {
            _provider = provider;
        }

        public ILlmProvider GetProvider(string? providerId = null) {
            return _provider;
        }
    }
}