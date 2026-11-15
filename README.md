# Clínica Acolher

**Clínica online para psicanalistas:** agenda, sessões por vídeo, financeiro e anotações em um só lugar, com sigilo em primeiro lugar.

![CI](https://github.com/GustavoSilveira1012/Clinica-Acolher/actions/workflows/quality.yml/badge.svg)

> 🚧 **Em construção.** O projeto está na fase de descoberta e fundação técnica (milestones M0 e M1). Nenhuma funcionalidade está pronta para uso real ainda. Acompanhe pelo [Project no GitHub](https://github.com/users/SEU-USUARIO/projects/NUMERO).

---

## O que é

O paciente agenda, paga e faz a sessão por vídeo pela plataforma. O profissional organiza a agenda semanal, acompanha cobranças e faltas, emite recibos e registra anotações clínicas cifradas. Todo atendimento é online, sem sessões presenciais.

O primeiro uso será um **piloto gratuito e controlado** com uma clínica de psicanálise. Depois, a plataforma será oferecida como SaaS para outras clínicas e profissionais.

## Funcionalidades planejadas

| Área | O que vai fazer |
|---|---|
| Agenda | Horários fixos semanais, sessões avulsas, bloqueios, política de faltas com aceite do paciente |
| Atendimento online | Sala de vídeo exclusiva por sessão, sala de espera, protocolo de emergência, **sem gravação** |
| Pacientes | Cadastro mínimo, responsável legal para menores, nome social, contato de emergência |
| Financeiro | Cobranças, Pix e cartão, recibos, pacotes com direito de arrependimento |
| Anotações clínicas | Rascunho e finalização, versões, adendos, criptografia e auditoria |
| Portal do paciente | Próximas sessões, pagamentos, recibos e privacidade pelo celular |
| Privacidade | Consentimentos versionados, direitos do titular (LGPD), retenção e bloqueio legal |
| Notificações | Lembretes discretos por e-mail e WhatsApp, sem revelar o motivo do atendimento |

## Princípios

- **Sigilo por padrão:** sessões nunca gravadas, notificações discretas, nada clínico em logs.
- **Dado clínico é do vínculo paciente ↔ profissional**, nunca da clínica.
- **Nada importante é apagado:** histórico imutável, correções por novos registros.
- **Somente dados fictícios** em desenvolvimento, testes e demonstração.

## Documentação

| Documento | Conteúdo |
|---|---|
| [`docs/projeto.md`](./docs/projeto.md) | Arquitetura, padrões técnicos, segurança, roadmap e critérios de entrada no piloto |
| [`docs/Regras de negocio.md`](./docs/Regras%20de%20negocio.md) | Regras de negócio (`RN-...`) e conformidade legal |
| [`docs/glossario.md`](./docs/glossario.md) | Vocabulário comum do domínio |
| [`docs/decisoes/`](./docs/decisoes/) | Registro de decisões (ADRs) |
| [`CONTRIBUTING.md`](./CONTRIBUTING.md) | Convenções de branches, commits e pull requests |

## Stack

| Camada | Tecnologias |
|---|---|
| Backend | C# · .NET 10 · ASP.NET Core · ASP.NET Core Identity · Entity Framework Core |
| Frontend | Angular · TypeScript · Signals · Tailwind CSS |
| Banco de dados | PostgreSQL 17+ |
| Testes | xUnit · Testcontainers · Vitest · Playwright |
| Infraestrutura | Docker · GitHub Actions · nuvem com região no Brasil |

## Estrutura

```text
clinica-acolher/
├── backend/
│   ├── src/        API, Application, Domain e Infrastructure
│   └── tests/      testes unitários e de integração
├── frontend/
│   └── src/        aplicação Angular
├── docs/           regras, decisões, escopo e documentação do produto
├── .github/        workflows de CI
├── .editorconfig
├── .gitignore
└── docker-compose.yml
```

Detalhes de arquitetura e organização estão em [`docs/projeto.md`](./docs/projeto.md).

## Como rodar localmente

### Pré-requisitos

- Docker Desktop com Docker Compose
- .NET 10 SDK
- Node.js 22 e npm

### Subir o sistema com Docker

No PowerShell, na raiz do repositório:

```powershell
if (-not (Test-Path .env)) { Copy-Item .env.example .env }
```

Edite `.env` e substitua a senha de exemplo por uma senha local forte. Não versione esse arquivo. Inicie o banco:

```powershell
docker compose up -d postgres
```

Aplique as migrações do banco. O comando abaixo lê as configurações do `.env` sem imprimi-las no terminal:

```powershell
Set-Location backend
$settings = @{}
Get-Content ..\.env | ForEach-Object {
  if ($_ -match '^\s*([^#=]+)=(.*)$') {
    $settings[$matches[1].Trim()] = $matches[2].Trim()
  }
}
$env:ConnectionStrings__Default = "Host=localhost;Port=5433;Database=$($settings['POSTGRES_DB']);Username=$($settings['POSTGRES_USER']);Password=$($settings['POSTGRES_PASSWORD'])"
dotnet tool restore
dotnet ef database update --project src/ClinicaAcolher.Infrastructure --startup-project src/ClinicaAcolher.Api
Remove-Item Env:ConnectionStrings__Default
Set-Location ..
```

Depois, suba a API e o frontend:

```powershell
docker compose up --build
```

Abra `http://localhost:8080`. O frontend encaminha chamadas `/api` à API dentro do Compose. O PostgreSQL fica disponível localmente na porta `5433`.

### Desenvolvimento com recarga automática

Depois de iniciar o PostgreSQL e aplicar as migrações como acima, execute a API localmente em um terminal PowerShell:

```powershell
Set-Location backend
$settings = @{}
Get-Content ..\.env | ForEach-Object {
  if ($_ -match '^\s*([^#=]+)=(.*)$') {
    $settings[$matches[1].Trim()] = $matches[2].Trim()
  }
}
$env:ConnectionStrings__Default = "Host=localhost;Port=5433;Database=$($settings['POSTGRES_DB']);Username=$($settings['POSTGRES_USER']);Password=$($settings['POSTGRES_PASSWORD'])"
$env:ASPNETCORE_URLS = 'http://localhost:5080'
dotnet run --project src/ClinicaAcolher.Api
```

Em outro terminal:

```powershell
Set-Location frontend
npm ci
npm start
```

Abra `http://localhost:4200`; o proxy Angular encaminha `/api` para `http://localhost:5080`.

### Testes

```powershell
Set-Location backend
dotnet test

Set-Location ..\frontend
npm ci
npm test -- --watch=false
```

## Como contribuir

1. Escolha uma [issue](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues), de preferência da view **Piloto** (prioridades P0 e P1).
2. Crie uma branch: `feat/123-nome-curto`.
3. Faça commits no padrão Conventional Commits: `feat(agenda): impede sessões sobrepostas`.
4. Abra um pull request com `Closes #123` e preencha o checklist de segurança.
5. O merge só acontece com o CI verde.

Veja o [guia de contribuição](./CONTRIBUTING.md) para detalhes e exemplos.

## Segurança e privacidade

- **Nunca** use dados reais de pacientes em desenvolvimento, testes, issues, commits ou capturas de tela.
- **Nunca** versione senhas, chaves ou o arquivo `.env`.
- Encontrou uma vulnerabilidade? **Não abra issue pública.** Siga as instruções do [`SECURITY.md`](./SECURITY.md).

## Licença

Código proprietário. Todos os direitos reservados. Uso, cópia ou distribuição sem autorização expressa não são permitidos.
