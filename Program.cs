using System;
using System.IO;
using System.Linq;
using Emailcs;

Console.WriteLine("--- Gerenciador de Candidaturas ---");

// Procura a raiz do projeto (onde está o arquivo .csproj) para não salvar dentro da pasta bin/
string diretorioBase = AppDomain.CurrentDomain.BaseDirectory;
while (!Directory.GetFiles(diretorioBase, "*.csproj").Any())
{
    var diretorioPai = Directory.GetParent(diretorioBase);
    if (diretorioPai == null)
    {
        diretorioBase = AppDomain.CurrentDomain.BaseDirectory; // Volta pro padrão se não achar
        break;
    }
    diretorioBase = diretorioPai.FullName;
}

string caminhoArquivo = Path.Combine(diretorioBase, "candidatura csv", "candidaturas.csv");
var gerenciador = new GerenciadorCandidaturas(caminhoArquivo);

// Carregar candidaturas existentes
gerenciador.CarregarCsv();

Console.WriteLine("\n--- Verificando E-mails (IMAP) ---");
try
{
    var emails = LeitorImapMailKit.LerUltimosEmails();
    var filtro = new FiltroEmail();

    if (emails.Count == 0)
    {
        Console.WriteLine("Nenhum e-mail novo encontrado.");
    }
    else
    {
        foreach (var email in emails)
        {
            var resultado = filtro.Classificar(email);
            
            Console.WriteLine($"E-mail de: {email.Remetente}");
            Console.WriteLine($"Assunto: {email.Assunto}");
            Console.WriteLine($"Classificação: {resultado.Classificacao}");
            
            // Opcional: Aplicar marcador no provedor de e-mail (Descomente para usar)
            // LeitorImapMailKit.AplicarMarcador(email, resultado.Classificacao);

            Console.WriteLine(new string('-', 30));
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao verificar e-mails: {ex.Message}");
    Console.WriteLine("Dica: Verifique se as variáveis de ambiente IMAP estão configuradas.");
}

Console.WriteLine("\n--- Adicionando dados de exemplo ---");
gerenciador.Adicionar(new Candidatura
{
    Empresa = "INTUO Softwares",
    Vaga = "Desenvolvedor Júnior",
    DataCandidatura = DateTime.Today,
    Status = "Enviado",
    linkVaga = "https://...",
    Plataforma = "LinkedIn"
});

gerenciador.AtualizarStatus("INTUO Softwares", "Desenvolvedor Júnior", DateTime.Today, "Entrevista Agendada");

Console.WriteLine("\n--- Lista de Candidaturas Salvas ---");
gerenciador.Listar();
gerenciador.Salvar();