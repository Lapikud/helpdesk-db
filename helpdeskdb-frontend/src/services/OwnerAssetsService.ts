import { IOwnerAsset, IOwnerAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";
import { unwrap } from "./errors";

export class OwnerAssetsService extends EntityService<IOwnerAsset, IOwnerAssetAdd> {
	constructor() {
		super('ownerassets')
	}
	/**
	 * `null` strictly means the asset has no owner mapping; a failed fetch
	 * rejects with `ApiError` instead. Callers (the edit-dialog queries) rely
	 * on that distinction — a swallowed error here would present as "no
	 * mapping" and let a save silently rewrite the asset's mappings.
	 */
	async getOwnerAssetByAssetId(assetId: string): Promise<IOwnerAsset | null> {
		const ownerAssets = await unwrap(this.getAllAsync());
		return ownerAssets.find(oa => oa.assetId === assetId) ?? null;
	}
}
