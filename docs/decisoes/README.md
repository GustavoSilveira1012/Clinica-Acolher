# Registro de decisões

Esta pasta mantém decisões técnicas e de produto que afetam o comportamento, a arquitetura, a segurança ou o escopo da Clínica Acolher. Registre uma decisão quando houver alternativas relevantes ou quando uma escolha futura precisar ser compreendida sem depender do histórico de conversas.

## Como registrar

1. Copie [`MODELO.md`](./MODELO.md) para um novo arquivo Markdown nesta pasta.
2. Use um identificador sequencial disponível e um título curto no nome do arquivo. Antes de escolher o número, confira os arquivos existentes; preserve os identificadores históricos, mesmo quando sua formatação variar.
3. Preencha contexto, alternativas consideradas, decisão e consequências. Marque como **proposta** enquanto aguarda validação; use **aceita** somente quando o responsável pela decisão realmente a tiver aprovado.
4. Vincule issues e regras de negócio pertinentes. Se uma decisão substituir outra, indique o identificador anterior e atualize o status do registro substituído.
5. Registre incertezas, condições para implementação e validações pendentes. Uma decisão documentada não significa que a funcionalidade foi implementada ou liberada.
6. Use apenas exemplos e dados fictícios. Não inclua dados de pacientes, credenciais, tokens ou outros segredos.

## Decisões existentes

| Registro | Assunto | Status |
|---|---|---|
| [0001 — Autenticação por cookie](./0001-autenticacao-por-cookie.md) | Autenticação com ASP.NET Core Identity e cookie de sessão | Aceita |
| [001 — Psicólogos na fase 2](./001-psicologos-na-fase-2.md) | Público da fase SaaS e condições de liberação | Decisão aceita; liberação condicionada |
| [002 — Formatos do piloto](./002-formatos-do-piloto.md) | Vídeo, anotações e pagamentos no piloto | Direção definida; aprovação do escopo pendente |
| [003 — Anotações clínicas no piloto](./003-anotacoes-clinicas-no-piloto.md) | Inclusão de anotações e condições para dados reais | Inclusão definida; liberação condicionada |

O registro `0001-autenticacao-por-cookie.md` é a primeira decisão técnica já documentada nesta pasta. Esta lista é um índice, não substitui os registros individuais.
