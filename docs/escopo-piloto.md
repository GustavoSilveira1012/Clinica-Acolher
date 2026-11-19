# Escopo do MVP do piloto — Clínica Acolher

**Versão:** 2, de 16/11/2026 (registro da aprovação; escopo preservado).<br>
**Tarefa:** [#2 — Definir o escopo do MVP do piloto](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/2).  
**Situação:** aprovação da cliente piloto reportada por Gustavo Silveira em 16/11/2026. A data original da aprovação não foi informada; as pendências de detalhamento e implementação abaixo continuam separadas dessa aprovação.

## Objetivo

Permitir que a cliente piloto organize os pacientes, atendimentos online, anotações e recebimentos em uma operação com uma profissional, sem secretaria. O piloto deve provar o fluxo completo, do agendamento ao recibo, começando com dados fictícios e avançando para uso real somente após os critérios de entrada do projeto.

As listas abaixo definem o trabalho necessário, não funcionalidades já prontas. Fontes: seção 16 de [Regras de negócio](Regras%20de%20negocio.md), [jornada do paciente](jornada-paciente.md), [rotina da profissional](rotina-profissional.md) e esclarecimento de Gustavo Silveira em 14/11/2026.

## O que entra

| Área | Entrega mínima proposta | Evidência de conclusão |
| --- | --- | --- |
| Operação | Uma profissional e sua clínica, com acesso administrativo restrito. | A profissional consegue realizar sua rotina sem secretaria. |
| Pacientes | Cadastrar, buscar, editar, usar nome social, desativar e reativar; manter vínculos e histórico. | Fluxo de cadastro testado e bloqueio de novas sessões para paciente desativado resolvido. |
| Agenda | Agenda semanal; sessões avulsas e séries; confirmar, cancelar, remarcar e registrar realização ou falta; impedir conflitos. | Fluxos testados, inclusive remarcação e horários em fusos diferentes. |
| Disponibilidade | Duração padrão entre 50 e 60 minutos, dias de trabalho, pausas e bloqueios necessários. | Horários confirmados pela profissional e corretamente aplicados ao agendamento. |
| Atendimento online | Vídeo dentro da plataforma **e** opção de atendimento por ferramenta externa definida. | Ambos os caminhos testados, com acesso restrito e procedimento para falha técnica. |
| Anotações | Registrar no sistema e permitir a rotina de registro fora dele. Não exigir digitação no sistema durante a sessão. | Registro no sistema protegido; procedimento externo de guarda e acesso definido; fonte oficial do registro acordada. |
| Pagamentos | Oferecer **Pix e cartão**, à escolha do paciente. Prever integração com provedor; selecionar o fornecedor e confirmar o fluxo antes de implementar. | Pagamento confirmado, falha e repetição de notificação testados; nenhum dado de cartão armazenado pela plataforma. |
| Financeiro | Controle por sessão, mensalidade e pacote, conforme a entrevista; pagamentos associados às cobranças e recibo após confirmação. | A profissional identifica valores pagos e pendentes, sem duplicidade de lançamentos. |
| Política de faltas | Definir condições, registrar versão e aceite, distinguir falta de falha técnica. | Regras validadas; sem aceite registrado, falta ou cancelamento não gera cobrança. |
| Acesso do paciente | Fluxo mínimo para informações da sessão, acesso ao atendimento, pagamento e recibo, com autenticação e permissões adequadas. | Paciente fictício conclui os caminhos de vídeo e os dois meios de pagamento sem acessar dados de terceiros. |
| Segurança e operação | Controles exigidos nos documentos do projeto para o uso real. | Evidências da seção “Entrada no piloto” abaixo. |

Vídeo e anotações nos dois formatos, mais Pix e cartão, foram definidos pelo responsável pelo projeto. Em 16/11/2026, Gustavo informou que sua mãe aprovou as decisões restantes e que encerrou as issues que não se aplicavam. Este documento registra esse relato, sem presumir contratação de fornecedores ou procedimentos ainda não descritos. Apoiar ferramentas externas não inclui construir uma integração para todo fornecedor nem sincronizar automaticamente anotações externas.

## O que fica fora deste primeiro piloto

Esta lista integra o escopo cuja aprovação foi reportada pelo responsável pelo projeto.

- Venda do SaaS para outras clínicas, planos comerciais e cobrança da assinatura da plataforma.
- Marketplace e entrada de psicólogos, prevista para a fase 2 sob condições próprias.
- Equipe com vários profissionais, secretaria e gestão de salas físicas.
- Atendimento presencial, que continua fora da regra vigente e depende de decisão separada.
- Boleto, conforme a entrevista; Pix e cartão permanecem no piloto.
- Aplicativo móvel nativo, sincronização com Google Agenda/Outlook e relatórios avançados.
- Site público para captação, campanhas e automações de marketing. A preocupação com captação continua no backlog, mas não é requisito desta proposta de piloto assistido.
- Integração automática com todas as ferramentas de vídeo ou de anotações externas.
- Assinatura financeira recorrente automática e automações avançadas de cobrança. Isso não exclui controlar mensalidades e pacotes no piloto.

Gravação de sessões e uso de conteúdo clínico para marketing ou treinamento de IA são proibições do projeto, não funcionalidades adiadas.

## Decisões necessárias antes de liberar o uso real

| Decisão | Situação |
| --- | --- |
| Aprovação da lista completa “entra” e “fica fora” pela cliente piloto | Reportada por Gustavo em 16/11/2026; data original não informada. |
| Segunda a sexta, das 9h às 18h; almoço, pausas e fuso | Informação preliminar; confirmar. |
| Provedor de vídeo integrado e ferramenta externa permitida | Pendentes; validar acesso, privacidade, suporte e contingência de ambos. |
| Fonte oficial das anotações e procedimento quando forem feitas fora do sistema | Pendente; definir acesso, guarda e eventual transferência, sem presumir sincronização. |
| Provedor de Pix/cartão, recebedor, taxas e momento do pagamento | Pendentes. Os dois meios entram no escopo. |
| Valores, condições de mensalidades/pacotes e política de faltas | Pendentes. |
| Emissão e entrega do recibo; orientações fiscais | Pendentes; tarefa #242. |
| Menores e pacientes no exterior | Desejo relatado na entrevista; inclusão no primeiro grupo depende das regras de responsáveis, consentimento, fuso e operação aplicáveis, incluindo #241. Não foram descartados definitivamente. |

## Milestones e sequência de entrega

Manter as milestones temáticas existentes; M13 coordena a execução do piloto e depende das entregas necessárias das demais. Não mover todas as tarefas para M13 nem considerar um épico inteiro concluído só porque o recorte mínimo ficou pronto.

| Milestone | Recorte necessário para este piloto |
| --- | --- |
| M0 · Produto e descoberta | Aprovação reportada; detalhar decisões operacionais que ainda não têm resultado registrado. |
| M1 a M4 · Fundação, pacientes, profissionais e agenda | Completar a operação de uma profissional, disponibilidade e consistência dos agendamentos. |
| M5 · Autenticação e permissões | Login, 2FA da equipe, recuperação de acesso e autorização no servidor por recurso. |
| M6 · Atendimento online | Vídeo integrado e externo, com controles e contingência validados para ambos. |
| M7 · Financeiro | Pix e cartão; cobranças, mensalidades/pacotes, confirmação, conciliação e recibos do recorte mínimo. |
| M8 · Prontuário e auditoria | Anotações internas protegidas e procedimento externo definido, sem sincronização automática obrigatória. |
| M9 · LGPD e documentos legais | Documentos e procedimentos necessários ao público e aos fornecedores efetivamente escolhidos. |
| M10 e M11 · Notificações e portal | Acesso mínimo às informações, sessão, pagamento e recibo; priorizar comunicações necessárias ao fluxo aprovado. |
| M12 · Qualidade e operação | Testes, backup/restauração, monitoramento e deploy; incluir #245 a #248 na avaliação de prontidão da autenticação. |
| M13 · Piloto | Validar os fluxos com dados fictícios, liberar gradualmente o grupo aprovado e registrar feedback. |
| M14 e M15 · SaaS e futuro | Comercialização e expansões excluídas do piloto. |

Descrições atualizadas e conferidas no GitHub em 14/11/2026: [M6](https://github.com/GustavoSilveira1012/Clinica-Acolher/milestone/7), [M7](https://github.com/GustavoSilveira1012/Clinica-Acolher/milestone/8), [M8](https://github.com/GustavoSilveira1012/Clinica-Acolher/milestone/9) e [M13](https://github.com/GustavoSilveira1012/Clinica-Acolher/milestone/14). As tarefas já classificadas por tema permanecem nas suas milestones; lacunas específicas devem ser detalhadas antes de iniciar cada entrega. Não há prazo de lançamento aprovado neste documento.

## Entrada no piloto

Seguir a seção 25 de [projeto.md](projeto.md): resolver bloqueadores P0, comprovar HTTPS, autenticação, 2FA, autorização, isolamento, proteção dos registros, auditoria, backup e restauração, monitoramento, documentos revisados e plano de incidentes. A opção por ferramenta externa não dispensa os controles aplicáveis previstos pelo projeto.

Progressão proposta: **ensaio com dados fictícios → uma pessoa real elegível → três a cinco → uma semana de uso acompanhado**. Manter ambientes e dados de teste separados dos reais.

No ensaio, verificar: cadastro e login; agendamento e conflito; atendimento integrado e externo; anotações internas e procedimento externo; Pix e cartão; recibo; cancelamento, remarcação e falta; recuperação de falha técnica. Registrar problemas sem dados reais de pacientes.

## Aprovação e conclusão da issue #2

| Registro | Valor |
| --- | --- |
| Base do escopo | Versão 1 — 14/11/2026; esta revisão registra o relato de aprovação sem alterar as listas. |
| Decisões de Gustavo Silveira | Vídeo dentro/fora, anotações dentro/fora e Pix/cartão incluídos. |
| Aprovação da cliente piloto | Reportada por Gustavo Silveira em 16/11/2026. |
| Data original e ajustes solicitados pela cliente | Não informados; nenhum ajuste específico foi presumido. |

- [x] Listas e impactos registrados em `docs/`, sem dados reais de pacientes.
- [x] Aprovação da cliente piloto registrada por relato do responsável em 16/11/2026.
- [x] Ajustes das milestones conferidos no GitHub.

A issue #2 permanece encerrada. Concluir a definição de escopo não significa que o MVP esteja implementado ou liberado para uso real. Vídeo externo foi detalhado na [#251](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/251), e o procedimento de anotações externas na [#252](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/252). Ver a [auditoria do backlog](auditoria-backlog-2026-11-16.md).

Link do prototipo do projeto: https://www.figma.com/make/oOH8Aj6ekBhKtlsY2840Ol/Prot%C3%B3tipo-da-Cl%C3%ADnica-Acolher?t=ZRvSjZ44LHLoTh4C-20&fullscreen=1
