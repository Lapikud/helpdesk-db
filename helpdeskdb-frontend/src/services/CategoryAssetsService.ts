import { ICategoryAsset, ICategoryAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";
import { unwrap } from "./errors";

export class CategoryAssetsService extends EntityService<ICategoryAsset, ICategoryAssetAdd> {
	constructor() {
		super('categoryassets')
	}
	/**
	 * `null` strictly means the asset has no category mapping; a failed fetch
	 * rejects with `ApiError` instead. Callers (the edit-dialog queries) rely
	 * on that distinction — a swallowed error here would present as "no
	 * mapping" and let a save silently rewrite the asset's mappings.
	 */
	async getCategoryAssetByAssetId(assetId: string): Promise<ICategoryAsset | null> {
		const categoryAssets = await unwrap(this.getAllAsync());
		return categoryAssets.find(ca => ca.assetId === assetId) ?? null;
	}
}
