# Decisão 004 — Criptografia de campos sensíveis

**Data:** 16/11/2026 · **Status:** aceita

## Decisão
Campos sensíveis (anotações clínicas, contato de emergência) são cifrados na aplicação antes de gravar, com criptografia de envelope: AES-256-GCM com chave de dados por registro, embrulhada por uma chave mestra guardada fora do banco. Um contexto (dados associados) liga cada valor cifrado ao registro dono dele.

## Consequências
- Nem quem administra o banco lê o conteúdo (RN-PRT-02, RN-SEG-07).
- A rotação da chave mestra é possível mantendo as chaves antigas para leitura.
- Perder a chave mestra torna os dados ilegíveis: exige guarda e cópia de segurança separadas do backup do banco antes de dados reais.
- Campos cifrados não podem ser pesquisados pelo banco.