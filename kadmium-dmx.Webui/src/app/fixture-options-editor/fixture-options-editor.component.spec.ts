import { async, ComponentFixture, TestBed, fakeAsync } from '@angular/core/testing';

import { FixtureOptionsEditorComponent, FixtureContainer } from './fixture-options-editor.component';
import { FormsModule } from "@angular/forms";
import { LabelledInputComponent } from "../labelled-input/labelled-input.component";
import { TableInputComponent } from "../table-input/table-input.component";
import { APIClient, FixtureData } from '../../api';
import { MatDialogRef, MatFormField, MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle, MatCheckbox, MatDivider, MatDialogClose, MAT_DIALOG_DATA } from '../../../node_modules/@angular/material';
import { MockComponent } from '../../../node_modules/ng-mocks';
import { InjectionToken } from '../../../node_modules/@angular/core';
import { from } from '../../../node_modules/rxjs';

describe('FixtureOptionsEditorComponent', () =>
{
    let component: FixtureOptionsEditorComponent;
    let fixture: ComponentFixture<FixtureOptionsEditorComponent>;

    let fixtureContainer: FixtureContainer;

    beforeEach(async(() =>
    {
        fixtureContainer = {
            fixture: {
                address: 1,
                group: "",
                type: {
                    manufacturer: "Some guy",
                    model: "Some thing"
                },
                options: { maxBrightness: 1, axisInversions: [], axisRestrictions: [] }
            }
        };
        TestBed.configureTestingModule({
            declarations: [
                FixtureOptionsEditorComponent,
                MockComponent(MatFormField),
                MockComponent(MatExpansionPanel),
                MockComponent(MatExpansionPanelTitle),
                MockComponent(MatExpansionPanelHeader),
                MockComponent(MatDivider),
                MockComponent(MatCheckbox),
                MockComponent(MatDialogClose)

            ],
            imports: [FormsModule]
        });

        TestBed.overrideComponent(FixtureOptionsEditorComponent, {
            set: {
                providers: [
                    { provide: APIClient, useValue: jasmine.createSpyObj<APIClient>({ getFixtureDefinition: from([]) }) },
                    { provide: MatDialogRef, useValue: jasmine.createSpyObj<MatDialogRef<FixtureOptionsEditorComponent>>({ close: null }) },
                    { provide: MAT_DIALOG_DATA, useValue: fixtureContainer }
                ]
            }
        });

        TestBed.compileComponents();
    }));

    beforeEach(() =>
    {
        fixture = TestBed.createComponent(FixtureOptionsEditorComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', fakeAsync(() =>
    {
        expect(component).toBeTruthy();
    }));
});
