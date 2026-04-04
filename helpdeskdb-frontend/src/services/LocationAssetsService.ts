import { ILocationAsset, ILocationAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class LocationAssetsService extends EntityService<ILocationAsset, ILocationAssetAdd> {
	constructor() {
		super('locationassets')
	}
	async getLocationAssetByAssetId(assetId: string): Promise<ILocationAsset | null> {
		try {
			const locationAssetsResponse = await this.getAllAsync();
			if (!locationAssetsResponse.data) {
				console.error("Error getting locationassets:", locationAssetsResponse.errors);
				return null;
			}

			const locationAssetByAssetId: ILocationAsset | undefined = locationAssetsResponse.data?.find(ca => ca.assetId === assetId);
			if (locationAssetByAssetId === undefined) {
				return null;
			}

			return locationAssetByAssetId;
		} catch (error) {
			console.error("Error fetching asset name:", error);
			return null;
		}
	}
}
