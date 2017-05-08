import { Component, OnInit } from '@angular/core';
import { GroupService } from "../group.service";
import { NotificationsService } from "../notifications.service";
import { Title } from "@angular/platform-browser";
import { Group } from "../group";
import { StatusCode } from "../status-code.enum";
import { FileSaver } from "../file-saver";
import { AsyncFileReader } from "../async-file-reader";

@Component({
    selector: 'app-groups',
    templateUrl: './groups.component.html',
    styleUrls: ['./groups.component.css'],
    providers: [GroupService]
})
export class GroupsComponent implements OnInit
{
    saving: boolean;
    groups: Group[];

    constructor(private groupsService: GroupService, private notificationsService: NotificationsService, title: Title)
    {
        title.setTitle("Groups");
        this.saving = false;
        this.groups = [];
    }

    async ngOnInit(): Promise<void>
    {
        try
        {
            this.groups = await this.groupsService.get();
        }
        catch (reason)
        {
            this.notificationsService.add(StatusCode.Error, reason)
        }
    }

    private getNextOrder(): number
    {
        let maxOrder = 0;
        this.groups.forEach(value => 
        {
            if (value.order > maxOrder)
            {
                maxOrder = value.order;
            }
        });
        return maxOrder + 1;
    }

    private add(): void
    {
        let group = new Group("");
        group.order = this.getNextOrder();
        this.groups.push(group);
    }

    private delete(group: Group): void
    {
        let index = this.groups.indexOf(group);
        this.groups.splice(index, 1);
    }

    private swap(oldIndex: number, newIndex: number): void
    {
        let oldOrder = this.groupsSorted[oldIndex].order;
        let newOrder = this.groupsSorted[newIndex].order;
        this.groupsSorted[oldIndex].order = newOrder;
        this.groupsSorted[newIndex].order = oldOrder;
    }

    private getOtherGroupNames(group: Group): string[]
    {
        let result = this.groups.filter(item => item != group).map(grp => grp.name);
        return result;
    }

    private get groupsSorted(): Group[]
    {
        return this.groups.sort((a, b) => a.order - b.order);
    }

    private async save(): Promise<void>
    {
        this.saving = true;
        try
        {
            await this.groupsService.put(this.groups);
            this.notificationsService.add(StatusCode.Success, "Saved successfully")
        }
        catch (reason)
        {
            this.notificationsService.add(StatusCode.Error, reason);
        }
        finally
        {
            this.saving = false;
        }
    }

    private async download(): Promise<void>
    {
        try
        {
            FileSaver.Save("groups.json", this.groups);
        }
        catch (error)
        { }
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
            let groups = await AsyncFileReader.read<Group[]>(file);
            groups.sort((a, b) => a.order - b.order);
            for (let group of groups)
            {
                group.id = 0;
                group.order = this.getNextOrder();
                this.groups.push(group);
            }
            this.notificationsService.add(StatusCode.Success, "Successfully added " + groups.length + " groups");
        }
        catch (reason)
        {
            this.notificationsService.add(StatusCode.Error, reason);
        }
    }

}