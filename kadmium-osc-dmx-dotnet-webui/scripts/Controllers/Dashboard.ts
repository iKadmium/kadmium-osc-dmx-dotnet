﻿interface Status
{
    code: string;
    message: string;
    name: string;
    controller: string;
}

export class Dashboard
{
    static AlertClasses: string[] = ["alert-danger", "alert-success", "alert-warning"];
    static PanelClasses: string[] = ["panel-danger", "panel-success", "panel-warning"];
    static Glyphs: string[] = ["glyphicon-remove-sign", "glyphicon-ok-sign", "glyphicon-info-sign", "glyphicon-question-sign"];

    OnGetStatusSuccess(data: any, textStatus: string, xhr: JQueryXHR) : void
    {
        let status = JSON.parse(data) as Status;
        let panel = $(".status-panel").filter((index, element) =>
        {
            return $(element).data("id") == status.name
                && $(element).data("controller") == status.controller;
        });

        let glyph = panel.find(".status-glyph");
        let statusText = panel.find(".status-message");
        let panelBody = panel.find(".panel-body");

        panel.removeClass(Dashboard.PanelClasses.join(" "));
        panelBody.removeClass(Dashboard.AlertClasses.join(" "));
        glyph.removeClass(Dashboard.Glyphs.join(" "));

        switch (status.code)
        {
            case "NotStarted":
                panel.addClass("panel-warning");
                glyph.addClass("glyphicon-info-sign");
                panelBody.addClass("alert-warning");
                break;
            case "Running":
                panel.addClass("panel-success");
                glyph.addClass("glyphicon-ok-sign");
                panelBody.addClass("alert-success");
                break;
            case "Error":
                panel.addClass("panel-danger");
                glyph.addClass("glyphicon-remove-sign");
                panelBody.addClass("alert-danger");
                break;
        }

        statusText.text(status.message);
    }

    OnGetStatusError(xhr: JQueryXHR, textStatus: string, errorThrown: string) : void
    {
    }

    AddAlert(text: string, classes: string)
    {
        let alertDiv = document.createElement("div");
        $(alertDiv).addClass(classes);
        $(alertDiv).text(text);
        let closeButton = document.createElement("a");
        $(closeButton).addClass("close");
        closeButton.innerText = "×";
        $(closeButton).attr("data-dismiss", "alert");

        alertDiv.appendChild(closeButton);
        $("#status-alerts")[0].appendChild(alertDiv);
    }

    OnLoadVenueSuccess(data: any, textStatus: string, xhr: JQueryXHR): void
    {
        addAlert("Venue loaded successfully", "alert alert-success");
    }

    function onLoadVenueError(xhr: JQueryXHR, textStatus: string, errorThrown: string): void
    {
        addAlert("Error loading venue: " + errorThrown, "alert alert-danger");
    }
    
    function onLoad(): void
    {
        let panels = $(".status-panel");
        panels.each((index, elem) =>
        {
            let controller = $(elem).data("controller");
            let name = $(elem).data("id");
            let url = getActionURL(controller, "Status", name);
            let settings: JQueryAjaxSettings = {};
            settings.url = url;
            settings.success = onGetStatusSuccess;
            settings.error = onGetStatusError;
            $.ajax(settings);
            window.setInterval(() =>
            {
                $.ajax(settings);
            }, 1000);
        });

        let venueLoadLinks = $(".venue-load-link");
        venueLoadLinks.on("click", (e: JQueryEventObject) =>
        {
            let id = $(this).data("id");
            let url = getActionURL("Venues", "Load", id);
            let settings: JQueryAjaxSettings = {};
            settings.url = url;
            settings.success = onLoadVenueSuccess;
            settings.error = onLoadVenueError;
            $.ajax(settings);
        });
    }

    window.addEventListener("load", onLoad);
}