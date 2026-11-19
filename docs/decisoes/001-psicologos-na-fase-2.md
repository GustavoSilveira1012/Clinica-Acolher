# Decisão 001 — Psicólogos na fase 2

**Data:** 13/11/2026  
**Status:** decisão de produto aceita; liberação operacional condicionada.  
**Responsável pela decisão:** Gustavo Silveira, responsável pelo projeto.  
**Tarefa:** [#6 — Decidir se psicólogos entram na fase 2](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/6)  
**Regras relacionadas:** seção 1, RN-PRO-02 e seção 12 de [Regras de negócio](../Regras%20de%20negocio.md).

## Decisão

**Incluir psicólogos na fase 2, condicionado ao cumprimento dos requisitos aplicáveis.**

A fase 2 corresponde à oferta da plataforma como SaaS para outras clínicas e profissionais. A decisão não amplia automaticamente o piloto da fase 1, não antecipa o marketplace e não autoriza liberar atendimentos antes da verificação dos requisitos.

O responsável pelo projeto confirmou esta escolha na conversa de 13/11/2026. Não se presume aprovação da cliente piloto nem validação jurídica ou profissional a partir dessa confirmação.

## Contexto e alternativas

O piloto começa com a cliente piloto, sem outros profissionais ou secretaria. Aceitar psicólogos posteriormente amplia o público da plataforma, mas exige adequar cadastro, documentação clínica, permissões e operação à categoria.

Foram consideradas três alternativas:

| Alternativa | Consequência | Resultado |
| --- | --- | --- |
| Adiar a decisão até avaliar o piloto | Preserva a indefinição do público da fase 2. | Não escolhida. |
| Incluir psicólogos com condições para liberação | Permite planejar os requisitos desde agora, mantendo uma etapa de verificação antes de operar. | Escolhida. |
| Manter a fase 2 apenas para psicanalistas | Reduz o público dessa fase e posterga o suporte específico a psicólogos. | Não escolhida. |

## Impactos no produto e na implementação

Os itens abaixo são ações de projeto decorrentes da decisão; não representam uma lista jurídica exaustiva nem requisitos já concluídos.

| Área | Impacto e ação necessária |
| --- | --- |
| Cadastro e aprovação | Identificar categoria e CRP; definir como verificar a situação do registro, quem aprova e como documentar a conferência. Preencher um número não comprova sua validade. |
| Identificação profissional | Exibir categoria e identificação de forma correta; revisar perfis, comunicações e documentos para evitar atribuir títulos não comprovados. |
| Atendimento online | Revisar os fluxos à luz da Resolução CFP nº 9/2024 e das orientações aplicáveis. Não criar exigência de cadastro no e-Psi baseada na norma antiga. |
| Registros clínicos | Definir os tipos de registro suportados, seu conteúdo, responsáveis e procedimentos de acesso, cópia e entrega. Distinguir prontuário de materiais com compartilhamento restrito. |
| Permissões | Revisar a regra atual de que o paciente nunca acessa anotações: o tratamento depende do tipo de registro. Implementar acesso autorizado e entrega controlada, sem abertura indiscriminada de materiais restritos. |
| Guarda e encerramento | Validar os prazos por tipo de documento e o destino dos registros quando o profissional sai. Não programar eliminação automática aos cinco anos com base apenas no rascunho atual. |
| Segurança | Concluir os controles previstos no projeto: autenticação, autorização por recurso, isolamento entre clientes, criptografia dos dados clínicos, auditoria e recuperação de backups. |
| Contratos e operação | Revisar responsabilidades da plataforma, clínica e profissional, documentos de contratação e procedimentos de suporte. Avaliar as exigências aplicáveis ao modelo real de prestação do serviço. |
| Recibos e financeiro | Validar as obrigações fiscais específicas antes de definir emissão ou integrações. A decisão de aceitar a categoria não resolve essas obrigações. |
| Situações específicas | Validar separadamente atendimento a menores e pacientes no exterior antes de disponibilizar esses fluxos. |

## O que existe e o que ainda falta

O código atual já distingue a categoria `Psychologist`, exige o preenchimento de `RegistrationNumber` para essa categoria e possui estados de aprovação do profissional. Isso é uma base técnica, não uma comprovação de regularidade do CRP nem de prontidão para operar com psicólogos.

Esta decisão não implementa verificação externa do registro, prontuário, revisão de contratos ou os controles de segurança pendentes. Também não cria um bloqueio automático por fase no código.

## Referências consultadas e ajustes necessários

Consulta realizada em 13/11/2026. As referências ajudam a identificar impactos; a aplicação ao produto deve ser validada com profissional habilitado e orientação jurídica pertinente antes da liberação.

- **Atendimento online:** o CRP-12 informa que a Resolução CFP nº 9/2024 substituiu as normas anteriores e dispensou o cadastro no e-Psi. A inscrição ativa e as diretrizes éticas e técnicas continuam relevantes. A adequação do atendimento online é avaliada pela profissional. [Orientação oficial sobre atendimento online](https://transparencia.cfp.org.br/crp12/pergunta-frequente/atendimentopsicologicoonline/).
- **Registros e acesso:** a orientação do CRP-12 distingue prontuário e registro documental exclusivo e descreve o acesso do usuário ao prontuário. Por isso, a regra genérica de negar todo acesso do paciente precisa ser revista para esta categoria. [Orientação oficial sobre registro documental](https://transparencia.cfp.org.br/crp12/pergunta-frequente/registro-documental/).
- **Retenção:** a mesma orientação aborda o mínimo de cinco anos das normas profissionais e o prazo de vinte anos previsto na Lei nº 13.787/2018 para os prontuários nela tratados. É necessário validar o enquadramento dos registros do produto antes de definir descarte. [Orientação oficial sobre guarda dos registros](https://transparencia.cfp.org.br/crp12/pergunta-frequente/registro-documental/).

## Condições para liberar psicólogos na fase 2

- [ ] Requisitos aplicáveis ao modelo de operação revisados e registrados.
- [ ] Processo de conferência do CRP e aprovação definido e validado.
- [ ] Regras de prontuário, acesso, guarda e encerramento revisadas no documento de negócio.
- [ ] Funcionalidades necessárias e controles de segurança implementados e testados.
- [ ] Contratos, orientações e procedimentos operacionais pertinentes revisados.
- [ ] Evidências de validação registradas e liberação autorizada pelo responsável pelo produto.

Se essas condições não estiverem atendidas no lançamento da fase 2, a entrada de psicólogos será adiada até a conclusão delas. A decisão de incluí-los permanece; a data de liberação não fica garantida.

## Conclusão da tarefa de decisão

- [x] Decisão registrada com impactos.
- [x] Resultado documentado em `docs/decisoes/`.
- [x] Nenhum dado real de paciente registrado.

A tarefa de decidir o público é distinta das tarefas de implementar e validar os requisitos. Sua conclusão não declara a plataforma apta a atender psicólogos agora.
