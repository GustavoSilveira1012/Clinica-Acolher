# Decisão 003 — Anotações clínicas no piloto

**Data:** 14/11/2026.  
**Status:** inclusão definida por Gustavo Silveira; liberação para dados reais condicionada aos controles abaixo.  
**Issue:** [#5 — Decidir se o piloto guarda anotações clínicas](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/5).  
**Regras relacionadas:** RN-PRT-01 a RN-PRT-09 e RN-GER-04.

## Contexto e decisão

O piloto incluirá anotações clínicas dentro da plataforma e permitirá que a profissional mantenha anotações fora dela. A decisão detalha a [decisão 002](002-formatos-do-piloto.md), conforme a resposta de Gustavo sobre oferecer os dois formatos.

As anotações internas entram no escopo de M8. Seu uso com dados reais só será liberado depois da implementação e validação da criptografia, do controle de acesso e da auditoria. A possibilidade de usar uma rotina externa não retira a entrega interna do [escopo do piloto](../escopo-piloto.md).

## O que entra

- Registro associado ao vínculo entre paciente e profissional, conforme RN-PRT-01. Pertencer à clínica não concede acesso ao conteúdo.
- Rascunhos com histórico de salvamentos, finalização e correções por adendo, preservando o registro original.
- Acesso ao conteúdo pelo profissional responsável, com autorização verificada também na API.
- Auditoria das operações de leitura, criação, finalização, adendo e exportação, sem copiar o conteúdo clínico para os logs.
- Definição, com a profissional, de qual registro será a fonte oficial quando houver anotações internas e externas e de como complementar esse registro após a sessão.

## Condições para liberar dados reais

- Texto cifrado antes da persistência, com chaves fora do banco e acesso restrito às chaves.
- Testes que comprovem o bloqueio de acesso de outros profissionais e de usuários sem autorização, inclusive ao histórico e às exportações.
- Auditoria funcionando e conteúdo clínico ausente de logs técnicos, notificações, e-mails e painéis administrativos.
- Backup e restauração validados, incluindo a recuperação controlada das chaves necessárias para ler os registros.
- Procedimentos de guarda, acesso, exportação e retenção definidos e validados para a categoria profissional do piloto. Este documento não estabelece um prazo legal de guarda.

Até essas condições serem comprovadas, desenvolvimento, testes e demonstrações usam somente dados fictícios. A conclusão desta decisão não atesta que os controles já estejam implementados.

## Limites e impactos

Não entram sincronização automática com ferramentas externas, importação irrestrita de anotações ou acesso geral da administração ao conteúdo. Os registros não serão usados para marketing, venda de dados ou treinamento de IA, conforme RN-GER-04.

A rotina externa precisa ter local de guarda, responsáveis, acesso e procedimento de transferência definidos com a profissional; não se presume que ela receba automaticamente as proteções do sistema.

M8 permanece responsável pela implementação das anotações e depende dos controles de segurança e operação necessários antes da liberação do piloto. Se esses controles atrasarem, a liberação das anotações reais também será adiada; retirar a funcionalidade do piloto exigirá rever o escopo.

## Registro e pendências

A decisão de incluir anotações está registrada. Em 16/11/2026, Gustavo reportou a aprovação do escopo pela cliente piloto (issue #2), sem informar a data original da conversa. A definição dos procedimentos externos segue na issue #252, e a comprovação das condições de liberação permanece pendente. Nenhum dado real de paciente foi utilizado neste documento.
