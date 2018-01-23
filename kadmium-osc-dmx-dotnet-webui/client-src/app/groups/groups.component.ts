import { Component, OnInit, ViewChild } from '@angular/core';
import { Title } from "@angular/platform-browser";
import { StatusCode } from "../status-code.enum";
import { FileSaver } from "../file-saver";
import { AsyncFileReader } from "../async-file-reader";
import { GroupService } from "api/services";
import { Group } from "api/models";
import { MatTableDataSource } from "@angular/material/table";
import { MatSnackBar } from '@angular/material';
import { NgForm } from '@angular/forms';
import { AnimationLibrary } from "app/animation-library";
import { EditorComponent } from "app/editor-component/editor-component";

@Component({
    selector: 'app-groups',
    templateUrl: './groups.component.html',
    styleUrls: ['./groups.component.css'],
    providers: [GroupService],
    animations: [AnimationLibrary.animations()]
})
export class GroupsComponent extends EditorComponent implements OnInit
{
    groups: Group[];

    public saving: boolean;
    public loading: boolean;

    @ViewChild("groupsForm") formChild: NgForm;

    constructor(private groupsService: GroupService, private snackbar: MatSnackBar, title: Title)
    {
        super();
        title.setTitle("Groups");
        this.saving = false;
        this.loading = true;
    }

    ngOnInit()
    {
        this.form = this.formChild;
        this.groupsService.getGroups()
            .toPromise()
            .then(response => 
            {
                this.groups = response;
                this.loading = false;
            }).catch(reason => this.snackbar.open(reason, "Close", { duration: 3000 }));
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

    public add(): void
    {
        let group: Group = {
            order: this.getNextOrder()
        };

        this.groups.push(group);
    }

    public delete(group: Group): void
    {
        let index = this.groups.indexOf(group);
        this.groups.splice(index, 1);
    }

    public swap(oldIndex: number, newIndex: number): void
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

    public getElementIndex(group: Group): number
    {
        return this.groups.indexOf(group);
    }

    private get groupsSorted(): Group[]
    {
        return this.groups.sort((a, b) => a.order - b.order);
    }

    public async save(): Promise<void>
    {
        this.saving = true;
        try
        {
            await this.groupsService.putGroup(this.groups);
            this.saved = true;
            this.snackbar.open("Saved successfully", "Close", { duration: 3000 })
        }
        catch (reason)
        {
            this.snackbar.open(reason, "Close", { duration: 3000 });
        }
        finally
        {
            this.saving = false;
        }
    }

    public async download(): Promise<void>
    {
        try
        {
            FileSaver.Save("groups.json", this.groups);
        }
        catch (error)
        { }
    }

    public upload(fileInput: any): void
    {
        (fileInput as HTMLInputElement).click();
    }

    public async filesSelected(files: File[]): Promise<void>
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
            this.snackbar.open("Successfully added " + groups.length + " groups", "Close", { duration: 3000 });
        }
        catch (reason)
        {
            this.snackbar.open(reason, "Close", { duration: 3000 });
        }
    }

}
