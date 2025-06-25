using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.API.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("")]
        [Route("Home")]
        [Route("Index")]
        public IActionResult Index()
        {
            var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

            if (System.IO.File.Exists(htmlPath))
            {
                var content = System.IO.File.ReadAllText(htmlPath);
                return Content(content, "text/html");
            }

            return Content(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>URL Shortener</title>
                    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css' rel='stylesheet'>
                </head>
                <body>
                    <div class='container mt-4'>
                        <h1>🚀 URL Shortener Çalışıyor!</h1>
                        <p>Static files sorunu var ama API çalışıyor.</p>
                        <a href='/swagger' class='btn btn-primary'>API Test (Swagger)</a>
                    </div>
                </body>
                </html>
            ", "text/html");
        }

        [HttpGet("css")]
        public IActionResult GetCSS()
        {
            var cssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "style.css");
            if (System.IO.File.Exists(cssPath))
            {
                var content = System.IO.File.ReadAllText(cssPath);
                return Content(content, "text/css");
            }
            return NotFound();
        }

        [HttpGet("js")]
        public IActionResult GetJS()
        {
            var jsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "app.js");
            if (System.IO.File.Exists(jsPath))
            {
                var content = System.IO.File.ReadAllText(jsPath);
                return Content(content, "application/javascript");
            }
            return NotFound();
        }
    }
}