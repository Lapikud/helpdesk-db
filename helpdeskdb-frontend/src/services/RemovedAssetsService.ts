import { IRemovedAsset, IRemovedAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class RemovedAssetsService extends EntityService<IRemovedAsset, IRemovedAssetAdd> {
	constructor(){
		super('removedAssets')
	}
}
