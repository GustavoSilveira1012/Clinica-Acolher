import { destinoSeguro } from './safe-redirect';

describe('destinoSeguro', () => {
  it('aceita caminhos internos', () => {
    expect(destinoSeguro('/pacientes/novo')).toBe('/pacientes/novo');
  });

  it('recusa destinos externos ou vazios', () => {
    expect(destinoSeguro(null)).toBe('/agenda');
    expect(destinoSeguro('//site-falso.com')).toBe('/agenda');
    expect(destinoSeguro('///site-falso.com')).toBe('/agenda');
    expect(destinoSeguro('/\\site-falso.com')).toBe('/agenda');
    expect(destinoSeguro('/agenda\\..\\//site-falso.com')).toBe('/agenda');
    expect(destinoSeguro('https://site-falso.com')).toBe('/agenda');
  });
});