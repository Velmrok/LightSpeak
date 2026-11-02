import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ServerPage } from './server-page';

describe('ServerPage', () => {
  let component: ServerPage;
  let fixture: ComponentFixture<ServerPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServerPage],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
