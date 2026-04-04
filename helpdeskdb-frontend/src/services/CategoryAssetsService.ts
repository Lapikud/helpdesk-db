import { ICategoryAsset, ICategoryAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";

export class CategoryAssetsService extends EntityService<ICategoryAsset, ICategoryAssetAdd> {
	constructor() {
		super('categoryassets')
	}
	async getCategoryAssetByAssetId(assetId: string): Promise<ICategoryAsset | null> {
		try {
			const categoryAssetsResponse = await this.getAllAsync();
			if (!categoryAssetsResponse.data) {
				console.error("Error getting categoryassets:", categoryAssetsResponse.errors);
				return null;
			}

			const categoryAssetByAssetId: ICategoryAsset | undefined = categoryAssetsResponse.data?.find(ca => ca.assetId === assetId);
			if (categoryAssetByAssetId === undefined) {
				return null;
			}

			return categoryAssetByAssetId;
		} catch (error) {
			console.error("Error fetching asset name:", error);
			return null;
		}
	}
}
