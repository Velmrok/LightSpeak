import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChannelCreateForm } from './channel-create-form';

describe('ChannelCreateForm', () => {
  let component: ChannelCreateForm;
  let fixture: ComponentFixture<ChannelCreateForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChannelCreateForm],
    }).compileComponents();

    fixture = TestBed.createComponent(ChannelCreateForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
