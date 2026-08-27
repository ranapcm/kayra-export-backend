using KayraExport.Application.Interfaces;
using KayraExport.Application.Products.Commands.CreateProduct;
using KayraExport.Core.Entities;
using Moq;

namespace KayraExport.Tests;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesProductAndClearsCache()
    {
        var productRepository = new Mock<IProductRepository>();
        var cacheService = new Mock<ICacheService>();

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

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            cacheService.Object);

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
    }

    [Fact]
    public async Task Handle_TrimsNameAndDescription()
    {
        var productRepository = new Mock<IProductRepository>();
        var cacheService = new Mock<ICacheService>();

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

        var handler = new CreateProductCommandHandler(
            productRepository.Object,
            cacheService.Object);

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
    }
}