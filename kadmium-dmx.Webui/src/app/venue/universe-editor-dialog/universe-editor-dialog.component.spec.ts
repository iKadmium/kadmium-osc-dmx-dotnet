import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatFormField, MAT_DIALOG_DATA } from '@angular/material';
import { UniverseData } from 'api';
import { MockComponent } from 'ng-mocks';
import { UniverseTestHelpers } from '../../test/universe-test-helpers';
import { UniverseEditorDialogComponent } from './universe-editor-dialog.component';

describe('UniverseEditorDialogComponent', () =>
{
	let component: UniverseEditorDialogComponent;
	let fixture: ComponentFixture<UniverseEditorDialogComponent>;
	let data: UniverseData;

	beforeEach(async(() =>
	{
		data = UniverseTestHelpers.getUniverse();

		TestBed.configureTestingModule({
			declarations: [
				UniverseEditorDialogComponent,
				MockComponent(MatFormField)
			],
			imports: [
				ReactiveFormsModule
			],
			providers: [
				{ provide: MatDialogRef, useValue: jasmine.createSpyObj<MatDialogRef<UniverseEditorDialogComponent>>({ close: null }) },
				{ provide: MAT_DIALOG_DATA, useValue: data }
			]
		});
		TestBed.compileComponents();
	}));

	beforeEach(() =>
	{
		fixture = TestBed.createComponent(UniverseEditorDialogComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () =>
	{
		expect(component).toBeTruthy();
	});
});
