using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // Endpoint para obter todos os usuários
    [HttpGet]
    public IActionResult GetAllUsers()
    {
        var users = _context.Users.ToList();
        return Ok(users);
    }

    // Endpoint para obter um usuário pelo ID
    [HttpGet("{id}")]
    public IActionResult GetUserById(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Verifica se o usuário existe no banco de dados
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == EncryptPassword(model.Password));

        if (user == null)
        {
            ModelState.AddModelError("", "Credenciais inválidas.");
            return View(model);
        }

        return Ok(new { message = "Login realizado com sucesso." });
    }

    public IActionResult CreateUser(User newUser)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Criptografa a senha antes de salvar o usuário
        newUser.Password = EncryptPassword(newUser.Password);

        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Limpa a senha não criptografada do objeto para evitar expô-la indevidamente
        //newUser.PlainPassword = null;

        return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, newUser);
    }

    // Endpoint para atualizar informações do usuário
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        if (id != updatedUser.Id)
        {
            return BadRequest();
        }

        _context.Entry(updatedUser).State = EntityState.Modified;

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Users.Any(u => u.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // Endpoint para excluir um usuário
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return NoContent();
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

    // Endpoint para solicitar redefinição de senha
    [HttpGet]
    public ActionResult ForgotPassword()
    {
        return View();
    }

    // Endpoint para processar a solicitação de redefinição de senha
    [HttpPost]
    public ActionResult ForgotPassword(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError("", "Nenhum usuário encontrado com este email.");
            return View();
        }

        // Gerar um token de redefinição de senha
        var token = Guid.NewGuid().ToString();

        // Salvar o token no banco de dados para o usuário
        user.ResetPasswordToken = token;
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Define um tempo de expiração de uma hora para o token
        _context.SaveChanges();

        // Enviar e-mail com o link de redefinição de senha
        SendPasswordResetEmail(user.Email, token);

        return RedirectToAction("Layout", "Shared");
    }

    // Método para enviar e-mail de redefinição de senha
    private void SendPasswordResetEmail(string email, string token)
    {
        var callbackUrl = Url.Action("ResetPassword", "Account", new { email = email, token = token }, protocol: HttpContext.Request.Scheme);

        var message = new MailMessage();
        message.To.Add(new MailAddress(email));
        message.Subject = "Redefinir Senha";
        message.Body = $"Por favor, clique no link a seguir para redefinir sua senha: <a href='{callbackUrl}'>Redefinir Senha</a>";
        message.IsBodyHtml = true;

        using (var smtpClient = new SmtpClient("smtp.example.com"))
        {
            smtpClient.Port = 587;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("example@gmail.com", "senha");
            smtpClient.EnableSsl = true;

            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail de redefinição de senha: {ex.Message}");
            }
        }
    }

    // Método para redefinir a senha
    [HttpGet]
    public ActionResult ResetPassword(string email, string token)
    {
        // Verifica se o token de redefinição de senha é válido e se ainda está dentro do tempo de expiração
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.ResetPasswordToken == token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            // Token inválido ou expirado
            return RedirectToAction("Layout", "Shared");
        }

        // Exiba uma página de redefinição de senha para o usuário
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    public ActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Verifica se o token de redefinição de senha é válido e se ainda está dentro do tempo de expiração
        var user = _context.Users.FirstOrDefault(u => u.Email == model.Email && u.ResetPasswordToken == model.Token && u.ResetPasswordTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            // Token inválido ou expirado
            return RedirectToAction("Layout", "Shared");
        }

        // Define a nova senha para o usuário e limpa o token de redefinição de senha
        user.Password = EncryptPassword(model.NewPassword); // Usando a nova senha fornecida no ViewModel
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;
        _context.SaveChanges();

        // Redireciona o usuário para a página de login
        return RedirectToAction("Layout", "Shared");
    }


    // Método para criptografar a senha
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
