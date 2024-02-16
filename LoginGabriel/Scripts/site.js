$(document).ready(function () {
    // Mostrar modal de login quando o botão "Login" é clicado
    $("#loginButton").click(function () {
        $("#loginModal").modal("show");
    });

    // Lidar com o envio do formulário de login
    $("#loginForm").submit(function (event) {
        event.preventDefault(); // Evita o envio do formulário tradicional

        // Obter os dados do formulário
        var email = $("#email").val();
        var password = $("#password").val();

        // Aqui você pode enviar uma requisição AJAX para o backend para realizar a autenticação
        // Exemplo:
        $.ajax({
            url: "/Account/Login",
            method: "POST",
            data: {
                email: email,
                password: password
            },
            success: function (response) {
                // Verificar se a autenticação foi bem-sucedida
                if (response.success) {
                    // Redirecionar para a página de boas-vindas
                    window.location.href = "/Home/Welcome";
                } else {
                    // Exibir mensagem de erro para o usuário
                    alert("Credenciais inválidas. Por favor, tente novamente.");
                }
            },
            error: function () {
                // Exibir mensagem de erro genérica em caso de falha na requisição
                alert("Ocorreu um erro ao tentar fazer login. Por favor, tente novamente.");
            }
        });
    });
    $("#forgotPasswordLink").click(function (e) {
        e.preventDefault(); // Evita que o link execute seu comportamento padrão (navegar para uma nova página)

        // Redireciona para a action ForgotPassword do AccountController
        window.location.href = "@Url.Action("ForgotPassword", "Account")";
    });
});

