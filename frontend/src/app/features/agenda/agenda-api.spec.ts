import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AgendaApi } from './agenda-api';

describe('AgendaApi', () => {
  let api: AgendaApi;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AgendaApi, provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(AgendaApi);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('cria uma série com o dia, hora local e data de início', () => {
    api.createSeries('relationship-1', new Date(2026, 10, 17, 15, 0), 50, 'America/Sao_Paulo').subscribe();

    const request = http.expectOne('/api/v1/series');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      relationshipId: 'relationship-1',
      weekday: 'Tuesday',
      localStartTime: '15:00:00',
      startsOn: '2026-11-17',
      durationMinutes: 50,
      timeZoneId: 'America/Sao_Paulo',
    });
    request.flush({});
  });

  it('encerra uma série na data informada', () => {
    api.endSeries('series-1', '2026-12-01').subscribe();

    const request = http.expectOne('/api/v1/series/series-1/end');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ lastDay: '2026-12-01' });
    request.flush({});
  });
});
