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
| [`PROJETO.md`](./PROJETO.md) | Arquitetura, padrões técnicos, segurança, roadmap e critérios de entrada no piloto |
| [`docs/regras-de-negocio.md`](./docs/regras-de-negocio.md) | Regras de negócio (`RN-...`) e conformidade legal (LGPD, CDC, Marco Civil e outras) |
| [`docs/decisoes/`](./docs/decisoes/) | Registro de decisões (ADRs) |
| [`CONTRIBUTING.md`](./CONTRIBUTING.md) | Como trabalhar no projeto: branches, commits e pull requests |
| [`SECURITY.md`](./SECURITY.md) | Como relatar uma vulnerabilidade |
| [`PILOT_FEEDBACK.md`](./PILOT_FEEDBACK.md) | Feedback do piloto, sem dados de pacientes |

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
├── backend/        solução .NET (Domain, Application, Infrastructure, Api, testes)
├── frontend/       aplicação Angular (core, features, shared)
├── e2e/            testes de ponta a ponta
├── docs/           regras de negócio, decisões, runbooks e documentos de LGPD
├── .github/        workflows de CI/CD e modelos de issue e PR
└── docker-compose.yml
```

Detalhes de cada pasta na seção 7 do [`PROJETO.md`](./PROJETO.md).

## Como rodar localmente

> Os comandos abaixo passam a funcionar à medida que as tarefas do milestone **M1 · Fundação técnica** forem concluídas.

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (versão LTS) e Angular CLI: `npm install -g @angular/cli`
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com) e [GitHub CLI](https://cli.github.com)

### Passo a passo

```bash
# 1. Clonar
git clone https://github.com/SEU-USUARIO/clinica-acolher.git
cd clinica-acolher

# 2. Variáveis locais (troque a senha no arquivo .env)
cp .env.example .env

# 3. Banco de dados
docker compose up -d postgres

# 4. Backend: http://localhost:5080/api/health
cd backend
dotnet restore
dotnet ef database update --project src/ClinicaAcolher.Infrastructure --startup-project src/ClinicaAcolher.Api
dotnet run --project src/ClinicaAcolher.Api

# 5. Frontend, em outro terminal: http://localhost:4200
cd frontend
npm install
npm start
```

Ou tudo de uma vez, com Docker:

```bash
docker compose up --build
```

### Testes

```bash
cd backend && dotnet test                 # unitários e integração (precisa do Docker)
cd frontend && npx ng test --watch=false  # frontend
npx playwright test                       # ponta a ponta, na pasta e2e/
```

## Como contribuir

1. Escolha uma issue no [Project](https://github.com/users/SEU-USUARIO/projects/NUMERO), de preferência da view **Piloto** (prioridades P0 e P1).
2. Crie uma branch: `feat/123-nome-curto`.
3. Faça commits no padrão Conventional Commits: `feat(agenda): impede sessões sobrepostas`.
4. Abra um pull request com `Closes #123` e preencha o checklist de segurança.
5. O merge só acontece com o CI verde.

Mais detalhes em [`CONTRIBUTING.md`](./CONTRIBUTING.md) e na seção 27 do [`PROJETO.md`](./PROJETO.md).

## Segurança e privacidade

- **Nunca** use dados reais de pacientes em desenvolvimento, testes, issues, commits ou capturas de tela.
- **Nunca** versione senhas, chaves ou o arquivo `.env`.
- Encontrou uma vulnerabilidade? **Não abra issue pública.** Siga as instruções do [`SECURITY.md`](./SECURITY.md).

## Licença

Código proprietário. Todos os direitos reservados. Uso, cópia ou distribuição sem autorização expressa não são permitidos.
