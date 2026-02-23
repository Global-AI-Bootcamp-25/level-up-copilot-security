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
                "ZXlKaGJHY2lPaUpJVXpJMU5pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SnpkV0lpT2lJeE1qTTBOVFkzT0Rrd0lpd2libUZ0WlNJNklreGxkbVZzVlhCSGFYUklkV0pEYjNCcGJHOTBJaXdpY0dGemMzZHZjbVFpT2lKc1pYWmxiRlZ3TWpZaUxDSnBZWFFpT2pFMU1UWXlNemt3TWpKOS5rbjAwUHVVWkJiY0FmY3NMYWxFSmZydkR2MTNWOExBMC1YSEJ4NURHTkQ0",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkxldmVsVXBHaXRIdWJDb3BpbG90IiwicGFzc3dvcmQiOiJsZXZlbFVwMjYiLCJpYXQiOjE1MTYyMzkwMjJ9.kn00PuUZBbcAfcsLalEJfrvDv13V8LA0-XHBx5DGND4"
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
