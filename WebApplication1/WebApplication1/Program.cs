using Serilog;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IProductService>(provider =>
            {
                var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var httpContext = httpContextAccessor.HttpContext;

                if (httpContext.Request.Headers.TryGetValue("X-I-AM-VIP", out var _))
                {
                    return new VipProductService();
                }
                return new ProductService();
            });
            builder.Services.AddSignalR();

            var appName = typeof(Program).Assembly.GetName().Name;

            var loggerConfiguration = new LoggerConfiguration();

            Log.Logger = loggerConfiguration.MinimumLevel.Debug()
                                            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateLogger();
            builder.Host.UseSerilog();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<MainHub>("/hub");

            app.Run();
        }
    }
    public interface IProductService
    {
        IEnumerable<Product> GetProducts();
    }
    public class ProductService : IProductService
    {
        private Guid _id = Guid.NewGuid();
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product { Id = 1, Name = "Пюрешка", Price = 50 };
            yield return new Product { Id = 2, Name = "Котлетка куриная", Price = 100 };
            yield return new Product { Id = 3, Name = "Салатик", Price = 70 };
        }
    }
    public class VipProductService : IProductService
    {
        private Guid _id = Guid.NewGuid();
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product { Id = 1, Name = "Пюрешка", Price = 50 };
            yield return new Product { Id = 2, Name = "Котлетка куриная", Price = 100 };
            yield return new Product { Id = 3, Name = "Салатик", Price = 70 };
            yield return new Product { Id = 3, Name = "Премиум бифштекс", Price = 15000 };
        }
    }
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
