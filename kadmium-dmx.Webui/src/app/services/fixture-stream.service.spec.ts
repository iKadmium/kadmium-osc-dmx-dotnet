import { TestBed, inject } from '@angular/core/testing';

import { FixtureStreamService } from './fixture-stream.service';

describe('FixtureStreamService', () =>
{
  beforeEach(() =>
  {
    TestBed.configureTestingModule({
      providers: [FixtureStreamService]
    });
  });

  it('should ...', inject([FixtureStreamService], (service: FixtureStreamService) =>
  {
    expect(service).toBeTruthy();
  }));
});
