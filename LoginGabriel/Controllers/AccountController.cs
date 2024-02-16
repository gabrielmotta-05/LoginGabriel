using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using LoginGabriel.Models;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController()
    {
        _context = new ApplicationDbContext();
    }

    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_context.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email já está sendo usado por outro usuário.");
            return View(model);
        }

        var newUser = new User
        {
            Name = model.Name,
            Email = model.Email,
            Password = EncryptPassword(model.Password)
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Envie e-mail de confirmação (opcional)

        return RedirectToAction("Login", "Account");
    }

    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var hashedPassword = EncryptPassword(model.Password);

        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == hashedPassword);

        if (user == null)
        {
            ModelState.AddModelError("", "Credenciais inválidas.");
            return View(model);
        }

        // Armazenar informações do usuário em cookie ou session (opcional)

        return RedirectToAction("Index", "Home");
    }

    public ActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public ActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("", "Nenhum usuário encontrado com este email.");
            return View();
        }

        // Gerar nova senha aleatória ou enviar link para redefinição de senha (conforme diferencial)

        return RedirectToAction("Login", "Account");
    }

    private string EncryptPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashedBytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
        base.Dispose(disposing);
    }
}
