using System.ComponentModel.DataAnnotations;
using KayraExport.Application.Interfaces;
using KayraExport.Application.Products.Dtos;
using KayraExport.Application.Products.Events;
using KayraExport.Core.Entities;
using MediatR;

namespace KayraExport.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommand : IRequest<ProductDto>
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }
}

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IEventPublisher eventPublisher)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Stock = request.Stock
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            ProductCacheKeys.All,
            cancellationToken);

        var productCreatedEvent = new ProductCreatedEvent(
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow,
            ProductId: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Stock: product.Stock);

        await _eventPublisher.PublishAsync(
            productCreatedEvent,
            "product.created",
            cancellationToken);

        return ProductDto.FromEntity(product);
    }
}