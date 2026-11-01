import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ServerAvatar } from './server-avatar';

describe('ServerAvatar', () => {
  let component: ServerAvatar;
  let fixture: ComponentFixture<ServerAvatar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServerAvatar],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerAvatar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
