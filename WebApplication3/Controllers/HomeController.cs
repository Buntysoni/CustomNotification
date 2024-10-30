using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebApplication3.Models;
using WebPush;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static Dictionary<string, PushSubscription> subscriptions = new();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Subscribe(string requestBody)
        {
            var r = Newtonsoft.Json.JsonConvert.DeserializeObject<SubsribeData>(requestBody);

            PushSubscription pushD = new PushSubscription
            {
                Auth = r.Subscription.Keys.Auth,
                Endpoint = r.Subscription.Endpoint,
                P256DH = r.Subscription.Keys.P256dh
            };

            var sessionCode = r.SessionCode;
            subscriptions[r.SessionCode] = pushD;

            return Ok(new { sessionCode });
        }

        [HttpPost]
        public IActionResult SendNotification([FromBody] NotificationRequest request)
        {
            if (!request.IsAll)
            {
                if (subscriptions.TryGetValue(request.SessionCode, out var subscription))
                {
                    var vapidDetails = new VapidDetails("mailto:example@yourdomain.com", "BCrjxLet8V4qp_p9Bp-ENACCp2Su2mcDCXnfqY1nkH8Ir63CMyLr1_FYFv87N6j2EfuC2KQ15EnJ5hYHUSiFfmk", "q52iOg2uc2Td8U0L6m17TSCWWasRmudn7vrb6Gm9lro");
                    var webPushClient = new WebPushClient();

                    var payload = JsonSerializer.Serialize(new { title = request.Title, body = request.Message });

                    try
                    {
                        webPushClient.SendNotification(subscription, payload, vapidDetails);
                    }
                    catch (Exception)
                    {
                        //handle error
                    }
                    return Ok("Notification sent");
                }
            }
            else
            {
                foreach (var item in subscriptions)
                {
                    if (subscriptions.TryGetValue(item.Key, out var subscription))
                    {
                        var vapidDetails = new VapidDetails("mailto:example@yourdomain.com", "BCrjxLet8V4qp_p9Bp-ENACCp2Su2mcDCXnfqY1nkH8Ir63CMyLr1_FYFv87N6j2EfuC2KQ15EnJ5hYHUSiFfmk", "q52iOg2uc2Td8U0L6m17TSCWWasRmudn7vrb6Gm9lro");
                        var webPushClient = new WebPushClient();

                        var payload = JsonSerializer.Serialize(new { title = request.Title, body = request.Message });

                        try
                        {
                            webPushClient.SendNotification(subscription, payload, vapidDetails);
                        }
                        catch (Exception)
                        {
                            //handle error
                        }
                    }
                }
            }
            return NotFound("Session not found");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class SubsribeData
    {
        public Subscription? Subscription { get; set; }
        public string? SessionCode { get; set; }
    }

    public class Subscription
    {
        public string? Endpoint { get; set; }
        public Keys? Keys { get; set; }
    }

    public class Keys
    {
        public string? P256dh { get; set; }
        public string? Auth { get; set; }
    }

    public class NotificationRequest
    {
        public string? SessionCode { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsAll { get; set; }
    }
}
