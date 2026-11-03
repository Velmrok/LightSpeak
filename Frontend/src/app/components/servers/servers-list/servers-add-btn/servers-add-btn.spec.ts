import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ServersAddBtn } from './servers-add-btn';

describe('ServersAddBtn', () => {
  let component: ServersAddBtn;
  let fixture: ComponentFixture<ServersAddBtn>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServersAddBtn],
    }).compileComponents();

    fixture = TestBed.createComponent(ServersAddBtn);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
