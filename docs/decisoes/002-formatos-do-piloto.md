# Decisão 002 — Formatos de atendimento, anotações e pagamento no piloto

**Data:** 14/11/2026.  
**Status:** direção de produto definida por Gustavo Silveira; escopo completo ainda depende da aprovação da cliente piloto na issue #2.

## Decisão e contexto

Após discutir o MVP, o responsável pelo projeto decidiu incluir vídeo dentro e fora da plataforma, anotações nos dois formatos e pagamento por Pix ou cartão, conforme a escolha do paciente. O [escopo do piloto](../escopo-piloto.md) detalha as entregas e pendências.

Isso substitui a restrição anterior de vídeo exclusivamente dentro da plataforma. O atendimento continua online; não foi autorizado atendimento presencial. Anotações externas podem fazer parte da rotina sem obrigar a digitação no sistema durante a sessão.

## Consequências

- M6 deve cobrir os dois caminhos de vídeo. Escolher um fornecedor integrado e definir a ferramenta externa permitida; não oferecer suporte irrestrito a qualquer ferramenta.
- Rever a aplicação das regras RN-ONL em cada caminho. Não presumir que a plataforma consiga controlar sala de espera, expiração de links ou coletar metadados em um serviço externo sem integração. Validar capacidades e procedimentos antes de liberar esse caminho.
- M8 deve entregar registros internos protegidos e definir, com a profissional, a fonte oficial do registro e os procedimentos de guarda e acesso externos. Não há sincronização automática incluída nesta decisão.
- M7 deve oferecer Pix e cartão. A implementação da integração, o provedor, as taxas, o recebedor e os fluxos de confirmação continuam pendentes.
- As opções externas não retiram vídeo interno, anotações internas ou qualquer um dos dois meios de pagamento da lista de entrada.
- Permanecem as regras do projeto sobre sigilo, acesso autorizado, ausência de gravação e uso de dados fictícios em desenvolvimento.

## Validação pendente

A cliente piloto deve revisar o escopo completo, inclusive o que fica fora, e confirmar os procedimentos de sua rotina. Esta decisão não comprova implementação, contratação de fornecedores ou prontidão para pacientes reais.
