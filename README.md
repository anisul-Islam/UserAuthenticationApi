# Today's Plan

1. Middleware
2. CORS Setup
3. env Setup
4. Deploy Postgres Database in Supabase
5. Deploy Web API in Render

## 1. Create a simple Logger Middleware

```csharp
      // Inline Middleware: You can define middleware directly in the Program.cs or Startup.cs file using lambda expressions.
      app.Use(async (context, next) =>
      {

          // Get the client IP address
          var clientIp = context.Connection.RemoteIpAddress?.ToString();

          // Start a stopwatch to measure processing time
          var stopwatch = Stopwatch.StartNew();

          // Log Request details
          Console.WriteLine($"[{DateTime.UtcNow}] [Request] " +
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

```

## add authorization in swagger

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

## 2. CORS Setup

- scenario 1: A React App on http://localhost:5173 can make api calls to your backend.

  - scenario 2: Your production front-end hosted at http://studywithanis.com can make api calls to your backend.
  - scenario 3: A request comes from https://malicious-site.com. is trying to make api calls to your backend.

  ```csharp
  // install this: dotnet add package Microsoft.AspNetCore.Cors

  // setup the following lines in your startup file
  builder.Services.AddCors(options =>
      {

        // you can create multiple policies to use specifically for each controller
          options.AddPolicy("AllowSpecificOrigins", builder =>
          {
              builder.WithOrigins("http://localhost:5173", // Specify the allowed origins
                                  "https://www.yourclientapp.com", // Add additional origins as needed
                                  // "*") // This accept request from all origins => not recommended
                      )
                    .AllowAnyMethod() // Allows all methods
                    // .WithMethods("GET, POST") // Allows only specific methods
                    .AllowAnyHeader() // Allows all headers
                    // .WithHeaders("Content-Type", "Authorization") // Allow Specific Headers
                    .AllowCredentials(); // Allows credentials like cookies, authorization headers, etc.
          });
      });

  //Applies the "AllowSpecificOrigins" policy to all endpoints in your application.
  // Place this line before app.UseAuthentication(); and app.UseAuthorization();.
  app.UseCors("MyAllowSpecificOrigins");
  app.UseAuthentication();
  app.UseAuthorization();

  // if you want to setup a policy only for a specific controller then use the following
    [EnableCors("AllowSpecificOrigins")]
    public class ProductsController : ControllerBase
    {
        // ...
    }
  ```

  - Purpose of the Code: Configures a CORS policy that allows specific trusted origins to access your API with any method, any header, and include credentials.
    - Key Components:
      - builder.Services.AddCors: Registers CORS services.
      - options.AddPolicy: Defines a named CORS policy.
      - policyBuilder.WithOrigins: Specifies allowed origins.
      - AllowAnyMethod, AllowAnyHeader, AllowCredentials: Sets what is permitted in cross-origin requests.

## 3. .env setup

- step 1: first you can setup default value in appsetting.json
- step 2: create .env file in root directory

  ```csharp
      Jwt__Key=your_secret_key_here_make_it_long_and_random
      Jwt__Issuer=LocalhostIssuer
      Jwt__Audience=LocalhostAudience
  ```

- step 3: Then, use a library like DotNetEnv to load these variables. First, add the DotNetEnv package to your project:

  ```bash
  dotnet add package DotNetEnv
  ```

- setp 4: Load environment variables in program.cs

  ```csharp
  var builder = WebApplication.CreateBuilder(args);

  // Load environment variables from .env file
  DotNetEnv.Env.Load();

  // Get JWT settings from environment variables
  var jwtKey = Environment.GetEnvironmentVariable("Jwt__Key") ?? throw new InvalidOperationException("JWT Key is missing in environment variables.");
  var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? throw new InvalidOperationException("JWT Issuer is missing in environment variables.");
  var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? throw new InvalidOperationException("JWT Audience is missing in environment variables.");
  Console.WriteLine($"----------jwtKey: {jwtKey}---------");
  ```

- setp 5: Update Code everywhere with environment variables

## 4. Deploy Postgres Database

- Supabase is an open-source database infrastructure built on PostgreSQL. Using Supabase's simple setup and user interface, users can quickly build a complete web and mobile backend to really focus on building their product within minutes, without getting caught up in overly complex data structures.

- Go to Supabse: create a new database project and generate password: 7Sey75jV@MZLm9D
- Find the connection string => settings => Database => Connection string => .NET => SESSION MODE => Copy the string
- add new .env variable and store the supabase connection string

  ```shell
  <!-- inside .env file  -->
  ConnectionStrings__DefaultConnection=User Id=postgres.ekjksguivtuccfsbxang;Password=7Sey75jV@MZLm9D;Server=aws-0-us-east-1.pooler.supabase.com;Port=6543;Database=postgres;
  ```

- use the new connection in program.cs

  ```csharp
  // Get the database connection string from environment variables
  var defaultConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? throw new InvalidOperationException("Default Connection is missing in environment variables.");

  builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnection));
  ```

- delete previous migration script and create new

  ```json
  dotnet ef migrations add InitialPro
  dotnet ef database update
  ```

## 5. Deploy Web API in Render

### Prerequisites

1. **Render Account**: Sign up for a Render account at [render.com](https://render.com/).
2. **GitHub Repository**: Ensure your .NET web API is pushed to a GitHub repository.

#### Steps to Deploy a .NET Web API to Render

##### 1. Prepare Your .NET Web API

Ensure your .NET web API project is ready for deployment. Typically, this involves:

- Having a `.csproj` file in the root directory of your project.
- Ensuring the project builds and runs correctly locally.
- Add a `Dockerfile` install Docker extension

###### Example `Dockerfile`

If you prefer using a Dockerfile for more control over the build process, here’s a basic example for a .NET web API:

```dockerfile
# Use the official .NET image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["YourProject.csproj", "./"]
RUN dotnet restore "YourProject.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "YourProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YourProject.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YourProject.dll"]
```

Replace `YourProject.csproj` and `YourProject.dll` with the actual names of your project files.

###### 2. Create a New Web Service on Render

1. **Login to Render**: Go to [render.com](https://render.com/) and log in to your account.

2. **Create New Web Service**:

   - Click the `New` button in the Render dashboard.
   - Select `Web Service`.

3. **Connect to GitHub**:

   - Connect your GitHub account to Render if you haven’t already.
   - Select the repository containing your .NET web API.

4. **Configure the Web Service**:

   - **Name**: Enter a name for your service.
   - **Branch**: Select the branch you want to deploy (e.g., `main`).
   - **Build Command**: Render automatically detects .NET projects and usually sets this to `dotnet publish -c Release -o out`.
   - **Start Command**: Render sets this to `dotnet yourapp.dll` (replace `yourapp.dll` with your project's DLL name, usually matching the project name).

5. **Environment**: Select the environment type (e.g., `Standard`).

6. **Region**: Select the deployment region.

7. **Instance Type**: Choose the instance type based on your resource requirements.

8. **Click `Create Web Service`**.
