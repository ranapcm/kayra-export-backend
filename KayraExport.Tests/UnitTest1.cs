using KayraExport.Application.Interfaces;
using KayraExport.Application.Products.Commands.CreateProduct;
using KayraExport.Application.Products.Events;
using KayraExport.Core.Entities;
using Moq;

namespace KayraExport.Tests;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesProductClearsCacheAndPublishesEvent()
    {
        var productRepository = new Mock<IProductRepository>();
        var cacheService = new Mock<ICacheService>();
        var eventPublisher = new Mock<IEventPublisher>();

        productRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        productRepository
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        cacheService
            .Setup(cache => cache.RemoveAsync(
                "products:all",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        eventPublisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<ProductCreatedEvent>(),
                "product.created",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            cacheService.Object,
            eventPublisher.Object);

        var command = new CreateProductCommand
        {
            Name = "Test Monitor",
            Description = "27 inch test monitor",
            Price = 7499.90m,
            Stock = 15
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal("Test Monitor", result.Name);
        Assert.Equal("27 inch test monitor", result.Description);
        Assert.Equal(7499.90m, result.Price);
        Assert.Equal(15, result.Stock);

        productRepository.Verify(
            repository => repository.AddAsync(
                It.Is<Product>(product =>
                    product.Name == "Test Monitor" &&
                    product.Price == 7499.90m &&
                    product.Stock == 15),
                It.IsAny<CancellationToken>()),
            Times.Once);

        productRepository.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        cacheService.Verify(
            cache => cache.RemoveAsync(
                "products:all",
                It.IsAny<CancellationToken>()),
            Times.Once);

        eventPublisher.Verify(
            publisher => publisher.PublishAsync(
                It.Is<ProductCreatedEvent>(integrationEvent =>
                    integrationEvent.ProductId == result.Id &&
                    integrationEvent.Name == "Test Monitor" &&
                    integrationEvent.Description ==
                        "27 inch test monitor" &&
                    integrationEvent.Price == 7499.90m &&
                    integrationEvent.Stock == 15),
                "product.created",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TrimsNameAndDescription()
    {
        var productRepository = new Mock<IProductRepository>();
        var cacheService = new Mock<ICacheService>();
        var eventPublisher = new Mock<IEventPublisher>();

        productRepository
            .Setup(repository => repository.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        productRepository
            .Setup(repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        cacheService
            .Setup(cache => cache.RemoveAsync(
                "products:all",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        eventPublisher
            .Setup(publisher => publisher.PublishAsync(
                It.IsAny<ProductCreatedEvent>(),
                "product.created",
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            cacheService.Object,
            eventPublisher.Object);

        var command = new CreateProductCommand
        {
            Name = "  Laptop  ",
            Description = "  Test description  ",
            Price = 100m,
            Stock = 5
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Test description", result.Description);

        eventPublisher.Verify(
            publisher => publisher.PublishAsync(
                It.Is<ProductCreatedEvent>(integrationEvent =>
                    integrationEvent.Name == "Laptop" &&
                    integrationEvent.Description ==
                        "Test description"),
                "product.created",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}