using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace MyApp.API.Controllers
{
    /// <summary>
    /// Insecure controller for testing purposes
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class InsecureController : ControllerBase
    {
        /// <summary>
        /// Retrieves some suspicious values
        /// </summary>
        /// <returns>A collection of string suspicious values</returns>
        /// <response code="200">Returns the list of values</response>
        [HttpGet]
        [Route("/my-values")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] {
                "1234Qwert%",
                "non-pi information",
                "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lKdGVVRlFTU0lzSW5CaGMzTjNiM0prSWpvaVpHOTBibVYwTWpBeU5TVXVJaXdpYm1GdFpTSTZJa1J2ZEc1bGRDQXlNREkxSWl3aWFXRjBJam94TlRFMk1qTTVNREl5ZlEuR0c1bmhkVVZuOUNsU0ZGYWd0MHRQb2RKdy12TGpCSzJQSHhtSWgwOTFUOA==",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJteUFQSSIsInBhc3N3b3JkIjoiZG90bmV0MjAyNSUuIiwibmFtZSI6IkRvdG5ldCAyMDI1IiwiaWF0IjoxNTE2MjM5MDIyfQ.GG5nhdUVn9ClSFFagt0tPodJw-vLjBK2PHxmIh091T8"
            };
        }

        /// <summary>
        /// Creates both secure and insecure cookies for testing purposes
        /// </summary>
        /// <returns>HTTP 200 OK response</returns>
        /// <response code="200">Operation completed successfully</response>
        [HttpPost]
        [Route("/cookies")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> Post()
        {
            // Set secure cookie with all security features
            Response.Cookies.Append(
                    "SecureCookie",
                    "Q3rty12d0tnet",
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

            // Set insecure cookie with minimal security
            Response.Cookies.Append(
                "InsecureCookie",
                "InsecureValue123",
                new CookieOptions
                {
                    // No security options set
                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return Ok();
        }
        
        [HttpDelete]
        [Route("/cookies")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> Delete()
        {
            Response.Cookies.Delete("SecureCookie");
            Response.Cookies.Delete("InsecureCookie");

            return Ok();
        }


    }
}
