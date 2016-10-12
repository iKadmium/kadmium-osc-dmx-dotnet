﻿import {MVC} from "../..//MVC";
import {ModalEditor} from "./ModalEditor";
import {ListControllerData} from "./ListControllerData";

export class ListController<T extends ListControllerData>
{
    deleteItemID: string;
    modalEditor: ModalEditor<T>;
    controllerConstructor: () => T;

    constructor(controllerConstructor: () => T)
    {
        this.controllerConstructor = controllerConstructor;
        this.deleteItemID = "";

        let that = this;
        window.addEventListener("load", (ev: Event) =>
        {
            $("#modal-delete").on("click", (e: JQueryEventObject) =>
            {
                $("#modal-delete").prop("disabled", true);
                jQuery.post(ListController.getActionURL("Delete", that.deleteItemID), $.proxy(that.onSave, that));
            });

            $("#confirm-delete").on("show.bs.modal", (e: JQueryEventObject) =>
            {
                that.deleteItemID = $(e.relatedTarget).data("item-id");
                $(".item-id").text(that.deleteItemID);
            });
            this.modalEditor = new ModalEditor<T>(controllerConstructor);
        });
    }

    static rename(original: string, newName: string): void
    {
        let item = ListController.get(original);
        item.data("item-id", newName);
        item.children(".list-item-id").text(newName);
        item.children().children().each((index, elem) =>
        {
            $(elem).data("item-id", newName);
        });
    }

    static add(name: string): void
    {
        let prototype = $(".list-item-prototype");
        let parent = prototype.parent();
        let item = prototype.clone(true);
        item.removeClass("list-item-prototype");
        item.data("item-id", name);
        item.children(".list-item-id").text(name);
        item.children().children().each((index, elem) =>
        {
            $(elem).data("item-id", name);
        });
        parent.append(item);

    }

    static delete(name: string): void
    {
        let item = ListController.get(name);
        item.remove();
    }

    static getActionURL(action: string, id: string): string
    {
        let controller = $("#items").data("controller-url");
        return MVC.getActionURL(controller, action, id);
    }

    static get(name: string): JQuery
    {
        return $("#items").children().not(".list-item-prototype").filter((index, element) => $(element).data("item-id") == name);
    }

    onSave(data: any, textStatus: string, jqXHR: JQueryXHR)
    {
        ListController.delete(this.deleteItemID);
        $("#modal-delete").prop("disabled", false);
        ($('#confirm-delete') as any).modal("hide");
    }

    onLoad(): void
    {
        
    }
}