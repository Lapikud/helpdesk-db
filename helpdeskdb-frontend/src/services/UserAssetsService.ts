
import { EntityService } from "./EntityService";
import { IUserAsset, IUserAssetAdd } from "@/types/domain/DomainTypes";

export class UserAssetsService extends EntityService<IUserAsset, IUserAssetAdd> {
	constructor(){
		super('userassets')
	}
}
