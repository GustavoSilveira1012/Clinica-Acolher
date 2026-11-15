# Como contribuir

Obrigado por ajudar a construir a Clínica Acolher. Antes de começar, escolha uma issue ou registre a necessidade no GitHub. Evite trabalho fora do escopo combinado e use somente dados fictícios: nunca inclua dados reais de pacientes, credenciais, tokens ou segredos em código, testes, documentação, commits ou capturas de tela.

## Branches

Crie uma branch a partir da branch base atualizada. Use letras minúsculas, hífens e o número da issue:

```text
<tipo>/<numero>-<descricao-curta>
```

Tipos recomendados:

| Tipo | Quando usar |
|---|---|
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug |
| `docs` | Documentação |
| `test` | Testes |
| `refactor` | Refatoração sem mudança intencional de comportamento |
| `chore` | Manutenção geral |
| `ci` | Integração e entrega contínuas |
| `build` | Sistema de build e dependências |

Exemplos:

```text
feat/123-series-semanais
fix/124-fuso-da-agenda
docs/125-atualizar-glossario
```

Mantenha a descrição curta, sem espaços, acentos ou dados de pacientes. Se não houver issue, use uma descrição que identifique o trabalho e abra uma issue para dar rastreabilidade.

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/pt-br/v1.0.0/):

```text
<tipo>(<escopo opcional>): <descricao>
```

- Escreva o tipo em minúsculas e use um escopo curto quando ajudar a identificar a área.
- Descreva a mudança de forma objetiva, preferencialmente com verbo no infinitivo; não termine com ponto.
- Faça commits pequenos e focados. Não inclua arquivos locais, `.env`, segredos ou dados reais.
- Para uma mudança incompatível, use `!` no cabeçalho e explique no rodapé `BREAKING CHANGE:`.
- Quando útil, referencie a issue no rodapé com `Refs #123`. Deixe `Closes #123` para o pull request que realmente concluir a issue.

Exemplos:

```text
feat(agenda): adicionar séries semanais
fix(auth): apagar senha após falha no login
docs(contribuicao): documentar convenções
test(agenda): cobrir conflito em série recorrente
```

Exemplo de mudança incompatível:

```text
feat(api)!: exigir autenticação nas rotas

BREAKING CHANGE: clientes sem sessão agora recebem HTTP 401.
```

## Pull requests

1. Rebaseie ou atualize sua branch com a branch base e resolva conflitos.
2. Execute os testes e validações relevantes; informe no PR os comandos e resultados.
3. Abra o PR com um resumo, a issue relacionada e os impactos. Use `Closes #123` apenas quando o PR atender a issue.
4. Preencha o [template de pull request](./.github/pull_request_template.md), incluindo os itens de segurança, privacidade e acessibilidade aplicáveis.
5. Aguarde o CI verde e a revisão antes do merge.

Se uma regra de negócio, decisão ou instrução de execução mudar, atualize a documentação correspondente. Mudanças que afetam dados clínicos, permissões ou isolamento entre clínicas devem explicar como esses limites foram preservados e testados.
