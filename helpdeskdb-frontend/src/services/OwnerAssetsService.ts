import { IOwnerAsset, IOwnerAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class OwnerAssetsService extends EntityService<IOwnerAsset, IOwnerAssetAdd> {
	constructor() {
		super('ownerassets')
	}
	async getOwnerAssetByAssetId(assetId: string): Promise<IOwnerAsset | null> {
		try {
			const ownerAssetsResponse = await this.getAllAsync();
			if (!ownerAssetsResponse.data) {
				console.error("Error getting ownerassets:", ownerAssetsResponse.errors);
				return null;
			}

			const ownerAssetByAssetId: IOwnerAsset | undefined = ownerAssetsResponse.data?.find(ca => ca.assetId === assetId);
			if (ownerAssetByAssetId === undefined) {
				return null;
			}

			return ownerAssetByAssetId;
		} catch (error) {
			console.error("Error fetching asset name:", error);
			return null;
		}
	}
}
