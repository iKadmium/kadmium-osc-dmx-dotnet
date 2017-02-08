import { Component } from '@angular/core';
import { Http } from '@angular/http';
import { NgModule } from '@angular/core';

import { SettingsService } from "./settings.service";
import { Settings } from "./settings";

var $ = require("jquery");

@Component({
    selector: 'settings',
    template: require('./settings.component.html'),
    providers: [SettingsService]
})
export class SettingsComponent
{
    settings: Settings;

    constructor(private settingsService: SettingsService)
    {
        this.settingsService.get().then(data =>
        {
            this.settings = data;
        });
    }

    public async save(): Promise<void>
    {
        await this.settingsService.save(this.settings);
    }
}