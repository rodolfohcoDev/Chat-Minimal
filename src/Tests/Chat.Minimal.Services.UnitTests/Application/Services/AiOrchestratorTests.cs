using System;
using System.Threading;
using System.Threading.Tasks;
using Chat.Minimal.IAs.Services.Domain.Interfaces;
using Chat.Minimal.IAs.Services.DTOs;
using Chat.Minimal.Services.Application.Services;
using Chat.Minimal.Services.Domain.Entities;
using Chat.Minimal.Services.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chat.Minimal.Services.UnitTests.Application.Services;

public class AiOrchestratorTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<ILogger<AiOrchestrator>> _mockLogger;
    private readonly AiOrchestrator _orchestrator;
    private readonly SqliteConnection _connection;

    public AiOrchestratorTests()
    {
        // Usar SQLite In-Memory com conexão compartilhada
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new Chat.Minimal.Services.UnitTests.Infrastructure.Data.TestApplicationDbContext(options);
        _context.Database.EnsureCreated(); // Criar tabelas

        _mockLlmService = new Mock<ILlmService>();
        _mockLogger = new Mock<ILogger<AiOrchestrator>>();

        _orchestrator = new AiOrchestrator(
            _mockLlmService.Object,
            _context,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateResponseAsync_WithValidConfig_ShouldReturnResponseAndLogSuccess()
    {
        // Arrange
        var config = new AiConfig
        {
            Name = "Valid Config",
            Provider = "TestProvider",
            Model = "TestModel",
            ApiKey = "TestKey",
            IsActive = true,
            TokenLimit = 1000,
            TokensUsed = 0,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        _context.AiConfigs.Add(config);
        await _context.SaveChangesAsync();

        var expectedResponse = "AI Response Content";
        _mockLlmService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<AiProviderConfig>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _orchestrator.GenerateResponseAsync("conv-1", "Qual é a capital?", "user-1");

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(expectedResponse);
        result.Provider.Should().Be("TestProvider");
        result.Model.Should().Be("TestModel");
        result.InputTokens.Should().BeGreaterThan(0);
        result.OutputTokens.Should().BeGreaterThan(0);

        // Verificar Log no Banco
        var log = await _context.AiInteractionLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.ConversationId.Should().Be("conv-1");
        log.Status.Should().Be("Success");
        log.ResponseContent.Should().Be(expectedResponse);
        
        // Verificar Uso Atualizado
        var updatedConfig = await _context.AiConfigs.FirstAsync();
        updatedConfig.TokensUsed.Should().BeGreaterThan(0);

        // Verificar passagem de configuração correta para o serviço
        _mockLlmService.Verify(x => x.GenerateResponseAsync(
            "conv-1",
            "Qual é a capital?",
            It.IsAny<string>(),
            It.Is<AiProviderConfig>(c => c.ApiKey == "TestKey" && c.Provider == "TestProvider"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenConfigExpired_ShouldThrowException()
    {
        // Arrange
        var config = new AiConfig
        {
            Name = "Expired Config",
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            // Campos obrigatórios para SQLite
            Provider = "Test",
            Model = "Test",
            ApiKey = "Test" 
        };
        _context.AiConfigs.Add(config);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _orchestrator.GenerateResponseAsync("conv-1", "test");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expirou*");
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenTokenLimitReached_ShouldThrowException()
    {
        // Arrange
        var config = new AiConfig
        {
            Name = "Limit Config",
            IsActive = true,
            TokenLimit = 100,
            TokensUsed = 100,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Provider = "Test",
            Model = "Test",
            ApiKey = "Test"
        };
        _context.AiConfigs.Add(config);
        await _context.SaveChangesAsync();

        // Act
        Func<Task> act = async () => await _orchestrator.GenerateResponseAsync("conv-1", "test");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Limite de tokens atingido*");
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenNoActiveConfig_ShouldThrowException()
    {
        // Arrange
        // (Sem config no banco)

        // Act
        Func<Task> act = async () => await _orchestrator.GenerateResponseAsync("conv-1", "test");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Nenhuma configuração de IA ativa*");
    }

    [Fact]
    public async Task GenerateResponseAsync_WhenServiceThrows_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var config = new AiConfig
        {
            Name = "Error Config",
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            TokenLimit = 1000,
            Provider = "Test",
            Model = "Test",
            ApiKey = "Test"
        };
        _context.AiConfigs.Add(config);
        await _context.SaveChangesAsync();

        _mockLlmService
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AiProviderConfig>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service Error"));

        // Act
        Func<Task> act = async () => await _orchestrator.GenerateResponseAsync("conv-1", "test");

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Service Error");

        // Verificar Log de Erro
        var log = await _context.AiInteractionLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.Status.Should().Be("Error");
        log.ErrorMessage.Should().Be("Service Error");
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        _context.Dispose();
    }
}
