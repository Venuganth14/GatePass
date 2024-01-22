using JWT.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace GatePass_Project
{
    public class SLTLoginController : Controller
    {

        private readonly IConfiguration _configuration;
        // Inject IConfiguration service to access app settings
        public SLTLoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public RedirectResult SLTLogin()
        {
            //    using (var client = new HttpClient())
            //    {
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://oneidentitytest.slt.com.lk/authServer/auth?response_type=code&client_id=GatePass&redirect_uri=https://220.247.224.226:9300/SLTLogin/SLTLoginRedirect&scope=openid%20email&state=1234zyx");

            //// Add your cookies to the request headers
            //request.Headers.Add("Cookie", ".AspNetCore.Antiforgery.TmywGdJytIY=CfDJ8KHpbe-VjV9Hv0cg12Jx5BibFhPbHZifF6kaHXdkQLdqUBdYhF8HYWeVjHXEDjwyhlA2J0_xSesgod-5LBMxallXYwJyK1xAU4XGOOn99aR4exIl7OW8qg2SRzGLy-xasgVMHOzSTGLvD7gqHow0E_w; .AspNetCore.Session=CfDJ8KHpbe%2BVjV9Hv0cg12Jx5BibY6c%2FCDkFYNey0%2F%2BaFq2te6k%2FCXvq8ciEfbpSkVRAWiO0DGJfz5htelrG9tjYtPBFJrQfCcXfHZXCFYNKYLYazmQ5q7h4GxkStygTAsefZDaKmr7%2BEWlJCdiFyJbpqb8xSwxSHtJ59feFZyNmI%2BWP");

            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            //// Read and return the response content
            /*
            When i click the SLT IAM button i am taken to the Auth Server login page, once i login i am given a OTP and i need to verify this OTP, which  provides authorization code, which we need to exchange for the access token, which we then use to fetch data about the logged in user.

here is the first part of the controller which redirects the us
            */
            //var responseData = await response.Content.ReadAsStringAsync();

            // You can do further processing or return the data as needed
             return Redirect("https://oneidentitytest.slt.com.lk/authServer/auth?response_type=code&client_id=GatePass&redirect_uri=https://220.247.224.226:9300/SLTLogin/SLTLoginRedirect&scope=openid%20email&state=1234zyx");
            //}
        }

        public async Task<ActionResult> GetToken(MyModel urlDetails)
        {
            string code = urlDetails.code;
            string state = urlDetails.state;
            string clientId = "GatePass";
            string clientSecret = "1b1900640e8f4842818d0ec8ea518ba6";
            string preBase64 = clientId + ":" + clientSecret;

            var messageBytes = Encoding.UTF8.GetBytes(preBase64);
            var encodeMessage = Convert.ToBase64String(messageBytes);

            string baseUrl = "https://oneidentitytest.slt.com.lk/authServer/token";

            using (HttpClient client = new HttpClient())
            {
                // Adding parameters to the URL
                string urlWithParams = baseUrl + "?" + "grant_type=authorization_code&code=" + code + "&" + "redirect_uri=https://220.247.224.226:9301/SLTLogin/SLTLoginRedirect";

                client.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodeMessage);

                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(urlWithParams, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(responseContent);
                    var idToken = jo["id_token"].ToString();
                    var jwtEncodedString = idToken;

                    var token = new JwtSecurityToken(jwtEncodedString: jwtEncodedString);

                    if (jo["access_token"] != null && jo["token_type"] != null)
                    {
                        var accessToken = jo["access_token"].ToString();
                        var tokenType = jo["token_type"].ToString();
                        return View("Success", accessToken); // Return the access token to the view
                    }
                    else
                    {
                        Console.WriteLine("Not Fetched");
                        return View("Error"); // Return an error view
                    }
                }
                else
                {
                    Console.WriteLine("Request failed");
                    return View("Error"); // Return an error view
                }
            }
        }








        //[HttpGet]
        //public ActionResult SLTLoginRedirect([FromQuery] string code, [FromQuery] string state)
        //{
        //    try
        //    {
        //        Console.WriteLine($"Received state: {state}, code: {code}");

        //        string token = GetToken(code, state);

        //        if (token != null)
        //        {
        //            string userInfo = GetUserInfo(token);

        //            ViewBag.Information = userInfo;

        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            return View("SLTLogin");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in SLTLoginRedirect: {ex.Message}");
        //        return View("SLTLogin");
        //    }
        //}

        //public string GetToken(string code, string state)
        //{
        //    var tokenEndpoint = _configuration["Auth:TokenEndpoint"];
        //    var clientId = _configuration["Auth:ClientId"];
        //    var clientSecret = _configuration["Auth:ClientSecret"];
        //    string preBase64 = clientId + ":" + clientSecret;

        //    var messageBytes = System.Text.Encoding.UTF8.GetBytes(preBase64);
        //    var encodedMessage = Convert.ToBase64String(messageBytes);

        //    using (var client = new HttpClient())
        //    {
        //        string urlWithParams = $"{tokenEndpoint}?grant_type=authorization_code&code={code}&state={state}&redirect_uri={_configuration["Auth:RedirectUri"]}";
        //        client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
        //        client.DefaultRequestHeaders.Add("Authorization", "Basic " + encodedMessage);

        //        var tokenResponse = client.PostAsync(urlWithParams, null).Result;

        //        if (tokenResponse.IsSuccessStatusCode)
        //        {
        //            var tokenContent = tokenResponse.Content.ReadAsStringAsync().Result;
        //            var jo = Newtonsoft.Json.Linq.JObject.Parse(tokenContent);
        //            var token = jo["access_token"].ToString();
        //            return token;
        //        }
        //    }

        //    return null;
        //}

        //public string GetUserInfo(string token)
        //{
        //    var infoEndpoint = _configuration["Auth:InfoEndpoint"];

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        //        var infoResponse = client.GetAsync(infoEndpoint).Result;

        //        if (infoResponse.IsSuccessStatusCode)
        //        {
        //            var infoContent = infoResponse.Content.ReadAsStringAsync().Result;
        //            ViewBag.UserInfo = infoContent;
        //        }
        //    }

        //    return null;
        //}



        /*
        [HttpGet]
        public async Task<string> GetToken(string state, string code)
        {
            var tokenEndpoint = "https://oneidentitytest.slt.com.lk/authServer/token";
            var clientId = "GatePass";
            var clientSecret = "1b1900640e8f4842818d0ec8ea518ba6";
            string preBase64 = clientId + ":" + clientSecret;

            var messageBytes = Encoding.UTF8.GetBytes(preBase64);
            var encodeMessage = Convert.ToBase64String(messageBytes);

            string accessToken = null;

            using (var client = new HttpClient())
            {
                // Add the parameters to the URL
                string urlWithParams = $"{tokenEndpoint}?grant_type=authorization_code&code={code}&redirect_uri=https://220.247.224.226:9300/SLTLogin/SLTLoginRedirect";

                client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("Authorization", "basic " + encodeMessage);

                var tokenResponse = await client.PostAsync(urlWithParams, null);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                    var jo = JObject.Parse(tokenContent);
                    accessToken = jo["access_token"].ToString();
                }
            }

            return accessToken;
        }

        [HttpGet]
        public async Task<string> GetInformation(string accessToken)
        {
            var infoEndpoint = "https://oneidentitytest.slt.com.lk/authServer/userInfo";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var infoResponse = await client.GetAsync(infoEndpoint);

                if (infoResponse.IsSuccessStatusCode)
                {
                    var infoContent = await infoResponse.Content.ReadAsStringAsync();
                    ViewBag.UserInfo = infoContent;
                }
            }

            return null;
        }

        [HttpGet]
        public async Task<ActionResult> SLTLoginRedirect(string state, string code)
        {

            // Log or print the received values for debugging
            Console.WriteLine($"Received state: {state}, code: {code}");

            // Use the GetToken method to get the access token
            string accessToken = await GetToken(state, code);

            if (accessToken != null)
            {
                // Use the GetInformation method to get the user information
                await GetInformation(accessToken);

                // Redirect the user to the homepage
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("SLTLogin");
            }
        }

        */

    }
}


