/**
 * Cálculo de contraste do WCAG 2.1 (RN-SEG-13).
 * https://www.w3.org/TR/WCAG21/#dfn-relative-luminance
 * https://www.w3.org/TR/WCAG21/#dfn-contrast-ratio
 */

const HEX = /^#([0-9a-f]{3}|[0-9a-f]{6})$/i;

function canalLinear(valor: number): number {
  const c = valor / 255;
  return c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
}

export function luminanciaRelativa(cor: string): number {
  if (!HEX.test(cor)) {
    throw new Error(`Cor inválida: "${cor}". Use hexadecimal, como #24564f.`);
  }
  let hex = cor.slice(1);
  if (hex.length === 3) {
    hex = [...hex].map((c) => c + c).join('');
  }
  const [r, g, b] = [0, 2, 4].map((i) => canalLinear(parseInt(hex.slice(i, i + 2), 16)));
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

export function razaoContraste(corA: string, corB: string): number {
  const [clara, escura] = [luminanciaRelativa(corA), luminanciaRelativa(corB)].sort((a, b) => b - a);
  return (clara + 0.05) / (escura + 0.05);
}
