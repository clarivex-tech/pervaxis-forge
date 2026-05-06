# Genesis Provider Integration Guide

This guide shows how to integrate and use all 8 Genesis AWS providers in your microservice.

**Genesis provides infrastructure concerns** - caching, messaging, storage, search, notifications, workflows, AI, and reporting. Your code focuses on business logic while Genesis handles AWS connectivity, resilience, and observability.

---

## Table of Contents

1. [Caching (ElastiCache)](#1-caching-elasticache)
2. [Messaging (SQS/SNS)](#2-messaging-sqssns)
3. [File Storage (S3)](#3-file-storage-s3)
4. [Search (OpenSearch)](#4-search-opensearch)
5. [Notifications (SES/SNS)](#5-notifications-sessns)
6. [Workflow (Step Functions)](#6-workflow-step-functions)
7. [AI Assistance (Bedrock)](#7-ai-assistance-bedrock)
8. [Reporting (Metabase)](#8-reporting-metabase)

---

## 1. Caching (ElastiCache)

**Use for:** Session storage, API response caching, rate limiting, temporary data.

### Configuration

```json
{
  "Genesis": {
    "Caching": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "ConnectionString": "localhost:6379",
      "KeyPrefix": "myservice",
      "EnableTenantIsolation": true,
      "DefaultExpiration": "01:00:00"
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Caching.AWS.Extensions;

builder.Services.AddGenesisCaching(
    builder.Configuration.GetSection("Genesis:Caching"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Caching.AWS" Version="1.0.0" />
```

### Usage Example

```csharp
using Pervaxis.Core.Abstractions.Genesis.Modules;

public class ProductService
{
    private readonly ICache _cache;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ICache cache, ILogger<ProductService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<Product?> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{productId}";

        // Try cache first
        var cached = await _cache.GetAsync<Product>(cacheKey, cancellationToken);
        if (cached.IsSuccess && cached.Data != null)
        {
            _logger.LogInformation("Cache hit for product {ProductId}", productId);
            return cached.Data;
        }

        // Cache miss - fetch from database
        _logger.LogInformation("Cache miss for product {ProductId}, fetching from database", productId);
        var product = await FetchFromDatabaseAsync(productId, cancellationToken);

        if (product != null)
        {
            // Cache for 1 hour
            await _cache.SetAsync(cacheKey, product, TimeSpan.FromHours(1), cancellationToken);
        }

        return product;
    }

    public async Task InvalidateProductCacheAsync(string productId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:{productId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogInformation("Invalidated cache for product {ProductId}", productId);
    }

    private async Task<Product?> FetchFromDatabaseAsync(string productId, CancellationToken cancellationToken)
    {
        // Your database logic here
        await Task.Delay(100, cancellationToken); // Simulate DB call
        return new Product { Id = productId, Name = "Sample Product", Price = 99.99m };
    }
}

public record Product
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}
```

### Testing

```csharp
using NSubstitute;
using Pervaxis.Core.Abstractions.Genesis.Modules;
using Pervaxis.Genesis.Base.Result;

public class ProductServiceTests
{
    [Fact]
    public async Task GetProductAsync_CacheHit_ReturnsCachedProduct()
    {
        // Arrange
        var mockCache = Substitute.For<ICache>();
        var mockLogger = Substitute.For<ILogger<ProductService>>();
        
        var cachedProduct = new Product { Id = "123", Name = "Cached Product", Price = 49.99m };
        mockCache.GetAsync<Product>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ProviderResult<Product>.Success(cachedProduct));

        var service = new ProductService(mockCache, mockLogger);

        // Act
        var result = await service.GetProductAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cached Product", result.Name);
        await mockCache.Received(1).GetAsync<Product>(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProductAsync_CacheMiss_FetchesAndCaches()
    {
        // Arrange
        var mockCache = Substitute.For<ICache>();
        var mockLogger = Substitute.For<ILogger<ProductService>>();
        
        mockCache.GetAsync<Product>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(ProviderResult<Product>.Success(null));

        var service = new ProductService(mockCache, mockLogger);

        // Act
        var result = await service.GetProductAsync("123");

        // Assert
        Assert.NotNull(result);
        await mockCache.Received(1).SetAsync(
            Arg.Any<string>(), 
            Arg.Any<Product>(), 
            Arg.Any<TimeSpan>(), 
            Arg.Any<CancellationToken>());
    }
}
```

---

## 2. Messaging (SQS/SNS)

**Use for:** Event-driven architecture, asynchronous processing, decoupled communication between services.

### Configuration

```json
{
  "Genesis": {
    "Messaging": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "QueueUrl": "http://localhost:4566/000000000000/orders-queue",
      "TopicArn": "arn:aws:sns:us-east-1:000000000000:orders-topic",
      "EnableTenantIsolation": true,
      "MessageRetentionPeriod": 345600,
      "VisibilityTimeout": 30
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Messaging.AWS.Extensions;

builder.Services.AddGenesisMessaging(
    builder.Configuration.GetSection("Genesis:Messaging"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Messaging.AWS" Version="1.0.0" />
```

### Usage Example

```csharp
using Pervaxis.Core.Abstractions.Genesis.Modules;

public class OrderService
{
    private readonly IMessaging _messaging;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IMessaging messaging, ILogger<OrderService> logger)
    {
        _messaging = messaging;
        _logger = logger;
    }

    // Publishing events
    public async Task CreateOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        // Save order to database (not shown)
        
        // Publish event for other services
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _messaging.PublishAsync(
            "order-created", 
            orderCreatedEvent, 
            cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Published OrderCreated event for order {OrderId}", order.Id);
        }
        else
        {
            _logger.LogError("Failed to publish OrderCreated event: {Error}", result.ErrorMessage);
        }
    }

    // Consuming messages
    public async Task ProcessOrderMessagesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _messaging.ReceiveAsync<OrderCreatedEvent>(10, 30, cancellationToken);

        if (result.IsSuccess && result.Data.Any())
        {
            foreach (var message in result.Data)
            {
                _logger.LogInformation("Processing order {OrderId}", message.Content.OrderId);
                
                // Process the order
                await ProcessOrderAsync(message.Content, cancellationToken);

                // Delete message after successful processing
                await _messaging.DeleteAsync(message.MessageId, cancellationToken);
            }
        }
    }

    private async Task ProcessOrderAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken)
    {
        // Your business logic here
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Order {OrderId} processed successfully", orderEvent.OrderId);
    }
}

public record Order
{
    public string Id { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}

public record OrderCreatedEvent
{
    public string OrderId { get; init; } = string.Empty;
    public string CustomerId { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

---

## 3. File Storage (S3)

**Use for:** Document storage, image uploads, file downloads, large object storage.

### Configuration

```json
{
  "Genesis": {
    "FileStorage": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "BucketName": "myservice-files",
      "KeyPrefix": "uploads",
      "EnableTenantIsolation": true,
      "EnableServerSideEncryption": true,
      "PresignedUrlExpiration": "00:15:00"
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.FileStorage.AWS.Extensions;

builder.Services.AddGenesisFileStorage(
    builder.Configuration.GetSection("Genesis:FileStorage"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.FileStorage.AWS" Version="1.0.0" />
```

---

## 4. Search (OpenSearch)

**Use for:** Full-text search, product catalogs, log analytics, document search.

### Configuration

```json
{
  "Genesis": {
    "Search": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "ServiceUrl": "http://localhost:4566",
      "IndexPrefix": "myservice",
      "EnableTenantIsolation": true,
      "DefaultPageSize": 20,
      "MaxPageSize": 100
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Search.AWS.Extensions;

builder.Services.AddGenesisSearch(
    builder.Configuration.GetSection("Genesis:Search"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Search.AWS" Version="1.0.0" />
```

---

## 5. Notifications (SES/SNS)

**Use for:** Transactional emails, SMS alerts, push notifications, customer communications.

### Configuration

```json
{
  "Genesis": {
    "Notifications": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "SenderEmail": "noreply@myservice.com",
      "SenderName": "MyService",
      "EnableTenantIsolation": true,
      "DefaultSmsTopicArn": "arn:aws:sns:us-east-1:000000000000:sms-topic",
      "DefaultPushTopicArn": "arn:aws:sns:us-east-1:000000000000:push-topic"
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Notifications.AWS.Extensions;

builder.Services.AddGenesisNotifications(
    builder.Configuration.GetSection("Genesis:Notifications"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Notifications.AWS" Version="1.0.0" />
```

---

## 6. Workflow (Step Functions)

**Use for:** Multi-step business processes, order fulfillment, approval workflows, long-running tasks.

### Configuration

```json
{
  "Genesis": {
    "Workflow": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566",
      "Region": "us-east-1",
      "StateMachineArn": "arn:aws:states:us-east-1:000000000000:stateMachine:order-fulfillment",
      "EnableTenantIsolation": true,
      "ExecutionNamePrefix": "myservice"
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Workflow.AWS.Extensions;

builder.Services.AddGenesisWorkflow(
    builder.Configuration.GetSection("Genesis:Workflow"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Workflow.AWS" Version="1.0.0" />
```

---

## 7. AI Assistance (Bedrock)

**Use for:** Content generation, chatbots, recommendations, image generation, data analysis.

### Configuration

```json
{
  "Genesis": {
    "AIAssistance": {
      "UseLocalStack": false,
      "Region": "us-east-1",
      "TextModelId": "anthropic.claude-3-sonnet-20240229-v1:0",
      "ImageModelId": "stability.stable-diffusion-xl-v1",
      "EnableTenantIsolation": true,
      "DefaultMaxTokens": 1024,
      "DefaultTemperature": 0.7
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.AIAssistance.AWS.Extensions;

builder.Services.AddGenesisAIAssistance(
    builder.Configuration.GetSection("Genesis:AIAssistance"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.AIAssistance.AWS" Version="1.0.0" />
```

---

## 8. Reporting (Metabase)

**Use for:** Business dashboards, analytics, data visualization, scheduled reports.

### Configuration

```json
{
  "Genesis": {
    "Reporting": {
      "BaseUrl": "http://localhost:3000",
      "ApiKey": "your-metabase-api-key",
      "EnableTenantIsolation": true,
      "DefaultTimeout": 30
    }
  }
}
```

### Dependency Injection

```csharp
// Program.cs
using Pervaxis.Genesis.Reporting.AWS.Extensions;

builder.Services.AddGenesisReporting(
    builder.Configuration.GetSection("Genesis:Reporting"));
```

### NuGet Package

```xml
<PackageReference Include="Pervaxis.Genesis.Reporting.AWS" Version="1.0.0" />
```

---

## General Best Practices

### 1. Always Use CancellationToken

```csharp
public async Task<Result> MyMethodAsync(CancellationToken cancellationToken = default)
{
    // Pass cancellationToken to all Genesis provider calls
    await _cache.GetAsync<string>("key", cancellationToken);
}
```

### 2. Handle ProviderResult Properly

```csharp
var result = await _cache.GetAsync<Product>("key");

if (result.IsSuccess)
{
    // Use result.Data
}
else
{
    // Log result.ErrorMessage
    // Decide: throw, return default, retry, etc.
}
```

### 3. Use Structured Logging

```csharp
_logger.LogInformation(
    "Processed order {OrderId} for customer {CustomerId}",
    orderId,
    customerId);
```

### 4. Multi-Tenancy

Genesis handles tenant isolation automatically when:
- `EnableTenantIsolation = true` in options
- `ITenantContext` is registered and resolved

Your code doesn't need to add tenant prefixes - Genesis does it.

### 5. LocalStack for Development

All providers support LocalStack. Set in appsettings.Development.json:

```json
{
  "Genesis": {
    "ProviderName": {
      "UseLocalStack": true,
      "LocalStackUrl": "http://localhost:4566"
    }
  }
}
```

### 6. Mock Genesis Providers in Tests

Use NSubstitute or Moq:

```csharp
var mockCache = Substitute.For<ICache>();
mockCache.GetAsync<string>(Arg.Any<string>())
    .Returns(ProviderResult<string>.Success("cached-value"));
```

### 7. Error Handling Strategy

```csharp
try
{
    var result = await _provider.OperationAsync(...);
    
    if (!result.IsSuccess)
    {
        // Genesis resilience (Polly) already retried
        // This is a real failure - handle accordingly
        _logger.LogError("Operation failed: {Error}", result.ErrorMessage);
        // Throw, return error result, use fallback, etc.
    }
}
catch (GenesisException ex)
{
    // Infrastructure failure (AWS connectivity, etc.)
    _logger.LogError(ex, "Genesis provider error");
}
```

---

## Summary

This guide covers all 8 Genesis providers with:
- ✅ Configuration examples
- ✅ DI registration
- ✅ Real usage patterns
- ✅ Testing strategies
- ✅ Best practices

**Genesis handles infrastructure. You focus on business logic.**

For questions or issues, see:
- Genesis repository: https://github.com/clarivex-tech/pervaxis-genesis
- Documentation: https://clarivex.tech/docs/genesis

---

*Genesis Provider Integration Guide*  
*Pervaxis Platform · Clarivex Technologies*
