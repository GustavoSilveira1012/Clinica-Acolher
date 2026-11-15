import tokens from '../../../../docs/design-tokens.json';
import { luminanciaRelativa, razaoContraste } from './contraste';

type Tema = keyof typeof tokens.cores;
type TipoPar = keyof typeof tokens.contraste.minimos;

interface Par {
  tema: string;
  frente: string;
  fundo: string;
  tipo: string;
  uso: string;
}

function cor(tema: string, nome: string): string {
  const paleta = tokens.cores[tema as Tema] as Record<string, string> | undefined;
  const valor = paleta?.[nome];
  if (!valor) {
    throw new Error(`Token inexistente: ${tema}.${nome}`);
  }
  return valor;
}

describe('cálculo de contraste WCAG 2.1', () => {
  it('calcula a luminância relativa dos extremos', () => {
    expect(luminanciaRelativa('#000000')).toBe(0);
    expect(luminanciaRelativa('#ffffff')).toBe(1);
    expect(luminanciaRelativa('#fff')).toBe(1);
  });

  it('calcula a razão de contraste de referência', () => {
    expect(razaoContraste('#000000', '#ffffff')).toBeCloseTo(21, 5);
    expect(razaoContraste('#ffffff', '#000000')).toBeCloseTo(21, 5);
    expect(razaoContraste('#777777', '#777777')).toBe(1);
    // #767676 sobre branco é o cinza mais claro aceito para texto normal (≈ 4,54:1).
    expect(razaoContraste('#767676', '#ffffff')).toBeCloseTo(4.54, 2);
  });

  it('recusa cores fora do formato hexadecimal', () => {
    expect(() => luminanciaRelativa('verde')).toThrow();
    expect(() => luminanciaRelativa('#12345')).toThrow();
  });
});

describe('RN-SEG-13: contraste dos design tokens (WCAG 2.1 AA)', () => {
  const pares = tokens.contraste.pares as Par[];

  it('declara pares para os temas claro e escuro', () => {
    expect(pares.some((p) => p.tema === 'claro')).toBe(true);
    expect(pares.some((p) => p.tema === 'escuro')).toBe(true);
  });

  it('usa apenas tipos de par com mínimo definido', () => {
    for (const par of pares) {
      expect(tokens.contraste.minimos[par.tipo as TipoPar], `tipo "${par.tipo}"`).toBeDefined();
    }
  });

  it.each(pares.map((p) => [`${p.tema}: ${p.frente} sobre ${p.fundo} (${p.uso})`, p] as const))(
    '%s atinge o mínimo AA',
    (_nome, par) => {
      const minimo = tokens.contraste.minimos[par.tipo as TipoPar];
      const razao = razaoContraste(cor(par.tema, par.frente), cor(par.tema, par.fundo));
      expect(razao, `${par.frente} sobre ${par.fundo}: ${razao.toFixed(2)}:1 < ${minimo}:1`).toBeGreaterThanOrEqual(minimo);
    },
  );

  it('registra ajustes que realmente passaram a cumprir o AA', () => {
    for (const ajuste of tokens.ajustes) {
      const [tema, nome] = ajuste.token.split('.');
      expect(cor(tema, nome)).toBe(ajuste.ajustada);
      expect(ajuste.original).not.toBe(ajuste.ajustada);
    }
  });
});
