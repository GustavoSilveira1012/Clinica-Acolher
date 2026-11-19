//Só aceita voltar para um caminho interno do próprio sistema.
export function destinoSeguro(voltar: string | null): string {
  const interno = voltar !== null
    && voltar.startsWith('/')
    && !voltar.startsWith('//')
    && !voltar.includes('\\');
  return interno ? voltar : '/agenda';
}