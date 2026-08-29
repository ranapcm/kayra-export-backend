using System.ComponentModel.DataAnnotations;
using KayraExport.Application.Interfaces;
using KayraExport.Application.Products.Dtos;
using KayraExport.Application.Products.Events;
using MediatR;

namespace KayraExport.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; set; }

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

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cacheService,
        IEventPublisher eventPublisher)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
    }

    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID '{request.Id}' was not found.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            ProductCacheKeys.All,
            cancellationToken);

        var productUpdatedEvent = new ProductUpdatedEvent(
            EventId: Guid.NewGuid(),
            OccurredAt: DateTime.UtcNow,
            ProductId: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Stock: product.Stock);

        await _eventPublisher.PublishAsync(
            productUpdatedEvent,
            "product.updated",
            cancellationToken);

        return ProductDto.FromEntity(product);
    }
}