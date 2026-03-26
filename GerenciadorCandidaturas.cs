using  System.Globalization;

class GerenciadorCandidaturas
{
    private readonly string caminhoArquivo;
    private List<Candidatura> candidaturas;

    public GerenciadorCandidaturas(string caminhoArquivo)
    {
        this.caminhoArquivo = caminhoArquivo;
        candidaturas = new List<Candidatura>();
    }


    public void Adicionar(Candidatura candidatura)
    {
        candidaturas.Add(candidatura);
    }

    public void Salvar()
    {
        var diretorio = Path.GetDirectoryName(caminhoArquivo);
        if (!string.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);
        }

        var linhas= new  List<string>();

        linhas.Add("Empresa;Vaga;DataCandidatura;Status;LinkVaga;Plataforma");

        foreach (var c in candidaturas)
        {
            linhas.Add($"{Escapar(c.Empresa)};{Escapar(c.Vaga)};{c.DataCandidatura:yyyy-MM-dd};{Escapar(c.Status)};{Escapar(c.linkVaga)};{Escapar(c.Plataforma)}");
        }

        File.WriteAllLines(caminhoArquivo, linhas);
        Console.WriteLine($"csv salvo em {caminhoArquivo}");
    }

    private string Escapar(string valor)
    {
        if (valor == null) return "";
        if (valor.Contains(';') || valor.Contains('"'))
            return $"\"{valor.Replace("\"", "\"\"")}\"";
        return valor;
    }

    private string Desescapar(string valor)
    {
        if (string.IsNullOrEmpty(valor)) return "";
        if (valor.StartsWith("\"") && valor.EndsWith("\""))
        {
            valor = valor.Substring(1, valor.Length - 2);
            valor = valor.Replace("\"\"", "\"");
        }
        return valor;
    }


    public void CarregarCsv()
    {
        if (!File.Exists(caminhoArquivo))
        {
            Console.WriteLine("Arquivo não encontrado. Criando novo arquivo.");
            return;
        }

        var linhas = File.ReadAllLines(caminhoArquivo);
        candidaturas.Clear();

        for (int i = 1; i < linhas.Length; i++)
        {
            var campos = linhas[i].Split(';');
            if (campos.Length < 6) continue;

            var candidatura = new Candidatura
            {
                Empresa = Desescapar(campos[0]),
                Vaga = Desescapar(campos[1]),
                DataCandidatura = DateTime.ParseExact(campos[2], "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Status = Desescapar(campos[3]),
                linkVaga = Desescapar(campos[4]),
                Plataforma = Desescapar(campos[5])
            };

            candidaturas.Add(candidatura);

            Console.WriteLine($"{candidaturas.Count} candidaturas carregadas.");
        }
    }

    public void AtualizarStatus(string empresa, string vaga, DateTime dataCandidatura, string novoStatus)
    {
        var candidaturaEncontrada = candidaturas.FirstOrDefault(c => c.Empresa == empresa && c.Vaga == vaga && c.DataCandidatura == dataCandidatura);
        if (candidaturaEncontrada == null)
        {
            Console.WriteLine("Candidatura nao encontrada para atualizar.");
            return;
        }

        candidaturaEncontrada.Status = novoStatus;
        Console.WriteLine($"Status atualizado para {novoStatus} na candidatura {candidaturaEncontrada.Empresa} - {candidaturaEncontrada.Vaga}");
    }

    public void Listar()
    {
        foreach (var c in candidaturas)
        {
            // :dd/MM/yyyy formata a data no padrão brasileiro
            Console.WriteLine($"{c.DataCandidatura:dd/MM/yyyy} | {c.Empresa} | {c.Vaga} | {c.Status}");
        }
    }

}
