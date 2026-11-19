# 0001 — Autenticação por cookie de sessão

**Data:** 2026-11-14 · **Status:** aceita

## Contexto
O PROJETO.md previa JWT com refresh token. O frontend Angular e a API são servidos no mesmo domínio (nginx).

## Decisão
Usar ASP.NET Core Identity com cookie de sessão HttpOnly, Secure e SameSite=Strict, com expiração de 30 minutos por inatividade.

## Consequências
- Nenhuma credencial acessível ao JavaScript (proteção contra roubo por XSS).
- Sessões encerráveis pelo servidor.
- Proteção contra CSRF via SameSite=Strict e API que só aceita JSON.
- Um futuro app móvel precisará de outro esquema de autenticação (ex.: tokens).