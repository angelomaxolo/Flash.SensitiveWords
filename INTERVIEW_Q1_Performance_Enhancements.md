# Question 1: What would you do to enhance performance of your project?

## Immediate Performance Wins (High Impact, Medium Effort)

### 1. Intelligent Caching Strategy
- Implement **in-memory caching** for sensitive words (using `IMemoryCache`) since this dataset is read-heavy and changes infrequently
- Add a **cache invalidation strategy** when words are modified (CRUD operations invalidate the cache)
- Consider **distributed caching** (Redis) if deployed across multiple instances to avoid data inconsistency
- Estimated improvement: **80-90% faster filtering** on subsequent requests

**Implementation approach:**
```csharp
public class CachedSensitiveWordService : ISensitiveWordService
{
    private const string CACHE_KEY = "sensitive_words_cache";
    private readonly IMemoryCache _cache;
    private readonly ISensitiveWordService _innerService;
    
    public async Task<List<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CACHE_KEY, out List<SensitiveWord> words))
            return words;
        
        var data = await _innerService.GetAllAsync(cancellationToken);
        _cache.Set(CACHE_KEY, data, TimeSpan.FromHours(1));
        return data;
    }
    
    public async Task CreateAsync(SensitiveWord word, CancellationToken cancellationToken)
    {
        await _innerService.CreateAsync(word, cancellationToken);
        _cache.Remove(CACHE_KEY); // Invalidate cache
    }
}
```

### 2. Optimize String Matching Algorithm
- Replace simple loop-based filtering with **Aho-Corasick algorithm** for multi-pattern matching
- This allows simultaneous matching of all sensitive words in a single pass instead of multiple iterations
- Reduces complexity from O(n*m) to O(n+m+z) where z is the number of matches
- Estimated improvement: **50-70% faster** on large word lists

**Why this matters:**
- Current filtering likely iterates through each word checking the message
- Aho-Corasick builds a state machine that processes the message once
- Perfect for our use case with a fixed set of patterns (sensitive words)

### 3. Database Query Optimization
- Add indexes on frequently queried columns:
```sql
CREATE INDEX idx_sensitive_word_value ON SensitiveWords(Value);
CREATE INDEX idx_sensitive_word_created_date ON SensitiveWords(CreatedDate);
```
- Ensure proper indexing on the `Value` column used in filtering
- Use **eager loading** strategically to minimize query trips
- Implement **query result pagination** for `GetAllAsync()` to reduce memory overhead

**Impact:** Reduces database query time from milliseconds to microseconds for common queries

---

## Scalability Enhancements (Medium-Term)

### 4. Batch Operations
- Add endpoint for bulk sensitive word imports: `POST /sensitivewords/batch`
- Allow batch filtering of multiple messages in a single request
- Reduces API call overhead for bulk operations

```csharp
[HttpPost("batch")]
public async Task<IResult> FilterMultipleMessages(
    [FromBody] List<FilterMessageRequest> requests, 
    CancellationToken cancellationToken)
{
    // Process multiple messages in one round-trip
    var results = new List<FilterMessageResponse>();
    foreach (var request in requests)
    {
        var result = await service.FilterAsync(request.Message, cancellationToken);
        results.Add(new FilterMessageResponse { FilteredMessage = result });
    }
    return Results.Ok(results);
}
```

### 5. Response Compression
- Enable **gzip/brotli compression** in the API middleware for responses
- Particularly effective for Swagger metadata and large word lists
- Minimal CPU cost, significant bandwidth savings

```csharp
app.UseResponseCompression();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});
```

**Benefit:** 60-80% reduction in bandwidth for large response payloads

### 6. Connection Pooling & Resource Management
- Ensure EF Core connection pooling is configured correctly
- Monitor and tune `MaxPoolSize` based on load testing
- Implement proper `using` statements and async patterns (already done, but verify)

```csharp
services.AddDbContextPool<SensitiveWordsDbContext>(options =>
    options.UseSqlServer(connectionString),
    poolSize: 128);
```

---

## Advanced Optimizations (High Effort, Strategic)

### 7. Read Replicas for Analytics
- Separate read/write operations: writes to primary DB, reads from replica for reporting
- Reduces contention on the main database
- Enables heavy analytics without impacting production queries

**Architecture:**
```
User Requests → Primary DB (writes)
                ↓
              Replica (reads for analytics)
```

### 8. Microservice Decomposition (if traffic justifies)
- Extract filtering service as a separate lightweight service
- Allows independent scaling of read-heavy vs. write-heavy operations
- Deploy filtering service in multiple regions for global low-latency access

**Benefits:**
- Filter service can scale independently
- Admin service can handle lower traffic with fewer instances
- Failure isolation

### 9. Performance Monitoring & Telemetry
- Integrate **Application Insights** (already configured in appsettings)
- Add custom metrics:
  - Filtering latency (p50, p95, p99)
  - Cache hit rates
  - Database query execution times
  - Exception frequencies
  - API response size trends

```csharp
var filteringStopwatch = Stopwatch.StartNew();
var result = await service.FilterAsync(message, cancellationToken);
filteringStopwatch.Stop();

telemetryClient.TrackEvent("MessageFiltered", 
    new Dictionary<string, string> { { "wordCount", words.Count.ToString() } },
    new Dictionary<string, double> { { "LatencyMs", filteringStopwatch.ElapsedMilliseconds } });
```

---

## Performance Testing Strategy

### Load Testing Scenarios
1. **Concurrent filtering requests:** 1000 concurrent users filtering messages
2. **Large word list scenario:** 100,000+ sensitive words
3. **Large message scenario:** Multi-KB messages to filter
4. **Cache invalidation load:** High update frequency with concurrent reads

### Tools
- **k6** for load testing
- **JMeter** for complex scenarios
- **Application Insights** for production monitoring

### Success Metrics
- 95th percentile response time: < 100ms
- Cache hit rate: > 85%
- Database query time: < 50ms
- Throughput: > 10,000 requests/second
