using Microsoft.AspNetCore.Mvc;

using AppLoginv2.Data;
using AppLoginv2.Models;
using Microsoft.EntityFrameworkCore;
using AppLoginv2.ViewModels;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AppLoginv2.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public AccesoController(AppDBContext appDBContext) 
        {
            _appDBContext = appDBContext;
        }


        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            if (modelo.Clave !=modelo.ConfirmarClave) 
            {
                ViewData["Mensaje"] = "Las claves son diferentes";
                return View();
            }

            Usuario usuario = new Usuario()
            {
                Nombre=modelo.Nombre,
                Correo=modelo.Correo,
                Clave=modelo.Clave

            };

            await _appDBContext.Usuarios.AddAsync(usuario);
            await _appDBContext.SaveChangesAsync();

            if (usuario.IdUsuario !=0)return RedirectToAction("Login","Acceso");
            ViewData["Mensaje"] = "No se puede crear el usuario";
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if(User.Identity!.IsAuthenticated)return RedirectToAction("Index","Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo) 
        {
            Usuario? usuario_encontrado=await _appDBContext.Usuarios
                                        .Where(u=>
                                            u.Correo==modelo.Correo &&
                                            u.Clave==modelo.Clave 
                                        ).FirstOrDefaultAsync();

            if (usuario_encontrado == null) 
            {
                ViewData["Mensaje"] = "No se encontraron coincidencias";
                return View();
            }

            List<Claim>claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,usuario_encontrado.Nombre)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal (claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult CheckSession()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }



    }
}
