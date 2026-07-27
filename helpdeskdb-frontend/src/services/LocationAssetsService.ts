import { ILocationAsset, ILocationAssetAdd } from "@/types/domain/DomainTypes";
import { EntityService } from "./EntityService";
import { unwrap } from "./errors";

export class LocationAssetsService extends EntityService<ILocationAsset, ILocationAssetAdd> {
	constructor() {
		super('locationassets')
	}
	/**
	 * `null` strictly means the asset has no location mapping; a failed fetch
	 * rejects with `ApiError` instead. Callers (the edit-dialog queries) rely
	 * on that distinction — a swallowed error here would present as "no
	 * mapping" and let a save silently rewrite the asset's mappings.
	 */
	async getLocationAssetByAssetId(assetId: string): Promise<ILocationAsset | null> {
		const locationAssets = await unwrap(this.getAllAsync());
		return locationAssets.find(la => la.assetId === assetId) ?? null;
	}
}
