import { TestBed } from '@angular/core/testing';
import { TokenService } from './token'; // Changed from Token to TokenService

describe('TokenService', () => { // Changed from Token to TokenService
  let service: TokenService; // Changed from Token to TokenService

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenService); // Changed from Token to TokenService
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});