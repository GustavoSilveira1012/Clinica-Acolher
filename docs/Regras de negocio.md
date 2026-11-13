# Clínica Acolher — Regras de negócio e conformidade legal

Nov 7, 2026 · @Gustavo Silveira

Este documento é um **rascunho de regras de negócio** para a plataforma Clínica Acolher. Ele precisa de duas validações antes de virar regra definitiva: com a cliente piloto (perguntas na última seção) e com um advogado especializado em proteção de dados e saúde.

As referências legais foram escritas de memória, sem consulta à internet. Confira o texto vigente de cada lei, resolução e norma antes de implementar. Cada regra tem um código (ex.: `RN-AGD-03`) para ser citado nas tarefas do GitHub e nos testes.

## 1. Visão do produto

A Clínica Acolher é uma **clínica online**: o paciente agenda, paga e faz a sessão por vídeo dentro da plataforma, e o profissional gerencia agenda, financeiro e anotações no mesmo lugar. Não há atendimento presencial: toda sessão acontece por vídeo.

O produto evolui em três fases, e cada uma muda as responsabilidades legais:

| Fase | Modelo | Quem é o controlador dos dados do paciente (LGPD) |
| --- | --- | --- |
| 1. Piloto | Clínica da cliente piloto usando a plataforma | A clínica; a plataforma é operadora |
| 2. SaaS | Outras clínicas e profissionais assinam a plataforma | Cada clínica ou profissional; a plataforma é operadora |
| 3. Marketplace (futuro) | Pacientes encontram profissionais na plataforma | Plataforma e profissional passam a ser controladores, cada um no seu tratamento |

**Regras gerais**

- `RN-GER-01` A plataforma não presta atendimento clínico. Quem atende é sempre o profissional, que responde pela conduta clínica.
- `RN-GER-02` A plataforma não exibe promessa de cura, diagnóstico ou resultado terapêutico em nenhum texto.
- `RN-GER-03` A plataforma não é serviço de emergência. Toda tela do paciente informa onde buscar ajuda em crise (CVV 188, SAMU 192).
- `RN-GER-04` Nenhum dado clínico é usado para marketing, treinamento de IA ou venda, em nenhuma fase.

**Decisões em aberto**

- [ ] Nome e marca: "Acolher" é muito comum entre clínicas. Fazer busca no INPI (classes 9, 42 e 44) antes de investir na marca.
- [ ] Provedor de vídeo: próprio (WebRTC) ou terceiro. Terceiro com servidor fora do Brasil implica transferência internacional de dados.
- [ ] Se a plataforma vai guardar anotações clínicas de psicanalistas ou só agenda e financeiro na fase 1.
- [ ] Se psicólogos serão aceitos desde a fase 2 (traz exigências do CFP, ver seção 12).

## 2. Perfis e permissões

O acesso a dados clínicos depende do **vínculo terapêutico**, nunca só do cargo. Um administrador da clínica gerencia a clínica, mas não lê anotações de sessão.

| Perfil | Pode | Não pode |
| --- | --- | --- |
| Paciente | Ver e editar o próprio cadastro, agendar, pagar, entrar na sessão, baixar recibos, exercer direitos LGPD | Ver anotações clínicas do profissional |
| Responsável legal | Tudo o que o paciente menor pode, em nome dele | Ver anotações clínicas |
| Profissional | Agenda própria, pacientes com vínculo ativo, anotações dos próprios pacientes, financeiro próprio | Ver pacientes ou anotações de outros profissionais |
| Secretaria | Agenda, cadastro administrativo, cobranças da clínica | Qualquer dado clínico |
| Administrador da clínica | Equipe, configurações, financeiro da clínica, relatórios agregados | Dados clínicos |
| Suporte da plataforma | Dados técnicos e de conta, com acesso temporário aprovado pelo cliente | Dados clínicos, salvo procedimento de emergência auditado |

- `RN-PER-01` Toda consulta a dado de paciente verifica, no servidor, se quem pede tem permissão para **aquele** paciente (não basta estar logado).
- `RN-PER-02` Uma pessoa pode ter mais de um perfil (ex.: profissional e administrador). As permissões não se somam para dados clínicos: vale sempre o vínculo terapêutico.
- `RN-PER-03` Suporte nunca pede senha. O acesso é concedido pelo cliente, com escopo, motivo, prazo de expiração e registro em auditoria.
- `RN-PER-04` Encerrado o vínculo terapêutico, o profissional mantém acesso de leitura às anotações que produziu, pelo prazo de guarda, mas não cria novas.

## 3. Profissionais

Todo profissional passa por verificação antes de atender, e a plataforma exibe a formação de forma honesta, sem títulos que a pessoa não tem.

- `RN-PRO-01` Cadastro obrigatório: nome completo, nome profissional, e-mail, telefone, formação declarada e aceite dos termos de uso.
- `RN-PRO-02` O profissional informa sua categoria: **psicanalista**, **psicólogo** ou outra. Psicólogo informa número de registro no CRP; a plataforma confere o registro antes de liberar atendimentos (ver seção 12 sobre exigências do CFP).
- `RN-PRO-03` Psicanalista não é profissão regulamentada por lei no Brasil. A plataforma não usa as palavras "psicólogo", "terapeuta registrado" ou "CRP" para quem não comprovou esse registro.
- `RN-PRO-04` O perfil só fica visível para pacientes após aprovação da clínica (fase 1 e 2) ou da plataforma (fase 3).
- `RN-PRO-05` Vínculo com clínica tem início e fim registrados. Quem sai e volta ganha um novo período, preservando o histórico.
- `RN-PRO-06` Desativar profissional nunca apaga histórico de sessões, financeiro ou anotações. Sessões futuras devem ser remanejadas ou canceladas antes da desativação, com aviso aos pacientes.
- `RN-PRO-07` Autenticação em dois fatores é obrigatória para profissionais, secretaria e administradores.

**Vínculo terapêutico**

O vínculo liga um paciente a um profissional e é a base de toda permissão clínica (`RN-PER-01`).

- `RN-VIN-01` Um vínculo só começa com paciente ativo e profissional aprovado. Paciente menor de idade só inicia vínculo com responsável legal cadastrado (`RN-PAC-03`).
- `RN-VIN-02` Um paciente não pode ter dois vínculos não encerrados com o mesmo profissional ao mesmo tempo.
- `RN-VIN-03` Estados: ativo, suspenso e encerrado. Ativo pode ser suspenso ou encerrado; suspenso pode voltar a ativo ou ser encerrado; encerrado é final. Se o paciente voltar, nasce um novo vínculo.
- `RN-VIN-04` Encerrar exige motivo administrativo e registra a data. Só um vínculo ativo permite agendar novas sessões.

## 4. Pacientes

Coletamos o mínimo de dados necessário para cada finalidade, e pedimos mais só quando a finalidade surge (minimização, LGPD art. 6º, III).

| Dado | Quando pedir | Para quê |
| --- | --- | --- |
| Nome, e-mail, telefone | Cadastro | Identificação, agendamento, avisos |
| Data de nascimento | Cadastro | Saber se é menor de idade |
| Contato de emergência | Antes da primeira sessão online | Protocolo de crise (seção 6) |
| Cidade e endereço da sessão | Antes de cada sessão online | Acionar ajuda local em emergência |
| CPF e endereço completo | Só ao emitir recibo ou nota fiscal | Obrigação fiscal |

- `RN-PAC-01` O paciente pode se cadastrar sozinho ou ser cadastrado pela clínica. No segundo caso, recebe convite para criar a própria senha e aceitar os termos.
- `RN-PAC-02` Paciente nunca é apagado enquanto houver obrigação de guarda. O pedido de exclusão segue a seção 11.
- `RN-PAC-03` **Menores de 18 anos** só são atendidos com um responsável legal cadastrado e consentimento registrado do responsável (LGPD art. 14).
- `RN-PAC-04` Para menores de 12 anos (criança, pelo ECA), o responsável faz o agendamento e os pagamentos. Adolescentes de 12 a 17 podem entrar na sessão com o próprio acesso, vinculado ao responsável.
- `RN-PAC-05` A clínica decide, caso a caso, o que o responsável pode ver de um adolescente (agenda e financeiro, sim; anotações, nunca).
- `RN-PAC-06` Ao completar 18 anos, o paciente passa a controlar a própria conta e o responsável perde o acesso automaticamente, com aviso a ambos.
- `RN-PAC-07` Nome social é respeitado em todas as telas, recibos e comunicações; o nome civil aparece apenas onde a lei exigir.

## 5. Agenda e sessões

A agenda gira em torno do **horário fixo semanal**, que é o padrão da psicanálise, com sessões avulsas como exceção.

**Disponibilidade**

- `RN-AGD-01` O profissional define dias, horários, duração padrão da sessão (ex.: 50 min) e intervalo entre sessões.
- `RN-AGD-02` Bloqueios pontuais (férias, feriados, congressos) suspendem a disponibilidade sem apagar as séries.
- `RN-AGD-03` Dois atendimentos ativos do mesmo profissional nunca se sobrepõem. A regra é garantida pelo banco de dados, não só pela tela.
- `RN-AGD-04` Um paciente também não pode ter duas sessões no mesmo horário, mesmo com profissionais diferentes. A regra é garantida pelo banco de dados.

**Séries recorrentes**

- `RN-AGD-05` Uma série semanal gera as sessões futuras até uma data final ou por tempo indeterminado (gerando sempre as próximas 12 semanas).
- `RN-AGD-06` Alterar uma série pede o alcance: só esta sessão, esta e as próximas, ou a série inteira. Sessões passadas nunca são alteradas.
- `RN-AGD-07` Quando um feriado cai num horário fixo, a sessão é marcada para revisão em vez de ser cancelada automaticamente.

**Estados da sessão**

| Estado | Pode ir para | Observação |
| --- | --- | --- |
| Agendada | Confirmada, Realizada, Falta, Cancelada, Remarcada | Estado inicial |
| Confirmada | Realizada, Falta, Cancelada, Remarcada | Paciente confirmou presença |
| Realizada | Nenhum (final) | Libera cobrança e anotação |
| Falta | Nenhum (final) | Aplica a política de faltas (seção 7) |
| Cancelada | Nenhum (final) | Registra quem cancelou e quando |
| Remarcada | Nenhum (final) | Aponta para a nova sessão criada |

- `RN-AGD-08` Estados finais não voltam. Um erro de registro é corrigido por um novo evento com justificativa, preservando o original na auditoria.
- `RN-AGD-09` Todos os horários são gravados em UTC e exibidos no fuso de quem está vendo. Paciente em outro fuso vê o horário convertido, com o fuso indicado.
- `RN-AGD-10` Toda sessão é online, por vídeo dentro da plataforma. Não existe sessão presencial, e a interface não oferece essa opção.
- `RN-AGD-11` Uma sessão só pode ser registrada como realizada ou como falta a partir do horário de início dela.

## 6. Atendimento online

A sala de vídeo é o coração da clínica online: ela precisa ser tão sigilosa quanto um consultório com a porta fechada.

**Sala de vídeo**

- `RN-ONL-01` Cada sessão tem uma sala exclusiva, criada para aquele atendimento. O link não é reutilizável nem adivinhável.
- `RN-ONL-02` Só entram o profissional e os participantes daquela sessão, autenticados. Ninguém entra apenas com o link.
- `RN-ONL-03` A sala abre 10 minutos antes do horário e fecha 30 minutos depois do fim previsto. O paciente aguarda numa sala de espera até o profissional liberar a entrada.
- `RN-ONL-04` **Sessões nunca são gravadas** pela plataforma, nem em áudio, nem em vídeo, nem em transcrição. A interface não oferece essa opção.
- `RN-ONL-05` O vídeo é transmitido com criptografia. O provedor de vídeo é contratado como operador, com cláusulas de sigilo e de proteção de dados (ver seção 11 sobre servidores fora do Brasil).
- `RN-ONL-06` A plataforma registra apenas metadados técnicos: quem entrou, quando, por quanto tempo e a qualidade da conexão. Nunca o conteúdo.
- `RN-ONL-07` Se a conexão cair, ambos podem voltar à mesma sala enquanto ela estiver aberta. O profissional pode encerrar e oferecer reposição.

**Segurança do paciente e emergência**

- `RN-ONL-08` Antes da primeira sessão online, o paciente informa um contato de emergência e aceita as orientações de uso (local reservado, fone de ouvido, conexão estável).
- `RN-ONL-09` No início de cada sessão online, o paciente confirma a cidade onde está. Em situação de risco, o profissional sabe qual serviço local acionar.
- `RN-ONL-10` O profissional tem, dentro da sala, um acesso rápido aos dados de emergência daquele paciente, registrado em auditoria.
- `RN-ONL-11` A tela da sessão mostra ao paciente, de forma discreta, os contatos do CVV (188) e do SAMU (192).
- `RN-ONL-12` Sessão com paciente fora do Brasil exige confirmação do profissional, pois pode envolver regras de outro país.

## 7. Faltas, cancelamentos e reagendamentos

Cada profissional define sua política, mas ela precisa estar escrita e aceita pelo paciente **antes** da primeira sessão. Cobrança surpresa gera conflito e problema com o Código de Defesa do Consumidor.

| Situação | Padrão sugerido (configurável) |
| --- | --- |
| Cancelamento com 24 h ou mais de antecedência | Sem cobrança |
| Cancelamento com menos de 24 h | Cobrança integral ou parcial, conforme política |
| Falta sem aviso | Cobrança integral, conforme política |
| Cancelamento pelo profissional | Nunca cobrado; oferecer reposição |
| Falha técnica da plataforma | Nunca cobrado; oferecer reposição |

- `RN-CAN-01` A política de faltas tem versão. O paciente aceita uma versão específica, e a cobrança sempre usa a versão aceita na data da sessão.
- `RN-CAN-02` Mudança na política vale só para sessões futuras e exige novo aceite do paciente.
- `RN-CAN-03` Remarcar dentro do prazo da política cria uma nova sessão e encerra a anterior como "Remarcada", sem cobrança.
- `RN-CAN-04` O profissional pode isentar uma cobrança de falta, registrando o motivo.
- `RN-CAN-05` Uma falha técnica comprovada pelos registros de conexão (`RN-ONL-06`) nunca vira falta do paciente.
- `RN-CAN-06` Todo cancelamento registra quem cancelou, quando, com quanta antecedência e o motivo informado.
- `RN-CAN-07` Sem aceite registrado de uma política, nenhuma falta ou cancelamento é cobrado do paciente.

## 8. Financeiro

Cobrança, pagamento e documento fiscal são três coisas separadas: o paciente pode dever sem ter pago, pagar uma vez por várias sessões e receber o recibo depois.

**Cobranças e pagamentos**

- `RN-FIN-01` Cada sessão realizada (ou falta cobrável) gera uma **cobrança** com valor, vencimento e recebedor (clínica ou profissional).
- `RN-FIN-02` Modelos de cobrança suportados: por sessão, mensal (soma das sessões do mês) e pacote pré-pago (seção 9).
- `RN-FIN-03` Formas de pagamento: Pix, cartão de crédito e boleto, por meio de um provedor de pagamentos. **A plataforma nunca armazena número de cartão.**
- `RN-FIN-04` Um pagamento pode quitar várias cobranças, e uma cobrança pode ser paga em partes.
- `RN-FIN-05` Valores são guardados em centavos ou tipo decimal, nunca em número de ponto flutuante.
- `RN-FIN-06` Nada é apagado no financeiro. Estorno, desconto e correção são novos lançamentos que referenciam o original.
- `RN-FIN-07` A confirmação de pagamento vem do provedor (webhook assinado) e é processada uma única vez, mesmo se o aviso chegar repetido.
- `RN-FIN-08` Cobrança vencida é um estado calculado (data de hoje passou do vencimento), não um campo salvo.
- `RN-FIN-09` Lembretes de cobrança são discretos e nunca mencionam o motivo do atendimento.

**Recibos e notas fiscais**

- `RN-FIN-10` Recibo é emitido após o pagamento, com nome e documento do profissional, categoria profissional real, nome do paciente, data das sessões e valor.
- `RN-FIN-11` O recibo **não** afirma que o valor é dedutível no Imposto de Renda. Pelas regras da Receita Federal que conhecemos, psicanálise só é dedutível quando realizada por psicólogo ou médico; confirmar a regra vigente.
- `RN-FIN-12` Verificar se profissionais pessoa física da plataforma estão obrigados a emitir recibos pelo aplicativo Receita Saúde, e integrar ou orientar conforme o caso.
- `RN-FIN-13` Nota fiscal de serviço (NFS-e), quando o recebedor é pessoa jurídica, segue o padrão nacional ou o municipal. Nota autorizada nunca é editada: é cancelada e substituída.
- `RN-FIN-14` Acompanhar a reforma tributária sobre consumo (IBS/CBS, LC 214/2025), que altera a emissão de notas a partir de 2026.
- `RN-FIN-15` Documentos fiscais e financeiros são guardados por no mínimo 5 anos (prazo tributário).

**Mensalidade da plataforma**

- `RN-FIN-16` O que a clínica paga à Clínica Acolher (assinatura do SaaS) é contabilizado separadamente do que os pacientes pagam à clínica. Os dois nunca se misturam.

## 9. Pacotes, assinaturas e direito de arrependimento

Pacotes vendidos pela internet dão ao paciente **7 dias para desistir** com reembolso, pelo Código de Defesa do Consumidor (art. 49), e a plataforma precisa calcular isso sozinha.

- `RN-PCT-01` Pacote = quantidade de sessões, valor total e validade (ex.: 4 sessões em 45 dias). Cada versão publicada de um pacote é imutável; mudanças criam nova versão.
- `RN-PCT-02` O saldo de sessões é calculado a partir de lançamentos (compra, consumo, estorno de consumo, expiração), nunca de um campo editável.
- `RN-PCT-03` Com mais de um pacote ativo, consome primeiro o que vence antes.
- `RN-PCT-04` **Arrependimento:** compra feita pela plataforma pode ser cancelada em até 7 dias corridos. Se nenhuma sessão foi usada, o reembolso é integral. Se houve sessão realizada nesse prazo, reembolsa-se o valor proporcional às sessões não usadas (confirmar entendimento com advogado).
- `RN-PCT-05` Após os 7 dias, o cancelamento segue a política do profissional, que precisa estar escrita no momento da compra.
- `RN-PCT-06` Reembolso pode ser em dinheiro (estorno) ou crédito para uso futuro, à escolha do paciente.
- `RN-PCT-07` Sessões vencidas por expiração da validade não são reembolsadas, desde que a validade tenha sido informada com destaque na compra.
- `RN-PCT-08` Assinatura mensal gera uma cobrança por ciclo. Ciclo que começa no dia 31 vence no último dia dos meses mais curtos, sem deslocar os meses seguintes.
- `RN-PCT-09` Antes de qualquer compra, o paciente vê: preço total, número de sessões, validade, política de faltas e de cancelamento e o contato do vendedor (Decreto 7.962/2013, comércio eletrônico).

## 10. Prontuário e anotações clínicas

As anotações são o dado mais sensível da plataforma: ficam cifradas, só o profissional responsável lê, e nada é sobrescrito.

- `RN-PRT-01` Anotação pertence ao vínculo **paciente ↔ profissional**, não à clínica. Trocar de clínica não transfere anotações para a clínica nova.
- `RN-PRT-02` O texto das anotações é cifrado antes de ir para o banco de dados, com chave guardada fora do banco. Nem quem administra o banco consegue lê-lo.
- `RN-PRT-03` A anotação começa como **rascunho** (editável) e passa a **finalizada** (imutável). Correção depois de finalizada é feita por adendo, que aparece junto do original.
- `RN-PRT-04` Cada salvamento explícito do rascunho gera uma versão. Versões antigas nunca são apagadas.
- `RN-PRT-05` Toda leitura, criação, finalização, adendo e exportação de anotação é registrada na auditoria (quem, quando, de onde).
- `RN-PRT-06` Exportar anotações é operação de alto risco: um paciente por vez, intervalo de datas, link temporário de poucos minutos, limite de frequência e auditoria.
- `RN-PRT-07` Se o paciente pedir acesso às próprias anotações, o pedido segue a seção 11 e passa pela análise do profissional (há debate jurídico e ético sobre notas pessoais do analista; definir com advogado).
- `RN-PRT-08` **Psicólogos:** o CFP exige registro documental dos atendimentos e guarda por no mínimo 5 anos (Resolução CFP nº 1/2009, conferir texto vigente). **Psicanalistas** não têm norma legal específica; a plataforma aplica o mesmo padrão de guarda por segurança.
- `RN-PRT-09` Anotações nunca aparecem em notificações, e-mails, relatórios administrativos, logs técnicos ou painéis da clínica.

## 11. LGPD

Dados de saúde mental são **dados pessoais sensíveis** (LGPD, art. 5º, II), e toda a plataforma é desenhada a partir dessa premissa.

**Bases legais por tratamento**

| Tratamento | Base legal provável | Observação |
| --- | --- | --- |
| Cadastro, agenda, sala de vídeo | Execução de contrato (art. 7º, V) e, para dado sensível, art. 11 | Ver nota abaixo |
| Anotações clínicas | Tutela da saúde (art. 11, II, f) para profissionais de saúde; consentimento específico e destacado (art. 11, I) para psicanalistas | **Validar com advogado** |
| Recibos, notas, financeiro | Cumprimento de obrigação legal (art. 7º, II) | Guarda mínima de 5 anos |
| Registros de acesso | Obrigação legal (Marco Civil, art. 15) | Guarda de 6 meses |
| Segurança e prevenção a fraude | Legítimo interesse (art. 7º, IX) e art. 11, II, g | Sem uso de conteúdo clínico |
| Dados de menores | Consentimento de ao menos um responsável (art. 14) |  |

Nota: a hipótese de "tutela da saúde" do art. 11 fala em procedimento realizado por **profissionais de saúde**. Como psicanálise não é profissão regulamentada, é prudente usar consentimento específico e destacado para psicanalistas, com opção de revogação clara.

**Papéis**

- `RN-LGPD-01` Nas fases 1 e 2, a clínica (ou o profissional autônomo) é **controladora** dos dados dos pacientes, e a Clínica Acolher é **operadora**. O contrato SaaS precisa dizer isso, com as obrigações de cada um (art. 39).
- `RN-LGPD-02` A Clínica Acolher é controladora dos dados das próprias contas de clientes (cadastro, cobrança da assinatura).
- `RN-LGPD-03` Indicar encarregado (DPO) e publicar o canal de contato. Agentes de pequeno porte podem ter regras simplificadas (Resolução CD/ANPD nº 2/2022), mas um canal de atendimento é sempre necessário.

**Direitos do titular (art. 18)**

- `RN-LGPD-04` O paciente pode, pela própria conta: confirmar e acessar seus dados, corrigir cadastro, baixar seus dados em formato estruturado e revogar consentimentos.
- `RN-LGPD-05` Pedidos de eliminação, anonimização ou bloqueio são registrados e respondidos pelo controlador. Dados com obrigação legal de guarda são bloqueados, não apagados, até o fim do prazo, e o paciente é informado.
- `RN-LGPD-06` Todo pedido tem protocolo, data de abertura e de resposta. Prazo interno de resposta: 15 dias.

**Retenção**

| Dado | Prazo | Depois do prazo |
| --- | --- | --- |
| Anotações clínicas | Mínimo 5 anos após o último atendimento (revisão manual) | Eliminação segura, salvo bloqueio judicial |
| Financeiro e fiscal | 5 anos | Eliminação ou anonimização |
| Registros de acesso | 6 meses | Eliminação |
| Metadados de sessão de vídeo | 6 meses | Eliminação |
| Cadastro de conta encerrada sem atendimentos | 30 dias | Eliminação |

- `RN-LGPD-07` Existe **bloqueio legal** (litígio, investigação, ordem judicial) que suspende qualquer eliminação automática.

**Transferência internacional e operadores**

- `RN-LGPD-08` Todo fornecedor que toca dados (nuvem, vídeo, e-mail, WhatsApp, pagamentos) é listado, com finalidade, país dos servidores e contrato de proteção de dados.
- `RN-LGPD-09` Preferir servidores no Brasil. Se houver servidor no exterior, usar mecanismo válido de transferência (art. 33; cláusulas-padrão da Resolução CD/ANPD nº 19/2024).

**Incidentes**

- `RN-LGPD-10` Incidente que possa causar risco ou dano relevante é comunicado à ANPD e aos titulares afetados. A Resolução CD/ANPD nº 15/2024 fixa prazo de 3 dias úteis; conferir o texto vigente.
- `RN-LGPD-11` Manter registro de todos os incidentes, inclusive os não comunicados, com a justificativa da avaliação.

**Documentação obrigatória**

- `RN-LGPD-12` Manter o registro das operações de tratamento (art. 37) e um Relatório de Impacto à Proteção de Dados (art. 38), porque há tratamento de dados sensíveis em larga escala.

## 12. Outras leis e normas aplicáveis

Além da LGPD, pelo menos oito normas afetam regras da plataforma. Todas precisam de conferência do texto vigente.

| Norma | O que exige da plataforma | Regras ligadas |
| --- | --- | --- |
| Código de Defesa do Consumidor (Lei 8.078/1990) | Informação clara de preço e condições; arrependimento em 7 dias para compra online (art. 49) | `RN-CAN-01`, `RN-PCT-04`, `RN-PCT-09` |
| Decreto 7.962/2013 (comércio eletrônico) | Identificação do fornecedor, resumo do contrato antes da compra, atendimento eletrônico | `RN-PCT-09` |
| Marco Civil da Internet (Lei 12.965/2014) | Guarda de registros de acesso a aplicações por 6 meses (art. 15); sigilo de comunicações | `RN-LGPD` retenção |
| Código Penal, art. 154 | Violação de segredo profissional é crime; reforça sigilo das sessões | `RN-ONL-04`, `RN-PRT-09` |
| Estatuto da Criança e do Adolescente (Lei 8.069/1990) | Proteção integral; papel dos responsáveis | `RN-PAC-03` a `RN-PAC-06` |
| Lei Brasileira de Inclusão (Lei 13.146/2015, art. 63) | Site e aplicativo acessíveis a pessoas com deficiência | Seção 13 |
| Lei da Telessaúde (Lei 14.510/2022) | Regras para atendimento remoto por profissionais de saúde regulamentados | Seção 6 |
| CFP, Resolução nº 11/2018 (se houver psicólogos) | Atendimento psicológico online; conferir exigência de cadastro do profissional na plataforma do CFP | `RN-PRO-02` |
| CFP, Resolução nº 1/2009 (se houver psicólogos) | Registro documental e guarda mínima de 5 anos | `RN-PRT-08` |
| Legislação tributária e reforma (LC 214/2025) | Emissão e guarda de documentos fiscais | `RN-FIN-13` a `RN-FIN-15` |

- `RN-LEG-01` Psicanálise não tem conselho profissional nem lei de regulamentação. A plataforma segue, por boa prática, os padrões de sigilo e guarda exigidos dos psicólogos.
- `RN-LEG-02` Termos de uso e política de privacidade são revisados por advogado antes do piloto com pacientes reais e a cada mudança relevante.

## 13. Segurança obrigatória

Estes itens são condição para atender o primeiro paciente real, não melhorias para depois.

- [ ] `RN-SEG-01` HTTPS em tudo, sem exceção.
- [ ] `RN-SEG-02` Senhas com hash forte (ex.: Argon2 ou PBKDF2 com muitas iterações), mínimo de 10 caracteres e bloqueio após tentativas erradas.
- [ ] `RN-SEG-03` Dois fatores obrigatórios para equipe; opcional e recomendado para pacientes.
- [ ] `RN-SEG-04` Sessão de login expira por inatividade (equipe: 30 min) e pode ser encerrada em todos os dispositivos.
- [ ] `RN-SEG-05` Autorização checada no servidor em cada requisição, por recurso (impede um usuário de ver dados alheios trocando um número na URL).
- [ ] `RN-SEG-06` Dados de uma clínica nunca aparecem para outra. Isolamento testado automaticamente a cada mudança.
- [ ] `RN-SEG-07` Anotações clínicas e contatos de emergência cifrados no banco.
- [ ] `RN-SEG-08` Trilha de auditoria à prova de adulteração para acessos a dados clínicos.
- [ ] `RN-SEG-09` Logs técnicos nunca contêm senha, token, conteúdo clínico ou dados de cartão.
- [ ] `RN-SEG-10` Segredos (senhas de banco, chaves de API) fora do código e do repositório.
- [ ] `RN-SEG-11` Backup diário cifrado, cópia imutável e **teste de restauração** feito e documentado antes do piloto.
- [ ] `RN-SEG-12` Dependências verificadas automaticamente contra vulnerabilidades conhecidas.
- [ ] `RN-SEG-13` Acessibilidade: navegação por teclado, contraste adequado, textos alternativos e compatibilidade com leitores de tela (WCAG 2.1 nível AA como meta).
- [ ] `RN-SEG-14` Teste de invasão (pentest) independente antes de vender para outras clínicas.

## 14. Comunicações e notificações

Toda mensagem parte do princípio de que outra pessoa pode estar olhando o celular do paciente.

- `RN-NOT-01` Canais: e-mail, WhatsApp e SMS. O paciente escolhe quais aceita e pode desativar a qualquer momento.
- `RN-NOT-02` Mensagens nunca citam motivo do atendimento, diagnóstico, "psicanálise" ou "terapia" no texto visível da notificação. Exemplo: "Lembrete: seu horário de amanhã às 15h. Abra o app para detalhes."
- `RN-NOT-03` Lembretes padrão: 24 h e 1 h antes da sessão, com opção de confirmar presença.
- `RN-NOT-04` Horário de silêncio: nenhuma mensagem automática entre 21h e 8h no fuso do paciente, exceto códigos de login.
- `RN-NOT-05` Mensagens de marketing só com consentimento separado, nunca misturado com o aceite dos termos.
- `RN-NOT-06` Telefone e e-mail de destino guardados cifrados; envios registrados sem o conteúdo completo.
- `RN-NOT-07` Limite de envios por clínica por hora, para evitar disparo em massa por engano ou abuso.
- `RN-NOT-08` WhatsApp segue as regras do provedor oficial (modelos de mensagem aprovados).

## 15. Documentos legais antes do lançamento

Oito documentos precisam existir, revisados por advogado, antes do primeiro paciente real.

| Documento | Para quem | Quando é necessário |
| --- | --- | --- |
| Termos de uso do paciente | Paciente | Piloto |
| Política de privacidade | Todos | Piloto |
| Termo de consentimento para dados sensíveis e atendimento online | Paciente (e responsável, se menor) | Piloto |
| Política de faltas e cancelamento (modelo editável pelo profissional) | Paciente | Piloto |
| Contrato SaaS com acordo de tratamento de dados (controlador × operador) | Clínica e profissional | Piloto (versão simplificada) e venda |
| Registro das operações de tratamento e RIPD | Interno | Piloto |
| Plano de resposta a incidentes | Interno | Piloto |
| Política de cookies | Visitantes do site | Lançamento comercial |

- `RN-DOC-01` Cada documento tem versão e data. A plataforma guarda qual versão cada pessoa aceitou e quando.
- `RN-DOC-02` Mudança relevante exige novo aceite antes do próximo uso, com resumo do que mudou.

## 16. Perguntas para validar com a cliente piloto

As respostas abaixo podem mudar regras marcadas como "padrão sugerido". Anote as respostas como ela falar e atualize a regra correspondente.

- [ ] Quanto dura a sessão e qual o intervalo entre uma e outra? (`RN-AGD-01`)
- [ ] Atende mais de uma vez por semana o mesmo paciente? (`RN-AGD-05`)
- [ ] Qual será a política de faltas e de cancelamento? Cobra falta sem aviso? (`RN-CAN`)
- [ ] Cobra por sessão, por mês ou vende pacotes? Aceita Pix, cartão, boleto? (`RN-FIN-02`, `RN-FIN-03`)
- [ ] Vai atender menores de idade? A partir de que idade? (`RN-PAC-03`)
- [ ] Pretende escrever anotações das sessões na plataforma ou fora dela? (`RN-PRT`)
- [ ] Vai emitir recibo como pessoa física ou nota fiscal como empresa? (`RN-FIN-10` a `RN-FIN-13`)
- [ ] Terá outros profissionais na clínica? Eles terão conta própria, dividirão receita com a clínica ou serão contratados? (`RN-PRO-05`)
- [ ] Haverá secretaria? O que ela pode ver e fazer? (seção 2)
- [ ] Atenderá pacientes que moram fora do Brasil? (`RN-ONL-12`)
- [ ] O que mais a preocupa em atender online?