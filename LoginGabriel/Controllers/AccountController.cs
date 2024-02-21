using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using LoginGabriel.Models;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Outros métodos do controlador

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

        // Verificar a força da senha
        var passwordStrength = CalculatePasswordStrength(model.Password);

        // Constroi o novo usuário
        var newUser = new User
        {
            Name = model.Name,
            Email = model.Email,
            Password = EncryptPassword(model.Password)
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Envia e-mail de confirmação
        SendConfirmationEmail(newUser.Email);

        return RedirectToAction("Login", "Account");
    }

    // Método para calcular a força da senha
    private int CalculatePasswordStrength(string password)
    {
        int strength = 0;
        if (password.Length >= 8) strength++; // Verifica o comprimento mínimo da senha
        if (password.Any(char.IsUpper)) strength++; // Verifica se a senha contém letras maiúsculas
        if (password.Any(char.IsLower)) strength++; // Verifica se a senha contém letras minúsculas
        if (password.Any(char.IsDigit)) strength++; // Verifica se a senha contém números
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) strength++; // Verifica se a senha contém caracteres especiais
        return strength;
    }

    // Método para enviar e-mail de confirmação
    private void SendConfirmationEmail(string email)
    {
        var message = new MailMessage();
        message.To.Add(new MailAddress(email)); // Endereço de e-mail do destinatário
        message.Subject = "Confirmação de Cadastro de usuário";
        message.Body = "Seu cadastro foi confirmado com sucesso.";

        using (var smtpClient = new SmtpClient("smtp.example.com"))
        {
            smtpClient.Port = 587; // Porta SMTP
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("exemplo@gmail.com", "senha"); // email que será enviado a confirmação
            smtpClient.EnableSsl = true; // Habilita SSL/TLS

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de confirmação: {ex.Message}");
            }
        }
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
