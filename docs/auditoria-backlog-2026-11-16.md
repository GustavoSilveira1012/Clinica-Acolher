# Auditoria do backlog — 16/11/2026

## Base e resultado

Revisão dos documentos, código publicado e issues do repositório Clínica Acolher, com base no commit `023df02977fc3c487e1b4a2b13bf8f62c8c95330`. As alterações locais em andamento de autorização não foram consideradas entregas concluídas.

Resultado: quatro issues encerradas pelos critérios funcionais verificados, uma encerrada por mudança de arquitetura e três novas issues para lacunas específicas dos documentos. Os demais trabalhos foram associados às issues existentes, evitando duplicação.

Gustavo informou nesta data que sua mãe aprovou as decisões restantes e que várias issues foram encerradas por não se aplicarem. O relato foi registrado no [escopo](escopo-piloto.md) e nas decisões [002](decisoes/002-formatos-do-piloto.md) e [003](decisoes/003-anotacoes-clinicas-no-piloto.md). A data original da aprovação não foi informada.

As issues #7, #8 e #13 já estavam encerradas como `not_planned` e foram preservadas. Isso não é evidência de contratação de advogado, escolha de nome final ou execução dos critérios originais dessas issues. A #4 foi preservada conforme o encerramento do responsável; a descrição das três dores ainda não consta como resultado concreto no documento da rotina. Não foram inventadas respostas.

A [#9 — domínio e perfis em redes sociais](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/9) permanece aberta. A referência do usuário à pendência de conta online foi associada a essa tarefa de descoberta, sem presumir que as demais milestones técnicas estejam concluídas.

## Encerramentos

| Issue | Resultado | Evidência e limite |
| --- | --- | --- |
| [#33 — Dockerfile backend](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/33) | Completed | Build multi-estágio e runtime separado; container local executando com UID/GID 1654, usuário app. |
| [#35 — Compose completo](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/35) | Completed | API, web e PostgreSQL em execução; banco healthy; web e health da API retornaram 200 via porta 8080. |
| [#84 — Login e bloqueio](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/84) | Completed | Identity padrão, senha mínima de 10 caracteres, bloqueio de 15 minutos após cinco falhas; AuthApiTests cobre bloqueio e respostas de autenticação. |
| [#88 — TOTP](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/88) | Completed | MFA exigido para Admin e Professional; paciente pode ativar opcionalmente. TwoFactorApiTests cobre bloqueio, ativação e login com código. |
| [#85 — JWT e refresh](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/85) | Not planned | Substituída pela ADR 0001 de autenticação por cookie; rotação de refresh tokens não foi implementada nem marcada como concluída. |

Esses encerramentos registram as entregas delimitadas, não a prontidão global para produção. As checklists genéricas das issues foram preservadas sem marcar evidências não verificadas. Houve revisão técnica do código nesta auditoria, mas não há aprovação formal de review registrada nos PRs consultados. A gestão/revogação de sessões (#86), autorização (#90/#91), CSRF (#94), chaves (#245/#246), hashes de recuperação (#247) e reuso de TOTP (#248) continuam pendentes. Um futuro papel de secretaria deve integrar a exigência de MFA em #90.

## Novas issues

| Issue | Milestone | Entrega |
| --- | --- | --- |
| [#251 — Vídeo externo](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/251) | M6 | Caminho externo autorizado por sessão, capacidades reais do provedor, contingência e testes. Complementa #99, #101, #103, #105 e #109. |
| [#252 — Anotações externas](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/252) | M8 | Fonte oficial, guarda, acesso, continuidade e eventual transferência, com procedimento validado. Complementa o prontuário interno e #151/#152/#154. |
| [#253 — Validar preço SaaS](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/253) | M14 | Executar a pesquisa de preço e custos prevista na hipótese da #11, antes de abrir vendas. Não bloqueia o piloto assistido. |

As três foram conferidas no Project 3 como Backlog; as cinco encerradas, como Done. A #253 não transforma R$ 49,90 em preço aprovado e não autoriza cobrança real.

## Requisitos já cobertos

| Documento/requisito | Cobertura existente e ajuste |
| --- | --- |
| Pix e cartão no piloto | #116 a #121, #124 e #127: provedor, integração, confirmação, conciliação, recibos, pacotes e mensalidades. Sem criar outro épico financeiro. |
| Anotações internas | #132 a #140: criptografia, rascunho/finalização, histórico, adendos, interface, exportação, auditoria e chaves. A decisão de incluir não comprova implementação. |
| Vídeo nos dois formatos | #99, #105 e #109 receberam critérios complementares para fornecedor externo, ausência de gravação e limites da coleta de metadados. |
| Diferenciais da pesquisa | #192 e #193 passaram a referenciar as três hipóteses para feedback e retrospectiva. Não são vantagens exclusivas comprovadas nem funcionalidades automaticamente novas. |
| Modelo de cobrança | #195 e #197 referenciam a hipótese de plano/teste e a validação #253. A implementação comercial segue em M14. |
| Cookies no Angular | #95 foi alinhada à ADR 0001; removida a exigência obsoleta de anexar JWT. Guards por perfil continuam pendentes. |

## Issues mantidas abertas por lacunas verificadas

- **#71 — Séries:** existem janela de 84 dias, worker e teste de avanço/idempotência. Entretanto, GenerateUntil filtra apenas a data, podendo gerar uma ocorrência já passada no mesmo dia. Usa também a data UTC como início, podendo ignorar uma ocorrência futura na noite local anterior. O lote fixo de 500, ordenado por Id, pode impedir progresso de outras séries quando as primeiras permanecem pendentes. Esses pontos foram registrados na própria issue, com testes solicitados.
- **#89 — Recuperação 2FA:** o uso único funciona e tem teste, mas o armazenamento somente como hash depende de #247.
- **#90/#91 — Autorização:** login e MFA não substituem permissões por perfil/recurso e vínculo terapêutico. Os arquivos locais em edição não foram tratados como entrega validada.
- **#95/#96 — Fluxos de autenticação:** guard por sessão/MFA não conclui guard por perfil, cadastro, verificação e recuperação de acesso.
- **#34 — Frontend Docker:** servir o frontend não comprova os cabeçalhos de segurança previstos nessa issue.
- **#73 — Fusos:** testes de séries não comprovam a apresentação da agenda no fuso de outro usuário.
- **#21/#28/#31/#38/#39:** auditoria NuGet não substitui toda a configuração de analisadores; guia visual não instala/aplica Tailwind; dependência Prettier não comprova ESLint; atualização de dependências não é scanner de segredos; CI verde não é relatório de cobertura.

Os achados das séries são de inspeção estática, não reprodução contra pacientes reais. Exemplos para testes: em São Paulo, criar às 18h uma série para as 15h do próprio dia deve ignorar essa ocorrência; às 00h UTC de 18/11/2026 ainda são 21h de 17/11 e uma sessão às 22h locais continua futura.

## Validação e próximos passos

- [CI consultado](https://github.com/GustavoSilveira1012/Clinica-Acolher/actions/runs/43674621432): sucesso no commit auditado, incluindo build/testes do backend e frontend. A suíte não foi repetida localmente nesta auditoria documental.
- Docker consultado apenas para leitura e health; containers API/web criados em 14/11/2026, sem reconstrução ou reinício nesta revisão. Isso comprova o ambiente existente, não uma nova imagem de todas as alterações locais.
- Alterações desta tarefa: corpos/estados de issues e documentação. Nenhuma funcionalidade implementada, nenhum dado real de paciente utilizado e nenhum commit ou push executado.
- Priorizar as lacunas de autorização e proteção já abertas, os ajustes de séries e as entregas do piloto nas milestones atuais. Executar validação comercial da #253 depois do piloto.

Os documentos locais desta auditoria ainda precisam entrar em um commit para que o registro atualizado da aprovação e este relatório sejam publicados no repositório.
