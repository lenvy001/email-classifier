# Classificador de E-mails (C# + MailKit)

Projeto em C# para:
- ler os últimos e-mails via IMAP (Gmail ou outro provedor),
- classificar por regras configuráveis em `regras.json`,
- aplicar marcador no e-mail conforme a classificação.

O processamento roda em modo contínuo (loop de serviço), ideal para execução 24/7.

## Stack

- .NET 10 (`net10.0`)
- `MailKit` para IMAP
- `System.Text.Json` para leitura das regras

## Estrutura

- `Program.cs` -> loop contínuo de processamento (24/7).
- `email.cs` -> modelos, classificação por score e operações IMAP.
- `regras.json` -> palavras-chave, remetentes, pesos e limiares.
- `MeuPrimeiroCSharp.csproj` -> projeto e dependências.

## Como funciona

### 1) Leitura de e-mails

A função `LeitorImapMailKit.LerUltimosEmails()`:
- conecta no IMAP,
- abre a mailbox (`INBOX` por padrão),
- busca todos os UIDs,
- pega os últimos `IMAP_LIMIT` (padrão 10),
- monta objetos `Email` com remetente, assunto, corpo e UID.

### 2) Classificação

`FiltroEmail.Classificar(email)` calcula score para 3 classes:
- `prioridade`
- `candidatura`
- `alerta de vaga`

A classificação usa:
- palavras-chave no assunto e corpo,
- remetentes por classe,
- pesos por campo (`assunto`, `corpo`, `remetente`),
- limiar mínimo e diferença entre classes.

Se o score não atingir confiança mínima, classifica como `outros`.

### 3) Marcador

`LeitorImapMailKit.AplicarMarcador(email, classificacao)` aplica label:
- `prioridade` -> `entrevista`
- `candidatura` -> `candidatura`
- `alerta de vaga` -> `alerta`
- `outros` -> não aplica marcador

Para Gmail, usa extensão de labels (`GMailExt1`).

## Variáveis de ambiente

Obrigatórias:
- `IMAP_USER`
- `IMAP_PASSWORD` (use senha de app no Gmail)

Opcionais:
- `IMAP_HOST` (default: `imap.gmail.com`)
- `IMAP_PORT` (default: `993`)
- `IMAP_MAILBOX` (default: `INBOX`)
- `IMAP_LIMIT` (default: `10`)
- `IMAP_APLICAR_MARCADOR` (`1` para sim, `0` para não; default `1`)
- `WORKER_INTERVAL_SECONDS` (intervalo entre ciclos; default `60`, mínimo `5`)

## Tutorial (Gmail): gerar senha de app para IMAP

Para usar Gmail com IMAP neste projeto, não use sua senha normal da conta.
Use uma senha de app (16 caracteres):

1. Acesse `https://myaccount.google.com/`.
2. Entre em `Segurança`.
3. Ative `Verificação em duas etapas` (2FA), se ainda não estiver ativa.
4. Volte para `Segurança` e abra `Senhas de app`.
5. Em `Selecionar app`, escolha `Outro (nome personalizado)` e informe algo como `email-classifier`.
6. Clique em `Gerar` e copie a senha exibida.
7. Use essa senha no `IMAP_PASSWORD`.

Exemplo:

```powershell
$env:IMAP_USER="seuemail@gmail.com"
$env:IMAP_PASSWORD="SENHA_DE_APP_GERADA_PELO_GOOGLE"
```

Se `Senhas de app` não aparecer, confira:
- 2FA está ativa na conta.
- conta Google Workspace pode ter bloqueio por política do administrador.

## Executar localmente

```powershell
cd c:\Users\lenvy\Documents\projetos\MeuPrimeiroCSharp

$env:IMAP_USER="seuemail@gmail.com"
$env:IMAP_PASSWORD="SUA_SENHA_DE_APP"
$env:IMAP_HOST="imap.gmail.com"
$env:IMAP_PORT="993"
$env:IMAP_MAILBOX="INBOX"
$env:IMAP_LIMIT="10"
$env:IMAP_APLICAR_MARCADOR="1"

dotnet run
```

## Ajuste fino de precisão

Edite `regras.json` para calibrar:
- `limiarClassificacao`
- `diferencaMinimaEntreClasses`
- `pesos.assunto`, `pesos.corpo`, `pesos.remetente`
- listas `palavrasChave*` e `remetentes*`

Sugestão prática:
- muitos falsos positivos -> aumente `limiarClassificacao` e remova termos genéricos
- muitos `outros` em e-mails válidos -> reduza `limiarClassificacao` e adicione termos específicos

## Segurança

- Não coloque senha no código.
- Não commitar credenciais.
- No Gmail, use senha de app com 2FA ativo.
- Revogue senhas de app não reconhecidas na conta Google.
