# Hipótese de cobrança do SaaS

**Issue:** [#11 — Definir modelo de cobrança do SaaS](https://github.com/GustavoSilveira1012/Clinica-Acolher/issues/11)  
**Status:** hipótese para validação; não é preço aprovado nem oferta comercial.  
**Data:** 14/11/2026.

## Hipótese inicial

Quando a Clínica Acolher estiver pronta para a fase SaaS, cobrar uma assinatura mensal em reais por profissional ativo:

- **Plano inicial único:** R$ 49,90 por profissional por mês.
- **Clínicas:** contratar uma licença por profissional que usa o sistema; sem taxa fixa por clínica nesta primeira hipótese.
- **Unidade cobrada:** cada conta profissional com acesso habilitado no ciclo; pacientes não ocupam licença.
- **Cobrança:** mensal, sem fidelidade e sem taxa de implantação.
- **Teste:** até 14 dias, sem cartão cadastrado; a assinatura só começa após aceite explícito do cliente.
- **Sem comissão:** não cobrar percentual sobre sessões, pacotes ou pagamentos feitos pelos pacientes.

R$ 49,90 é um preço-âncora para pesquisa, não uma promessa de lançamento. A proposta evita criar vários planos antes de conhecer os custos e a disposição a pagar. A licença por profissional é simples de explicar e cresce com o tamanho da equipe. A mensalidade do SaaS fica separada do dinheiro que os pacientes pagam à clínica, conforme [RN-FIN-16](./Regras%20de%20negocio.md).

Não definir nesta etapa limites de pacientes, sessões ou armazenamento como se fossem ilimitados. Custos variáveis de vídeo, mensagens, armazenamento e processamento de pagamentos precisam ser levantados. Se houver franquias, adicionais ou limites, devem ser claros antes da contratação. Taxas de provedores de pagamento, se existirem, não são comissão da Acolher e devem ser apresentadas separadamente.

## Por que testar R$ 49,90

A pesquisa de concorrentes consultada em 14/11/2026 encontrou preços publicados que vão de opções anuais equivalentes a cerca de R$ 19,90–39,90 por mês até planos mensais individuais a R$ 109 ou mais. O iClinic publica planos de R$ 99 a R$ 299 por profissional/mês. Esses produtos, públicos e condições não são diretamente equivalentes.

O preço proposto fica acima de algumas opções de entrada e abaixo de planos mensais mais amplos. Isso é apenas uma posição inicial para testar: a pesquisa não comprova que a Acolher tenha valor superior, menor custo ou disposição de pagamento nesse patamar. As ofertas podem mudar; conferir as condições vigentes antes de qualquer decisão comercial. Detalhes e fontes estão em [pesquisa de concorrentes](./pesquisa-concorrentes.md).

## Como validar com clientes

Fazer a validação depois do piloto e antes de abrir vendas, com demonstração em ambiente fictício:

1. Conversar com cerca de oito potenciais clientes: profissionais autônomos e responsáveis por clínicas pequenas. Registrar o perfil comercial necessário para segmentar os resultados, sem registrar dados de pacientes.
2. Antes de mostrar preços, entender como organizam agenda, registros e recebimentos hoje, quais ferramentas pagam, quais problemas são recorrentes e quem decide a compra.
3. Demonstrar os mesmos fluxos e condições para todas as pessoas, usando somente dados fictícios. Não apresentar funcionalidades planejadas como se já estivessem disponíveis.
4. Testar R$ 49,90 por profissional/mês e comparar com alternativas de R$ 29,90 e R$ 69,90. Perguntar o que a pessoa espera receber, em qual faixa consideraria caro ou barato demais e qual seria o próximo passo concreto para contratar.
5. Registrar respostas e objeções por perfil, além do número de pessoas que pedem uma proposta ou aceitam participar de um teste pago quando a oferta estiver pronta. Não tratar intenção verbal isolada como venda.

**Sinal inicial para manter a hipótese:** pelo menos cinco de oito participantes do público-alvo consideram R$ 49,90 aceitável para a demonstração apresentada, e pelo menos três pedem uma proposta ou manifestam interesse concreto em um teste pago futuro. A amostra é exploratória, não representa o mercado; se o sinal não aparecer, revisar valor percebido, público, escopo ou preço antes de escolher um plano.

Não cobrar por uma versão incompleta nem iniciar uma assinatura real durante entrevistas. Antes de vender, cumprir os critérios de lançamento SaaS em [projeto](./projeto.md), incluindo multi-organização, planos e limites, onboarding e contrato. Confirmar também condições fiscais e contratuais com profissionais qualificados.

## Métricas após o lançamento

Avaliar o preço com comportamento real, além das entrevistas:

- conversão do período de teste em assinatura paga;
- cancelamentos e motivos, por perfil e tempo de uso;
- receita recorrente mensal;
- custo variável por profissional e margem após provedores e suporte.

Revisar a hipótese quando houver uma amostra suficiente de testes e ciclos pagos para distinguir um problema de preço de problemas de ativação, produto ou suporte. Não alterar preços retroativamente para assinantes sem comunicação e tratamento contratual apropriados.

## Limites e rastreabilidade

- Esta hipótese cobre somente a assinatura do software. Não define preços de sessões, pacotes clínicos ou marketplace.
- O marketplace continua fora do escopo desta decisão e tem responsabilidades e custos diferentes.
- Nenhum dado real de paciente deve ser coletado ou usado na pesquisa de preço.
- A issue cita “Seção 43 do PROJETO.md”; no repositório consultado não há `PROJETO.md` nem seção 43 identificável. Esta hipótese se ancora na regra disponível `RN-FIN-16`, no ciclo de assinatura descrito em [projeto](./projeto.md) e no critério de lançamento da fase SaaS. Confirmar a referência da issue antes de transformar esta hipótese em regra definitiva.
