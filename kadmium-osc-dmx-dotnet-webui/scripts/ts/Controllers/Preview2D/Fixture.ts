﻿import {MovementData, MovementViewModel} from "../Fixtures/Movement";
import {FixtureData as FixtureDefinitionData} from "../Fixtures/Fixture";
import {ChannelData} from "../Fixtures/Channel";
import {MVC} from "../MVC";

import * as ko from "knockout";

export interface FixtureData
{
    name: string;
    address: number;
    group: string;
    definition: FixtureDefinitionData;
}

class ChannelViewModel
{
    name: KnockoutObservable<string>;
    address: KnockoutObservable<number>;
    min: number;
    max: number;
    dmxValue: KnockoutObservable<number>;
    value: KnockoutComputed<number>;
    
    constructor(name: string, address: number, min: number, max: number)
    {
        this.name = ko.observable<string>(name);
        this.address = ko.observable<number>(address);
        this.min = min;
        this.max = max;
        this.dmxValue = ko.observable<number>(0);
        this.value = ko.computed<number>(() => ChannelViewModel.rebaseRange(this.dmxValue(), this.min, this.max, 0, 1));
    }

    static rebaseRange(value: number, oldMin: number, oldMax: number, newMin: number, newMax: number): number
    {
        let oldRange = oldMax - oldMin;
        let percentageOfOldRange = (value - oldMin) / oldRange;

        let newRange = newMax - newMin;
        let newValue = percentageOfOldRange * newRange + newMin;

        return newValue;
    }

    static load(data: ChannelData, startChannel: number): ChannelViewModel
    {
        let channel = new ChannelViewModel(data.name, data.dmx + startChannel - 1, data.min, data.max);
        return channel;
    }
}

class MovementLookupTable
{
    [name: string]: MovementViewModel;
    constructor()
    {
    }
}

class ChannelLookupTable
{
    [name: string]: ChannelViewModel;

    constructor()
    {   
    }
}

export class FixtureViewModel
{
    channelLookupTable: ChannelLookupTable;
    channels: KnockoutObservableArray<ChannelViewModel>;
    movements: MovementLookupTable;
    address: KnockoutObservable<number>;
    name: KnockoutObservable<string>;

    strobe: KnockoutObservable<boolean>;

    pan: KnockoutComputed<number>;
    tilt: KnockoutComputed<number>;
    
    color: KnockoutComputed<string>;

    canvas: HTMLCanvasElement;
    context: CanvasRenderingContext2D;

    static STROBE_HZ = 15;
    static STROBE_MILLIS = 1000 / FixtureViewModel.STROBE_HZ;

    constructor(type: string, address: number, channelData: ChannelData[])
    {
        this.address = ko.observable<number>(address);
        this.name = ko.observable<string>(type);
        
        this.channelLookupTable = new ChannelLookupTable();
        this.channels = ko.observableArray<ChannelViewModel>(); 
        this.movements = new MovementLookupTable();
        this.strobe = ko.observable<boolean>();

        this.channels.subscribe((newValue: KnockoutArrayChange<ChannelViewModel>[]) =>
        {
            for (let change of newValue)
            {
                switch (change.status)
                {
                    case "added":
                        this.channelLookupTable[change.value.name()] = change.value;
                        break;
                    case "deleted":
                        delete this.channelLookupTable[change.value.name()];
                        break;
                }
            }
        }, this, "arrayChange");

        for (let channelDataItem of channelData)
        {
            let channel = ChannelViewModel.load(channelDataItem, this.address());
            this.channels.push(channel);
        }

        setInterval(() =>
        {
            let oldValue = this.strobe();
            let newValue: boolean;
            if (this.optionalChannelGet("Strobe") > 0)
            {
                newValue = !oldValue;
            }
            else
            {
                newValue = false;
            }
            if (newValue != oldValue)
            {
                this.strobe(newValue);
            }
        }, FixtureViewModel.STROBE_MILLIS);
        
        this.color = ko.computed<string>(() => 
        {
            let red = this.strobe() ? 0 : this.optionalChannelGet("Red") * 255;
            let green = this.strobe() ? 0 : this.optionalChannelGet("Green") * 255;
            let blue = this.strobe() ? 0 : this.optionalChannelGet("Blue") * 255;
            return "rgb(" + red + "," + green + "," + blue + ")";
        });

        this.color.subscribe((newValue: string) =>
        {
            this.render();
        });

        this.pan = ko.computed<number>(() =>
        {
            if (this.channelLookupTable["Pan"] != null)
            {
                return this.channelLookupTable["Pan"].value();
            }
            else if (this.channelLookupTable["PanCoarse"] != null)
            {
                return this.channelLookupTable["PanCoarse"].value();
            }
            else
            {
                return null;
            }
        });

        this.tilt = ko.computed<number>(() =>
        {
            if (this.channelLookupTable["Tilt"] != null)
            {
                return this.channelLookupTable["Tilt"].value();
            }
            else if (this.channelLookupTable["TiltCoarse"] != null)
            {
                return this.channelLookupTable["TiltCoarse"].value();
            }
            else
            {
                return null;
            }
        });

        this.pan.subscribe((newValue: number) =>
        {
            this.render();
        });

        this.color.notifySubscribers();
        this.pan.notifySubscribers();
    }

    render(): void
    {
        if (this.canvas == null)
        {
            this.canvas = ($("#canvas-" + this.address())[0] as HTMLCanvasElement);
            if (this.canvas == null)
            {
                return;
            }
            this.context = this.canvas.getContext("2d");
        }

        this.drawFill();
        this.drawMovements();
    }

    drawFill(): void
    {
        let fillStyle = this.color();

        this.context.fillStyle = fillStyle;
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
        this.context.strokeStyle = FixtureViewModel.invertColor(
            this.optionalChannelGet("Red"), this.optionalChannelGet("Green"), this.optionalChannelGet("Blue")
        );
    }

    drawMovements(): void
    {
        if (this.pan() != null)
        {
            let value: number = this.pan();
            let x: number = this.canvas.width * value;
            this.context.beginPath();
            this.context.moveTo(x, 0);
            this.context.lineTo(x, this.canvas.height);
            this.context.closePath();
            this.context.stroke();
        }
        if (this.tilt() != null)
        {
            let value: number = this.tilt();
            let y: number = this.canvas.height * value;
            this.context.beginPath();
            this.context.moveTo(0, y);
            this.context.lineTo(this.canvas.width, y);
            this.context.closePath();
            this.context.stroke();
        }
    }

    optionalChannelGet(...channels: string[]): number
    {
        for (let channelName of channels)
        {
            if (this.channelLookupTable[channelName] != null)
            {
                return this.channelLookupTable[channelName].value();
            }
        }
        return 0;
    }
    
    update(data: number[]): void
    {
        for (let channel of this.channels())
        {
            let value = data[channel.address() - 1];
            if (value >= channel.min && value <= channel.max)
            {
                channel.dmxValue(value);
            }
        }
    }
    
    static invertColor(red: number, green: number, blue: number): string
    {
        red = 255 - red;
        green = 255 - green;
        blue = 255 - blue;

        return "rgb(" + red + ", " + green + ", " + blue + ")";
    }
    
    static load(data: FixtureData): FixtureViewModel
    {
        let fixtureViewModel = new FixtureViewModel(data.name, data.address, data.definition.channels);
        
        for (let movementData of data.definition.movements)
        {

        }
        return fixtureViewModel;
    }
}