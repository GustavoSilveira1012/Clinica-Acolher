//Só aceita voltar para um caminho interno do próprio sistema.
export function destinoSeguro(voltar: string | null): string {
    const interno = !!voltar && voltar.startsWith('/') && !voltar.startsWith('/\\');
    return interno ? voltar! : '/agenda';
}