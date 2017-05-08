import { Component, OnInit } from '@angular/core';
import { FixtureDefinitionService } from "../fixture-definition.service";
import { AsyncFileReader } from "../async-file-reader";
import { FixtureDefinition, FixtureDefinitionSkeleton } from "../fixture-definition";
import { NotificationsService } from "../notifications.service";
import { StatusCode } from "../status-code.enum";
import { Title } from "@angular/platform-browser";

@Component({
    selector: 'app-fixture-definitions',
    templateUrl: './fixture-definitions.component.html',
    styleUrls: ['./fixture-definitions.component.css'],
    providers: [FixtureDefinitionService]
})
export class FixtureDefinitionsComponent implements OnInit
{
    manufacturerFilterEnabled: boolean;
    manufacturerFilter: string;
    skeletons: FixtureDefinitionSkeleton[];

    constructor(private fixtureDefinitionsService: FixtureDefinitionService,
        private notificationsService: NotificationsService, title: Title)
    {
        title.setTitle("Fixture Definitions");
        this.skeletons = [];
    }

    async ngOnInit(): Promise<void>
    {
        try
        {
            this.skeletons = await this.fixtureDefinitionsService.getSkeletons();
            if (this.manufacturers.length > 0)
            {
                this.manufacturerFilter = this.manufacturers[0];
            }
        }
        catch (reason)
        {
            this.notificationsService.add(StatusCode.Error, reason);
        }
    }

    private get manufacturers(): string[]
    {
        return this.skeletons
            .map((value: FixtureDefinitionSkeleton) => value.manufacturer)
            .filter((value: string, index: number, array: string[]) => array.indexOf(value) === index);
    }

    private get filteredData(): FixtureDefinitionSkeleton[]
    {
        if (this.manufacturerFilterEnabled)
        {
            return this.skeletons.filter((value: FixtureDefinitionSkeleton) => value.manufacturer == this.manufacturerFilter);
        }
        else
        {
            return this.skeletons;
        }
    }

    private getEditUrl(fixture: FixtureDefinitionSkeleton): string
    {
        return "fixture-definitions" + "/" + fixture.id;
    }

    private getDownloadUrl(fixture: FixtureDefinitionSkeleton): string
    {
        return "/api/FixtureDefinition" + "/" + fixture.id;
    }

    private async deleteConfirm(fixture: FixtureDefinitionSkeleton): Promise<void>
    {
        if (window.confirm("Are you sure you want to delete the definition for " + fixture.manufacturer + " " + fixture.model + "?"))
        {
            try
            {
                await this.fixtureDefinitionsService.delete(fixture);

                this.notificationsService.add(StatusCode.Success, fixture.manufacturer + " " + fixture.model + " was deleted");
                let index = this.skeletons.indexOf(fixture);
                this.skeletons.splice(index, 1);
            }
            catch (reason)
            {
                this.notificationsService.add(StatusCode.Error, reason);
            }
        }
    }

    private upload(fileInput: any): void
    {
        (fileInput as HTMLInputElement).click();
    }

    private async filesSelected(files: File[]): Promise<void>
    {
        for (let file of files)
        {
            await this.uploadFile(file);
        }
    }

    private async uploadFile(file: File): Promise<void>
    {
        try
        {
            let definition = await AsyncFileReader.read<FixtureDefinition>(file);
            definition.id = await this.fixtureDefinitionsService.post(definition);
            this.skeletons.push(definition);
            this.notificationsService.add(StatusCode.Success, "Successfully added " + definition.manufacturer + " " + definition.model);
        }
        catch (reason)
        {
            this.notificationsService.add(StatusCode.Error, reason);
        }
    }

}