# Today's Plan

- Middleware
- CORS Setup
- .env Setup
- deploy Database
- deploy Web API

````csharp
- Create a simple Logger Middleware

      ```csharp
      // Inline Middleware: You can define middleware directly in the Program.cs or Startup.cs file using lambda expressions.
      app.Use(async (context, next) =>
      {
          // Generate a Correlation ID for the request
          var correlationId = Guid.NewGuid().ToString();
          context.Items["CorrelationId"] = correlationId;

          // Get the client IP address
          var clientIp = context.Connection.RemoteIpAddress?.ToString();

          // Start a stopwatch to measure processing time
          var stopwatch = Stopwatch.StartNew();

          // Log Request details
          Console.WriteLine($"[{DateTime.UtcNow}] [Request] [CorrelationId: {correlationId}] " +
                            $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString} " +
                            $"from {clientIp}");

          await next.Invoke();

          // Log Response details
          stopwatch.Stop();
          Console.WriteLine($"[{DateTime.UtcNow}] [Response]" +
                            $"Status Code: {context.Response.StatusCode}, " +
                            $"Time Taken: {stopwatch.ElapsedMilliseconds} ms");
          Console.WriteLine($"After Calling: {context.Response.StatusCode}");
      });

````
