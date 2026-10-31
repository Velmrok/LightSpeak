import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServersCreateForm } from './servers-create-form';

describe('ServersCreateForm', () => {
  let component: ServersCreateForm;
  let fixture: ComponentFixture<ServersCreateForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ServersCreateForm],
    }).compileComponents();

    fixture = TestBed.createComponent(ServersCreateForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
