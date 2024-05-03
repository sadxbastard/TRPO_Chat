//using Microsoft.AspNetCore.Mvc;

//namespace WebApplication1.Controllers
//{
//    [Route("[controller]")]
//    [ApiController]
//    public class ProductController : ControllerBase
//    {
//        private IProductService _productService;
//        private readonly IServiceProvider _serviceProvider;

//        public ProductController(IProductService productService, IServiceProvider serviceProvider)
//        {
//            _productService = productService;
//            _serviceProvider = serviceProvider;
//        }

//        [HttpGet]
//        public IActionResult Index()
//        {
//            var t = _serviceProvider.GetService<IProductService>();

//            return Ok(_productService.GetProducts());
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebApplication1.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IHubContext<MainHub> _hubContext;

        public ProductController(IProductService productService, IHubContext<MainHub> hubContext)
        {
            _productService = productService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(_productService.GetProducts());
        }
    }
}
