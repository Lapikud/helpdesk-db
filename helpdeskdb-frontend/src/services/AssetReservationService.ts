import { IAssetReservation, IAssetReservationAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class AssetReservationService extends EntityService<IAssetReservation, IAssetReservationAdd> {
    constructor(){
        super('assetReservations')
    }
}