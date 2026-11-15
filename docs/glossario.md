# Glossário do domínio — Clínica Acolher

Este glossário padroniza os termos usados no produto, nas conversas de negócio e no código. As regras de negócio identificadas abaixo são a referência para o comportamento; se uma regra mudar, atualize também este documento. As definições não substituem validação clínica, fiscal ou jurídica.

| Termo | Definição no domínio | Distinções e referências |
|---|---|---|
| **Paciente** | Pessoa que recebe atendimento e tem cadastro próprio na plataforma. O nome social, quando informado, é o nome de exibição. | Não é sinônimo de usuário: uma conta de acesso é uma credencial, enquanto paciente é a pessoa atendida. O paciente não vê as anotações clínicas do profissional. `RN-PAC-01`, `RN-PAC-07`, `RN-PER-01`. |
| **Profissional** | Pessoa que presta o atendimento e é responsável pela conduta clínica. No piloto, o foco é a psicanalista da clínica; outras categorias dependem das decisões de fase. | Não confundir com administrador da clínica ou suporte da plataforma, que têm permissões diferentes. `RN-GER-01`, seção 3 e `RN-PER-02`. |
| **Vínculo terapêutico** | Relação entre um paciente e um profissional que representa o contexto de atendimento e fundamenta a autorização de acesso aos dados daquele paciente. | É uma relação própria, não apenas um cargo ou uma associação administrativa. Só vínculo ativo permite agendar novas sessões; suspender, retomar e encerrar não apaga o histórico. `RN-VIN`, `RN-PER-01` a `RN-PER-04`. |
| **Sessão** | Atendimento individual, online, agendado para um paciente e um profissional em um intervalo de início, fim e fuso horário. No modelo técnico atual, a ocorrência agendada é representada por `Appointment`. | “Sessão” é o termo de produto; `Appointment` é o nome da entidade no código. Não é sinônimo de série semanal, pagamento ou anotação clínica. Estados: agendada, confirmada, realizada, falta, cancelada ou remarcada. `RN-AGD-01` a `RN-AGD-11`. |
| **Sessão avulsa** | Sessão agendada sem pertencer a uma série recorrente. | A recorrência é uma forma de organizar agendamentos, não uma única sessão repetida. `RN-AGD-05`. |
| **Série semanal** | Regra de recorrência que descreve o dia da semana, horário local, duração, fuso e período de uma sequência de sessões. Cada ocorrência é uma sessão independente. | Encerrar a série impede ocorrências futuras conforme a data final; não transforma a série em uma sessão nem altera sessões passadas. No código, `AppointmentSeries`. `RN-AGD-05` a `RN-AGD-07`, `RN-AGD-12`. |
| **Falta** | Estado final de uma sessão em que o paciente não compareceu, registrado a partir do horário de início. | Não é o mesmo que cancelamento. Uma falta só pode gerar cobrança se a política aplicável tiver sido aceita; falha técnica comprovada não deve ser tratada como falta do paciente. `RN-AGD-11`, `RN-CAN-01`, `RN-CAN-05`, `RN-CAN-07`. |
| **Cancelamento** | Encerramento de uma sessão antes da realização, com registro de quem cancelou, quando, antecedência e motivo informado. | Cancelamento não implica automaticamente cobrança. Remarcação também não é cancelamento simples: a sessão original é encerrada como remarcada e aponta para uma nova sessão. `RN-AGD-06`, `RN-CAN-03`, `RN-CAN-06`, `RN-CAN-07`. |
| **Política de faltas e cancelamento** | Regras versionadas que definem o tratamento de faltas e cancelamentos e que o paciente aceita antes de se aplicarem às sessões. | A versão aceita na data da sessão orienta a decisão. Até haver aceite registrado, não se cobra falta ou cancelamento. A política da clínica piloto ainda precisa ser definida. `RN-CAN-01`, `RN-CAN-02`, `RN-CAN-07`. |
| **Cobrança** | Registro de um valor devido à clínica ou ao profissional, relacionado a uma sessão realizada ou a uma falta cobrável, com valor e vencimento. | É uma obrigação financeira, não a confirmação de que o dinheiro foi recebido. Modelos previstos: por sessão, mensal ou pacote. Cobrança da clínica para o paciente é separada da assinatura SaaS paga à Acolher. `RN-FIN-01`, `RN-FIN-02`, `RN-FIN-16`. |
| **Pagamento** | Registro da quitação total ou parcial de uma ou mais cobranças, confirmada pelo provedor de pagamento. | Pode quitar várias cobranças; uma cobrança pode receber pagamentos parciais. Não é sinônimo de cobrança nem de recibo. `RN-FIN-03` a `RN-FIN-07`. |
| **Recibo** | Documento que comprova o pagamento recebido e identifica as informações exigidas para o atendimento pago. | É emitido depois do pagamento. Não é a cobrança nem a nota fiscal; não deve afirmar que o valor é dedutível no Imposto de Renda. `RN-FIN-10` a `RN-FIN-15`. |
| **Nota fiscal de serviço (NFS-e)** | Documento fiscal de serviço emitido quando aplicável ao recebedor pessoa jurídica, conforme o padrão nacional ou municipal. | Não é recibo. Nota autorizada não é editada: deve ser cancelada e substituída segundo o procedimento aplicável. `RN-FIN-13`. |
| **Assinatura SaaS** | Valor devido pela clínica ou profissional à Clínica Acolher pelo uso da plataforma. | É distinta das cobranças e pagamentos de sessões feitos pelos pacientes à clínica ou ao profissional. `RN-FIN-16`. |

## Convenções de linguagem

- Nas telas e conversas com pacientes, preferir **sessão** a “consulta” e **profissional** ao nome de uma categoria quando a categoria não for relevante.
- Usar **falta** somente para ausência do paciente registrada como estado da sessão; usar **cancelamento** quando uma das partes encerra a sessão antes de sua realização.
- Dizer **cobrança pendente** quando há valor devido ainda não quitado; dizer **pagamento confirmado** após confirmação do provedor.
- Reservar **recibo** para o documento emitido após o pagamento. Não chamar uma cobrança pendente de recibo.
- Distinguir sempre os valores pagos pelo paciente à clínica/profissional da assinatura paga pelo cliente à Clínica Acolher.

## Termos ainda sujeitos a decisão

- A política de faltas e cancelamento da clínica piloto ainda não foi definida. Até existir versão aceita pelo paciente, nenhuma falta ou cancelamento deve gerar cobrança (`RN-CAN-07`).
- O glossário descreve as regras de negócio atuais, não confirma que todas as funcionalidades correspondentes já estejam implementadas ou liberadas para dados reais.

## Critérios da issue #15

- [x] Definidos paciente, sessão, série, vínculo terapêutico, falta, cobrança e recibo.
- [x] Resultado registrado em `docs/`.
- [x] Nenhum dado real de paciente usado.
