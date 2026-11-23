import { PortalPolicy } from './portal.models';

/** A política de faltas em frases simples, para o paciente ler antes de aceitar. */
export function descreverPolitica(p: PortalPolicy): string[] {
  const prazo = p.minimumNoticeHours === 1 ? '1 hora' : `${p.minimumNoticeHours} horas`;
  const cobraCancelamento = p.lateCancellationChargePercent > 0 && p.minimumNoticeHours > 0;

  return [
    cobraCancelamento
      ? `Se você cancelar com menos de ${prazo} de antecedência, será cobrado ${p.lateCancellationChargePercent}% do valor da sessão.`
      : 'Cancelamentos feitos por você não são cobrados.',
    ...(cobraCancelamento ? [`Cancelamentos com ${prazo} ou mais de antecedência não são cobrados.`] : []),
    p.noShowChargePercent > 0
      ? `Se você faltar sem avisar, será cobrado ${p.noShowChargePercent}% do valor da sessão.`
      : 'Faltas não são cobradas.',
    'Sessões canceladas pelo profissional ou por falha técnica da plataforma nunca são cobradas de você.',
  ];
}